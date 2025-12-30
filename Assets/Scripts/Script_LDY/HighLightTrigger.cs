using UnityEngine;

public class SimpleHighlight : MonoBehaviour
{
    public float checkRange = 2.0f; // 你想要的触发距离，比如2米

    private Outline myOutline;
    private Transform playerHead;

    void Start()
    {
        // 1. 获取物品身上的Outline组件
        myOutline = GetComponent<Outline>();

        // 2. 自动找到你的VR摄像机（也就是你的头）
        // 只要你的场景里有标为MainCamera的相机，这一行就能自动找到
        if (Camera.main != null)
        {
            playerHead = Camera.main.transform;
        }

        // 3. 游戏刚开始先强制关掉高亮
        if (myOutline != null) myOutline.enabled = false;
    }

    void Update() // 每一帧都运行
    {
        if (playerHead == null || myOutline == null) return;

        // --- 核心逻辑就是这一句话 ---

        // 计算 物品 和 头 之间的距离
        float distance = Vector3.Distance(transform.position, playerHead.position);

        // 如果距离小于设定值，就设为true(显示)，否则设为false(隐藏)
        if (distance <= checkRange)
        {
            myOutline.enabled = true;
        }
        else
        {
            myOutline.enabled = false;
        }
    }
}