using UnityEngine;

/// Defines the type and slot of a module card
public enum ModuleType
{
    Greeting,   // How the target is addressed
    Context,    // Addional general text
    Hook,       // The lure of the message
    Claim,      // The false claim to make 
    Urgency,    // Time pressure/consequence
    Action,     // What we want the victim to do
    Link,       // The malicious URL/payload
    SocialProof,   // Fake verifications, followers etc
    Emoji,      // Emotional manupulation
    Authority,  // Impersonation signals (optional)
    Signature   // Closing text (name, titles, contact etc)
}

/// Represents a single phishing technique module "card" that can be dragged into the composer
[System.Serializable]
public class PhishingModule
{
    public string id; // Unique ID
    public string displayText; // Text shown on module card
    public string postText; // Actual text inserted into post
    public ModuleType type; 

    [Range(0f, 1f)]
    public float successModifier;   // How much the card increases success chance

    [Range(-1f, 1f)]
    public float suspicionModifier; // How much the card increases or decreases suspicion

    public int unlockLevel;
    public string techniqueName; // eg. Urgency, Link

    // Visual
    public Color cardColour = Color.white;
    public Sprite icon;
}