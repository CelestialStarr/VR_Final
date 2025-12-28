using UnityEngine;

public class SceneUIHelper : MonoBehaviour
{
    // ---------------------------------------------------------
    // 把这个脚本挂在每个场景的 Canvas 或者某个空物体上
    // 然后按钮的 OnClick 拖拽这个脚本，选择对应的方法即可
    // ---------------------------------------------------------

    public void Click_GoToGameCafe()
    {
        // 检查一下是否安全（防止你直接从这个场景开始游戏，忘记加载Manager）
        if (CheckManager())
        {
            GameSceneManager.Instance.GoToGameCafe();
        }
    }

    public void Click_GoToTown()
    {
        if (CheckManager())
        {
            GameSceneManager.Instance.GoToTown();
        }
    }

    public void Click_GoToHouse2()
    {
        if (CheckManager())
        {
            GameSceneManager.Instance.GoToHouse2();
        }
    }

    public void Click_GoToExpressStation()
    {
        if (CheckManager())
        {
            GameSceneManager.Instance.GoToExpressStation();
        }
    }

    // 这是一个辅助检查方法，不用绑定给按钮
    private bool CheckManager()
    {
        if (GameSceneManager.Instance == null)
        {
            Debug.LogError("报错啦！找不到 GameSceneManager！\n" +
                           "请确保你是从【包含 GameSceneManager 的初始场景】开始运行游戏的，\n" +
                           "或者确保该 Manager 没有被意外销毁。");
            return false;
        }
        return true;
    }
}