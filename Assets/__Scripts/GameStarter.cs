using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically redirects to the main menu when Play is pressed in the editor,
/// regardless of which scene is currently open.
///
/// RuntimeInitializeOnLoadMethod fires ONCE per Play session (not on every
/// scene load), so game-over scene reloads work normally.
/// </summary>
public class GameStarter
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadMainMenu()
    {
        SceneManager.LoadScene("__MainMenu_Scene");
    }
}
