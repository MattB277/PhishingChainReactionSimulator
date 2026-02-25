using UnityEngine;
using System.Collections.Generic;

public class TimelineManager : MonoBehaviour
{
    [Header("Data Source")]
    public FeedPostDatabase database;

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
        List<TimelinePost> stagePosts = database.GetPostsForStage(stage);

        if (stagePosts.Count == 0)
        {
            Debug.LogWarning($"No posts found for Stage {stage}.");
            return;
        }

        // Spawn GameObjects
        foreach (TimelinePost data in stagePosts)
        {
            // Create physical object
            GameObject newPostObj = Instantiate(postPrefab, contentParent);

            // Find the controller script (TimelinePostInteraction.cs) instance on the new object
            FeedPostInteraction interactionScript = newPostObj.GetComponent<FeedPostInteraction>();

            // Inject the post data
            if (interactionScript != null)
            {
                interactionScript.SetupPost(data, stage);
            }
        }
    }
}