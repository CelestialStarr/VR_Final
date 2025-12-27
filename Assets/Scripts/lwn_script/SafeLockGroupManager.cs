using UnityEngine;

public class SafeLockGroupManager : MonoBehaviour
{
    [Header("Locks")]
    public SafeLock[] locks;   // 拖 3 个锁进来

    [Header("Door")]
    public SafeDoorAnimation safeDoor;

    private bool opened = false;

    void Update()
    {
        if (opened) return;

        foreach (var l in locks)
        {
            if (!l.IsUnlocked())
                return;
        }

        // ⭐ 全部解锁
        opened = true;
        safeDoor.StartOpening();
        Debug.Log("所有密码正确，保险箱开启！");
    }
}
