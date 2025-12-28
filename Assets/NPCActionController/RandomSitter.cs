using UnityEngine;
using System.Collections;

public class RandomSitter : MonoBehaviour
{
    [Header("Settings")]
    public Animator animator; // Drag your NPC Animator component here
    public string paramName = "IS SITTING"; // Must match the EXACT name in your Animator Parameter!

    [Header("Time Interval (Seconds)")]
    public float minTime = 5f; // Minimum time to stay in current state
    public float maxTime = 15f; // Maximum time to stay in current state

    void Start()
    {
        // If Animator is not manually assigned in Inspector, try to get it from this GameObject
        if (animator == null)
            animator = GetComponent<Animator>();

        // Start the infinite loop for random switching
        StartCoroutine(RandomSwitchRoutine());
    }

    IEnumerator RandomSwitchRoutine()
    {
        while (true) // Infinite loop
        {
            // 1. Wait for a random duration (between minTime and maxTime)
            float waitTime = Random.Range(minTime, maxTime);
            // Debug.Log($"NPC will switch pose in {waitTime:F1} seconds...");
            yield return new WaitForSeconds(waitTime);

            // 2. Check the current state
            // Get current boolean value of "IS SITTING"
            bool isSitting = animator.GetBool(paramName);

            // 3. Toggle the state
            // If true (Sitting) -> becomes false (Stand Up)
            // If false (Standing) -> becomes true (Sit Down)
            animator.SetBool(paramName, !isSitting);
        }
    }
}