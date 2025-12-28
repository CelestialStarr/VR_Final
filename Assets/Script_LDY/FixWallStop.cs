using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class FinalWallStop : MonoBehaviour
{
    [Header("必须设置")]
    public CharacterController characterController;
    public Transform headCamera;
    public TeleportationProvider teleportProvider;

    [Header("防穿墙参数")]
    public LayerMask wallLayers;
    public float bodyRadius = 0.2f;
    public float stepHeight = 0.35f;

    [Header("重力参数")]
    public float gravity = -9.81f;
    public float stickToGroundForce = -2.0f;

    // 内部状态
    private bool _isTeleporting = false;
    private float _teleportCooldown = 0f;
    private Vector3 _verticalVelocity = Vector3.zero;

    // --- 新增：用来记录上一帧的位置，用于手动检测传送 ---
    private Vector3 _lastFramePos;
    private bool _firstFrame = true;

    private void OnEnable()
    {
        if (teleportProvider != null)
            teleportProvider.endLocomotion += OnTeleportEnded;
    }

    private void OnDisable()
    {
        if (teleportProvider != null)
            teleportProvider.endLocomotion -= OnTeleportEnded;
    }

    private void OnTeleportEnded(LocomotionSystem system)
    {
        _isTeleporting = true;
        _teleportCooldown = 0.2f;
        _verticalVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (_isTeleporting)
        {
            _teleportCooldown -= Time.deltaTime;
            if (_teleportCooldown <= 0) _isTeleporting = false;
        }
    }

    private void LateUpdate()
    {
        if (headCamera == null || characterController == null) return;

        // 初始化第一帧
        if (_firstFrame)
        {
            _lastFramePos = transform.position;
            _firstFrame = false;
            return;
        }

        // =================================================================
        // 1. 【核心修复】手动检测：这一帧是否发生了瞬移？
        // =================================================================
        // 这里的逻辑是：如果官方脚本在 Update 里把我们移到了远处，
        // 那么在 LateUpdate 里，当前位置和上一帧位置的距离会非常大。
        float moveDist = Vector3.Distance(transform.position, _lastFramePos);

        // 如果一帧内移动超过 0.5米，说明肯定是传送了！
        if (moveDist > 0.5f)
        {
            // 既然是传送，强制进入“无敌状态”
            _isTeleporting = true;
            _teleportCooldown = 0.2f; // 冷却一点时间
            _verticalVelocity = Vector3.zero; // 清空重力速度

            // 重要：更新上一帧位置，然后直接退出，不执行任何重力或推人！
            _lastFramePos = transform.position;
            return;
        }

        // 如果正在传送冷却期，只更新位置记录，不干活
        if (_isTeleporting)
        {
            _lastFramePos = transform.position;
            return;
        }

        // =================================================================
        // 2. 只有确认没有传送，才执行重力和防穿墙
        // =================================================================

        ApplyGravity();
        CheckAndPushBackFromWalls();

        // 这一帧结束，记录位置
        _lastFramePos = transform.position;
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            if (_verticalVelocity.y < 0)
            {
                _verticalVelocity.y = stickToGroundForce;
            }
        }
        else
        {
            _verticalVelocity.y += gravity * Time.deltaTime;
        }

        characterController.Move(_verticalVelocity * Time.deltaTime);
    }

    private void CheckAndPushBackFromWalls()
    {
        Vector3 feetPos = transform.position;
        Vector3 currentHeadPos = headCamera.position;
        Vector3 rayStart = feetPos + Vector3.up * stepHeight;

        Vector3 targetBodyPos = new Vector3(currentHeadPos.x, rayStart.y, currentHeadPos.z);
        Vector3 direction = targetBodyPos - rayStart;
        float dist = direction.magnitude;

        if (dist > 0.001f)
        {
            if (Physics.SphereCast(rayStart, bodyRadius, direction.normalized, out RaycastHit hit, dist + 0.05f, wallLayers))
            {
                Vector3 pushBack = -direction.normalized * (dist - hit.distance + 0.01f);
                pushBack.y = 0;
                characterController.Move(pushBack);
            }
        }
    }
}