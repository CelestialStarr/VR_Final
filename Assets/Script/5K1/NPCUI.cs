using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // 兜底：如果 main 还没找到
        if (cam == null)
            cam = FindObjectOfType<Camera>();
    }

    void LateUpdate()
    {
        if (cam == null) return;

        transform.LookAt(cam.transform);
        transform.Rotate(0, 180f, 0);
    }
}
