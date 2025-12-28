using UnityEngine;

// 这个脚本依赖于 Outline 组件，如果没有它会自动添加
[RequireComponent(typeof(Outline))]
public class OutlinePulse : MonoBehaviour
{
    private Outline outline;

    [Header("呼吸设置")]
    [Tooltip("呼吸速度")]
    public float pulseSpeed = 2.0f; // 速度越快，闪烁越快

    [Tooltip("最小宽度")]
    public float minWidth = 0.0f;   // 最细的时候（0就是完全看不见）

    [Tooltip("最大宽度")]
    public float maxWidth = 6.0f;   // 最粗的时候

    void Awake()
    {
        outline = GetComponent<Outline>();
    }

    void Update()
    {
        // 只有当 Outline 组件被启用时，才进行呼吸计算
        // 这样可以配合之前的“距离检测”脚本：距离远了enabled=false，这里就会自动停止消耗性能
        if (outline.enabled)
        {
            // --- 数学原理 ---
            // Mathf.Sin(Time.time * speed) 会产生一个 -1 到 1 之间的波浪值
            // 我们把它映射到 0 到 1 之间： (Sin + 1) / 2
            float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

            // 使用 Lerp 在最小宽度和最大宽度之间插值
            outline.OutlineWidth = Mathf.Lerp(minWidth, maxWidth, wave);
        }
    }
}