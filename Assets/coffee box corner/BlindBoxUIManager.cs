using UnityEngine;
using UnityEngine.UI;

public class BlindBoxUIManager : MonoBehaviour
{
    [Header("开包设置")]
    // ★ 新增：你需要把那个 Parcle 的 ItemData 拖到这里
    public ItemData requiredKeyItem;

    [Header("需要激活的3D物体")]
    public GameObject blindBoxObject;

    [Header("UI 控件")]
    public GameObject uiPanel;

    void Start()
    {
        if (blindBoxObject != null)
        {
            blindBoxObject.SetActive(false);
        }
    }

    public void OnOpenButtonClicked()
    {
        // ★ 核心修改：先问背包管理器“能不能扣除一个包裹？”
        if (InventoryManager.Instance != null && requiredKeyItem != null)
        {
            // 调用我们刚才写的函数，如果返回 true，说明扣除成功
            bool isSuccess = InventoryManager.Instance.TryConsumeItem(requiredKeyItem);

            if (isSuccess)
            {
                // === 扣除成功，执行开包动画逻辑 ===
                PerformOpenBoxLogic();
            }
            else
            {
                // === 扣除失败 (背包里没有)，可以在这里加个提示音或者弹窗 ===
                Debug.Log("开包失败：背包里没有对应的包裹！");
            }
        }
        else
        {
            Debug.LogError("请检查：InventoryManager是否存在？BlindBoxUIManager上的 RequiredKeyItem 是否拖入了数据？");
        }
    }

    // 把原来的显示逻辑封装在这里，保持代码整洁
    private void PerformOpenBoxLogic()
    {
        if (blindBoxObject != null)
        {
            blindBoxObject.SetActive(true);
        }

        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        Debug.Log("UI点击：消耗物品成功，开始播放开盒动画！");
    }
}