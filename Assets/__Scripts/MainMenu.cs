using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu — lets the player choose Prospector or Golf Solitaire.
/// Attach to any GameObject in the __MainMenu_Scene scene.
/// </summary>
public class MainMenu : MonoBehaviour
{
    private GUIStyle titleStyle;
    private GUIStyle gameStyle;
    private bool stylesReady = false;

    void InitStyles()
    {
        if (stylesReady) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 52,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white }
        };

        gameStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        stylesReady = true;
    }

    void OnGUI()
    {
        InitStyles();

        float cx = Screen.width  * 0.5f;
        float cy = Screen.height * 0.5f;
        float bw = 340f;
        float bh = 90f;
        float gap = 30f;

        // ── Title ──────────────────────────────────────────────────────────────
        GUI.Label(new Rect(cx - 300, cy - 180, 600, 90), "Solitaire Collection", titleStyle);

        // ── Prospector button ──────────────────────────────────────────────────
        if (GUI.Button(new Rect(cx - bw * 0.5f, cy - bh - gap * 0.5f, bw, bh),
                       "Prospector", gameStyle))
        {
            SceneManager.LoadScene("__Prospector_Scene_0");
        }

        // ── Golf button ────────────────────────────────────────────────────────
        if (GUI.Button(new Rect(cx - bw * 0.5f, cy + gap * 0.5f, bw, bh),
                       "Golf", gameStyle))
        {
            SceneManager.LoadScene("__Golf_Scene_0");
        }
    }
}
