using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static NPCInfo;

public class AlertController : MonoBehaviour
{
    [Header("References")]
    public NPCInfo npcInfo; // 必须引用 NPCInfo

    [Header("Settings")]
    public float maxAlertValue = 100f;
    [Tooltip("正常状态下的每秒自然衰减值")]
    public float normalDecaySpeed = 10f;
    [Tooltip("警戒状态解除所需的冷却时间（秒）")]
    public float alertCooldownDuration = 30f;

    [Header("Status (Read Only)")]
    public float currentAlertValue = 0f;
    public bool isAlerted = false; // 是否处于警戒/战斗状态
    [SerializeField] private float currentCooldownTimer = 0f; // 当前剩余的冷却时间
    // 我们还需要一个倍率来控制“偷盗判定/警戒增加速度”
    public float sensitivityMultiplier = 1.0f;

    // 定义两个事件，方便其他脚本（如动画、AI导航）监听
    public UnityEvent OnEnterAlert; // 进入警戒状态时触发
    public UnityEvent OnExitAlert;  // 解除警戒状态时触发

    private void Start()
    {
        npcInfo = GetComponent<NPCInfo>();
        ApplyTraitEffects();
    }
    void Update()
    {
        HandleAlertDecay();
    }
    void ApplyTraitEffects()
    {
        if (npcInfo == null) return;

        // === 实现 [警惕] 特质 ===
        if (npcInfo.HasTrait(NPCTrait.Vigilant))
        {
            // 警惕：警戒条上限变短（更容易满），或者敏感度倍率变高

            sensitivityMultiplier = 1.5f;
            Debug.Log("Trait生效: 警惕 (敏感度 x1.5)");
        }

        // === 实现 [和善] 特质 ===
        if (npcInfo.HasTrait(NPCTrait.Kind))
        {
            // 和善：敏感度倍率变低，更难被发现
            sensitivityMultiplier = 0.5f;
            Debug.Log("Trait生效: 和善 (敏感度 x0.5)");
        }
    }

    /// <summary>
    /// 供外部调用的函数：增加警戒值
    /// </summary>
    /// <param name="amount">增加的具体数值（通常是 速度 * Time.deltaTime）</param>

    // 修改之前的 IncreaseAlert 函数，加入倍率计算
    public void IncreaseAlert(float amount)
    {
        // 这里的 amount 是外部传入的基础增加量
        // 最终增加量 = 基础量 * 特质倍率
        float finalAmount = amount * sensitivityMultiplier;

        currentAlertValue += finalAmount;

        // ... (其余逻辑保持不变)
    }

    /// <summary>
    /// 内部逻辑：处理自然降低
    /// </summary>
    private void HandleAlertDecay()
    {
        // 只有在没有持续增加警戒值（即没有在IncreaseAlert被调用）的帧，这里才会起作用
        // 但由于我们在IncreaseAlert里重置了timer，所以逻辑如下：

        if (isAlerted)
        {
            // === 警戒状态下的逻辑 ===
            if (currentCooldownTimer > 0)
            {
                // 30秒倒计时中，警戒值不下降
                currentCooldownTimer -= Time.deltaTime;
            }
            else
            {
                // 倒计时结束，开始下降
                ApplyDecay();

                // 如果降到0，解除警戒
                if (currentAlertValue <= 0)
                {
                    ExitAlertState();
                }
            }
        }
        else
        {
            // === 正常状态下的逻辑 ===
            // 只要没满，就一直尝试下降（除非外部正在调用IncreaseAlert抵消了这里）
            if (currentAlertValue > 0)
            {
                ApplyDecay();
            }
        }
    }

    private void ApplyDecay()
    {
        currentAlertValue -= normalDecaySpeed * Time.deltaTime;
        if (currentAlertValue < 0) currentAlertValue = 0;
    }

    private void EnterAlertState()
    {
        isAlerted = true;
        Debug.Log("进入警戒状态！保持警惕30秒！");
        OnEnterAlert?.Invoke();
    }

    private void ExitAlertState()
    {
        isAlerted = false;
        currentAlertValue = 0;
        Debug.Log("解除警戒，回归正常。");
        OnExitAlert?.Invoke();
    }

}