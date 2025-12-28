using UnityEngine;
using UnityEngine.UI;

public class StreetPhoneSystem : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject phonePanel;
    public Text phoneText;
    public Button closeButton;

    [TextArea]
    public string mentorMessage = "Boss: Hey kid. Good job on the cash. \nNow, go to the Bank and steal a Safe for me. \nDon't ask questions.";

    void Start()
    {
        // Hide phone at start
        if (phonePanel != null)
            phonePanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePhone);
    }

    // Called by StoryMissionListener
    public void ShowPhoneCall()
    {
        if (phonePanel != null)
        {
            phonePanel.SetActive(true);

            if (phoneText != null)
                phoneText.text = mentorMessage;

            Debug.Log("System: Phone UI Opened.");
        }
    }

    void ClosePhone()
    {
        if (phonePanel != null)
            phonePanel.SetActive(false);
    }
}