using UnityEngine;

public class PoliceTestTrigger : MonoBehaviour
{
    [Header("测试说明")]
    [TextArea]
    public string instruction = "运行游戏后，按下键盘上的 'T' 键模拟报警。\n按下 'R' 键强行重置所有警察（可选）。";

    void Update()
    {
        // 按 T 键：模拟报警
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (PoliceManager.Instance != null)
            {
                Debug.Log("【测试】按下 T 键：模拟 NPC 发现玩家，呼叫警察！");

                // 我们假装案发现场就在这个脚本挂载物体的位置
                // 这样你可以把这个物体到处移动，测试哪个警察会被唤醒
                PoliceManager.Instance.DispatchNearestPolice(transform.position);
            }
            else
            {
                Debug.LogError("【错误】场景里找不到 PoliceManager！请检查是否创建了 GameManager 物体并挂载了脚本。");
            }
        }
    }

    // 画个图示，让你在 Scene 窗口里看到“模拟的案发地点”在哪里
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position, "此处模拟偷窃", true);
    }
}