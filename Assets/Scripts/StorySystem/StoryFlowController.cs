using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryFlowController : MonoBehaviour
{
    // 1. 添加单例模式，这样其他脚本才能用 .Instance 访问
    public static StoryFlowController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public enum StoryState
    {
        Cafe_Intro,
        Mentor_Talked,
        Can_Leave_Cafe,
        Street_Tutorial,
        Tutorial_Complete
    }

    public StoryState currentState = StoryState.Cafe_Intro;

    void Start()
    {
        EnterState(currentState);
    }

    void EnterState(StoryState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case StoryState.Cafe_Intro:
                Debug.Log("Story: In cafe. Talk to mentor.");
                DisableTheft();
                break;
            case StoryState.Mentor_Talked:
                Debug.Log("Story: Mentor gave first task.");
                break;
            case StoryState.Can_Leave_Cafe:
                Debug.Log("Story: You may leave the cafe.");
                break;
            case StoryState.Street_Tutorial:
                Debug.Log("Story: Street tutorial started.");
                EnableTheft();
                break;
            case StoryState.Tutorial_Complete:
                Debug.Log("Story: Tutorial completed.");
                break;
        }
    }

    // 2. 修复报错逻辑
    public void CheckMoneyProgress()
    {
        // 你的代码里依赖了 GameState 和 MentorStage，但因为你没有提供 GameState 脚本，
        // 且 MentorStage 枚举可能未定义，为了消除报错，我先将这部分逻辑注释掉了。
        // 等你写好 GameState 和 MentorStage 后，再取消注释。

        /* var gs = GameState.Instance;
        if (gs.money >= 100 && gs.mentorStage == MentorStage.Stage1_GoStreet)
        {
            gs.mentorStage = MentorStage.Stage2_LockpickUnlocked;
            TriggerMentorDialogue(); 
        }
        */

        Debug.Log("CheckMoneyProgress 被调用了 (逻辑暂时挂起)");
    }

    // 3. 补全缺失的方法 (占位符)
    // 报错提示没有 TriggerMentorDialogue，这里加一个空的
    private void TriggerMentorDialogue()
    {
        Debug.Log("触发导师对话");
        // 这里写后续的对话逻辑
    }

    // ===== External Triggers =====

    public void OnMentorInteraction()
    {
        if (currentState == StoryState.Cafe_Intro)
        {
            EnterState(StoryState.Mentor_Talked);
            EnterState(StoryState.Can_Leave_Cafe);
        }
    }

    public void OnExitCafe()
    {
        if (currentState == StoryState.Can_Leave_Cafe)
        {
            EnterState(StoryState.Street_Tutorial);
            LoadStreetScene();
        }
    }

    // ===== System Placeholders =====

    void EnableTheft()
    {
        Debug.Log("System: Theft Enabled");
    }

    void DisableTheft()
    {
        Debug.Log("System: Theft Disabled");
    }

    void LoadStreetScene()
    {
        // 确保你的 Build Settings 里有这个场景，否则会报错
        // SceneManager.LoadScene("StreetScene"); 
        Debug.Log("尝试加载 StreetScene");
    }
}