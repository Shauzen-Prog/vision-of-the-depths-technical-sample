using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScrollerSubState : IDrillSubState, IPausableSubState
{
    private readonly TextureScroller _textureScroller;
    private readonly MaterialScrollConfigListSO _materialConfigList;
    private readonly NewMaterialManager _materialManager;

    private IEventBus _eventBus;
    
    private TypeOfMaterialToDrill _currentMaterialType;
    
    
    private bool _isPaused;
    
    public TextureScrollerSubState(TextureScroller scroller, MaterialScrollConfigListSO configList, 
        NewMaterialManager materialManager)
    {
        _textureScroller = scroller;
        _materialConfigList = configList;
        _materialManager = materialManager;
    }

    /// <summary>
    /// Llamar cuando spawnea un material nuevo.
    /// </summary>
    public void OnMaterialSpawned(TypeOfMaterialToDrill type)
    {
        if(_materialManager.CurrentMaterial == null) return;
        
        _textureScroller.StartShake(_materialManager.CurrentMaterial.typeOfMaterialToDrill);

        if (_materialManager.NextMaterial.texture != null && _materialManager.CurrentMaterial != null)
        {
            _textureScroller?.SetupMaterials(_materialManager.CurrentMaterial.texture, 
                _materialManager.NextMaterial.texture);
        }
    }

    /// <summary>
    /// Llamar cuando se destruye el material actual. Idealmente pasale el tipo del que sigue.
    /// </summary>
    public void OnMaterialDestroyed(Texture nextTypeTexture)
    {
        if(_materialManager.NextMaterial == null) return;
        
        _textureScroller.TransitionToNext(nextTypeTexture);
        //_textureScroller.TransitionToNext(_materialManager.NextMaterial != null ? _materialManager.NextMaterial.typeOfMaterialToDrill : default);
    }

    /// <summary>
    /// Llamar cada tick para ajustar velocidad dinámica del scroll según RequirementStatus.
    /// </summary>
    public void UpdateScrollSpeed(TypeOfMaterialToDrill type)
    {
        var config = _materialConfigList.GetConfig(type);

        if (config == null)
        {
            Debug.LogWarning($"[TextureScrollerSubState] No se encontró config para {type}");
            return;
        }

        _textureScroller.SetScrollSpeed(config.scrollSpeed);
    }

    public void OnEnter() { }
    public void OnExit() {  }
    
    public void InjectEventBus(IEventBus eventBus)
    {
        _eventBus = eventBus;    
    }

    public void Pause()
    {
        _textureScroller.Pause();
    }

    public void Resume()
    {
        _textureScroller.Resume();
    }
}
