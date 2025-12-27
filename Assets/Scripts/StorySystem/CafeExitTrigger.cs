using UnityEngine;

public class CafeExitTrigger : MonoBehaviour
{
    public StoryFlowController storyController;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            storyController.OnExitCafe();
        }
    }
}
