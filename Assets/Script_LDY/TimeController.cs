using UnityEngine;
using TMPro;
using System.Collections;

public class TimeGameplayManager : MonoBehaviour
{
    [Header("References")]
    public DayNightCycleURP_Final dayCycleScript;

    [Header("UI - Clock")]
    [Tooltip("UI Panel containing the clock text")]
    public GameObject clockPanel;
    private TextMeshProUGUI clockText; // 自动获取
    public bool updateEveryHalfHourOnly = true;

    [Header("UI - Popups")]
    public GameObject dayPopupPanel;
    private TextMeshProUGUI dayPopupText; // 自动获取
    public float dayPopupDuration = 3f;

    [Header("UI - Fatigue / Work")]
    public GameObject workFatiguePopup; // 对应之前的 stealingWorkPopup
    private TextMeshProUGUI workFatigueText; // 自动获取

    [Header("Settings")]
    [Tooltip("连续游玩多少个'游戏小时'后提示？")]
    public float fatigueThresholdHours = 16f;
    [Tooltip("睡觉一次跳过多少小时？")]
    public float sleepHours = 8f;

    // 内部变量
    private bool isClockVisible = false; // 用于记录时钟当前是开还是关

    // [修改] 记录“连续清醒时间”（游戏小时）
    private float currentAwakeHours = 0f;
    private bool hasTriggeredFatiguePopup = false;

    private int lastDisplayedHour = -1;
    private int lastDisplayedMinute = -1;

    void Start()
    {
        if (dayCycleScript == null)
            dayCycleScript = FindFirstObjectByType<DayNightCycleURP_Final>();

        // 订阅天数变化事件（无论是自然过天，还是睡觉跳过天，都会触发这个）
        dayCycleScript.OnDayChanged += HandleDayChanged;

        // 自动查找UI组件
        if (clockPanel != null) clockText = clockPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (dayPopupPanel != null) dayPopupText = dayPopupPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (workFatiguePopup != null) workFatigueText = workFatiguePopup.GetComponentInChildren<TextMeshProUGUI>();

        // 初始化UI状态
        if (clockPanel != null) clockPanel.SetActive(false); // 默认隐藏
        if (dayPopupPanel != null) dayPopupPanel.SetActive(false);
        if (workFatiguePopup != null) workFatiguePopup.SetActive(false);
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

    // 1. 处理按键输入 (N 和 K)
    void HandleInput()
    {
        // [修改] N键开关时钟 (Toggle)
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (clockPanel != null)
            {
                isClockVisible = !isClockVisible; // 状态取反
                clockPanel.SetActive(isClockVisible);
            }
        }

        // [新增] K键睡觉
        if (Input.GetKeyDown(KeyCode.K))
        {
            PerformSleep();
        }
    }

    // 2. 睡觉逻辑
    void PerformSleep()
    {
        if (dayCycleScript == null) return;

        // 让时间系统跳过8小时
        dayCycleScript.SkipTime(sleepHours);

        // 重置疲劳值
        currentAwakeHours = 0f;
        hasTriggeredFatiguePopup = false;

        Debug.Log("玩家睡觉了，体力恢复，疲劳计时清零。");
    }

    // 3. 计算“连续游玩时间” (之前是偷窃时间)
    void UpdateFatigueLogic()
    {
        // 计算这一帧过去了多少“游戏小时”
        // Time.deltaTime 是现实秒
        // dayCycleScript.dayLengthSeconds 是游戏一天对应的现实秒
        float gameHoursPassed = (Time.deltaTime / dayCycleScript.dayLengthSeconds) * 24f;

        currentAwakeHours += gameHoursPassed;

        // 如果累计时间超过16小时，并且还没弹过窗
        if (currentAwakeHours >= fatigueThresholdHours && !hasTriggeredFatiguePopup)
        {
            TriggerFatiguePopup();
            hasTriggeredFatiguePopup = true; // 锁定，防止一直弹
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
            // if (workFatigueText != null)
            //     workFatigueText.text = "你已经工作12小时"; // 按你要求的文案

            workFatiguePopup.SetActive(true);
            yield return new WaitForSeconds(3f);
            workFatiguePopup.SetActive(false);
        }
    }

    // 4. 更新时钟UI (只在显示时更新)
    void UpdateClockDisplay()
    {
        // 如果面板关着，就不浪费性能去算字了
        if (clockPanel == null || !clockPanel.activeSelf) return;

        int hour, minute;
        dayCycleScript.GetCurrentTime(out hour, out minute);

        if (updateEveryHalfHourOnly) minute = (minute < 30) ? 0 : 30;

        // 只有数字变了才改Text，优化性能
        if (hour != lastDisplayedHour || minute != lastDisplayedMinute)
        {
            lastDisplayedHour = hour;
            lastDisplayedMinute = minute;
            if (clockText != null)
                clockText.text = $"{hour:00}:{minute:00}";
        }
    }

    // 5. 天数变化弹窗
    void HandleDayChanged(int newDayCount)
    {
        StartCoroutine(ShowDayPopupRoutine(newDayCount));
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