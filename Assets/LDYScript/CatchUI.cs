using UnityEngine;
using System.Collections;

public class CatchUIManager : MonoBehaviour
{
    [Header("UI 组件引用 (请拖入)")]
    [SerializeField] private GameObject arrestedTextObject; // 对应 "You are arrested"
    [SerializeField] private GameObject masterPanelObject;  // 对应 师傅头像+按钮

    [Header("设置")]
    [SerializeField] private float textDuration = 3.0f;

    // --- 新增：游戏开始时，强制隐藏 UI ---
    void Start()
    {
        if (arrestedTextObject != null) arrestedTextObject.SetActive(false);
        if (masterPanelObject != null) masterPanelObject.SetActive(false);
    }

    // 由警察调用
    public void ShowCatchUI()
    {
        // 显示 "You are arrested"
        if (arrestedTextObject != null)
        {
            arrestedTextObject.SetActive(true);
            StartCoroutine(HideArrestedTextDelay());
        }

        // 显示 师傅面板
        if (masterPanelObject != null)
        {
            masterPanelObject.SetActive(true);
        }
    }

    IEnumerator HideArrestedTextDelay()
    {
        yield return new WaitForSeconds(textDuration);
        if (arrestedTextObject != null) arrestedTextObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}