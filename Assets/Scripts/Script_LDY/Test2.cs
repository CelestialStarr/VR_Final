using UnityEngine;

public class SimpleWallCollision : MonoBehaviour
{
    [Header("Component Settings")]
    public Transform headCamera;
    public CharacterController characterController;

    [Header("Parameter Settings")]
    public float fixedHeight = 1.7f;

    [Header("Gravity Settings")]
    [Tooltip("Gravity acceleration. Recommended to set higher (e.g., -20) for crisper falling.")]
    public float gravity = -20.0f;

    [Tooltip("Grounding force when walking on slopes. Higher values stick more firmly to the ground.")]
    public float stickToGroundForce = -10.0f;

    private Vector3 _velocity;

    private void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        characterController.minMoveDistance = 0f;
    }

    private void FixedUpdate()
    {
        if (headCamera == null || characterController == null) return;

        Vector3 cameraLocalPos = transform.InverseTransformPoint(headCamera.position);

        Vector3 targetCenter = new Vector3(cameraLocalPos.x, characterController.height / 2, cameraLocalPos.z);

        characterController.center = targetCenter;
        characterController.height = fixedHeight;

        characterController.Move(new Vector3(0.0001f, -0.0001f, 0.0001f));
        characterController.Move(new Vector3(-0.0001f, 0.0001f, -0.0001f));

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            if (_velocity.y < 0)
            {
                _velocity.y = stickToGroundForce;
            }
        }
        else
        {
            _velocity.y += gravity * Time.fixedDeltaTime;
        }

        characterController.Move(_velocity * Time.fixedDeltaTime);
    }
}