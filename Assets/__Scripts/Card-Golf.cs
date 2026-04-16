using UnityEngine;

// Defines the possible states a Golf card can be in.
// Used by GolfGame to track each CardProspector's current role.
public enum eGolfCardState { stock, tableau, waste }

// CardGolf is no longer used as a prefab — GolfGame works directly with
// CardProspector. This file is kept only to define eGolfCardState.
