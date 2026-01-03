using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class SmartLocomotionController : MonoBehaviour
{
    [Header("核心设置")]
    public Transform headCamera;
    public LayerMask wallLayers; 

    [Header("参数微调")]
    public float bodyRadius = 0.2f;
    public float stepHeight = 0.35f; 

    [Header("传送兼容 (重点)")]
    [Tooltip("如果一帧内水平移动超过这个距离，视为传送，不做物理阻挡。")]
    public float teleportThreshold = 0.5f;

    [Header("重力设置")]
    public float gravity = -20.0f;
    public float stickToGroundForce = -10.0f;

   
    private CharacterController _cc;
    private Vector3 _lastHeadPos;
    private Vector3 _velocity;
    private bool _isFirstFrame = true;

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
  
        _cc.stepOffset = 0.3f;
        _cc.minMoveDistance = 0f;
        _cc.skinWidth = 0.05f;
    }

    private void FixedUpdate()
    {
        if (headCamera == null) return;

        if (_isFirstFrame)
        {
            _lastHeadPos = headCamera.position;
            _isFirstFrame = false;
            return;
        }

  
        float targetHeight = Mathf.Clamp(headCamera.localPosition.y, 0.5f, 2.5f);
        _cc.height = targetHeight;

        Vector3 headLocalPos = transform.InverseTransformPoint(headCamera.position);
        Vector3 targetCenter = new Vector3(headLocalPos.x, _cc.height / 2f, headLocalPos.z);
        _cc.center = targetCenter;

        Vector3 currentHeadPos = headCamera.position;
        Vector3 moveVector = currentHeadPos - _lastHeadPos;

        Vector3 horizontalMove = moveVector;
        horizontalMove.y = 0;
        float moveDist = horizontalMove.magnitude;

   
        if (moveDist > teleportThreshold)
        {
   
            _velocity = Vector3.zero;
            _lastHeadPos = currentHeadPos; 

            return;
        }

        if (moveDist > 0.001f)
        {
            
            Vector3 feetPos = transform.position;
            Vector3 rayStart = feetPos + Vector3.up * stepHeight; 
            float checkDist = moveDist + 0.1f; 

         
            if (Physics.SphereCast(rayStart, bodyRadius, horizontalMove.normalized, out RaycastHit hit, checkDist, wallLayers))
            {
              
                Vector3 pushBack = -horizontalMove;
                _cc.Move(pushBack);
            }
            else
            {
                _lastHeadPos = currentHeadPos;
            }
        }
        else
        {
            _lastHeadPos = currentHeadPos;
        }

   
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (_cc.isGrounded)
        {
            if (_velocity.y < 0) _velocity.y = stickToGroundForce;
        }
        else
        {
            _velocity.y += gravity * Time.fixedDeltaTime;
        }
        _cc.Move(_velocity * Time.fixedDeltaTime);
    }
}