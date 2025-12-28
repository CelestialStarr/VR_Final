using UnityEngine;

public class BlindBoxController : MonoBehaviour
{
    [Header("Reward Settings (必须配置)")]
    public GameObject[] propPrefabs;
    public Transform spawnPoint;

    [Header("Box Settings")]
    public Transform boxRoot;           // 整个盒子
    public float liftHeight = 0.5f;
    public float liftSpeed = 5f;

    [Header("Lid Settings")]
    public Transform leftLid;
    public Transform rightLid;
    public float openAngle = 110f;
    public float openSpeed = 5f;

    [Header("Scale Settings (关键修改)")]
    public Vector3 finalRewardScale = Vector3.one;
    [Range(0.1f, 1f)]
    public float insideBoxScale = 0.1f;

    [Header("Drop Settings")]
    public float rotateDuration = 1.5f;
    public float throwForce = 3f;

    // 内部变量
    private Vector3 boxStartPos;
    private Vector3 boxTargetPos;
    private Quaternion leftClosedRot;
    private Quaternion rightClosedRot;
    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;

    private GameObject currentReward;
    private bool isLifting = false;
    private bool isOpening = false;
    private bool isRewardShowing = false;

    private float currentRotateTime = 0f;
    private bool hasDropped = false;

    // ★ 修改 1：使用 Awake 进行一次性的数据计算
    // 这些数据（比如盖子打开的角度、盒子的原始位置）永远不会变，所以只算一次
    void Awake()
    {
        if (boxRoot == null) boxRoot = transform; // 防止空引用

        boxStartPos = boxRoot.localPosition; // 建议用 localPosition 以防父物体移动
        boxTargetPos = boxStartPos + Vector3.up * liftHeight;

        if (leftLid != null)
        {
            leftClosedRot = leftLid.localRotation;
            leftOpenRot = leftClosedRot * Quaternion.Euler(-openAngle, 0, 0);
        }

        if (rightLid != null)
        {
            rightClosedRot = rightLid.localRotation;
            rightOpenRot = rightClosedRot * Quaternion.Euler(-openAngle, 0, 0);
        }
    }

    // ★ 修改 2：使用 OnEnable 代替 Start
    // 每次 SetActive(true) 时，这个函数都会运行，相当于“重置键”
    void OnEnable()
    {
        ResetBoxState();     // 1. 重置所有变量和位置
        SpawnRandomReward(); // 2. 生成新奖励
        OpenBox();           // 3. 开始流程
    }

    // ★ 新增：专门用来重置状态的函数
    void ResetBoxState()
    {
        // 重置布尔值
        hasDropped = false;
        isLifting = false;
        isOpening = false;
        isRewardShowing = false;
        currentRotateTime = 0f;

        // 重置盒子位置和旋转
        if (boxRoot != null)
        {
            boxRoot.gameObject.SetActive(true); // 确保盒子本身是显示的
            boxRoot.localPosition = boxStartPos;
            boxRoot.localRotation = Quaternion.identity; // 重置旋转
        }

        // 重置盖子角度
        if (leftLid != null) leftLid.localRotation = leftClosedRot;
        if (rightLid != null) rightLid.localRotation = rightClosedRot;

        // 清理旧的奖励 (防止上次没掉出去卡在里面)
        if (currentReward != null)
        {
            // 如果它还在这个脚本控制下（还没掉出去），就销毁它
            if (currentReward.transform.parent == spawnPoint)
            {
                Destroy(currentReward);
            }
            currentReward = null;
        }
    }

    void SpawnRandomReward()
    {
        if (propPrefabs.Length == 0 || spawnPoint == null) return;

        int randomIndex = Random.Range(0, propPrefabs.Length);

        currentReward = Instantiate(propPrefabs[randomIndex], spawnPoint.position, spawnPoint.rotation);
        currentReward.transform.SetParent(spawnPoint);
        currentReward.transform.localScale = finalRewardScale * insideBoxScale;

        Rigidbody rb = currentReward.GetComponent<Rigidbody>();
        if (rb == null) rb = currentReward.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void OpenBox()
    {
        isLifting = true;
    }

    void Update()
    {
        if (hasDropped) return;

        // --- Step 1: 升起 ---
        if (isLifting)
        {
            // 使用 localPosition 处理
            boxRoot.localPosition = Vector3.Lerp(boxRoot.localPosition, boxTargetPos, Time.deltaTime * liftSpeed);
            if (Vector3.Distance(boxRoot.localPosition, boxTargetPos) < 0.05f)
            {
                boxRoot.localPosition = boxTargetPos;
                isLifting = false;
                isOpening = true;
            }
        }

        // --- Step 2: 开盖 ---
        if (isOpening)
        {
            if (leftLid != null) leftLid.localRotation = Quaternion.Lerp(leftLid.localRotation, leftOpenRot, Time.deltaTime * openSpeed);
            if (rightLid != null) rightLid.localRotation = Quaternion.Lerp(rightLid.localRotation, rightOpenRot, Time.deltaTime * openSpeed);

            if (leftLid != null && Quaternion.Angle(leftLid.localRotation, leftOpenRot) < openAngle * 0.2f)
            {
                isRewardShowing = true;
            }
        }

        // --- Step 3: 盒子整体旋转 ---
        if (isRewardShowing)
        {
            float rotateSpeed = 360f / rotateDuration;
            boxRoot.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

            currentRotateTime += Time.deltaTime;

            if (currentRotateTime >= rotateDuration)
            {
                DropReward();
            }
        }
    }

    void DropReward()
    {
        hasDropped = true;

        if (currentReward != null)
        {
            currentReward.transform.SetParent(null);
            currentReward.transform.localScale = finalRewardScale;

            Rigidbody rb = currentReward.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(boxRoot.forward * 2f + Random.insideUnitSphere * throwForce, ForceMode.Impulse);
            }

            // 重要：让 currentReward 脱离变量控制，以免 ResetBoxState 误删它
            currentReward = null;
        }

        // 隐藏盒子（如果是通过 UI 控制父物体显隐，这里可以不需要隐藏 boxRoot，
        // 但既然你原本逻辑是隐藏，我保留这个逻辑）
        if (boxRoot != null)
        {
            // 注意：如果你是整个物体 SetActive(false)，这里其实不需要手动隐 boxRoot
            // 但为了视觉效果，先隐藏盒子
            boxRoot.gameObject.SetActive(false);

            // 实际上，这里隐藏了 boxRoot 可能会导致下次 OnEnable 时看不到盒子
            // 所以我在 ResetBoxState 里加了一句 boxRoot.gameObject.SetActive(true) 来修复它
        }

        // ★ 关键补充：
        // 既然开包完成了，通常我们需要通知 UI 脚本“这波结束了”，
        // 或者单纯等待玩家关闭 UI。
        // 由于你的 UI 是通过“按按钮 -> SetActive(true)”来控制的，
        // 只要这脚本挂在那个被激活的物体上，下次 UI 把它关掉再打开，
        // OnEnable 就会自动重置一切。
    }
}