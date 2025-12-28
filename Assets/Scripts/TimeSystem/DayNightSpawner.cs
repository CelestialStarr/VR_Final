using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

public class DayNightCycleURP_Final : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("游戏里一整天对应现实多少秒")]
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

    // 事件
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
        lastIsNight = IsNight();
        lastSpawnWindowActive = IsSpawnWindowActive();

        // 初始刷新
        ApplyVisuals();
        ApplyNPCPopulation();
        OnDayChanged?.Invoke(dayCount);
    }

    void Update()
    {
        if (!autoRun) return;

        // 正常时间流逝
        float timeStep = Time.deltaTime / Mathf.Max(1f, dayLengthSeconds);
        AdvanceTimeInternal(timeStep);
    }

    // [新增] 核心方法：跳过时间（单位：游戏小时）
    // 比如睡觉跳过8小时，就传 8.0f
    public void SkipTime(float hoursToAdd)
    {
        // 把小时换算成 0-1 的比例
        // 8小时 = 8/24 = 0.3333...
        float timeStep = hoursToAdd / 24f;
        AdvanceTimeInternal(timeStep);

        Debug.Log($"<color=yellow>睡觉！跳过了 {hoursToAdd} 小时</color>");
    }

    // 内部处理时间增加、跨天、触发事件
    private void AdvanceTimeInternal(float amount)
    {
        time01 += amount;

        // 如果超过1（即过了午夜）
        if (time01 >= 1f)
        {
            time01 -= 1f; // 归位，比如 1.2 变成 0.2
            dayCount++;   // 天数+1
            OnDayChanged?.Invoke(dayCount); // 通知UI更新 "DAY X"
        }

        // 检查白天黑夜变化
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

    // --- 视觉相关保持不变 ---
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