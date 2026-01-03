using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
public class InstantStealObject : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    [Header("Inventory Settings")]
    [Tooltip("Assign the ItemData (e.g., Gold, Apple) for this object here")]
    public ItemData itemData;

    [Header("Audio Settings")]
    [Tooltip("Sound clip to play when the steal is successful")]
    public AudioClip stealSuccessSound;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isStealing = false;
    public GameObject stealDetection;

    private AudioSource _audioSource;

    private void Start()
    {
        stealDetection.SetActive(false);
    }

    protected override void Awake()
    {
        base.Awake();
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        _audioSource = GetComponent<AudioSource>();
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
            Debug.LogWarning("Missing InventoryManager or ItemData");
        }

        if (addSuccess)
        {
            _isStealing = true;
            Debug.Log("Steal successful! Object hidden");

            if (_audioSource != null && stealSuccessSound != null)
            {
                AudioSource.PlayClipAtPoint(stealSuccessSound, transform.position);
            }
            else
            {
                Debug.LogWarning("Steal sound not set or AudioSource missing");
            }

            if (isSelected)
            {
                interactionManager.SelectExit(firstInteractorSelecting, this);
            }

            gameObject.SetActive(false);
        }
        else
        {
            _isStealing = false;
            Debug.Log("Steal failed: Inventory full or restricted. Object dropped.");

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