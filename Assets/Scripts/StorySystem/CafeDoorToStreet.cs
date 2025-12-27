using UnityEngine;
using UnityEngine.SceneManagement;

public class CafeDoorToStreet : MonoBehaviour
{
    public string streetSceneName = "Town";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var gs = GameState.Instance;
        if (gs == null) return;

        // Allow leaving when mentorStage >= 1 (mentor already sent you out)
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

