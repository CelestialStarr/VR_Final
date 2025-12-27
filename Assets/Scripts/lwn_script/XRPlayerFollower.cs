using UnityEngine;

public class XRPlayerFollower : MonoBehaviour
{
    public Transform cameraTransform;

    void Update()
    {
        Vector3 pos = cameraTransform.position;
        pos.y = transform.position.y; // 不跟随头部上下
        transform.position = pos;
    }
}
