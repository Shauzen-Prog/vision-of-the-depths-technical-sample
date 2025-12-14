using System;
using UnityEngine;
using Zenject;

public sealed class DisplaySettingsService : IDisplaySettingsService, ITickable, IInitializable, IDisposable
{
    private readonly IDisplaySettingsRepository _repo;
    
    private DisplaySettingsDTO _current = new DisplaySettingsDTO();

    private bool _useDebounceAutoSave = true;
    
    /// <summary>
    /// El debounce se usa para que no guarde en cada interacion del player el número en disco.
    /// Porque puede hacer que se crashee.
    /// </summary>
    private float _debounceDelay = 0.25f;
    private bool _pendingSave;
    private float _saveAt;
    
    public event Action<DisplaySettingsDTO> OnChanged;
    
    public float Gamma => _current.gamma;
    public bool isCalibrated => _current.calibrated;

    public DisplaySettingsService(IDisplaySettingsRepository repo)
    {
        _repo = repo;
    }
    
    /// <summary> Carga el repocitorio en memoria en el startup </summary>
    public void Initialize()
    {
        if(_repo.TryLoad(out var dto)) _current = dto;
        else _current = new DisplaySettingsDTO();
        
        OnChanged?.Invoke(_current);
    }
    
    /// <summary>
    /// Intenta flush final si quedó un guardado pendiente.
    /// </summary>
    public void Dispose()
    {
        if(_pendingSave) SaveNow();
    }
    /// <summary> Setea el Gamma (0.5 .. 2.5), notifica, y agenda persistencia</summary>
    public void SetGamma(float value)
    {
        var clamped = Mathf.Clamp(value, 0.5f, 2.5f);
        if(Mathf.Approximately(clamped, _current.gamma)) return;
        
        _current.gamma = clamped;
        MarkDirty();
    }

    /// <summary>Marca la configuracion como calibrada, notifica, y agenda persistencia.</summary>
    public void MarkCalibrated()
    {
        if(_current.calibrated) return;
        _current.calibrated = true;
        MarkDirty();
    }

    /// <summary>Persistencia inmediata (Bypasea el debounce)</summary>
    public void SaveNow()
    {
        _repo.Save(_current);
        _pendingSave = false;
    }

    /// <summary>Enable/Disable del debounced autosave y configuracion con delay</summary>
    public void ConfigureAutosave(bool enabled, float delaySeconds = 0.25f)
    {
        _useDebounceAutoSave = enabled;
        _debounceDelay = Mathf.Max(0.05f, delaySeconds);
    }

    public void Tick()
    {
        if(!_useDebounceAutoSave || ! _pendingSave) return;
        if(Time.realtimeSinceStartup >= _saveAt)
            SaveNow();
    }
    
    /// <summary>
    /// Notifica a los listener y agenda persistencia (inmediata o debounced)
    /// </summary>
    private void MarkDirty()
    {
        OnChanged?.Invoke(_current);

        if (_useDebounceAutoSave)
        {
            _pendingSave = true;
            _saveAt = Time.realtimeSinceStartup + _debounceDelay;
        }
        else
        {
            SaveNow();
        }
    }


}
