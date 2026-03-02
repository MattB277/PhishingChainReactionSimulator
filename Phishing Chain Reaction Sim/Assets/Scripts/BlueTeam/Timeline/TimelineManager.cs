using UnityEngine;
using System.Collections.Generic;

public class TimelineManager : MonoBehaviour
{
    [Header("Data Source")]
    public TimelinePostDatabase database;

    [Header("UI References")]
    public GameObject postPrefab;
    public Transform contentParent;     // The Content object inside Scroll View

    // Stage driven by BlueTeamManager.LoadStage()
    // 
    public void GenerateFeed(int stage)
    {
        // Destroy any existing posts
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Fetch post data from scriptableObject
        List<TimelinePostData> stagePosts = database.GetPostsForStage(stage);

        if (stagePosts.Count == 0)
        {
            Debug.LogWarning($"No posts found for Stage {stage}.");
            return;
        }

        // Spawn GameObjects
        foreach (TimelinePostData data in stagePosts)
        {
            // Create physical object
            GameObject newPostObj = Instantiate(postPrefab, contentParent);

            // Find the controller script (TimelinePostInteraction.cs) instance on the new object
            TimelinePost interactionScript = newPostObj.GetComponent<TimelinePost>();

            // Inject the post data
            if (interactionScript != null)
            {
                interactionScript.SetupPost(data, stage);
            }
        }
    }

    /// <summary>
    /// Removes a post's GameObject from the feed after it has been reported/analysed.
    /// Called by stage managers after a submission is evaluated.
    /// </summary>
    public void RemovePost(TimelinePost post)
    {
        if (post != null && post.gameObject != null)
        {
            Destroy(post.gameObject);
        }
    }
}