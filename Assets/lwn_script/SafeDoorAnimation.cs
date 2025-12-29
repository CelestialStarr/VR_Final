using UnityEngine;

public class SafeDoorAnimation : MonoBehaviour
{
    [Header("Door Rotation Settings")]
    public float openAngle = 105f;     // 门打开的角度
    public float openSpeed = 2f;       // 开门速度
    public Vector3 rotationAxis = Vector3.up; // 绕什么轴转，默认是Y轴(0,1,0)

    [Header("Audio")]
    public AudioSource doorAudio;      // 拖入开门的声音（如金属摩擦声）

    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpening = false;
    private bool audioPlayed = false;

    void Start()
    {
        // 记录门关着时的初始旋转
        closedRotation = transform.localRotation;
        // 计算开门后的目标旋转
        targetRotation = closedRotation * Quaternion.Euler(rotationAxis * openAngle);
    }

    void Update()
    {
        if (!isOpening) return;

        // 使用 Slerp 平滑旋转到开启角度
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );

        // 播放开门声音（只播一次）
        if (!audioPlayed && doorAudio != null)
        {
            doorAudio.Play();
            audioPlayed = true;
        }

        // 旋转基本完成后停止逻辑
        if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.1f)
        {
            isOpening = false;
        }
    }

    // 这是给指针脚本调用的公开方法
    public void StartOpening()
    {
        if (isOpening) return;
        isOpening = true;
        Debug.Log("保险箱校验成功：开门！");
    }
}