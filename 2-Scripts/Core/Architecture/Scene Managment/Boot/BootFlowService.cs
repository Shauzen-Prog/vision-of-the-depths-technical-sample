using System;

public sealed class BootFlowService : IBootFlowService
{
    private readonly IDisplaySettingsRepository _repo;
    private readonly ISceneRouter _router;

    /// <summary>
    /// 
    /// </summary>
    public BootFlowService(IDisplaySettingsRepository repo, ISceneRouter router)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _router = router ?? throw new ArgumentNullException(nameof(router));
    }
    
    /// <summary>
    ///  Evalua el JSON y decide la escena de destino
    /// </summary>
    public void DecideAndRoute()
    {
#if UNITY_EDITOR
        return; // En Editor no navegamos automáticamente
#else
        if (!_repo.TryLoad(out var dto) || dto == null || !dto.calibrated)
        {
            _router.GoTo(GameScenes.GammaCalibration);
            return;
        }
        _router.GoTo(GameScenes.MainMenu);
#endif
    }
}
