using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryFlowController : MonoBehaviour
{
    public enum StoryState
    {
        Cafe_Intro,
        Mentor_Talked,
        Can_Leave_Cafe,
        Street_Tutorial,
        Tutorial_Complete
    }

    public StoryState currentState = StoryState.Cafe_Intro;

    void Start()
    {
        EnterState(currentState);
    }

    void EnterState(StoryState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case StoryState.Cafe_Intro:
                Debug.Log("Story: In cafe. Talk to mentor.");
                DisableTheft();
                break;

            case StoryState.Mentor_Talked:
                Debug.Log("Story: Mentor gave first task.");
                break;

            case StoryState.Can_Leave_Cafe:
                Debug.Log("Story: You may leave the cafe.");
                break;

            case StoryState.Street_Tutorial:
                Debug.Log("Story: Street tutorial started.");
                EnableTheft();
                break;

            case StoryState.Tutorial_Complete:
                Debug.Log("Story: Tutorial completed.");
                break;
        }
    }

    // ===== External Triggers =====

    public void OnMentorInteraction()
    {
        if (currentState == StoryState.Cafe_Intro)
        {
            EnterState(StoryState.Mentor_Talked);
            EnterState(StoryState.Can_Leave_Cafe);
        }
    }

    public void OnExitCafe()
    {
        if (currentState == StoryState.Can_Leave_Cafe)
        {
            EnterState(StoryState.Street_Tutorial);
            LoadStreetScene();
        }
    }

    // ===== System Placeholders =====

    void EnableTheft()
    {
        Debug.Log("System: Theft Enabled");
        // Call teammate system later
    }

    void DisableTheft()
    {
        Debug.Log("System: Theft Disabled");
    }

    void LoadStreetScene()
    {
        SceneManager.LoadScene("StreetScene");
    }
}
