using UnityEngine;

// 确保物体上有 AudioSource 组件，如果没有会自动添加
[RequireComponent(typeof(AudioSource))]
public class EndGameManager : MonoBehaviour
{
    [Header("结局条件设置")]
    [Tooltip("达成富有结局需要的金币")]
    public int targetGold = 1000;

    [Tooltip("达成普通结局的天数 (例如填6，代表第5天结束进入第6天时触发)")]
    public int targetDayForNormalEnding = 6;

    [Header("结局音效设置")] // --- 新增：音效变量 ---
    [Tooltip("富有结局音效")]
    public AudioClip richEndingClip;
    [Tooltip("普通结局音效")]
    public AudioClip normalEndingClip;
    [Tooltip("死亡结局音效")]
    public AudioClip dieEndingClip;

    private bool isGameEnded = false;

    // 缓存引用
    private TimeGameplayManager timeManager;
    private DayNightCycleURP_Final dayCycle;
    private AudioSource audioSource; // --- 新增：音频源引用 ---

    private void Start()
    {
        // --- 1. 获取引用 ---
        timeManager = FindFirstObjectByType<TimeGameplayManager>();
        dayCycle = FindFirstObjectByType<DayNightCycleURP_Final>();

        // 获取或添加 AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // 确保开始时不自动播放

        // --- 2. 绑定事件 ---

        // A. 监听金币 (Rich Ending)
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onGoldChanged += CheckRichEnding;
        }

        // B. 监听天数 (Normal Ending)
        if (dayCycle != null)
        {
            dayCycle.OnDayChanged += CheckNormalEnding;
        }

        // C. 监听猝死 (Die Ending)
        if (timeManager != null)
        {
            timeManager.onFatigueDeath += TriggerDieEnding;
        }
    }

    private void OnDestroy()
    {
        // 记得取消订阅，防止报错
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.onGoldChanged -= CheckRichEnding;

        if (dayCycle != null)
            dayCycle.OnDayChanged -= CheckNormalEnding;

        if (timeManager != null)
            timeManager.onFatigueDeath -= TriggerDieEnding;
    }

    // --- 逻辑检测部分 ---

    // 1. 富有结局检测
    private void CheckRichEnding(int currentGold)
    {
        if (isGameEnded) return;
        if (currentGold >= targetGold)
        {
            EndGame(EndingType.Rich);
        }
    }

    // 2. 普通结局检测 (日子到了)
    private void CheckNormalEnding(int newDay)
    {
        if (isGameEnded) return;

        // 如果设置是6，当newDay变成6时（即第5天过完），触发结局
        if (newDay >= targetDayForNormalEnding)
        {
            EndGame(EndingType.Normal);
        }
    }

    // 3. 死亡结局 (由事件直接触发，不需要检测数值)
    private void TriggerDieEnding()
    {
        if (isGameEnded) return;
        EndGame(EndingType.Die);
    }

    // --- 统一的结束处理 ---

    private enum EndingType { Rich, Normal, Die }

    private void EndGame(EndingType type)
    {
        isGameEnded = true;

        // --- 新增：播放音效逻辑 ---
        PlayEndingSound(type);

        // 暂停游戏
        // 注意：如果在 TimeScale=0 时音效播不出来，请参考代码下方的【注意事项】
        Time.timeScale = 0f;

        if (EndingUIManager.Instance == null)
        {
            Debug.LogError("未找到 EndingUIManager！无法显示结局 UI");
            return;
        }

        switch (type)
        {
            case EndingType.Rich:
                Debug.Log("触发结局: 富豪");
                EndingUIManager.Instance.OpenRichEnding();
                break;
            case EndingType.Normal:
                Debug.Log("触发结局: 普通通关");
                EndingUIManager.Instance.OpenNormalEnding();
                break;
            case EndingType.Die:
                Debug.Log("触发结局: 过劳死");
                EndingUIManager.Instance.OpenDieEnding();
                break;
        }
    }

    // --- 新增：专门处理音效播放的函数 ---
    private void PlayEndingSound(EndingType type)
    {
        AudioClip clipToPlay = null;

        switch (type)
        {
            case EndingType.Rich:
                clipToPlay = richEndingClip;
                break;
            case EndingType.Normal:
                clipToPlay = normalEndingClip;
                break;
            case EndingType.Die:
                clipToPlay = dieEndingClip;
                break;
        }

        if (clipToPlay != null && audioSource != null)
        {
            // 使用 PlayOneShot 可以在同一个 AudioSource 播放多次声音而不切断（适合音效）
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}