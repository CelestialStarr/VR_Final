using UnityEngine;
using UnityEngine.AI;

public class PoliceAI : MonoBehaviour
{
    public enum PoliceState { Idle, Chasing, Returning }

    [Header("袨怓潼諷")]
    public PoliceState currentState = PoliceState.Idle;

    [Header("扢离")]
    [SerializeField] private float catchDistance = 2f;
    [SerializeField] private float chaseDuration = 30f;
    [SerializeField] private CatchUIManager uiManager;

    [Header("弝橇諷秶 (瞄陑党蜊)")]
    // 參劑舷褒伎耀倰ㄗ湍MeshRenderer腔饒跺赽昜极ㄘ迍善涴爵
    // 彆衄嗣跺旯极窒璃ㄛ膘祜參坳蠅溫婓肮珨跺虜昜极狟ㄛ綴迍饒跺虜昜极
    [SerializeField] private GameObject modelRoot;

    [Header("Audio")]
    [SerializeField] private AudioSource chaseAudio;


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

        // ▽瞄陑党蜊▼ 蚔牁羲宎奀ㄛ珂笐旯
        SetVisibility(false);
    }

    void Update()
    {
        switch (currentState)
        {
            case PoliceState.Idle:
                // 渾儂奀ㄛ悵誘怹岆笐旯腔ㄗ滅砦砩俋ㄘ
                // 彆斕砑坻笐旯挐軀ㄛ褫眕婓涴爵迡挐軀軀憮ㄛ筍悵厥 SetVisibility(false)
                break;

            case PoliceState.Chasing:
                HandleChasing();
                break;

            case PoliceState.Returning:
                HandleReturning();
                break;
        }
    }

    // --- 鼎 PoliceManager 覃蚚 ---
    public void StartChasing(Transform player)
    {
        currentTarget = player;
        currentState = PoliceState.Chasing;
        chaseTimer = chaseDuration;

        agent.isStopped = false;

        // ▽瞄陑党蜊▼ 諉善韜鍔ㄛ蕾覦珆倛ㄐ
        SetVisibility(true);
        if (chaseAudio != null && !chaseAudio.isPlaying)
        {
            chaseAudio.Play();
        }
    }

    // --- 囀窒軀憮 ---

    void HandleChasing()
    {
        if (currentTarget == null) return;

        agent.SetDestination(currentTarget.position);
        chaseTimer -= Time.deltaTime;

        // 潰脤蚰眸
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= catchDistance)
        {
            PerformArrest();
        }

        // 潰脤閉奀
        if (chaseTimer <= 0)
        {
            GiveUpChase();
        }
    }

    void HandleReturning()
    {
        // 軗隙埻萸
        // 涴爵樓珨萸船ㄛ滅砦腹萸杅昫船絳祡珨眻婓饒順雄
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // 善模賸
            currentState = PoliceState.Idle;
            transform.rotation = originalRotation;

            // ▽瞄陑党蜊▼ 隙善詣弇ㄛ笭陔笐旯
            SetVisibility(false);
            StopChaseAudio();

        }
    }

    void GiveUpChase()
    {
        Debug.Log("劑舷ㄩ奀潔善ㄛ雪豖﹝");
        currentState = PoliceState.Returning;
        agent.SetDestination(originalPosition);
        // 蛁砩ㄩ隙最腔奀緊遜岆珆倛腔ㄛ眻善隙善埻萸符秏囮
        StopChaseAudio();
    }

    void PerformArrest()
    {
        currentState = PoliceState.Idle;
        agent.isStopped = true;
        agent.ResetPath();

        if (animator) animator.SetTrigger("Catch");

        // [新增] 调用全局抓捕计数器
        // 先检查 Instance 是否存在，防止在没有Manager的场景报错
        if (PoliceCatchManager.Instance != null)
        {
            PoliceCatchManager.Instance.RegisterCatch();
        }
        else
        {
            Debug.LogWarning("当前场景未找到 PoliceCatchManager，抓捕次数未记录！");
        }

        //原本的UI逻辑保留（显示"你被抓了"的临时弹窗）
        if (uiManager != null) uiManager.ShowCatchUI();

        // 停止追逐音乐
        StopChaseAudio();
    }

    // --- 落翑源楊ㄩ諷秶珆笐 ---
    private void SetVisibility(bool isVisible)
    {
        if (modelRoot != null)
        {
            modelRoot.SetActive(isVisible);
        }
        else
        {
            // 彆斕咭賸迍 modelRootㄛ郭彸赻雄扆梑垀衄腔馺懂羲壽
            // 涴岆珨笱掘蚚源偶
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = isVisible;
            }

            // 肮奀珩羲壽 Canvas (彆芛階衄悛沭眳濬腔)
            Canvas[] canvases = GetComponentsInChildren<Canvas>();
            foreach (var c in canvases)
            {
                c.enabled = isVisible;
            }
        }
    }
    private void StopChaseAudio()
    {
        if (chaseAudio != null && chaseAudio.isPlaying)
        {
            chaseAudio.Stop();
        }
    }

}