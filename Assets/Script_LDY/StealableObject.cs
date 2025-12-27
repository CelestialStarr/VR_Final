using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections; // 需要引用这个来使用协程

public class StealableObject : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isStealing = false;

    public bool IsBeingStolen => _isStealing;

    protected override void Awake()
    {
        base.Awake();
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // 只要抓起来，就算开始偷，状态设为 true
        if (!_isStealing)
        {
            StartCoroutine(StealProcessRoutine());
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        // 如果手松开了（自己扔掉或者被强制松开），停止偷盗状态
        _isStealing = false;
        StopAllCoroutines();
    }

    // --- 修改点：把偷盗改成一个协程（Coroutine），模拟“读条”时间 ---
    private IEnumerator StealProcessRoutine()
    {
        _isStealing = true;
        Debug.Log("开始偷盗... 需要保持持有 3 秒...");

        // 等待 3 秒。在这 3 秒内，如果被 NPC 看到，IsBeingStolen 都是 true，可以被抓！
        yield return new WaitForSeconds(3.0f);

        // 3秒后如果没有被抓，也没有松手，进行最终判定
        if (_isStealing)
        {
            bool success = JudgeStealSuccess();
            if (success) HandleSuccess();
            else HandleFailure();
        }
    }

    private bool JudgeStealSuccess()
    {
        return Random.value > 0.5f; // 50% 概率成功
    }

    public void HandleSuccess()
    {
        _isStealing = false;
        Debug.Log("偷盗成功！物体消失");
        gameObject.SetActive(false);
    }

    // --- 这个方法就是给 NPC 调用的接口 ---
    public void HandleFailure()
    {
        // 防止重复调用
        if (!_isStealing && !gameObject.activeSelf) return;

        _isStealing = false;
        StopAllCoroutines(); // 停止那个 3 秒倒计时

        Debug.Log("偷盗失败/被抓！回到原位");

        // 1. 强制让手放开物体 (XRI 核心功能)
        if (isSelected)
        {
            interactionManager.SelectExit(firstInteractorSelecting, this);
        }

        // 2. 回到原位
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;

        // 确保刚体速度归零，不然它飞回去后可能会乱弹
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}