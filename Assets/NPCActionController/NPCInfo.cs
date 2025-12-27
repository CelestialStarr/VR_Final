using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInfo : MonoBehaviour
{
    public enum NPCTrait
    {
        Paranoid,       // 疑神疑鬼：容易张望
        Enthusiastic,   // 热情：自动对话
        Vigilant,       // 警惕：警戒范围大，判定严
        Kind,           // 和善：偷盗判定宽
        Noisy,          // 吵闹：吸引视线
        Impatient       // 急躁：路径不稳定
    }

    [Header("Character Info")]
    public string Chartag;

    [Header("Traits")]
    // 在Inspector面板里：
    // 1. 如果你留空 (Size = 0) -> 游戏开始时会自动随机生成
    // 2. 如果你加了内容 (比如点了+号选了 Vigilant) -> 游戏开始时就用你选的这个
    public List<NPCTrait> activeTraits = new List<NPCTrait>();

    void Awake()
    {
        // === 核心修改逻辑 ===
        // 检查列表是否为空
        if (activeTraits.Count == 0)
        {
            // 如果你在面板里什么都没填，我就帮你随机生成
            GenerateRandomTraits();
        }
        else
        {
            // 如果你在面板里填了东西，我就什么都不做
            // 直接保留你填的那些特质
            Debug.Log($"{Chartag} 使用了手动设置的性格: {activeTraits.Count} 个");
        }
    }

    void GenerateRandomTraits()
    {
        // 注意：这里不需要 activeTraits.Clear() 了，因为它肯定是空的才进得来

        int traitCount = Random.Range(1, 3); // 随机 1-2 个
        var allTraits = System.Enum.GetValues(typeof(NPCTrait));
        HashSet<NPCTrait> chosenTraits = new HashSet<NPCTrait>();

        while (chosenTraits.Count < traitCount)
        {
            NPCTrait randomTrait = (NPCTrait)allTraits.GetValue(Random.Range(0, allTraits.Length));
            chosenTraits.Add(randomTrait);
        }

        activeTraits.AddRange(chosenTraits);
        Debug.Log($"{Chartag} 自动生成了性格: {string.Join(", ", activeTraits)}");
    }

    public bool HasTrait(NPCTrait traitToCheck)
    {
        return activeTraits.Contains(traitToCheck);
    }

}