using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// 建议要求物体必须有 AudioSource 组件，防止报错
[RequireComponent(typeof(AudioSource))]
public class InstantStealObject : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    // ================= 保持你的原有设置 =================
    [Header("Inventory Settings")]
    [Tooltip("请把这个物体对应的 ItemData (比如 Gold, Apple) 拖到这里")]
    public ItemData itemData;

    [Header("Audio Settings")]
    [Tooltip("偷窃成功时播放的音效")]
    public AudioClip stealSuccessSound; // 【新增1】音效变量
    // ==========================================================

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isStealing = false;
    public GameObject stealDetection;

    private AudioSource _audioSource; // 【新增2】音频源引用

    private void Start()
    {
        stealDetection.SetActive(false);
    }

    protected override void Awake()
    {
        base.Awake();
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        // 【新增3】获取 AudioSource 组件
        _audioSource = GetComponent<AudioSource>();
        // 确保一开始不自动播放
        _audioSource.playOnAwake = false;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        stealDetection.SetActive(true);

        if (!_isStealing)
        {
            HandleSuccess();
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        stealDetection.SetActive(false);
        _isStealing = false;
    }

    public void HandleSuccess()
    {
        bool addSuccess = false;

        if (InventoryManager.Instance != null && itemData != null)
        {
            addSuccess = InventoryManager.Instance.AddItem(itemData);
        }
        else
        {
            Debug.LogWarning("注意：缺少 Manager 或 ItemData");
        }

        if (addSuccess)
        {
            // A. 成功：标记偷盗状态，物体消失
            _isStealing = true;
            Debug.Log("偷盗成功！物体消失");

            // 【新增4】播放音效逻辑
            if (_audioSource != null && stealSuccessSound != null)
            {
                // 使用 PlayOneShot 可以在物体被 SetActive(false) 之前的一瞬间发出声音
                // 但注意：如果下面直接 SetActive(false)，挂在物体身上的 AudioSource 也会立刻停止。
                // 解决方法：使用 AudioSource.PlayClipAtPoint 生成一个临时的声音播放点。

                AudioSource.PlayClipAtPoint(stealSuccessSound, transform.position);
            }
            else
            {
                Debug.LogWarning("未设置音效或找不到 AudioSource");
            }

            // 强制松手
            if (isSelected)
            {
                interactionManager.SelectExit(firstInteractorSelecting, this);
            }

            // 物体消失
            gameObject.SetActive(false);
        }
        else
        {
            // B. 失败（背包满了）
            _isStealing = false;
            Debug.Log("偷盗失败：背包已满或受限，物体掉落");

            if (isSelected)
            {
                interactionManager.SelectExit(firstInteractorSelecting, this);
            }
        }
    }

    public void HandleFailure()
    {
        if (!_isStealing && !gameObject.activeSelf) return;

        _isStealing = false;

        if (isSelected)
        {
            interactionManager.SelectExit(firstInteractorSelecting, this);
        }

        transform.position = _originalPosition;
        transform.rotation = _originalRotation;

        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}