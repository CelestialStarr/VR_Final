using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class SleeperNPC : MonoBehaviour
{
    [Header("模式设置")]
    [Tooltip("勾选这个，他就会一直睡，永远不起来")]
    public bool eternalSleep = true;

    [Header("打瞌睡设置 (仅当上面不勾选时有效)")]
    public float minSleepTime = 10f; // 睡多久
    public float maxSleepTime = 30f;
    public float wakeDuration = 5f;  // 醒来发呆多久

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (eternalSleep)
        {
            // 模式 1: 永久睡觉
            // 直接设置参数，让他一进入游戏就开始去睡觉
            animator.SetBool("IsSleeping", true);

            // 如果你没有“躺下”的过渡动画，想让他瞬间就在睡觉状态：
            // animator.Play("Sleep Idle"); 
        }
        else
        {
            // 模式 2: 打瞌睡循环
            StartCoroutine(NapRoutine());
        }
    }

    IEnumerator NapRoutine()
    {
        // 刚开始先去睡觉
        animator.SetBool("IsSleeping", true);

        while (true)
        {
            // 1. 睡觉阶段
            // 随机睡一段时间
            float sleepTime = Random.Range(minSleepTime, maxSleepTime);
            yield return new WaitForSeconds(sleepTime);

            // 2. 惊醒阶段
            // 只有当不是永久睡觉模式时，才醒来
            animator.SetBool("IsSleeping", false);

            // 等待“起床”动画播完 + 发呆时间
            // 假设起床动作要2秒，发呆 wakeDuration 秒
            yield return new WaitForSeconds(2.0f + wakeDuration);

            // 3. 继续去睡
            animator.SetBool("IsSleeping", true);

            // 等待“躺下”动画播完
            yield return new WaitForSeconds(2.0f);
        }
    }
}