using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    public enum SceneType
    {
        GameCafe,
        Town,
        House2,
        ExpressStation
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 核心加载逻辑 (保持不变) ---
    private void LoadScene(SceneType scene)
    {
        string sceneName = scene.ToString(); // 直接用枚举名作为场景名
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 去网吧
    /// </summary>
    public void GoToGameCafe()
    {
        Debug.Log("正在前往网吧...");
        LoadScene(SceneType.GameCafe);
    }

    /// <summary>
    /// 去小镇
    /// </summary>
    public void GoToTown()
    {
        Debug.Log("正在前往小镇...");
        LoadScene(SceneType.Town);
    }

    /// <summary>
    /// 去二号房
    /// </summary>
    public void GoToHouse2()
    {
        Debug.Log("正在前往 House2...");
        LoadScene(SceneType.House2);
    }

    /// <summary>
    /// 去快递站
    /// </summary>
    public void GoToExpressStation()
    {
        Debug.Log("正在前往快递站...");
        LoadScene(SceneType.ExpressStation);
    }
}