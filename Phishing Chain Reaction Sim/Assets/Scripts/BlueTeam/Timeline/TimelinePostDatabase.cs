using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "TimelinePostDatabase", menuName = "BlueTeam/TimelinePostDatabase")]
public class TimelinePostDatabase : ScriptableObject
{
    public List<TimelinePostData> allPosts = new List<TimelinePostData>();

    // Return a randomised list of all posts for a given stage
    public List<TimelinePostData> GetPostsForStage(int stage)
    {
        return allPosts.Where(p => p.stage == stage).OrderBy(p => Random.value).ToList();
    }

}
