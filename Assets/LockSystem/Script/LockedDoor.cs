using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("状态设置")]
    [SerializeField] private bool isUnlocked = false;
    private bool hasTriggered = false; // 这个变量只用于防止动画重复播放，不再限制距离检测

    [Header("绑定对象 - 核心")]
    [Tooltip("当门解锁时，需要设为 True 的那个外置 GameObject")]
    public GameObject externalObjectToActivate;

    [Tooltip("当门解锁时，需要显示的那个按钮")]
    public GameObject buttonToMakeActive;

    [Tooltip("需要隐藏的 Chisel 物体")]
    public GameObject chisel;

    [Header("绑定对象 - 互斥/重置")]
    [Tooltip("当门解锁时需要隐藏的按钮；当玩家离开范围后会再次显示")]
    public GameObject buttonToHideOnUnlock;

    [Tooltip("玩家的位置")]
    public Transform playerTransform;

    [Tooltip("离开这个距离后，重置显示状态")]
    public float resetDistance = 3.0f;

    [Header("动画设置")]
    public Animation doorAnimation;
    public string animationClipName = "DoorOpen";

    void Start()
    {
        if (playerTransform == null)
        {
            if (Camera.main != null) playerTransform = Camera.main.transform;
        }

        if (buttonToMakeActive != null) buttonToMakeActive.SetActive(false);
        if (buttonToHideOnUnlock != null) buttonToHideOnUnlock.SetActive(true);

        isUnlocked = false;
        hasTriggered = false;
    }

    void Update()
    {
        // 1. 解锁逻辑 (保持不变，确保动画只播一次)
        if (isUnlocked && !hasTriggered)
        {
            UnlockTheDoor();
            hasTriggered = true;
        }

        // 2. 离开范围重置逻辑 (已修改！)
        // 现在的逻辑：只要【外置物体是显示的】，就开始检测距离。
        // 不再关心它是由 LockedDoor 脚本触发显示的，还是被其他东西显示的。
        if (!hasTriggered && externalObjectToActivate != null && externalObjectToActivate.activeSelf)
        {
            CheckDistanceAndReset();
        }
    }

    void CheckDistanceAndReset()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > resetDistance)
        {
            Debug.Log("玩家离开范围，重置显示状态");
            externalObjectToActivate.SetActive(false); // 隐藏外置物体

            // 重新显示那个"互斥"按钮
            if (buttonToHideOnUnlock != null)
            {
                buttonToHideOnUnlock.SetActive(true);
            }
        }
    }

    // --- 功能区 ---

    public void UnlockTheDoor()
    {
        isUnlocked = true;
        Debug.Log("门已解锁状态被激活！");

        if (chisel != null)
        {
            chisel.SetActive(false); // <--- 设置为 False
        }

        if (buttonToMakeActive != null)
        {
            buttonToMakeActive.SetActive(true);
        }

        if (externalObjectToActivate != null)
        {
            externalObjectToActivate.SetActive(false);
        }

        // 播放动画
        if (doorAnimation != null)
        {
            if (!string.IsNullOrEmpty(animationClipName))
            {
                doorAnimation.Play(animationClipName);
            }
            else
            {
                doorAnimation.Play();
            }
        }

        
    }

    public void PerformUnlockActions()
    {
        // 显示外置物体
        if (externalObjectToActivate != null)
        {
            externalObjectToActivate.SetActive(true);
        }

        // 隐藏互斥按钮
        if (buttonToHideOnUnlock != null)
        {
            buttonToHideOnUnlock.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, resetDistance);
    }
}