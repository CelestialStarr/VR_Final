using UnityEngine;
using UnityEngine.AI;

public class PoliceAI : MonoBehaviour
{
    // --- 这里加上了 [SerializeField] ---
    [Header("目标与参数 (请拖入)")]
    [SerializeField] private Transform playerTarget; // 玩家的位置
    [SerializeField] private float catchDistance = 1.5f; // 抓捕距离

    [Header("UI 管理器引用 (请拖入)")]
    [SerializeField] private CatchUIManager uiManager; // 拖入挂载了 CatchUIManager 的物体

    // 这些组件通常不需要在 Inspector 显示，所以保持 private 即可
    private NavMeshAgent agent;
    private Animator animator;
    private bool isCaught = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isCaught) return;

        if (playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= catchDistance)
            {
                PerformArrest();
            }
        }
    }

    void PerformArrest()
    {
        isCaught = true;

        // 停止移动
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // 播放动画
        if (animator != null) animator.SetTrigger("Catch");

        Debug.Log("玩家被抓住了！");

        // 调用 UI
        if (uiManager != null)
        {
            uiManager.ShowCatchUI();
        }
        else
        {
            Debug.LogError("PoliceAI: 忘记在 Inspector 里拖入 UI Manager 了！");
        }
    }
}