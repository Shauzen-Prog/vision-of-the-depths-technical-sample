using System;
using UnityEngine;

public interface IDisplaySettingsService 
{
    float Gamma { get; }
    bool isCalibrated { get; }

    /// <summary> Lo levanta cuando la snapshot ingame gambia </summary>
    event Action<DisplaySettingsDTO> OnChanged;
    
    void SetGamma(float value);
    void MarkCalibrated();

    /// <summary> Con explicit saves por si después no hay autosaves </summary>
    void SaveNow();
    void ConfigureAutosave(bool enabled, float delaySeconds = 0.25f);
}
