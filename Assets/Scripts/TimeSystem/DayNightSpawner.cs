using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

public class DayNightCycleURP_Final : MonoBehaviour
{
    // [核心修改 1] 静态变量：这是“全局保险箱”，切换场景不会消失
    private static float _savedTime01 = -1f;
    private static int _savedDayCount = -1;

    [Header("Time Settings")]
    [Tooltip("Real-time seconds for one in-game day")]
    public float dayLengthSeconds = 300f;

    [Range(0f, 1f)]
    public float time01 = 0f;

    [Header("Game Loop Info")]
    public int dayCount = 1;

    public bool autoRun = true;

    [Header("Day / Night Settings")]
    [Range(0f, 1f)]
    public float dayPortion = 0.7f;

    [Header("Visuals")]
    public Light sunLight;
    public AnimationCurve sunIntensityOverDay;
    public Volume globalVolume;
    public Gradient colorFilterOverDay;
    public AnimationCurve postExposureOverDay;
    public AnimationCurve saturationOverDay;
    public AnimationCurve gainMultiplierOverDay;
    [Range(0f, 1f)]
    public float nightShadowTintStrength = 0.7f;

    [Header("NPC Population")]
    public AISpawner[] spawners;
    public int basePopulation = 10;
    public bool clearNPCsAtNight = true;
    [Range(0f, 1f)]
    public float spawnWindowStart = 0.1f;
    [Range(0f, 1f)]
    public float spawnWindowEnd = 0.8f;

    public event Action<int> OnDayChanged;
    public event Action<bool> OnNightStateChanged;

    private ColorAdjustments colorAdjustments;
    private LiftGammaGain liftGammaGain;
    private ShadowsMidtonesHighlights smh;

    private bool lastIsNight;
    private bool lastSpawnWindowActive;

    void Start()
    {
        CacheVolumeOverrides();

        // [核心修改 2] 场景开始时，检查有没有存档数据
        // 如果 savedTime01 大于等于0，说明我们是从别的场景过来的，需要读取数据
        if (_savedTime01 >= 0f)
        {
            time01 = _savedTime01;
            dayCount = _savedDayCount;
            Debug.Log($"<color=green>已加载跨场景时间数据: Day {dayCount}, Time {time01}</color>");
        }
        else
        {
            // 如果是第一次运行游戏，就保存初始状态
            _savedTime01 = time01;
            _savedDayCount = dayCount;
        }

        lastIsNight = IsNight();
        lastSpawnWindowActive = IsSpawnWindowActive();

        ApplyVisuals();
        ApplyNPCPopulation();

        // 强制刷新一次UI，确保UI显示正确的天数
        OnDayChanged?.Invoke(dayCount);
    }

    void Update()
    {
        if (!autoRun) return;

        float timeStep = Time.deltaTime / Mathf.Max(1f, dayLengthSeconds);
        AdvanceTimeInternal(timeStep);

        // [核心修改 3] 每一帧都把当前时间存入“保险箱”
        // 这样无论什么时候切换场景，数据都是最新的
        _savedTime01 = time01;
        _savedDayCount = dayCount;
    }

    // [新增] 如果你想在主菜单做“开始新游戏”按钮，必须调用这个方法！
    // 否则新游戏开始时，时间还是旧的。
    public void ResetGameTime()
    {
        _savedTime01 = 0f;
        _savedDayCount = 1;
        time01 = 0f;
        dayCount = 1;
        Debug.Log("游戏时间已重置！");
    }

    // --- 以下逻辑保持不变 ---

    public void SkipTime(float hoursToAdd)
    {
        float timeStep = hoursToAdd / 24f;
        AdvanceTimeInternal(timeStep);
    }

    private void AdvanceTimeInternal(float amount)
    {
        time01 += amount;

        if (time01 >= 1f)
        {
            time01 -= 1f;
            dayCount++;
            OnDayChanged?.Invoke(dayCount);
        }

        bool isNight = IsNight();
        bool spawnWindowActive = IsSpawnWindowActive();

        if (isNight != lastIsNight)
        {
            lastIsNight = isNight;
            OnNightStateChanged?.Invoke(isNight);
        }

        if (isNight != lastIsNight || spawnWindowActive != lastSpawnWindowActive)
        {
            lastSpawnWindowActive = spawnWindowActive;
            ApplyNPCPopulation();
        }

        ApplyVisuals();
    }

    public void GetCurrentTime(out int hour, out int minute)
    {
        float totalHours = time01 * 24f;
        hour = Mathf.FloorToInt(totalHours);
        minute = Mathf.FloorToInt((totalHours - hour) * 60f);
    }

    public bool IsNight() => time01 >= dayPortion;
    private bool IsSpawnWindowActive() => (time01 >= spawnWindowStart && time01 <= spawnWindowEnd);

    private void CacheVolumeOverrides()
    {
        if (globalVolume == null || globalVolume.profile == null) return;
        globalVolume.profile.TryGet(out colorAdjustments);
        globalVolume.profile.TryGet(out liftGammaGain);
        globalVolume.profile.TryGet(out smh);
    }

    private void ApplyVisuals()
    {
        if (sunLight != null && sunIntensityOverDay != null)
        {
            float sunAngle = time01 * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
            sunLight.intensity = sunIntensityOverDay.Evaluate(time01);
        }
        if (colorAdjustments != null)
        {
            if (colorFilterOverDay != null) colorAdjustments.colorFilter.value = colorFilterOverDay.Evaluate(time01);
            if (postExposureOverDay != null) colorAdjustments.postExposure.value = postExposureOverDay.Evaluate(time01);
            if (saturationOverDay != null) colorAdjustments.saturation.value = saturationOverDay.Evaluate(time01);
        }
        if (liftGammaGain != null && gainMultiplierOverDay != null)
        {
            float m = Mathf.Clamp(gainMultiplierOverDay.Evaluate(time01), 0.2f, 2.0f);
            Color mood = (colorFilterOverDay != null) ? colorFilterOverDay.Evaluate(time01) : Color.white;
            liftGammaGain.gain.value = new Vector4(mood.r * m, mood.g * m, mood.b * m, 1f);
        }
        if (smh != null)
        {
            float t = IsNight() ? nightShadowTintStrength : 0f;
            smh.shadows.value = Color.Lerp(Color.white, new Color(0.75f, 0.85f, 1.15f, 1f), t);
            smh.midtones.value = Color.Lerp(Color.white, new Color(0.92f, 0.97f, 1.08f, 1f), t * 0.6f);
        }
        DynamicGI.UpdateEnvironment();
    }

    private void ApplyNPCPopulation()
    {
        if (spawners == null) return;
        bool active = IsSpawnWindowActive() && !IsNight();
        foreach (var spawner in spawners)
        {
            if (spawner == null) continue;
            spawner.SetTargetPopulation(basePopulation);
            if (active) spawner.ExitNightMode(); else spawner.EnterNightMode();
        }
    }
}