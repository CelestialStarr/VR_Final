using UnityEngine;
using UnityEngine.UI;

public class MentorDialoguePanel_Legacy : MonoBehaviour
{
    [Header("UI (Legacy)")]
    public Text dialogueText;
    public Button continueButton;

    [Header("Story")]
    public StoryFlowController storyController;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    public string[] lines;

    private int index = 0;
    private bool finished = false;

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(Advance);
        }

        if (lines != null && lines.Length > 0)
            ShowLine(0);
        else if (dialogueText != null)
            dialogueText.text = "бн";
    }

    public void Advance()
    {
        if (finished) return;
        if (lines == null || lines.Length == 0) return;

        index++;

        if (index >= lines.Length)
        {
            finished = true;

            if (dialogueText != null)
                dialogueText.text = "Go to the street. First lesson: steal from a passerby.";

            if (continueButton != null)
                continueButton.interactable = false;

            if (storyController != null)
                storyController.OnMentorInteraction();

            return;
        }

        ShowLine(index);
    }

    void ShowLine(int i)
    {
        if (dialogueText == null) return;

        i = Mathf.Clamp(i, 0, lines.Length - 1);
        dialogueText.text = lines[i];
    }
}
