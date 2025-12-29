using UnityEngine;

public class AlertDebugTester : MonoBehaviour
{
    [Header("Reference")]
    public AlertController alertController;

    [Header("Test Settings")]
    public float increasePerSecond = 40f;

    [Tooltip("按住这个键会持续增加警戒值")]
    public KeyCode increaseKey = KeyCode.F;

    void Start()
    {
        if (alertController == null)
            alertController = GetComponent<AlertController>();
    }

    void Update()
    {
        if (alertController == null) return;

        // 按住键 → 持续增加警戒值
        if (Input.GetKey(increaseKey))
        {
            alertController.IncreaseAlert(increasePerSecond * Time.deltaTime);
        }
    }
}
