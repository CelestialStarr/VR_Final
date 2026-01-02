using UnityEngine;

public class PoliceCatchManager : MonoBehaviour
{
    public static PoliceCatchManager Instance;

    // [核心修改 1] 静态变量：这是“全局保险箱”，切换场景数据不丢
    // 即使这个脚本所在的物体被销毁，这个变量依然存在内存里
    private static int _globalCatchCount = 0;

    [Header("抓捕次数设置")]
    public int maxCatchCount = 3;
    public int currentCatchCount = 0;

    [Header("Ending Audio")]
    [SerializeField] private AudioSource prisonEndingAudio;

    private void Awake()
    {
        // 标准单例写法 (这一步是为了方便其他脚本调用 Instance)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // [注意] 我去掉了 DontDestroyOnLoad(gameObject);
        // 因为我们现在的策略是：允许物体销毁，数据存静态变量。
        // 这样可以避免 AudioSource 引用丢失的问题。
    }

    private void Start()
    {
        // [核心修改 2] 场景开始时，从“保险箱”里读取数据
        currentCatchCount = _globalCatchCount;

        Debug.Log($"场景加载完成，当前累计被抓次数: {currentCatchCount}");
    }

    /// <summary>
    /// 每次被抓时调用
    /// </summary>
    public void RegisterCatch()
    {
        currentCatchCount++;

        // [核心修改 3] 数据变化后，立刻同步到“保险箱”
        _globalCatchCount = currentCatchCount;

        Debug.Log($"【PoliceCatch】第 {currentCatchCount} 次被抓捕");

        if (currentCatchCount >= maxCatchCount)
        {
            TriggerPrisonEnding();
        }
    }

    void TriggerPrisonEnding()
    {
        Debug.Log("【Ending】触发 入狱结局 / 游戏结束");

        if (prisonEndingAudio != null)
        {
            prisonEndingAudio.Play();
        }

        if (EndingUIManager.Instance != null)
        {
            EndingUIManager.Instance.OpenPrisonEnding();
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("EndingUIManager.Instance 不存在！");
        }
    }

    // [核心修改 4] 重置游戏数据 (给“开始新游戏”按钮用)
    public void ResetCatchCount()
    {
        currentCatchCount = 0;
        _globalCatchCount = 0; // 记得把保险箱也清空
        Debug.Log("抓捕次数已重置");
    }
}