using UnityEngine;

public class StreetLampController : MonoBehaviour
{
    [Header("性能优化设置")]
    public bool useFakeLight = true;

    [Header("时间设置 (24小时制)")]
    [Tooltip("几点开灯？(比如 17 代表下午5点)")]
    public float turnOnHour = 17.0f;
    [Tooltip("几点关灯？(比如 6 代表早上6点)")]
    public float turnOffHour = 6.0f;

    [Header("组件引用")]
    public Light realLight;
    public Renderer bulbRenderer;
    public Renderer fakeLightSpotRenderer;

    [Header("材质设置")]
    public Material bulbOnMat;
    public Material bulbOffMat;

    private DayNightCycleURP_Final timeSystem;
    private bool isLampOn = false;

    void Start()
    {
        timeSystem = FindFirstObjectByType<DayNightCycleURP_Final>();
        if (timeSystem == null) Debug.LogError("找不到时间系统！");

        // 使用 InvokeRepeating 替代 Update，每 0.5 秒检查一次时间即可
        // 这样即使有 500 盏灯也不会卡顿
        InvokeRepeating(nameof(CheckTimeAndControlLight), 0f, 0.5f);
    }

    void CheckTimeAndControlLight()
    {
        if (timeSystem == null) return;

        // 1. 获取当前的小时数 (0 ~ 24)
        // timeSystem.time01 是 0~1，乘以 24 就是小时
        float currentHour = timeSystem.time01 * 24f;

        // 2. 判断是否在亮灯范围内
        // 逻辑：如果 现在时间 > 开灯时间 (17:00) 或者 现在时间 < 关灯时间 (6:00)
        bool shouldBeOn = (currentHour >= turnOnHour || currentHour < turnOffHour);

        // 3. 只有当状态发生改变时，才去执行开关灯操作 (节省性能)
        if (shouldBeOn != isLampOn)
        {
            isLampOn = shouldBeOn;
            SetLampState(isLampOn);
        }
    }

    void SetLampState(bool isOn)
    {
        // 灯泡材质
        if (bulbRenderer != null)
            bulbRenderer.sharedMaterial = isOn ? bulbOnMat : bulbOffMat;

        // 真实光源
        if (realLight != null)
        {
            if (useFakeLight) realLight.enabled = false;
            else realLight.enabled = isOn;
        }

        // 地面假光斑
        if (fakeLightSpotRenderer != null)
            fakeLightSpotRenderer.enabled = isOn;
    }
}