using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class SafeLock : MonoBehaviour
{
    [Header("Password Settings")]
    [Tooltip("正确的解锁角度（-180 ~ 180）")]
    [SerializeField] private float targetAngle = 45f;

    [Tooltip("判定成功的角度范围 (±度数)")]
    [SerializeField] private float rangeTolerance = 15f;

    [Tooltip("解锁所需时间（秒）")]
    [SerializeField] private float requiredTime = 3f;

    [Tooltip("越接近中心，解锁越快")]
    [SerializeField] private float maxSpeedMultiplier = 5f;

    [Header("Debug")]
    [SerializeField] private float currentUnlockTimer;
    [SerializeField] private bool isUnlocked = false;

    private AudioSource audioSource;
    private Rigidbody rb;
    private bool isInCorrectRange = false;

    



    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        ResetLock();
    }

    void OnDisable()
    {
        if (audioSource.isPlaying) audioSource.Stop();
    }

    void Update()
    {
        void Update()
        {
            if (isUnlocked) return;

            float currentAngle = GetXAngleFromVector();
            float diff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

            // 【Debug】把这一行解除注释，看看每一帧的角度是多少
            // Debug.Log($"当前角度: {currentAngle}, 目标差距: {diff}");

            if (diff <= rangeTolerance)
            {
                if (!isInCorrectRange)
                {
                    isInCorrectRange = true;

                    // 【排查关键点】如果刚运行就打印这句话，说明你的旋钮一开始就摆对了位置！
                    Debug.LogWarning($"检测到进入范围！触发播放。当前角度:{currentAngle}");

                    audioSource.Play();
                }

                ProcessUnlock(diff);
            }
            else
            {
                if (isInCorrectRange)
                {
                    isInCorrectRange = false;
                    audioSource.Stop();
                }
            }
        }

    }

    float GetXAngleFromVector()
    {
        Vector3 currentUp = transform.localRotation * Vector3.up;
        return Vector3.SignedAngle(Vector3.up, currentUp, Vector3.right);
    }

    void ResetLock()
    {
        currentUnlockTimer = requiredTime;
        isUnlocked = false;

        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
    }

    void HandleAudio(bool play)
    {
        if (play)
        {
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying) audioSource.Stop();
        }
    }

    void ProcessUnlock(float diff)
    {
        float lerp = 1f - (diff / rangeTolerance);
        float speed = Mathf.Lerp(1f, maxSpeedMultiplier, lerp);

        currentUnlockTimer -= Time.deltaTime * speed;

        if (currentUnlockTimer <= 0)
        {
            UnlockSuccess();
        }
    }

    void UnlockSuccess()
    {
        isUnlocked = true;
        currentUnlockTimer = 0f;
        audioSource.Stop();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        

        Debug.Log($"{name} 解锁成功！");

    }


    //  给外部（管理器）用
    public bool IsUnlocked()
    {
        return isUnlocked;
    }

}
