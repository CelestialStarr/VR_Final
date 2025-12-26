using UnityEngine;

public class MentorTrigger : MonoBehaviour
{
    public StoryFlowController storyController;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Mentor: Interaction triggered");
            storyController.OnMentorInteraction();
        }
    }
}
