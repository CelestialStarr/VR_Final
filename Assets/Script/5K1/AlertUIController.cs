using UnityEngine;
using UnityEngine.UI;

public class AlertUIController : MonoBehaviour
{
    [Header("References")]
    public AlertController alertController;

    [Header("UI Prefab")]
    public GameObject alertUIPrefab;

    [Header("UI Settings")]
    public float showUIThreshold = 30f;
    public Vector3 uiOffset = new Vector3(0, 1.8f, 0);

    private GameObject alertUIInstance;
    private Image alertFillImage;

    void Start()
    {
        if (alertController == null)
            alertController = GetComponent<AlertController>();

        // 自动生成 UI
        if (alertUIPrefab != null && alertController != null)
        {
            alertUIInstance = Instantiate(alertUIPrefab, transform);
            alertUIInstance.transform.localPosition = uiOffset;

            alertFillImage = alertUIInstance.GetComponentInChildren<Image>();
            alertUIInstance.SetActive(false);
        }

        // 监听警戒状态事件（可选，但很优雅）
        alertController.OnEnterAlert.AddListener(OnEnterAlert);
        alertController.OnExitAlert.AddListener(OnExitAlert);
    }

    void Update()
    {
        UpdateAlertUI();
    }

    void UpdateAlertUI()
    {
        if (alertUIInstance == null || alertController == null) return;

        float alertValue = alertController.currentAlertValue;
        float maxValue = alertController.maxAlertValue;

        // 巡逻 / 低警戒 → 不显示
        if (alertValue < showUIThreshold)
        {
            if (alertUIInstance.activeSelf)
                alertUIInstance.SetActive(false);
            return;
        }

        if (!alertUIInstance.activeSelf)
            alertUIInstance.SetActive(true);

        float t = alertValue / maxValue;
        alertFillImage.fillAmount = t;

        // 颜色变化
        if (t < 0.7f)
            alertFillImage.color = Color.yellow;
        else
            alertFillImage.color = Color.red;
    }

    void OnEnterAlert()
    {
        // 这里可以做 UI 特效 / 闪烁 / 图标变化
        // Debug.Log("UI: 进入警戒状态");
    }

    void OnExitAlert()
    {
        if (alertUIInstance != null)
            alertUIInstance.SetActive(false);
    }
}
