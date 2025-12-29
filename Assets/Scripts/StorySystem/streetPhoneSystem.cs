using UnityEngine;
using UnityEngine.UI;

public class StreetPhoneSystem : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject phonePanel;
    public Text phoneText;
    public Button closeButton;

    [TextArea]
    public string mentorMessage = "Boss: Hey kid. Good job on the cash. \nNow, go to the Bank and steal a Safe for me. \nDon't ask questions.";

    void Start()
    {
        if (phonePanel != null) phonePanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePhone);
    }

    public void ShowPhoneCall()
    {
        if (phonePanel != null)
        {
            phonePanel.SetActive(true);
            if (phoneText != null) phoneText.text = mentorMessage;
        }
    }

    // ★★★ 重点修改这里！ ★★★
    void ClosePhone()
    {
        // 1. 关闭 UI
        if (phonePanel != null)
            phonePanel.SetActive(false);

        // 2. 在挂断的一瞬间，推进剧情！
        // 只有当前是阶段 1 (赚钱阶段) 时，才进阶到 2 (偷保险箱)
        // 这样防止以后复用电话时重复触发
        if (GameState.Instance != null && GameState.Instance.storyStage == 1)
        {
            Debug.LogError("【电话挂断】剧情推进！进入阶段 2 (偷保险箱)");
            GameState.Instance.storyStage = 2;
        }
    }
}