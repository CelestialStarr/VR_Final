using UnityEngine;

public class BlindBoxController : MonoBehaviour
{
    [Header("Reward Settings (必须配置)")]
    public GameObject[] propPrefabs;
    public Transform spawnPoint;

    [Header("Box Settings")]
    public Transform boxRoot;
    public float liftHeight = 0.5f;
    public float liftSpeed = 5f;

    [Header("Lid Settings")]
    public Transform leftLid;
    public Transform rightLid;
    public float openAngle = 110f;
    public float openSpeed = 5f;

    // ★★★ 已删除 Scale Settings (不再控制大小) ★★★

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

    void Awake()
    {
        if (boxRoot == null) boxRoot = transform;

        boxStartPos = boxRoot.localPosition;
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

    void OnEnable()
    {
        ResetBoxState();
        SpawnRandomReward();
        OpenBox();
    }

    void ResetBoxState()
    {
        hasDropped = false;
        isLifting = false;
        isOpening = false;
        isRewardShowing = false;
        currentRotateTime = 0f;

        if (boxRoot != null)
        {
            boxRoot.gameObject.SetActive(true);
            boxRoot.localPosition = boxStartPos;
            boxRoot.localRotation = Quaternion.identity;
        }

        if (leftLid != null) leftLid.localRotation = leftClosedRot;
        if (rightLid != null) rightLid.localRotation = rightClosedRot;

        if (currentReward != null)
        {
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

        // ★★★ 重点：完全删除了缩放代码，保持Prefab原始大小 ★★★

        // 生成瞬间隐藏
        currentReward.SetActive(false);

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

        // Step 1: 升起
        if (isLifting)
        {
            boxRoot.localPosition = Vector3.Lerp(boxRoot.localPosition, boxTargetPos, Time.deltaTime * liftSpeed);
            if (Vector3.Distance(boxRoot.localPosition, boxTargetPos) < 0.05f)
            {
                boxRoot.localPosition = boxTargetPos;
                isLifting = false;
                isOpening = true;
            }
        }

        // Step 2: 开盖
        if (isOpening)
        {
            if (leftLid != null) leftLid.localRotation = Quaternion.Lerp(leftLid.localRotation, leftOpenRot, Time.deltaTime * openSpeed);
            if (rightLid != null) rightLid.localRotation = Quaternion.Lerp(rightLid.localRotation, rightOpenRot, Time.deltaTime * openSpeed);

            if (leftLid != null && Quaternion.Angle(leftLid.localRotation, leftOpenRot) < openAngle * 0.2f)
            {
                isRewardShowing = true;
            }
        }

        // Step 3: 盒子整体旋转
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
            // ★★★ 动画播完，在这里显示物体 ★★★
            currentReward.SetActive(true);

            currentReward.transform.SetParent(null);

            // ★★★ 重点：这里也删除了任何缩放代码 ★★★

            Rigidbody rb = currentReward.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(boxRoot.forward * 2f + Random.insideUnitSphere * throwForce, ForceMode.Impulse);
            }

            currentReward = null;
        }

        if (boxRoot != null)
        {
            boxRoot.gameObject.SetActive(false);
        }
    }
}