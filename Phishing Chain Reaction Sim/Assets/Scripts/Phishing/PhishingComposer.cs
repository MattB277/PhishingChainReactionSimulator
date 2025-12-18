using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PhishingComposer : MonoBehaviour
{
    [Header("Module References")]
    [SerializeField] private MessageDropZone dropZone;
    [SerializeField] private ModulePalette palette;
    
    [Header("Stat UI References")]
    [SerializeField] private Slider successSlider;
    [SerializeField] private Slider suspicionSlider;
    
    public void OnModulesChanged()
    {
        var currentModules = dropZone.GetCurrentModuleData();

        float successChance = Mathf.Clamp01(currentModules.Sum(m=> m.successModifier));
        float suspicionLevel = Mathf.Clamp01(currentModules.Sum(m=> m.suspicionModifier));

        successSlider.value = successChance;
        suspicionSlider.value = suspicionLevel;
    }

    public void ClearComposer()
    {
        dropZone.ClearAllModules();
        OnModulesChanged(); // recalculate sliders
    }
    
    public void SendPost()
    {
        // NOT YET IMPLEMENTED
        Debug.Log("SEND MESSAGE CLICKED");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
