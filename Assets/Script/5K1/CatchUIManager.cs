using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CatchUIManager : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private GameObject arrestedTextObject;
    [SerializeField] private GameObject masterPanelObject;

    [Header("结局对话组 (需挂载 CanvasGroup)")]
    [SerializeField] private CanvasGroup firstGroupCG;
    [SerializeField] private CanvasGroup secondGroupCG;

    [Header("时间设置")]
    [SerializeField] private float arrestedTextDuration = 3.0f;
    [SerializeField] private float groupSwitchDelay = 2.0f;
    [SerializeField] private float fadeDuration = 0.5f;

    // [新增] 用于缓存 TimeGameplayManager
    private TimeGameplayManager timeManager;

    void Start()
    {
        // 1. 自动寻找场景里的 TimeGameplayManager
        timeManager = FindFirstObjectByType<TimeGameplayManager>();

        if (arrestedTextObject != null) arrestedTextObject.SetActive(false);
        if (masterPanelObject != null) masterPanelObject.SetActive(false);

        // 初始化：确保激活但透明度为0
        if (firstGroupCG != null)
        {
            firstGroupCG.gameObject.SetActive(true);
            firstGroupCG.alpha = 0f;
        }
        if (secondGroupCG != null)
        {
            secondGroupCG.gameObject.SetActive(true);
            secondGroupCG.alpha = 0f;
        }
    }

    public void ShowCatchUI()
    {
        // 如果之前有暂停游戏，这里确保界面能动，但如果你希望界面出现时游戏暂停，可以加 Time.timeScale = 0;
        // 建议保持 Time.timeScale = 1 或者在 UI 动画播放时不暂停，视你需求而定。

        if (arrestedTextObject != null)
        {
            arrestedTextObject.SetActive(true);
            StartCoroutine(HideArrestedTextDelay());
        }

        if (masterPanelObject != null)
        {
            masterPanelObject.SetActive(true);
            StopCoroutine("PlayEndingSequence");
            StartCoroutine("PlayEndingSequence");
        }
    }

    IEnumerator PlayEndingSequence()
    {
        // 0. 重置状态
        if (firstGroupCG != null) firstGroupCG.alpha = 0f;
        if (secondGroupCG != null) secondGroupCG.alpha = 0f;

        // 1. 第一组 渐显
        yield return StartCoroutine(DoFade(firstGroupCG, 1f, fadeDuration));

        // 2. 等待
        yield return new WaitForSeconds(groupSwitchDelay);

        // 3. 第二组 渐显
        yield return StartCoroutine(DoFade(secondGroupCG, 1f, fadeDuration));
    }

    // 通用渐变工具
    IEnumerator DoFade(CanvasGroup cg, float targetAlpha, float duration)
    {
        if (cg == null) yield break;

        float startAlpha = cg.alpha;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    IEnumerator HideArrestedTextDelay()
    {
        yield return new WaitForSeconds(arrestedTextDuration);
        if (arrestedTextObject != null) arrestedTextObject.SetActive(false);
    }

    // [修改核心] 将原来的 QuitGame 替换为这个方法
    // 请记得在 Unity Inspector 里的 Button OnClick 事件重新绑定这个新方法！
    public void GoSleepAndClose()
    {
        Debug.Log("被抓获，强制送去睡觉...");

        // 1. 执行睡觉逻辑
        if (timeManager != null)
        {
            timeManager.PerformSleep();
        }
        else
        {
            // 防止没找到引用的保险措施
            timeManager = FindFirstObjectByType<TimeGameplayManager>();
            if (timeManager != null) timeManager.PerformSleep();
        }

        // 2. 关闭抓获的 UI 面板
        if (masterPanelObject != null) masterPanelObject.SetActive(false);
        if (arrestedTextObject != null) arrestedTextObject.SetActive(false);

        // 3. 确保时间恢复流动 (如果你的抓获逻辑之前暂停了游戏，这里必须恢复)
        Time.timeScale = 1f;
    }
}