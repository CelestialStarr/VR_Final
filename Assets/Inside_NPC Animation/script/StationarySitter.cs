using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class StationarySitter : MonoBehaviour
{
    [Header("时间设置 (秒)")]
    public float minStandTime = 5.0f; // 最少站5秒
    public float maxStandTime = 15.0f; // 最多站15秒

    public float minSitTime = 10.0f;   // 最少坐10秒
    public float maxSitTime = 30.0f;   // 最多坐30秒

    [Header("初始状态")]
    public bool startSitting = false; // 如果勾选，游戏开始时他就是坐着的

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 开启无限循环
        StartCoroutine(LifeCycleRoutine());
    }

    IEnumerator LifeCycleRoutine()
    {
        // 如果想要一开始是坐着的
        if (startSitting)
        {
            animator.Play("Sitting Idle"); // 强制直接切换到坐着状态，跳过坐下动作
            animator.SetBool("IsSitting", true);
        }

        while (true) // 无限循环
        {
            // --- 阶段 1: 决定当前状态 ---

            // 获取当前是否坐着
            bool isCurrentlySitting = animator.GetBool("IsSitting");

            if (isCurrentlySitting)
            {
                // === 如果目前是坐着的，准备站起来 ===

                // 1. 等待一段“坐着的时间”
                float waitTime = Random.Range(minSitTime, maxSitTime);
                yield return new WaitForSeconds(waitTime);

                // 2. 站起来
                animator.SetBool("IsSitting", false);

                // 3. 等待“站起动画”播放完（假设2秒），防止逻辑过快
                yield return new WaitForSeconds(2.0f);
            }
            else
            {
                // === 如果目前是站着的，准备坐下 ===

                // 1. 等待一段“站立的时间”
                float waitTime = Random.Range(minStandTime, maxStandTime);
                yield return new WaitForSeconds(waitTime);

                // 2. 坐下
                animator.SetBool("IsSitting", true);

                // 3. 等待“坐下动画”播放完
                yield return new WaitForSeconds(2.0f);
            }
        }
    }
}