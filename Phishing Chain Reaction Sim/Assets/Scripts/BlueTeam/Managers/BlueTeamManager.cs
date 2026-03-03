using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Stage Settings")]
    [Tooltip("The stage loaded on Start. Change in Inspector for testing.")]
    public int startingStage = 1;
    public int totalStages = 3;

    [Header("Progression")]
    [Tooltip("Posts the player must handle per stage (index 0 is Stage 1). Defaults to 1 if unset.")]
    public int[] requiredPostsPerStage = new int[] { 3, 3, 3 };

    [Header("Scene Transition")]
    [Tooltip("Scene to load when all stages are complete. Must be added to Build Settings.")]
    public string nextSceneName;

    public int CurrentStage { get; private set; }
    private int postsHandled;
    private int postsRequired;

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
        Debug.Log($"[BlueTeamManager] Stage {CurrentStage} complete.");

        int nextStage = CurrentStage + 1;

        if (nextStage > totalStages)
        {
            OnAllStagesComplete();
            return;
        }

        LoadStage(nextStage);
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