using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "FeedPostDatabase", menuName = "BlueTeam/FeedPostDatabase")]
public class FeedPostDatabase : ScriptableObject
{
    public List<TimelinePost> allPosts = new List<TimelinePost>();

    // Return a randomised list of all posts for a given stage
    public List<TimelinePost> GetPostsForStage(int stage)
    {
        return allPosts.Where(p => p.stage == stage).OrderBy(p => Random.value).ToList();
    }

}
