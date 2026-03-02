using UnityEngine;

public enum PhishReason
{
    Safe,
    Pressure,
    SuspiciousLink,
    Impersonation
}

public enum PhishCampaignType
{
    None,
    RegularPhishing,
    EventRelatedPhishing
}

public enum PhishImpactType
{
    None,
    CredentialTheft,
    FinancialFraud,
    MalwareInfection,
    DataHarvesting,
}

[System.Serializable]
public class TimelinePostData
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
    public int severity = 0; // for stage 2 scoring, severity influences points gained based on order of comments. 

    [Header("Stage 3: Contextual Analysis")]
    public PhishCampaignType campaignType = PhishCampaignType.None;
    public PhishImpactType impactType = PhishImpactType.None;
    public string eventTag;
    [Range(0, 5)] public int analysisDifficulty = 0;
}
