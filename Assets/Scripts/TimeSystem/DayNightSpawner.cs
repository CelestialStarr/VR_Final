using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

public class DayNightCycleURP_Final : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Real-time seconds for one in-game day")]
    public float dayLengthSeconds = 300f;

    [Range(0f, 1f)]
    [Tooltip("Normalized time of day (0 = start, 1 = end)")]
    public float time01 = 0f;

    public bool autoRun = true;

    [Header("Day / Night Settings")]
    [Range(0f, 1f)]
    [Tooltip("Portion of the day considered daytime (e.g., 0.7 = last 30% is night)")]
    public float dayPortion = 0.7f;

    [Header("Directional Light (Optional)")]
    public Light sunLight;
    public AnimationCurve sunIntensityOverDay;

    [Header("URP Global Volume (Required for visible change)")]
    [Tooltip("Assign your Global Volume. Profile must contain Color Adjustments override.")]
    public Volume globalVolume;

    [Header("Post-Processing (Color Adjustments)")]
    public Gradient colorFilterOverDay;
    public AnimationCurve postExposureOverDay;
    public AnimationCurve saturationOverDay;

    [Header("Optional Mood Enhancers (If present in the same Volume Profile)")]
    public AnimationCurve gainMultiplierOverDay;

    [Range(0f, 1f)]
    public float nightShadowTintStrength = 0.7f;

    [Header("NPC Population Control")]
    public AISpawner[] spawners;
    public int basePopulation = 10;

    [Tooltip("If true, night population is forced to 0 and existing NPCs are removed.")]
    public bool clearNPCsAtNight = true;

    [Header("Spawn Window Settings")]
    [Tooltip("NPC spawning is allowed only when time01 is within [spawnWindowStart, spawnWindowEnd].")]
    [Range(0f, 1f)]
    public float spawnWindowStart = 0.1f;

    [Range(0f, 1f)]
    public float spawnWindowEnd = 0.8f;

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

        ApplyVisuals();
        ApplyNPCPopulation();
    }

    private bool IsSpawnWindowActive()
    {
        return (time01 >= spawnWindowStart && time01 <= spawnWindowEnd);
    }


    void Update()
    {
        if (!autoRun) return;

        time01 += Time.deltaTime / Mathf.Max(1f, dayLengthSeconds);
        if (time01 > 1f) time01 -= 1f;

        bool isNight = IsNight();
        bool spawnWindowActive = IsSpawnWindowActive();

        bool nightChanged = (isNight != lastIsNight);
        bool spawnWindowChanged = (spawnWindowActive != lastSpawnWindowActive);

        if (nightChanged)
        {
            lastIsNight = isNight;
            OnNightStateChanged?.Invoke(isNight);
        }

        if (nightChanged || spawnWindowChanged)
        {
            lastSpawnWindowActive = spawnWindowActive;
            ApplyNPCPopulation();
        }

        ApplyVisuals();
    }

    public bool IsNight()
    {
        return time01 >= dayPortion;
    }

    public void SetTime01(float value)
    {
        time01 = Mathf.Repeat(value, 1f);

        bool isNight = IsNight();
        if (isNight != lastIsNight)
        {
            lastIsNight = isNight;
            OnNightStateChanged?.Invoke(isNight);
        }

        ApplyVisuals();
        ApplyNPCPopulation();
    }

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
            if (colorFilterOverDay != null)
                colorAdjustments.colorFilter.value = colorFilterOverDay.Evaluate(time01);

            if (postExposureOverDay != null)
                colorAdjustments.postExposure.value = postExposureOverDay.Evaluate(time01);

            if (saturationOverDay != null)
                colorAdjustments.saturation.value = saturationOverDay.Evaluate(time01);
        }

        if (liftGammaGain != null && gainMultiplierOverDay != null)
        {
            float m = Mathf.Clamp(gainMultiplierOverDay.Evaluate(time01), 0.2f, 2.0f);
            Color mood = (colorFilterOverDay != null) ? colorFilterOverDay.Evaluate(time01) : Color.white;
            Vector4 gain = new Vector4(mood.r * m, mood.g * m, mood.b * m, 1f);
            liftGammaGain.gain.value = gain;
        }

        if (smh != null)
        {
            float t = IsNight() ? nightShadowTintStrength : 0f;

            Color dayShadows = new Color(1f, 1f, 1f, 1f);
            Color nightShadows = new Color(0.75f, 0.85f, 1.15f, 1f);
            smh.shadows.value = Color.Lerp(dayShadows, nightShadows, t);

            Color dayMid = new Color(1f, 1f, 1f, 1f);
            Color nightMid = new Color(0.92f, 0.97f, 1.08f, 1f);
            smh.midtones.value = Color.Lerp(dayMid, nightMid, t * 0.6f);
        }

        DynamicGI.UpdateEnvironment();
    }

    private void ApplyNPCPopulation()
    {
        if (spawners == null || spawners.Length == 0) return;

        bool spawnWindowActive = (time01 >= spawnWindowStart && time01 <= spawnWindowEnd);
        bool isNight = IsNight();

        foreach (AISpawner spawner in spawners)
        {
            if (spawner == null) continue;

            spawner.SetTargetPopulation(basePopulation);

            if (spawnWindowActive && !isNight)
            {
                // Daytime: normal spawning
                spawner.ExitNightMode();
            }
            else
            {
                // Nighttime or outside spawn window
                spawner.EnterNightMode();
            }
        }
    }

}
