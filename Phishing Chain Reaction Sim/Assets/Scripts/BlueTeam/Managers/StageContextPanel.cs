using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a brief context / briefing popup at the start of each Blue Team stage.
/// BlueTeamManager calls Show(contextText) in LoadStage().
/// The player dismisses it via the close button before interacting with the feed.
/// </summary>
public class StageContextPanel : MonoBehaviour
{
    public static StageContextPanel Instance { get; private set; }

    [Header("UI References")]
    public GameObject panelRoot;
    public TextMeshProUGUI stageLabel;
    public TextMeshProUGUI contextText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (panelRoot != null) panelRoot.SetActive(false);
    }

    /// <summary>Show the context popup for the given stage number and context string.</summary>
    public void Show(int stage, string context)
    {
        if (panelRoot == null)
        {
            Debug.LogWarning("[StageContextPanel] panelRoot is not assigned.");
            return;
        }

        if (stageLabel != null)
            stageLabel.text = $"Stage {stage}";

        if (contextText != null)
            contextText.text = context;

        panelRoot.SetActive(true);
    }

    /// <summary>Called by the panel's close / continue button.</summary>
    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}
