using UnityEngine;

public enum PhishReason
{
    Safe,
    Pressure,
    SuspiciousLink,
    Impersonation,
    Greed,
    EmotionalManipulation
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

public enum PhishAdvice
{
    None,
    CheckLinks,           // maps to PhishReason.SuspiciousLink
    TakeYourTime,         // maps to PhishReason.Pressure
    VerifyIdentity,       // maps to PhishReason.Impersonation
    QuestionDeals,        // maps to PhishReason.Greed
    StayObjective,        // maps to PhishReason.EmotionalManipulation
}

public static class PhishAdviceMapping
{
    /// Returns the correct advice for a given giveaway reason.
    public static PhishAdvice FromReason(PhishReason reason)
    {
        switch (reason)
        {
            case PhishReason.SuspiciousLink:        return PhishAdvice.CheckLinks;
            case PhishReason.Pressure:              return PhishAdvice.TakeYourTime;
            case PhishReason.Impersonation:         return PhishAdvice.VerifyIdentity;
            case PhishReason.Greed:                 return PhishAdvice.QuestionDeals;
            case PhishReason.EmotionalManipulation: return PhishAdvice.StayObjective;
            default:                                return PhishAdvice.None;
        }
    }
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
    public PhishImpactType impactType = PhishImpactType.None;
    public int stage;
    public int severity = 0; // for stage 2 scoring, severity influences points gained based on order of comments. 

    [Header("Stage 3: Contextual Analysis")]
    public PhishCampaignType campaignType = PhishCampaignType.None;
    public string eventTag;
    [Range(0, 5)] public int analysisDifficulty = 0;
}
