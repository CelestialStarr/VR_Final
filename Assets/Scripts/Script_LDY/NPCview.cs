using UnityEngine;

public class NPCVision : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask stealableLayer;
    public LayerMask playerLayer;

    public Transform eyesPoint;
    public float visionRadius = 0.3f;

    [Header("Alert Settings")]
    public AlertController alertController;

    [Header("Vision Settings")]
    public float detectRange = 10f;
    public float alertIncreaseAmount = 30f;
    public float alertInterval = 0.5f;
    private float currentAlertTimer = 0f;

    [SerializeField] private bool isInPrivateArea = false;

    [Header("UI Feedback")]
    public GameObject alertUI;
    public float alertUIDuration = 2f;

    private bool hasAlertedPolice = false;

    private void Start()
    {
        if (alertController == null)
            alertController = GetComponent<AlertController>();

        if (alertUI != null)
            alertUI.SetActive(false);
    }

    void Update()
    {
        if (eyesPoint == null) return;
        CheckVision();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PrivateArea"))
        {
            isInPrivateArea = true;
            currentAlertTimer = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PrivateArea"))
        {
            isInPrivateArea = false;
        }
    }

    private void CheckVision()
    {
        Vector3 startPos = eyesPoint.position;
        Vector3 direction = eyesPoint.forward;
        RaycastHit hit;

        LayerMask combinedMask = stealableLayer | playerLayer;

        if (Physics.SphereCast(startPos, visionRadius, direction, out hit, detectRange, combinedMask))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (isInPrivateArea && hitObj.CompareTag("Player"))
            {
                Debug.Log("See Player");

                if (alertController != null && !alertController.isAlerted)
                {
                    Debug.Log("CheckTimer");
                    currentAlertTimer += Time.deltaTime;

                    if (currentAlertTimer >= alertInterval)
                    {
                        alertController.IncreaseAlert(alertIncreaseAmount);
                        Debug.Log("Player detected in private area! Increasing alert!");
                        currentAlertTimer = 0f;
                    }
                }

                return;
            }

            StealableObject target = hitObj.GetComponentInParent<StealableObject>();

            if (target != null && target.IsBeingStolen)
            {
                Debug.LogError("NPC: Caught you! Executing forced interrupt!");
                target.HandleFailure(true);

                if (alertUI != null && !hasAlertedPolice)
                {
                    alertUI.SetActive(true);
                    CancelInvoke(nameof(HideAlertUI));
                    Invoke(nameof(HideAlertUI), alertUIDuration);
                }

                if (!hasAlertedPolice)
                {
                    Debug.Log("NPC: Stop thief!");

                    if (PoliceManager.Instance != null)
                        PoliceManager.Instance.DispatchNearestPolice(transform.position);

                    hasAlertedPolice = true;

                    if (alertController != null)
                        alertController.IncreaseAlert(alertController.maxAlertValue);

                    Invoke(nameof(ResetAlert), 5f);
                }
            }
        }
        else
        {
            currentAlertTimer = 0f;
        }
    }

    void HideAlertUI()
    {
        if (alertUI != null)
            alertUI.SetActive(false);
    }

    void ResetAlert()
    {
        hasAlertedPolice = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (eyesPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(eyesPoint.position, visionRadius);
            Gizmos.DrawRay(eyesPoint.position, eyesPoint.forward * detectRange);
        }
    }
}