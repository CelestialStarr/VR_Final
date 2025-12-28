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
    [SerializeField] private float groupSwitchDelay = 2.0f;     // 第一组出现后等待多久出第二组
    [SerializeField] private float fadeDuration = 0.5f;         // 渐显需要多少秒

    void Start()
    {
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

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}