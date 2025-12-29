using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SitCharAni : MonoBehaviour
{
    private Animator anim;
    public AlertController alertSystem;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        // 如果 alertSystem 没有在面板手动赋值，则自动获取
        if (alertSystem == null)
            alertSystem = GetComponent<AlertController>();
    }

    // Update is called once per frame
    void Update()
    {
        // 安全检查：防止没有引用报错
        if (alertSystem == null || anim == null) return;

        UpdateAnimationState();
    }

    void UpdateAnimationState()
    {
        // 1. 优先检查是否已经处于“警戒/战斗”状态 (isAlerted)
        // 使用 alertSystem.isAlerted 而不是仅仅判断 value == max，
        // 是为了支持 AlertController 中的 30秒冷却逻辑。
        // 在冷却期间，虽然 value 可能下降，但在逻辑上 NPC 依然是警戒状态。
        if (alertSystem.isAlerted)
        {
            // 对应：当AlertValue=maxAlertValue时（或处于冷却期）
            anim.SetBool("isAlerted", true);
            anim.SetBool("Sus", false); // 进入战斗/警戒后，不再是单纯的怀疑(Sus)
        }
        // 2. 如果没有处于高度警戒，但警戒值大于 0
        else if (alertSystem.currentAlertValue > 0)
        {
            // 对应：当AlertValue大于0时，设Sus为true
            anim.SetBool("isAlerted", false);
            anim.SetBool("Sus", true);
        }
        // 3. 完全正常状态 (AlertValue <= 0)
        else
        {
            anim.SetBool("isAlerted", false);
            anim.SetBool("Sus", false);
        }
    }
}