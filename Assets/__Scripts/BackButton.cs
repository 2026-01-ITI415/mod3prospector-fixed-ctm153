using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Draws a "Main Menu" back button in the top-left corner of any game scene.
/// Attach this component to any GameObject in the scene.
/// </summary>
public class BackButton : MonoBehaviour
{
    private GUIStyle btnStyle;
    private bool stylesReady = false;

    void InitStyles()
    {
        if (stylesReady) return;

        btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        stylesReady = true;
    }

    void OnGUI()
    {
        InitStyles();

        // Top-left corner, with a small margin
        if (GUI.Button(new Rect(15, 15, 180, 50), "◀  Main Menu", btnStyle))
        {
            SceneManager.LoadScene("__MainMenu_Scene");
        }
    }
}
