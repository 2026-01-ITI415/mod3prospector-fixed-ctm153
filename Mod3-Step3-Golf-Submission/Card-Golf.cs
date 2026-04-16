using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum eGolfCardState { stock, tableau, waste }

/// <summary>

/// </summary>
public class CardGolf : Card
{
    [Header("Dynamic: CardGolf")]
    public eGolfCardState state = eGolfCardState.stock;

    public int layoutID;

    public JsonLayoutSlot layoutSlot;

    /// </summary>
    override public void OnMouseUpAsButton()
    {
        base.OnMouseUpAsButton();
    }
}
