using UnityEngine;

public class PackageOpenAnimation : MonoBehaviour
{
    [Header("Box Movement")]
    public Transform boxRoot;          // 整个盒子
    public float liftHeight = 0.3f;    // 升起高度
    public float liftSpeed = 1.5f;     // 升起速度

    [Header("Lid Settings")]
    public Transform leftLid;          // 左盖
    public Transform rightLid;         // 右盖
    public float openAngle = 70f;     // 打开角度
    public float openSpeed = 2f;       // 打开速度

    private Vector3 startPos;
    private Vector3 targetPos;

    private Quaternion leftClosedRot;
    private Quaternion rightClosedRot;
    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;

    private bool startOpening = false;
    private bool lifted = false;

    void Start()
    {
        // 记录初始位置
        startPos = boxRoot.position;
        targetPos = startPos + Vector3.up * liftHeight;

        // 记录初始旋转
        leftClosedRot = leftLid.localRotation;
        rightClosedRot = rightLid.localRotation;

        // 计算打开后的旋转
        leftOpenRot = leftClosedRot * Quaternion.Euler(-openAngle, 0,0 );
        rightOpenRot = rightClosedRot * Quaternion.Euler(-openAngle, 0,0 );
    }

    void Update()
    {
        if (!startOpening)
            return;

        // Step 1：盒子升起
        if (!lifted)
        {
            boxRoot.position = Vector3.Lerp(
                boxRoot.position,
                targetPos,
                Time.deltaTime * liftSpeed
            );

            if (Vector3.Distance(boxRoot.position, targetPos) < 0.01f)
            {
                lifted = true;
            }
        }
        // Step 2：盒盖打开
        else
        {
            leftLid.localRotation = Quaternion.Lerp(
                leftLid.localRotation,
                leftOpenRot,
                Time.deltaTime * openSpeed
            );

            rightLid.localRotation = Quaternion.Lerp(
                rightLid.localRotation,
                rightOpenRot,
                Time.deltaTime * openSpeed
            );
        }
    }

    //  对外触发（按钮 / Timeline / 事件）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OpenBox();
        }
    }

    public void OpenBox()
    {
        startOpening = true;

    }
}
