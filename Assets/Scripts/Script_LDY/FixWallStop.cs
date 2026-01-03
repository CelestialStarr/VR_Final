using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
public class PlayerBodyController : MonoBehaviour
{
    [Header("References")]
    public Transform headCamera;

    [Header("Settings")]
    public LayerMask wallLayers;
    public float gravity = -9.81f;
    public float bodyRadius = 0.2f;

    private CharacterController _cc;
    private Vector3 _verticalVelocity;
    private Vector3 _lastPos;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _lastPos = transform.position;
    }

    void LateUpdate()
    {
        if (headCamera == null) return;

        Vector3 headLocalPos = transform.InverseTransformPoint(headCamera.position);
        _cc.center = new Vector3(headLocalPos.x, _cc.center.y, headLocalPos.z);

        if (Vector3.Distance(transform.position, _lastPos) > 0.5f)
        {
            _verticalVelocity = Vector3.zero;
            _lastPos = transform.position;
            return;
        }

        ApplyGravity();
        CheckCollision();

        _lastPos = transform.position;
    }

    void ApplyGravity()
    {
        if (_cc.isGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = -2f;
        }
        else
        {
            _verticalVelocity.y += gravity * Time.deltaTime;
        }
        _cc.Move(_verticalVelocity * Time.deltaTime);
    }

    void CheckCollision()
    {
        Vector3 bottom = transform.TransformPoint(_cc.center + Vector3.down * (_cc.height / 2f - _cc.radius));
        Vector3 top = transform.TransformPoint(_cc.center + Vector3.up * (_cc.height / 2f - _cc.radius));

        bottom += Vector3.up * 0.2f;

        Collider[] hits = Physics.OverlapCapsule(bottom, top, bodyRadius, wallLayers);

        foreach (var hit in hits)
        {
            if (Physics.ComputePenetration(
                _cc, transform.position, transform.rotation,
                hit, hit.transform.position, hit.transform.rotation,
                out Vector3 dir, out float dist))
            {
                dir.y = 0;
                dir.Normalize();
                _cc.Move(dir * (dist + 0.01f));
            }
        }
    }
}