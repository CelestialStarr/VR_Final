using UnityEngine;

public class PauseController : MonoBehaviour
{
    [Header("Menu")]
    public GameObject menuObject;

    private bool isPaused = false;

    void Start()
    {
        // 游戏开始时隐藏菜单
        if (menuObject != null)
        {
            menuObject.SetActive(false);
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        // 1. 显示菜单 (一旦显示，UIFollowHead 就会自动开始运行跟随逻辑)
        menuObject.SetActive(true);

        // 2. 暂停游戏时间
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        // 1. 隐藏菜单
        menuObject.SetActive(false);

        // 2. 恢复游戏时间
        Time.timeScale = 1f;
    }
}