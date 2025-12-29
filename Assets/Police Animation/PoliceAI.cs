using UnityEngine;
using UnityEngine.AI;

public class PoliceAI : MonoBehaviour
{
    public enum PoliceState { Idle, Chasing, Returning }

    [Header("状态监控")]
    public PoliceState currentState = PoliceState.Idle;

    [Header("设置")]
    [SerializeField] private float catchDistance = 2f;
    [SerializeField] private float chaseDuration = 30f;
    [SerializeField] private CatchUIManager uiManager;

    [Header("视觉控制 (核心修改)")]
    // 请把警察角色模型（带MeshRenderer的那个子物体）拖到这里
    // 如果有多个身体部件，建议把它们放在同一个父物体下，然后拖那个父物体
    [SerializeField] private GameObject modelRoot;

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Transform currentTarget;
    private float chaseTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // 【核心修改】 游戏开始时，先隐身
        SetVisibility(false);
    }

    void Update()
    {
        switch (currentState)
        {
            case PoliceState.Idle:
                // 待机时，确保护卫是隐身的（防止意外）
                // 如果你想让他隐身巡逻，可以在这里写巡逻逻辑，但保持 SetVisibility(false)
                break;

            case PoliceState.Chasing:
                HandleChasing();
                break;

            case PoliceState.Returning:
                HandleReturning();
                break;
        }
    }

    // --- 供 PoliceManager 调用 ---
    public void StartChasing(Transform player)
    {
        currentTarget = player;
        currentState = PoliceState.Chasing;
        chaseTimer = chaseDuration;

        agent.isStopped = false;

        // 【核心修改】 接到命令，立刻显形！
        SetVisibility(true);
    }

    // --- 内部逻辑 ---

    void HandleChasing()
    {
        if (currentTarget == null) return;

        agent.SetDestination(currentTarget.position);
        chaseTimer -= Time.deltaTime;

        // 检查抓捕
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= catchDistance)
        {
            PerformArrest();
        }

        // 检查超时
        if (chaseTimer <= 0)
        {
            GiveUpChase();
        }
    }

    void HandleReturning()
    {
        // 走回原点
        // 这里加一点容差，防止浮点数误差导致一直在那抖动
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // 到家了
            currentState = PoliceState.Idle;
            transform.rotation = originalRotation;

            // 【核心修改】 回到岗位，重新隐身
            SetVisibility(false);
        }
    }

    void GiveUpChase()
    {
        Debug.Log("警察：时间到，撤退。");
        currentState = PoliceState.Returning;
        agent.SetDestination(originalPosition);
        // 注意：回程的时候还是显形的，直到回到原点才消失
    }

    void PerformArrest()
    {
        currentState = PoliceState.Idle;
        agent.isStopped = true;
        agent.ResetPath();

        if (animator) animator.SetTrigger("Catch");

        if (uiManager != null) uiManager.ShowCatchUI();

        // 抓到人后，你可以选择让他立刻消失，或者停留在原地摆Pose
        // 这里我选择暂时不消失，让玩家看到是谁抓了他
        // 如果想抓到瞬间消失，取消下面这行的注释：
        // SetVisibility(false); 
    }

    // --- 辅助方法：控制显隐 ---
    private void SetVisibility(bool isVisible)
    {
        if (modelRoot != null)
        {
            modelRoot.SetActive(isVisible);
        }
        else
        {
            // 如果你忘了拖 modelRoot，尝试自动寻找所有的渲染器来开关
            // 这是一种备用方案
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = isVisible;
            }

            // 同时也开关 Canvas (如果头顶有血条之类的)
            Canvas[] canvases = GetComponentsInChildren<Canvas>();
            foreach (var c in canvases)
            {
                c.enabled = isVisible;
            }
        }
    }
}