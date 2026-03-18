using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Central controller for Blue Team stage progression.
/// Stage managers call NotifyPostHandled() after each submission.
/// Once enough posts are handled the stage advances automatically.
/// </summary>
public class BlueTeamManager : MonoBehaviour
{
    public static BlueTeamManager Instance { get; private set; }

    [Header("References")]
    public TimelineManager timelineManager;
    public StageContextPanel stageContextPanel;

    [Header("Stage Settings")]
    [Tooltip("The stage loaded on Start. Change in Inspector for testing.")]
    public int startingStage = 1;
    public int totalStages = 3;

    [Header("Progression")]
    [Tooltip("Posts the player must handle per stage (index 0 is Stage 1). Defaults to 1 if unset.")]
    public int[] requiredPostsPerStage = new int[] { 3, 3, 3 };

    [Header("Stage Context")]
    [Tooltip("One context/briefing string per stage displayed in the popup when each stage begins.")]
    public string[] stageContextTexts = new string[]
    {
        "You are a social media moderator. Identify and report suspicious phishing posts targeting users. You should select the most prominent category of phishing indicators, but don't worry about being perfect - just try to catch the most obvious signs of phishing.",
        "Social Media is a place for online communities to grow, make use of this by warning members of your community about a phishing post. Use the comment function to explain to other users why you think this post is suspicious and what signs they should look out for. This will help protect them from falling for the scam.",
        "Contextual Awareness is key to identifying sophisticated phishing attempts. Use the report button to flag any posts that seem suspicious. Pay attention to the context of each post and identify whether it is targeting an event, or is regular phishing."
    };

    [Header("Scene Transition")]
    [Tooltip("Scene to load when all stages are complete. Must be added to Build Settings.")]
    public string nextSceneName;
    [Tooltip("Seconds to wait after final feedback closes before moving to the next scene.")]
    public float finalStageTransitionDelay = 8f;

    public int CurrentStage { get; private set; }
    private int postsHandled;
    private int postsRequired;
    private bool isTransitionPending;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadStage(startingStage);
    }

    /// Called by ReportManager / CommentManager / AnalysisManager after each submission.
    public void NotifyPostHandled()
    {
        postsHandled++;
        Debug.Log($"[BlueTeamManager] Post handled ({postsHandled}/{postsRequired}) in Stage {CurrentStage}.");

        if (postsHandled >= postsRequired)
        {
            NotifyStageComplete();
        }    
    }

    public void NotifyStageComplete()
    {
        if (isTransitionPending) return;

        Debug.Log($"[BlueTeamManager] Stage {CurrentStage} complete.");

        int nextStage = CurrentStage + 1;

        if (nextStage > totalStages) // transisiton to red team
        {
            isTransitionPending = true;
            StartCoroutine(CompleteAllStagesWhenFeedbackCloses());
            return;
        }

        isTransitionPending = true;
        StartCoroutine(LoadNextStageWhenFeedbackCloses(nextStage));
    }

    private IEnumerator LoadNextStageWhenFeedbackCloses(int nextStage)
    {
        // Allow the submission flow to finish showing feedback in this frame.
        yield return null;

        while (FeedbackManager.Instance != null && FeedbackManager.Instance.IsPanelVisible())
        {
            yield return null;
        }

        isTransitionPending = false;
        LoadStage(nextStage);
    }

    private IEnumerator CompleteAllStagesWhenFeedbackCloses()
    {
        // Allow the submission flow to finish showing feedback in this frame.
        yield return null;

        while (FeedbackManager.Instance != null && FeedbackManager.Instance.IsPanelVisible())
        {
            yield return null;
        }

        if (finalStageTransitionDelay > 0f)
            yield return new WaitForSeconds(finalStageTransitionDelay);

        isTransitionPending = false;
        OnAllStagesComplete();
    }

    public void LoadStage(int stage)
    {
        CurrentStage = stage;
        postsHandled = 0;

        int idx = stage - 1;

        // get required posts for this stage, default to 1 if not set or out of bounds
        postsRequired = (requiredPostsPerStage != null && idx >= 0 && idx < requiredPostsPerStage.Length && requiredPostsPerStage[idx] > 0)
            ? requiredPostsPerStage[idx]
            : 1;

        Debug.Log($"[BlueTeamManager] Loading stage {stage}. Required posts: {postsRequired}.");

        // Show the context popup for this stage
        if (stageContextPanel != null)
        {
            int textIdx = stage - 1;
            string context = (stageContextTexts != null && textIdx >= 0 && textIdx < stageContextTexts.Length)
                ? stageContextTexts[textIdx]
                : string.Empty;
            stageContextPanel.Show(stage, context);
        }

        if (timelineManager != null)
        {
            timelineManager.GenerateFeed(stage);
        }
        else
        {
            Debug.LogError("[BlueTeamManager] TimelineManager reference is not assigned.");
        }
    }

    private void OnAllStagesComplete()
    {
        isTransitionPending = false;
        Debug.Log("[BlueTeamManager] All stages complete!");
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex); // Unload current scene
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[BlueTeamManager] No next scene configured.");
        }
    }
}