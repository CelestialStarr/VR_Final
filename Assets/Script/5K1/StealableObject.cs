using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class StealableObject : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    [Header("Steal Difficulty")]
    [Range(1, 10)]
    public int difficultyLevel = 3; // 该物品需要的手势数量

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isStealing = false;

    protected override void Awake()
    {
        base.Awake();
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (!_isStealing)
        {
            StartCoroutine(StealProcessRoutine());
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        // 如果中途松手，直接判定失败
        if (_isStealing)
        {
            GestureGameManager.Instance.CompleteChallenge(false);
        }
    }

    private IEnumerator StealProcessRoutine()
    {
        _isStealing = true;
        Debug.Log("开始手势验证...");

        // 开启手势小游戏
        GestureGameManager.Instance.StartValidation(this, difficultyLevel);
        yield return null;
    }

    public void HandleSuccess()
    {
        _isStealing = false;
        Debug.Log("偷窃成功！");
        gameObject.SetActive(false);
    }

    public void HandleFailure()
    {
        _isStealing = false;
        StopAllCoroutines();

        StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        // 强制脱离抓取
        if (isSelected && interactionManager != null && firstInteractorSelecting != null)
        {
            interactionManager.SelectExit(firstInteractorSelecting, this);
        }

        // 等一帧，让 XR 系统完成释放
        yield return null;

        transform.position = _originalPosition;
        transform.rotation = _originalRotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

}