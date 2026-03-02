using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    /// <summary>
    /// This is a WIP and will not work until actual feedback UI and logic is implemented.
    /// The idea is to have a centralized manager that can be called from anywhere (like CommentManager or ReportManager) to display consistent feedback 
    /// to the player based on their results.
    /// This keeps the feedback logic separate and allows for easy updates to how feedback is presented
    /// without needing to change the core game logic.
    /// </summary>
    public static FeedbackManager Instance { get; private set; }

    public void ShowSuccess(string message)
    {
        // Implement your success feedback logic here (e.g., display a green checkmark and the message)
        Debug.Log("SUCCESS: " + message);
    }
    public void ShowFailure(string message)
    {
        // Implement your failure feedback logic here (e.g., display a red X and the message)
        Debug.Log("FAILURE: " + message);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
