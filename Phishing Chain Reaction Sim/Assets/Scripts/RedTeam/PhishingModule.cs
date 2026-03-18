using UnityEngine;

/// Defines the type and slot of a module card
public enum ModuleType
{
    Greeting,   // How the target is addressed
    Hook,       // The lure of the message
    Pressure,    // Time pressure/consequence
    Authority,  // Impersonation signals
    SocialProof,   // Fake verifications, followers etc
    CallToAction,   // The CTA urging the target to act
    PayloadLink    // The malicious link or attachment
}

/// Represents a single phishing technique module "card" that can be dragged into the composer
[System.Serializable]
public class PhishingModule
{
    public string id; // Unique ID
    public string displayText; // Text shown on module card
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