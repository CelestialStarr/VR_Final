using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InHouseCharAni : MonoBehaviour
{
    private Animator anim;
    public AlertController alertSystem;

    [Header("Interactions")]
    public GameObject phone;
    public GameObject player;
    public Waypoint currentWaypoint;

    [Header("Character Info")]
    public float movingSpeed = 1;
    public float turningSpeed = 300f;
    public float stopSpeed = 1f;

    [Header("Destination Var")]
    public Vector3 destination;
    public bool destinationReached;

    private float attackTimer = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (alertSystem == null)
            alertSystem = GetComponent<AlertController>();

        if (currentWaypoint != null)
            LocationDestination(currentWaypoint.GetPosition());

        anim.SetFloat("Speed", 1f);
        anim.SetBool("IsAlerted", false);
        anim.SetBool("PhoneActive", true);
    }

    void Update()
    {
        // 1. 基础状态检测
        bool isPhoneActive = (phone != null && phone.activeInHierarchy);
        anim.SetBool("PhoneActive", isPhoneActive);

        bool isAlerted = alertSystem.isAlerted;
        anim.SetBool("IsAlerted", isAlerted);

        float currentAlertVal = alertSystem.currentAlertValue;

        // --- 状态机逻辑分支 ---

        // 情况 A: 高度警戒状态 (isAlerted = True)
        // 此时 AlertValue 通常等于 max，或者处于冷却期
        if (isAlerted)
        {
            anim.SetFloat("Speed", 0f); // 动画停

            if (!isPhoneActive)
            {
                // 手机丢了 -> 追人逻辑
                if (player != null)
                {
                    LocationDestination(player.transform.position);
                    Walk(); // 物理移动去追
                }

                // 攻击逻辑
                attackTimer += Time.deltaTime;
                if (attackTimer >= 10f)
                {
                    anim.SetTrigger("Attack");
                    attackTimer = 0f;
                }
            }
            else
            {
                // 手机还在 -> 原地害怕/报警
                // 不调用 Walk()，NPC物理停止
                attackTimer = 0f;
            }
        }
        // 情况 B: 怀疑状态 (AlertValue > 0 且 isAlerted = False)
        // 对应需求：AlertValue小于最大值且大于0时
        else if (currentAlertVal > 0)
        {
            // 1. 将 Speed 设为 0 (播放 Idle/LookAround 动画)
            anim.SetFloat("Speed", 0f);

            // 2. NPC 停止移动 (不调用 Walk 函数)
            // 这里什么都不做，NPC就会停在当前位置
        }
        // 情况 C: 正常巡逻状态 (AlertValue <= 0)
        else
        {
            anim.SetFloat("Speed", 1f);

            // 正常巡逻移动
            Walk();

            // 到达路点切换
            if (destinationReached && currentWaypoint != null)
            {
                currentWaypoint = currentWaypoint.nextWaypoint;
                if (currentWaypoint != null)
                    LocationDestination(currentWaypoint.GetPosition());
            }
        }
    }

    public void Walk()
    {
        Vector3 destinationDirection = destination - transform.position;
        destinationDirection.y = 0f;
        float destinationDistance = destinationDirection.magnitude;

        if (destinationDistance >= stopSpeed)
        {
            destinationReached = false;
            if (destinationDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(destinationDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turningSpeed * Time.deltaTime);
            }
            transform.Translate(Vector3.forward * movingSpeed * Time.deltaTime);
        }
        else
        {
            destinationReached = true;
        }
    }

    public void LocationDestination(Vector3 newDestination)
    {
        this.destination = newDestination;
        destinationReached = false;
    }
}