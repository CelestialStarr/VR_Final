using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public int money = 0;
    public int moneyTargetForReward = 100;
    public bool rewardUnlocked = false;


    // 0 = first mentor dialogue (send to street)
    // 1 = returned after first mission (new mentor dialogue)
    // 2 = next stages (you can expand later)
    public int mentorStage = 0;

    // Mission flags
    public bool firstMissionDone = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
