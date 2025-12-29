using UnityEngine;
using UnityEngine.UI;

public class AlertUIController : MonoBehaviour
{
    [Header("References")]
    public AlertController alertController;

    [Header("UI Prefabs")]
    public GameObject alertUIPrefab;      // 警戒条 Canvas
    public GameObject warningUIPrefab;    // Warning Canvas（❗）

    [Header("Threshold Settings")]
    public float showAlertBarThreshold = 30f; // 开始显示警戒条
    public float warningThreshold = 100f;     // 满警戒显示 Warning

    [Header("UI Offsets")]
    public Vector3 alertUIOffset = new Vector3(0, 1.8f, 0);
    public Vector3 warningUIOffset = new Vector3(0, 2.4f, 0);

    private GameObject alertUIInstance;
    private GameObject warningUIInstance;
    private Image alertFillImage;

    void Start()
    {
        if (alertController == null)
            alertController = GetComponent<AlertController>();

        // ===== 警戒条 =====
        if (alertUIPrefab != null)
        {
            alertUIInstance = Instantiate(alertUIPrefab, transform);
            alertUIInstance.transform.localPosition = alertUIOffset;
            alertUIInstance.SetActive(false);

            alertFillImage = alertUIInstance.GetComponentInChildren<Image>();
        }

        // ===== Warning =====
        if (warningUIPrefab != null)
        {
            warningUIInstance = Instantiate(warningUIPrefab, transform);
            warningUIInstance.transform.localPosition = warningUIOffset;
            warningUIInstance.SetActive(false);
        }
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (alertController == null) return;

        float alertValue = alertController.currentAlertValue;
        float maxValue = alertController.maxAlertValue;

        // ===== 低于显示阈值：全部隐藏 =====
        if (alertValue < showAlertBarThreshold)
        {
            HideAllUI();
            return;
        }

        // ===== 显示警戒条（>= showAlertBarThreshold）=====
        ShowAlertBar(alertValue, maxValue);

        // ===== 满警戒：额外显示 Warning =====
        if (alertValue >= warningThreshold)
        {
            ShowWarning();
        }
        else
        {
            HideWarning();
        }
    }

    void ShowAlertBar(float alertValue, float maxValue)
    {
        if (alertUIInstance == null) return;

        if (!alertUIInstance.activeSelf)
            alertUIInstance.SetActive(true);

        if (alertFillImage != null && maxValue > 0f)
        {
            float t = Mathf.Clamp01(alertValue / maxValue);
            alertFillImage.fillAmount = t;
            alertFillImage.color = (t < 0.7f) ? Color.yellow : Color.red;
        }
    }

    void ShowWarning()
    {
        if (warningUIInstance != null && !warningUIInstance.activeSelf)
            warningUIInstance.SetActive(true);
    }

    void HideWarning()
    {
        if (warningUIInstance != null && warningUIInstance.activeSelf)
            warningUIInstance.SetActive(false);
    }

    void HideAllUI()
    {
        if (alertUIInstance != null)
            alertUIInstance.SetActive(false);

        if (warningUIInstance != null)
            warningUIInstance.SetActive(false);
    }
}