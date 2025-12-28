using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockSystem : MonoBehaviour
{
    // 1. 定义一个枚举，列出所有的 Paddle 编号
    // 这会在 Inspector 里生成一个下拉菜单
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

    // 2. 将 bool 改为 public，这样你在 Inspector 里能看到它们是否变成了 true
    public bool paddle1;
    public bool paddle2;
    public bool paddle3;
    public bool paddle4;
    public bool paddle5;
    public bool paddle6;
    public bool paddle7;

    // 3. 提供一个公共方法，供子物体调用
    public void SetPaddleActive(PaddleID id)
    {
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
        CheckUnlock(); // 这里可以顺便检查一下是否全部解锁
    }

    void CheckUnlock()
    {
        // 检查是否所有 bool 都为 true
        if (paddle1 && paddle2 && paddle3 && paddle4 && paddle5 && paddle6 && paddle7)
        {
            Debug.Log("全部解锁！");
            // 这里写解锁后的逻辑
        }
    }
}