using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Centralised feedback panel. Stage managers call ShowSuccess / ShowFailure
/// to display a brief result message after each post submission.
/// Attached to a UI panel that contains an icon, message text, and close button.
/// </summary>
public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI messageText;

    [Header("Display Text")]
    public string successHeader = "Success!";
    public string failureHeader = "Incorrect";

    [Header("Colours")]
    public Color successColour = new Color(0.2f, 0.8f, 0.2f, 0.23f); // green
    public Color failureColour = new Color(0.9f, 0.25f, 0.25f, 0.23f); // red

    [Header("Timing")]
    [Tooltip("Seconds before the panel auto-hides. Set to 0 to require manual close.")]
    public float displayDuration = 3f;

    private Coroutine hideRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    public void ShowSuccess(string message)
    {
        Show(successHeader, message, successColour);
    }

    public void ShowFailure(string message)
    {
        Show(failureHeader, message, failureColour);
    }

    /// Called from the panel's close button.
    public void HidePanel()
    {
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    private void Show(string header, string message, Color colour)
    {
        if (feedbackPanel == null || messageText == null)
        {
            Debug.LogWarning($"[FeedbackManager] UI references missing. Message: {message}");
            return;
        }

        // Stop any pending auto-hide
        if (hideRoutine != null) StopCoroutine(hideRoutine);

        if (headerText != null)
            headerText.text = header;

        messageText.text = message;
        feedbackPanel.GetComponent<Image>().color = colour;

        feedbackPanel.SetActive(true);

        if (displayDuration > 0f)
            hideRoutine = StartCoroutine(AutoHide());
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(displayDuration);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }
}
