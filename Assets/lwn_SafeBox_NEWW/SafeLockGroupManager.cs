using UnityEngine;
using System.Collections; // 引用协程必须

public class SafeLockGroupManager : MonoBehaviour
{
    [Header("Locks Reference")]
    [Tooltip("请将挂载了 SafeLock 脚本的三个物体拖入此处")]
    public SafeLock[] locks;   // 引用你最新的 SafeLock 脚本

    [Header("Door Reference")]
    public SafeDoorAnimation safeDoor; // 引用之前的开门脚本

    [Header("UI Settings")]
    [Tooltip("解锁成功后需要隐藏的父物体（锁盘）")]
    public GameObject lockPanel;

    [Tooltip("全部解锁后，延迟几秒再消失？(给玩家一点反应时间)")]
    public float hideDelay = 1.0f;

    private bool opened = false;

    void Update()
    {
        // 如果已经开锁了，就不再执行检查
        if (opened) return;

        // 遍历检查每一个锁的状态
        foreach (var lockScript in locks)
        {
            // 防空检查：如果数组里有空位，直接跳过或返回
            if (lockScript == null) return;

            // ⭐ 核心修改：调用你 SafeLock 脚本里的 IsUnlocked() 方法
            if (!lockScript.IsUnlocked())
            {
                // 只要有一个没开，就结束本次检查，等待下一帧
                return;
            }
        }

        // --- 代码运行到这里，说明上面循环没有被 return 打断，即所有锁都开了 ---

        HandleAllUnlocked();
    }

    void HandleAllUnlocked()
    {
        opened = true;
        Debug.Log("所有密码正确，保险箱开启！");

        // 1. 执行开门动画
        if (safeDoor != null)
        {
            safeDoor.StartOpening();
        }

        // 2. 开启协程：处理延时和隐藏逻辑
        StartCoroutine(HideLocksSequence());
    }

    IEnumerator HideLocksSequence()
    {
        // 等待设定的时间 (例如 1 秒)
        yield return new WaitForSeconds(hideDelay);

        // 3. 强制隐藏每一个锁物体 (解决 Cube 被抓取后脱离父物体不消失的问题)
        foreach (var lockScript in locks)
        {
            if (lockScript != null && lockScript.gameObject != null)
            {
                lockScript.gameObject.SetActive(false);
            }
        }

        // 4. 最后隐藏整个面板
        if (lockPanel != null)
        {
            lockPanel.SetActive(false);
        }
    }
}