using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class SmootherWaypointPatrol : MonoBehaviour
{
    [Header("路线设置")]
    public Transform waypointParent;

    [Header("移动参数")]
    public float moveSpeed = 3.0f;
    public float rotateSpeed = 5.0f;
    public float stopDistance = 0.2f; // 判定到达的距离

    [Header("休息机制 (关键修改)")]
    public float minWalkTime = 10.0f; // 最少走10秒
    public float maxWalkTime = 20.0f; // 最多走20秒
    public float restDuration = 5.0f; // 停下来休息多久

    [Header("攻击测试")]
    public bool testPunch = false;

    // 内部变量
    private Transform[] waypoints;
    private int currentIndex = 0;
    private bool isWaiting = false; // 是否正在休息
    private float nextRestTime;     // 下一次休息的目标时间点
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 1. 获取路径点
        if (waypointParent != null)
        {
            int count = waypointParent.childCount;
            waypoints = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                waypoints[i] = waypointParent.GetChild(i);
            }
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            this.enabled = false;
            return;
        }

        // 2. 初始化第一次休息时间
        ResetWalkTimer();
    }

    void Update()
    {
        // 攻击测试逻辑
        if (testPunch && !isWaiting)
        {
            StartCoroutine(PerformAttack());
            testPunch = false;
        }

        // 如果正在休息，就不要执行移动逻辑
        if (isWaiting) return;

        MoveAndRotate();
    }

    void MoveAndRotate()
    {
        if (waypoints.Length == 0) return;

        // --- 1. 获取目标 ---
        Transform target = waypoints[currentIndex];

        // 修正目标高度，防止 NPC 往天上飞
        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        // --- 2. 判断是否到达当前点 ---
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= stopDistance)
        {
            // === 核心修改逻辑 ===
            // 到达了一个点，我们检查一下：是不是该休息了？
            // 只有当“当前时间”超过了“预定的休息时间”，才停下来
            if (Time.time >= nextRestTime)
            {
                StartCoroutine(WaitRoutine());
            }
            else
            {
                // 还没走累，继续去下一个点，不停留！
                currentIndex = (currentIndex + 1) % waypoints.Length;
            }
            return;
        }

        // --- 3. 移动与旋转 ---
        // 移动
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // 旋转 (只有当有位移时才旋转，防止报错)
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            // 这里 Slerp 速度调高一点，防止密集点转弯转不过来
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        }

        // 只要没进入 WaitRoutine，就一直保持 Walking 动画
        animator.SetFloat("Speed", moveSpeed);
    }

    // 重置“下次休息时间”
    void ResetWalkTimer()
    {
        // 比如：当前时间 + 随机(10秒 到 20秒)
        nextRestTime = Time.time + Random.Range(minWalkTime, maxWalkTime);
    }

    IEnumerator WaitRoutine()
    {
        isWaiting = true;

        // 1. 停下脚步，播放 Idle
        animator.SetFloat("Speed", 0);

        // 2. 休息指定时间
        yield return new WaitForSeconds(restDuration);

        // 3. 休息完了，计算下一次什么时候休息
        ResetWalkTimer();

        // 4. 切换去下一个点
        currentIndex = (currentIndex + 1) % waypoints.Length;

        isWaiting = false;
    }

    IEnumerator PerformAttack()
    {
        isWaiting = true;
        animator.SetFloat("Speed", 0);
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(1.5f);
        isWaiting = false;
        // 攻击完如果不重置休息时间，可能刚走两步又停了，看你想不想重置
        // ResetWalkTimer(); 
    }
}