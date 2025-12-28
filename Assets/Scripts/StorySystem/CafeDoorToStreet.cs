using UnityEngine;
using UnityEngine.SceneManagement;

public class CafeDoorToStreet : MonoBehaviour
{
    public string streetSceneName = "Town";

    // Call this from UI Button OnClick()
    public void GoToStreet()
    {
        var gs = GameState.Instance;
        if (gs == null)
        {
            Debug.LogWarning("GameState not found.");
            return;
        }

        if (gs.mentorStage >= 1)
        {
            SceneManager.LoadScene(streetSceneName);
        }
        else
        {
            Debug.Log("Door: You are not ready to leave yet.");
        }
    }
}
