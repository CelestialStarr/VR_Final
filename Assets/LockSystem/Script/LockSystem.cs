using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockSystem : MonoBehaviour
{
    // --- 新增部分开始 ---
    [Header("绑定门控制器")]
    [Tooltip("请把挂有 LockedDoor 脚本的门物体拖到这里")]
    public LockedDoor targetDoor;

    // 添加一个标记，防止重复调用解锁
    private bool hasUnlocked = false;
    // --- 新增部分结束 ---

    public enum PaddleID
    {
        Paddle1,
        Paddle2,
        Paddle3,
        Paddle4,
        Paddle5,
        Paddle6,
        Paddle7
    }

    public bool paddle1;
    public bool paddle2;
    public bool paddle3;
    public bool paddle4;
    public bool paddle5;
    public bool paddle6;
    public bool paddle7;

    public void SetPaddleActive(PaddleID id)
    {
        // 如果已经解锁了，就不需要再进行逻辑判断了 (可选优化)
        if (hasUnlocked) return;

        switch (id)
        {
            case PaddleID.Paddle1: paddle1 = true; break;
            case PaddleID.Paddle2: paddle2 = true; break;
            case PaddleID.Paddle3: paddle3 = true; break;
            case PaddleID.Paddle4: paddle4 = true; break;
            case PaddleID.Paddle5: paddle5 = true; break;
            case PaddleID.Paddle6: paddle6 = true; break;
            case PaddleID.Paddle7: paddle7 = true; break;
        }

        Debug.Log(id + " 已激活！");
        CheckUnlock();
    }

    void CheckUnlock()
    {
        // 如果已经解锁过，直接返回，防止重复触发
        if (hasUnlocked) return;

        // 检查是否所有 bool 都为 true
        if (paddle1 && paddle2 && paddle3 && paddle4 && paddle5 && paddle6 && paddle7)
        {
            Debug.Log("全部 Paddle 已激活，正在执行解锁...");

            // 标记为已解锁
            hasUnlocked = true;

            // --- 核心修改：调用 LockedDoor 的功能 ---
            if (targetDoor != null)
            {
                // 调用我们在上一个脚本里写的公共方法
                targetDoor.UnlockTheDoor();
            }
            else
            {
                Debug.LogError("请在 Inspector 面板中给 LockSystem 脚本赋值 Target Door！");
            }
        }
    }
}