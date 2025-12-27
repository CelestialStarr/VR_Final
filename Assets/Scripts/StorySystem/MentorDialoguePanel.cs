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
    [TextArea(2, 4)] public string[] stage0Lines; // first time (teach + send to street)
    [TextArea(2, 4)] public string[] stage1Lines; // return after mission (new lesson)

    private string[] activeLines;
    private int index = 0;
    private bool finished = false;

    private float lastAdvanceTime = -999f;
    public float clickCooldown = 0.35f;

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(Advance);
            continueButton.interactable = true;
        }

        PickDialogueByStage();
        index = 0;
        finished = false;

        if (activeLines != null && activeLines.Length > 0)
            ShowLine(0);
        else if (dialogueText != null)
            dialogueText.text = "...";
    }

    void PickDialogueByStage()
    {
        var gs = GameState.Instance;

        // Default: stage0
        activeLines = stage0Lines;

        if (gs == null) return;

        // If mission done, switch to stage1 dialogue
        if (gs.mentorStage >= 1 || gs.firstMissionDone)
        {
            if (stage1Lines != null && stage1Lines.Length > 0)
                activeLines = stage1Lines;
        }
    }

    public void Advance()
    {
        if (finished) return;
        if (Time.unscaledTime - lastAdvanceTime < clickCooldown) return;
        lastAdvanceTime = Time.unscaledTime;

        if (activeLines == null || activeLines.Length == 0) return;

        index++;

        if (index >= activeLines.Length)
        {
            finished = true;

            var gs = GameState.Instance;

            // Stage0 ended -> allow leaving
            if (gs != null && gs.mentorStage == 0)
            {
                gs.mentorStage = 1;
            }

            if (dialogueText != null)
            {
                if (gs != null && gs.firstMissionDone)
                    dialogueText.text = "Good. Next lesson unlocked.";
                else
                    dialogueText.text = "Objective: Go to the street and steal one item.";
            }

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

        i = Mathf.Clamp(i, 0, activeLines.Length - 1);
        dialogueText.text = activeLines[i];
    }
}
