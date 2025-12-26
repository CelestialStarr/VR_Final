using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCActionManager : MonoBehaviour
{
    public Animator anim;
    public AlertController alertSystem;

    [Header("Settings")]
    public float dampTime = 0.15f; // 动画混合的缓冲时间，防止动作切换太生硬
    void Start()
    {
        anim = GetComponent<Animator>();
        alertSystem = GetComponent<AlertController>();
        if (alertSystem != null)
        {
            alertSystem.OnEnterAlert.AddListener(TriggerAlert);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleRunState();
    }

    // 处理 RunActive (0 = Walk, 1 = Run)
    void HandleRunState()
    {
        if (alertSystem == null) return;

        // 目标值：警戒了就是 1，没警戒就是 0
        float targetValue = alertSystem.isAlerted ? 1f : 0f;

        // 使用 SetFloat 的阻尼 (Damp) 功能，让数值平滑过渡，而不是瞬间跳变
        // 这样 NPC 会有一个从走到跑的加速过程，而不是瞬间鬼畜切换
        anim.SetFloat("RunActive", targetValue, dampTime, Time.deltaTime);
    }
    // 1. Alert (惊叹/发现敌人)
    // 这个已经绑定在 Start 里了，不需要手动调用，也可以公开给特殊剧情用
    public void TriggerAlert()
    {
        // 为了防止连续触发，可以先 Reset 一下
        anim.ResetTrigger("Alert");
        anim.SetTrigger("Alert");
        Debug.Log("动画：播放 Alert");
    }
    // 2. TakeALook (张望)
    // 这个需要被 Behavior 脚本调用（比如疑神疑鬼特质触发时）
    public void TriggerLookAround()
    {
        anim.SetTrigger("TakeALook");
        Debug.Log("动画：播放 TakeALook");
    }

    // 3. Attack (攻击)
    // 这个需要被你的战斗脚本调用
    public void TriggerAttack()
    {
        anim.SetTrigger("Attack");
        Debug.Log("动画：播放 Attack");
    }
}
