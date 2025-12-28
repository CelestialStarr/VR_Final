using UnityEngine;
using TMPro;
using System.Collections;

public class TimeGameplayManager : MonoBehaviour
{
    [Header("References")]
    public DayNightCycleURP_Final dayCycleScript;

    [Header("UI - Clock")]
    [Tooltip("包含时间文字的Panel")]
    public GameObject clockPanel;
    private TextMeshProUGUI clockText;
    public bool updateEveryHalfHourOnly = true;

    // [新增] UI - Day Status (常驻天数显示)
    [Header("UI - Day Status (Persistent)")]
    [Tooltip("常驻显示天数的Panel (类似于时钟)")]
    public GameObject dayStatusPanel;
    private TextMeshProUGUI dayStatusText; // 自动获取

    [Header("UI - Popups")]
    public GameObject dayPopupPanel;
    private TextMeshProUGUI dayPopupText;
    public float dayPopupDuration = 3f;

    [Header("UI - Fatigue / Work")]
    public GameObject workFatiguePopup;
    // private TextMeshProUGUI workFatigueText; // 方案一：代码不控制文字内容

    [Header("Settings")]
    public float fatigueThresholdHours = 16f;
    public float sleepHours = 8f;

    // 内部变量
    private bool isClockVisible = false;

    private float currentAwakeHours = 0f;
    private bool hasTriggeredFatiguePopup = false;

    private int lastDisplayedHour = -1;
    private int lastDisplayedMinute = -1;

    void Start()
    {
        if (dayCycleScript == null)
            dayCycleScript = FindFirstObjectByType<DayNightCycleURP_Final>();

        dayCycleScript.OnDayChanged += HandleDayChanged;

        // --- 1. 自动查找组件 ---
        if (clockPanel != null) clockText = clockPanel.GetComponentInChildren<TextMeshProUGUI>();

        // [新增] 查找常驻天数文字
        if (dayStatusPanel != null) dayStatusText = dayStatusPanel.GetComponentInChildren<TextMeshProUGUI>();

        if (dayPopupPanel != null) dayPopupText = dayPopupPanel.GetComponentInChildren<TextMeshProUGUI>();

        // [注意] workFatigueText在方案一中不需要获取，因为我们不改它的字

        // --- 2. 初始化UI状态 (默认隐藏) ---


        if (dayPopupPanel != null) dayPopupPanel.SetActive(false);
        if (workFatiguePopup != null) workFatiguePopup.SetActive(false);

        // --- 3. 初始化数据显示 ---
        // 游戏刚开始，也要显示当前是第几天
        UpdateDayStatusText(dayCycleScript.dayCount);
    }

    void OnDestroy()
    {
        if (dayCycleScript != null) dayCycleScript.OnDayChanged -= HandleDayChanged;
    }

    void Update()
    {
        HandleInput();
        UpdateClockDisplay();
        UpdateFatigueLogic();
    }

    void HandleInput()
    {
        // N键开关 HUD (时钟 + 天数)
       

        // K键睡觉
        if (Input.GetKeyDown(KeyCode.K))
        {
            PerformSleep();
        }
    }

    void PerformSleep()
    {
        if (dayCycleScript == null) return;
        dayCycleScript.SkipTime(sleepHours);
        currentAwakeHours = 0f;
        hasTriggeredFatiguePopup = false;
        Debug.Log("玩家睡觉了，体力恢复，疲劳计时清零。");
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

    // --- 天数相关逻辑 ---

    void HandleDayChanged(int newDayCount)
    {
        // 1. 触发中间的大弹窗 (DAY 2)
        StartCoroutine(ShowDayPopupRoutine(newDayCount));

        // 2. [新增] 更新常驻的 UI 文字
        UpdateDayStatusText(newDayCount);
    }

    // [新增] 专门用来更新常驻天数文字的方法
    void UpdateDayStatusText(int day)
    {
        if (dayStatusText != null)
        {
            // 你可以在这里自定义格式，比如 "Day: 1" 或 "第 1 天"
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