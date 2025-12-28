using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    [Tooltip("Paddle Bool")]
    public LockSystem.PaddleID targetPaddleID;

    public string clipName = "Unlock";

    private bool hasTriggered = false;
    private Animation animComp;

    void Start()
    {
        animComp = GetComponent<Animation>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Chisel"))
        {
            // 1. 播放 Animation 组件中的动画
            if (animComp != null)
            {
                // 如果没有指定名字，默认播放第一个 Clip
                if (string.IsNullOrEmpty(clipName))
                    animComp.Play();
                else
                    animComp.Play(clipName);
            }

            // 2. 通知父物体
            if (transform.parent != null)
            {
                LockSystem lockScript = transform.parent.GetComponent<LockSystem>();
                if (lockScript != null)
                {
                    // 将自己选定的 ID 传给父物体
                    lockScript.SetPaddleActive(targetPaddleID);
                    hasTriggered = true;
                }
            }
        }
    }
}