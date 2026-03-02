using UnityEngine;

/// <summary>
/// Central controller for Blue Team stage progression.
/// Stage managers (ReportManager, CommentManager, AnalysisManager) call
/// NotifyStageComplete() when the player finishes their current intervention.
/// </summary>
public class BlueTeamManager : MonoBehaviour
{
    public static BlueTeamManager Instance { get; private set; }

    [Header("References")]
    public TimelineManager timelineManager;

    [Header("Stage Settings")]
    [Tooltip("The stage loaded on Start. Change in Inspector for testing.")]
    public int startingStage = 1; // for testing individual stages
    public int totalStages = 3;

    public int CurrentStage { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadStage(startingStage);
    }


    // Called by any stage manager when the player has completed the current stage.
    /// Advances to the next stage or ends the session.
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

    /// Loads a specific stage: updates CurrentStage and refreshes the timeline.
    /// No need to let stage managers know the stage.
    public void LoadStage(int stage)
    {
        CurrentStage = stage;
        Debug.Log($"[BlueTeamManager] Loading stage {stage}.");

        // Regenerate the feed for the new stage
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
        // TODO: Show final results screen, summary, or transition out
    }
}