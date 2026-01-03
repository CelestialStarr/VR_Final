using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class TimeGameplayManager : MonoBehaviour
{
    [Header("References")]
    public DayNightCycleURP_Final dayCycleScript;

    [Header("UI - Clock")]
    public GameObject clockPanel;
    private TextMeshProUGUI clockText;
    public bool updateEveryHalfHourOnly = true;

    [Header("UI - Day Status")]
    public GameObject dayStatusPanel;
    private TextMeshProUGUI dayStatusText;

    [Header("UI - Popups")]
    public GameObject dayPopupPanel;
    private TextMeshProUGUI dayPopupText;
    public float dayPopupDuration = 3f;

    [Header("UI - Fatigue / Work")]
    public GameObject workFatiguePopup;

    [Header("Settings")]
    public float fatigueThresholdHours = 16f;
    public float sleepHours = 8f;

    [Tooltip("Hours of continuous wakefulness leading to death")]
    public float deathThresholdHours = 72f;

    public event Action onFatigueDeath;

    private float currentAwakeHours = 0f;
    private bool hasTriggeredFatiguePopup = false;
    private int lastDisplayedHour = -1;
    private int lastDisplayedMinute = -1;
    private bool isDead = false;

    void Start()
    {
        if (dayCycleScript == null)
            dayCycleScript = FindFirstObjectByType<DayNightCycleURP_Final>();

        dayCycleScript.OnDayChanged += HandleDayChanged;

        if (clockPanel != null) clockText = clockPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (dayStatusPanel != null) dayStatusText = dayStatusPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (dayPopupPanel != null) dayPopupText = dayPopupPanel.GetComponentInChildren<TextMeshProUGUI>();

        if (dayPopupPanel != null) dayPopupPanel.SetActive(false);
        if (workFatiguePopup != null) workFatiguePopup.SetActive(false);

        UpdateDayStatusText(dayCycleScript.dayCount);
    }

    void OnDestroy()
    {
        if (dayCycleScript != null) dayCycleScript.OnDayChanged -= HandleDayChanged;
    }

    void Update()
    {
        if (isDead) return;

        HandleInput();
        UpdateClockDisplay();
        UpdateFatigueLogic();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            PerformSleep();
        }
    }

    public void PerformSleep()
    {
        if (dayCycleScript == null) return;
        dayCycleScript.SkipTime(sleepHours);
        currentAwakeHours = 0f;
        hasTriggeredFatiguePopup = false;
        Debug.Log("Player slept. Fatigue reset.");
    }

    void UpdateFatigueLogic()
    {
        float gameHoursPassed = (Time.deltaTime / dayCycleScript.dayLengthSeconds) * 24f;
        currentAwakeHours += gameHoursPassed;

        if (currentAwakeHours >= fatigueThresholdHours && !hasTriggeredFatiguePopup)
        {
            TriggerFatiguePopup();
            hasTriggeredFatiguePopup = true;
        }

        if (currentAwakeHours >= deathThresholdHours)
        {
            isDead = true;
            Debug.Log("<color=red>Player reached threshold without sleep. Sudden death triggered!</color>");
            onFatigueDeath?.Invoke();
        }
    }

    void TriggerFatiguePopup()
    {
        StartCoroutine(ShowFatiguePopupRoutine());
    }

    IEnumerator ShowFatiguePopupRoutine()
    {
        if (workFatiguePopup != null)
        {
            workFatiguePopup.SetActive(true);
            yield return new WaitForSeconds(3f);
            workFatiguePopup.SetActive(false);
        }
    }

    void UpdateClockDisplay()
    {
        if (clockPanel == null || !clockPanel.activeSelf) return;

        int hour, minute;
        dayCycleScript.GetCurrentTime(out hour, out minute);

        if (updateEveryHalfHourOnly) minute = (minute < 30) ? 0 : 30;

        if (hour != lastDisplayedHour || minute != lastDisplayedMinute)
        {
            lastDisplayedHour = hour;
            lastDisplayedMinute = minute;
            if (clockText != null)
                clockText.text = $"{hour:00}:{minute:00}";
        }
    }

    void HandleDayChanged(int newDayCount)
    {
        StartCoroutine(ShowDayPopupRoutine(newDayCount));
        UpdateDayStatusText(newDayCount);
    }

    void UpdateDayStatusText(int day)
    {
        if (dayStatusText != null)
        {
            dayStatusText.text = $"Day {day}";
        }
    }

    IEnumerator ShowDayPopupRoutine(int day)
    {
        if (dayPopupPanel != null)
        {
            if (dayPopupText != null) dayPopupText.text = $"DAY {day}";
            dayPopupPanel.SetActive(true);
            yield return new WaitForSeconds(dayPopupDuration);
            dayPopupPanel.SetActive(false);
        }
    }
}