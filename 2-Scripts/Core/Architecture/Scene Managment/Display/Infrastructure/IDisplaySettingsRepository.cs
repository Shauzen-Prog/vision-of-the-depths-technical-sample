using UnityEngine;

/// <summary>
/// Abstract persistence boundary.
/// </summary>
public interface IDisplaySettingsRepository
{
    bool Exists();
    bool TryLoad(out DisplaySettingsDTO dto);
    void Save(DisplaySettingsDTO dto);
}
