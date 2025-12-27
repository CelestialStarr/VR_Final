using UnityEngine;
using UnityEngine.UI;

public class MoneyHUD_Legacy : MonoBehaviour
{
    public Text moneyText;

    void Update()
    {
        var gs = GameState.Instance;
        if (gs == null || moneyText == null) return;

        moneyText.text = "$" + gs.money;
    }
}
