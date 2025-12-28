using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PoliceManager : MonoBehaviour
{
    public static PoliceManager Instance; // 单例，方便NPC调用

    [Header("场景中所有的警察")]
    // 自动寻找，或者你可以手动拖进去
    public List<PoliceAI> allPolices = new List<PoliceAI>();

    [Header("玩家引用 (必须设置)")]
    public Transform playerTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 自动找到场景里所有的 PoliceAI 脚本
        if (allPolices.Count == 0)
        {
            allPolices = FindObjectsOfType<PoliceAI>().ToList();
        }
    }

    /// <summary>
    /// 调度最近的警察
    /// </summary>
    /// <param name="crimeScenePos">案发地点（NPC的位置）</param>
    public void DispatchNearestPolice(Vector3 crimeScenePos)
    {
        if (allPolices.Count == 0) return;

        PoliceAI nearestPolice = null;
        float minDistance = float.MaxValue;

        // 遍历所有警察，找个最近的且没在忙的
        foreach (var police in allPolices)
        {
            float dist = Vector3.Distance(police.transform.position, crimeScenePos);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPolice = police;
            }
        }

        if (nearestPolice != null)
        {
            Debug.Log($"调度了警察 {nearestPolice.name} 前往抓捕！");
            nearestPolice.StartChasing(playerTransform);
        }
    }
}