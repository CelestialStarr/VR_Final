using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    [Tooltip("Paddle Bool")]
    public LockSystem.PaddleID targetPaddleID;

    [Header("音频设置")]
    public AudioSource audioSource;
    public AudioClip soundClip;

    public string clipName = "Unlock";

    private bool hasTriggered = false;
    private Animation animComp;

    void Start()
    {
        animComp = GetComponent<Animation>();
    }
    public void PlaySoundEvent()
    {
        if (audioSource != null && soundClip != null)
        {
            audioSource.PlayOneShot(soundClip);
            Debug.Log("动画触发了音效！");
        }
        else
        {
            Debug.LogWarning("缺少 AudioSource 或 AudioClip！");
        }
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