using UnityEngine;

public class PoliceCatchManager : MonoBehaviour
{
    public static PoliceCatchManager Instance;

    [Header("抓捕次数设置")]
    public int maxCatchCount = 3;
    public int currentCatchCount = 0;

    [Header("Ending Audio")]
    [SerializeField] private AudioSource prisonEndingAudio;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 每次警察成功抓到玩家时调用
    /// </summary>
    public void RegisterCatch()
    {
        currentCatchCount++;
        Debug.Log($"【PoliceCatch】第 {currentCatchCount} 次抓捕");

        if (currentCatchCount >= maxCatchCount)
        {
            TriggerPrisonEnding();
        }
    }

    void TriggerPrisonEnding()
    {
        Debug.Log("【Ending】触发 铁窗泪 / 监狱结局");

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

    // 如果你有重新开始游戏
    public void ResetCatchCount()
    {
        currentCatchCount = 0;
    }
}
