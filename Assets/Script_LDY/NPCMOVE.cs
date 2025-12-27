using UnityEngine;

public class SimpleForward : MonoBehaviour
{
    public float speed = 3.0f; // 移动速度

    void Update()
    {
        // 核心代码就这一句：
        // 让物体朝着“它自己的前方”移动
        // Time.deltaTime 确保每秒移动的距离是固定的，不会因为掉帧而卡顿
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}