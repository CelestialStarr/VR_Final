using UnityEngine;

public class SlidingAutoDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform door;          // 门模型
    public Transform closedPoint;   // 关门位置（小球）
    public Transform openPoint;     // 开门位置（小球）
    public float speed = 2f;        // 移动速度

    private bool isOpen = false;

    void Update()
    {
        if (door == null || closedPoint == null || openPoint == null)
            return;

        if (isOpen)
        {
            door.position = Vector3.Lerp(
                door.position,
                openPoint.position,
                Time.deltaTime * speed
            );
        }
        else
        {
            door.position = Vector3.Lerp(
                door.position,
                closedPoint.position,
                Time.deltaTime * speed
            );
        }
    }

    private void OnTriggerEnter(Collider other)
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
    }
}
