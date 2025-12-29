using UnityEngine;

public class RotatingAutoDoor : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float openAngle = 90f;          // 打开角度
    public float rotateSpeed = 2f;         // 开关速度
    public Vector3 rotationAxis = Vector3.up; // 绕哪个轴转（Y轴常用）

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;

    void Start()
    {
        // 记录关门角度
        closedRotation = transform.localRotation;

        // 计算开门角度
        openRotation = closedRotation * Quaternion.Euler(rotationAxis * openAngle);
    }

    void Update()
    {
        Quaternion target = isOpen ? openRotation : closedRotation;

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            target,
            Time.deltaTime * rotateSpeed
        );
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = false;
        }
    }*/
}
