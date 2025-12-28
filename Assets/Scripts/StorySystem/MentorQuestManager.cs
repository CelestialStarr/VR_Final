using UnityEngine;

public class MentorQuestManager : MonoBehaviour
{
    public static MentorQuestManager Instance;

    public int firstTargetMoney = 100;
    private bool firstRewardGiven = false;

    private void Awake()
    {
        Instance = this;
    }

    public void CheckMoneyProgress(int currentMoney)
    {
        if (!firstRewardGiven && currentMoney >= firstTargetMoney)
        {
            TriggerLockpickReward();
        }
    }

    void TriggerLockpickReward()
    {
        firstRewardGiven = true;

        Debug.Log("师傅：不错，你已经赚到 100 了。");
        Debug.Log("师傅：现在教你点真本事。");

        // 1. 触发师傅剧情
        // 2. 解锁撬锁教学
        // 3. 给玩家 Lockpick 工具
    }
}
