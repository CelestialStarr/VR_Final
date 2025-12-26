using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NPCInfo;

public class CharacterNavigating : MonoBehaviour
{
    public NPCActionManager npcAnimator;
    public NPCInfo npcInfo;

    [Header("Character Info")]
    public float movingSpeed = 1;
    public float turningSpeed = 300f;
    public float stopSpeed = 1f;

    [Header("Destination Var")]
    public Vector3 destination;
    public bool destinationReached;

    [Header("Look Around Settings")] // 张望设置
    public float minLookInterval = 5f; // 最短间隔
    public float maxLookInterval = 60f; // 最长间隔
    [SerializeField] private float lookCooldownTimer; // 当前倒计时（可视化用）

    // 张望动画持续的时间（你可以根据实际动画长度调整）
    public float lookDuration = 8f;

    // 是否处于暂停状态
    private bool isPaused = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        npcAnimator = GetComponent<NPCActionManager>();
        npcInfo = GetComponent<NPCInfo>();
        ResetLookTimer();
    }

    // Update is called once per frame
    void Update()
    {
        // 只有在“未暂停”且“未到达目的地”时才处理移动和计时
        if (!isPaused)
        {
            // 只有在移动时才倒计时，防止刚停下又触发
            HandleLookAroundTimer();

            // 只有未暂停才走路
            Walk();
        }
    }

    public void Walk()
    {
        if (transform.position != destination)
        {
            Vector3 destinationDirection = destination - transform.position;
            destinationDirection.y = 0f;
            float destinationDistance = destinationDirection.magnitude;

            if (destinationDistance >= stopSpeed)
            {
                //Turning
                destinationReached = false;
                Quaternion targetRotation = Quaternion.LookRotation(destinationDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turningSpeed * Time.deltaTime);

                //Moving AI
                transform.Translate(Vector3.forward * movingSpeed * Time.deltaTime);
            }
            else
            {
                destinationReached = true;
            }
        }
    }

    public void LocationDestination(Vector3 destination)//changing destination
    {
        this.destination = destination;
        destinationReached = false;
    }

    // 处理张望倒计时的核心逻辑
    void HandleLookAroundTimer()
    {
        // 倒计时递减
        if (lookCooldownTimer > 0)
        {
            lookCooldownTimer -= Time.deltaTime;
        }
        else
        {
            StartCoroutine(LookAroundRoutine());
        }
    }
    IEnumerator LookAroundRoutine()
    {
        // 1. 暂停移动
        isPaused = true;

        // 2. 触发动画
        if (npcAnimator != null)
        {
            npcAnimator.TriggerLookAround();
        }

        // 3. 等待动画播放完毕 (这里等待 lookDuration 秒)
        yield return new WaitForSeconds(lookDuration);

        // 4. 重置倒计时 (让他在恢复行走后过一段时间再看)
        ResetLookTimer();

        // 5. 恢复移动
        isPaused = false;
    }
    // 重置计时器为随机值
    void ResetLookTimer()
    {
        float min = minLookInterval;
        float max = maxLookInterval;

        // 如果有疑神疑鬼特质，间隔时间减半（更频繁）
        if (npcInfo != null && npcInfo.HasTrait(NPCTrait.Paranoid))
        {
            min *= 0.5f;
            max *= 0.5f;
        }

        lookCooldownTimer = Random.Range(min, max);
    }
}