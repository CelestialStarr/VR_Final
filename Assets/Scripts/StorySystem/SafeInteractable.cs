using UnityEngine;
using UnityEngine.SceneManagement;

public class SafeInteractable : MonoBehaviour
{
    public ItemData safeData;
    public string returnSceneName = "CafeScene";

    private bool hasStolen = false;

    public void StealSafe()
    {
        if (hasStolen) return;
        hasStolen = true;

        if (!InventoryManager.Instance.AddItem(safeData))
        {
            hasStolen = false;
            return;
        }

        if (GameState.Instance != null && GameState.Instance.storyStage == 2)
        {
            GameState.Instance.storyStage = 3;
            Debug.Log("【剧情推进】保险箱已偷，Stage 2 → 3");
        }

        SceneManager.LoadScene(returnSceneName);
        Destroy(gameObject);
    }
}
