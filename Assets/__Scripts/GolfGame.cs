using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GolfGame manages Golf Solitaire gameplay.
/// Works with the existing CardProspector prefab — no separate prefab required.
///
/// Golf Rules:
///   - 35 tableau cards in 7 columns x 5 rows (ALL face-up / visible)
///   - Only the EXPOSED card of each column is playable (not blocked by any card below it)
///   - Play a card if its rank is exactly 1 higher or lower than the top waste card
///     (Ace wraps to King and vice versa)
///   - Draw from the stock pile when no valid move exists
///   - Win: clear all 35 tableau cards
///   - Lose: stock is empty and no valid moves remain
/// </summary>
[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(JsonParseLayout))]
public class GolfGame : MonoBehaviour
{
    // Public static singleton — CardProspector checks this to route clicks here
    public static GolfGame S;

    [Header("Dynamic")]
    public List<CardProspector> stock   = new List<CardProspector>();
    public List<CardProspector> waste   = new List<CardProspector>();
    public List<CardProspector> tableau = new List<CardProspector>();
    public CardProspector target;

    private Transform layoutAnchor;
    private Deck deck;
    private JsonLayout layout;

    // Tracks which Golf state each card is in (stock / tableau / waste)
    private Dictionary<CardProspector, eGolfCardState> cardState
        = new Dictionary<CardProspector, eGolfCardState>();

    // Maps layout slot IDs to their card so we can check blocking
    private Dictionary<int, CardProspector> idToCard
        = new Dictionary<int, CardProspector>();

    // -- Lifecycle -------------------------------------------------------------

    void Start()
    {
        S = this;

        layout = GetComponent<JsonParseLayout>().layout;
        deck   = GetComponent<Deck>();

        deck.InitDeck();
        Deck.Shuffle(ref deck.cards);

        // Pull all CardProspector instances out of the shuffled deck
        foreach (Card c in deck.cards)
        {
            CardProspector cp = c as CardProspector;
            if (cp == null) continue;
            stock.Add(cp);
            cardState[cp] = eGolfCardState.stock;
        }

        LayoutTableau();
        MoveToWaste(DrawFromStock());
        UpdateStock();
    }

    void OnDestroy()
    {
        // Clear singleton when scene unloads so Prospector clicks work normally
        if (S == this) S = null;
    }

    // -- Helpers ---------------------------------------------------------------

    CardProspector DrawFromStock()
    {
        if (stock.Count == 0) return null;
        CardProspector cp = stock[0];
        stock.RemoveAt(0);
        return cp;
    }

    eGolfCardState GetState(CardProspector cp)
    {
        return cardState.ContainsKey(cp) ? cardState[cp] : eGolfCardState.stock;
    }

    // -- Layout ----------------------------------------------------------------

    void LayoutTableau()
    {
        if (layoutAnchor == null)
        {
            layoutAnchor = new GameObject("_LayoutAnchor").transform;
        }

        foreach (JsonLayoutSlot slot in layout.slots)
        {
            CardProspector cp = DrawFromStock();
            if (cp == null) continue;

            // All tableau cards always show face-up in Golf
            cp.faceUp = true;
            cp.transform.SetParent(layoutAnchor);

            int z = int.Parse(slot.layer[slot.layer.Length - 1].ToString());
            cp.SetLocalPos(new Vector3(
                layout.multiplier.x * slot.x,
                layout.multiplier.y * slot.y,
                -z));

            cp.layoutID   = slot.id;
            cp.layoutSlot = slot;
            cardState[cp] = eGolfCardState.tableau;
            cp.SetSpriteSortingLayer(slot.layer);

            tableau.Add(cp);
            idToCard[slot.id] = cp;
        }
    }

    // -- Card movement ---------------------------------------------------------

    void MoveToWaste(CardProspector cp)
    {
        if (cp == null) return;

        // Push the current target behind into the waste pile
        if (target != null)
        {
            cardState[target] = eGolfCardState.waste;
            waste.Add(target);
            target.SetSpriteSortingLayer(layout.discardPile.layer);
            target.SetSortingOrder(-200 + (waste.Count * 3));
        }

        cp.faceUp = true;
        cp.transform.SetParent(layoutAnchor);
        cp.SetLocalPos(new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            0));
        cp.SetSpriteSortingLayer("Target");
        cp.SetSortingOrder(0);
        cardState[cp] = eGolfCardState.waste;
        target = cp;
    }

    void UpdateStock()
    {
        for (int i = 0; i < stock.Count; i++)
        {
            CardProspector cp = stock[i];
            cp.transform.SetParent(layoutAnchor);
            cp.SetLocalPos(new Vector3(
                layout.multiplier.x * layout.drawPile.x + layout.drawPile.xStagger * i,
                layout.multiplier.y * layout.drawPile.y,
                0.1f * i));
            cp.faceUp = false;
            cardState[cp] = eGolfCardState.stock;
            cp.SetSpriteSortingLayer(layout.drawPile.layer);
            cp.SetSortingOrder(-10 * i);
        }
    }

    // -- Exposed check ---------------------------------------------------------

    /// <summary>
    /// A tableau card is playable when nothing in its hiddenBy list is still on the tableau.
    /// All cards stay face-up for visibility; only exposed ones are valid moves.
    /// </summary>
    bool IsExposed(CardProspector cp)
    {
        if (cp.layoutSlot == null) return true;
        foreach (int blockID in cp.layoutSlot.hiddenBy)
        {
            if (idToCard.ContainsKey(blockID) &&
                GetState(idToCard[blockID]) == eGolfCardState.tableau)
                return false;
        }
        return true;
    }

    // -- Adjacency -------------------------------------------------------------

    bool IsAdjacent(CardProspector a, CardProspector b)
    {
        if (a == null || b == null) return false;
        int diff = Mathf.Abs(a.rank - b.rank);
        return diff == 1 || diff == 12; // 12 handles A-K wrap
    }

    // -- Win / Loss ------------------------------------------------------------

    void CheckGameOver()
    {
        if (tableau.Count == 0) { GameOver(true);  return; }
        if (stock.Count > 0)    return; // still cards to draw

        foreach (CardProspector cp in tableau)
        {
            if (IsExposed(cp) && IsAdjacent(cp, target)) return; // valid move exists
        }
        GameOver(false);
    }

    void GameOver(bool won)
    {
        ScoreManager.TALLY(won ? eScoreEvent.gameWin : eScoreEvent.gameLoss);
        CardSpritesSO.RESET();
        SceneManager.LoadScene("__Golf_Scene_0");
    }

    // -- Click handler (called by CardProspector when S != null) ---------------

    public static void CARD_CLICKED(CardProspector cp)
    {
        eGolfCardState state = S.GetState(cp);

        switch (state)
        {
            case eGolfCardState.stock:
                CardProspector drawn = S.DrawFromStock();
                if (drawn != null)
                {
                    S.MoveToWaste(drawn);
                    S.UpdateStock();
                    ScoreManager.TALLY(eScoreEvent.draw);
                }
                break;

            case eGolfCardState.tableau:
                if (!S.IsExposed(cp))            break; // blocked by another card
                if (!S.IsAdjacent(cp, S.target)) break; // not a valid rank move

                S.tableau.Remove(cp);
                S.idToCard.Remove(cp.layoutID);
                S.MoveToWaste(cp);
                ScoreManager.TALLY(eScoreEvent.mine);
                break;

            case eGolfCardState.waste:
                // Clicking the waste pile does nothing
                break;
        }

        S.CheckGameOver();
    }
}
