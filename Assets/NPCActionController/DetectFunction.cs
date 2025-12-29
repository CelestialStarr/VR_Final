using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectFunction : MonoBehaviour
{
    public AlertController alertSystem;

    // 1. 用一个变量“记住”我们是否在 PrivateArea 里
    private bool isInPrivateArea = false;

    private void Start()
    {
        if (alertSystem == null)
            alertSystem = GetComponent<AlertController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 2. 如果撞到的是大区域，记录状态为 true
        if (other.CompareTag("PrivateArea"))
        {
            isInPrivateArea = true;
        }

        // 3. 如果撞到的是偷窃检测物
        if (other.CompareTag("StealDetect"))
        {
            // 4. 检查刚才记录的状态：必须先在私人区域里，这个检测才生效
            if (isInPrivateArea)
            {
                Debug.Log("在私人区域内触发了偷窃警报！");
                alertSystem.IncreaseAlert(30);
            }
            else
            {
                // 可选：如果不在私人区域碰到这个物体（比如这东西被搬出来了），可能就不报警，或者报得少一点
                Debug.Log("碰到了检测物，但不在私人区域，没事。");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 5. 离开大区域时，忘记状态
        if (other.CompareTag("PrivateArea"))
        {
            isInPrivateArea = false;
            Debug.Log("离开了私人区域");
        }
    }
}