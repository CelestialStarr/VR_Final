using UnityEngine;
using System.Collections;

public class EndingUIManager : MonoBehaviour
{
    public static EndingUIManager Instance;

    [Header("UI Panels")]
    public GameObject Ending_Die;
    public GameObject Ending_Normal;
    public GameObject Ending_Rich;
    public GameObject Ending_Prison;

    [Header("Appear Effect")]
    public float fadeDuration = 0.6f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ===== 对外调用的方法 =====

    public void OpenDieEnding()
    {
        Appear(Ending_Die);
    }

    public void OpenNormalEnding()
    {
        Appear(Ending_Normal);
    }

    public void OpenRichEnding()
    {
        Appear(Ending_Rich);
    }

    public void OpenPrisonEnding()
    {
        Appear(Ending_Prison);
    }

    // ===== 出现效果核心 =====

    void Appear(GameObject panel)
    {
        if (!panel) return;

        panel.SetActive(true);

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (!cg) return;

        StopAllCoroutines();
        StartCoroutine(FadeIn(cg));
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }
}
