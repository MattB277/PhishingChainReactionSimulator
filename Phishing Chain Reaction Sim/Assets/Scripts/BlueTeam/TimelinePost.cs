using UnityEngine;

public enum PhishReason
{
    Safe,
    Pressure,
    SuspiciousLink,
    Impersonation
}

[System.Serializable]
public class TimelinePost
{
    public string id;

    [Header("Profile Info")]
    public string displayName;
    public string handle;
    public Sprite avatar;

    [Header("Content")]
    [TextArea(3,10)] public string postBody;
    public string timestamp;

    [Header("Defence Logic")]
    public bool isPhish;
    public PhishReason correctReason;
    public int stage;
}
