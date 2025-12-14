using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// View del panel "Sounds". Sólo conoce UI y delega en el Presenter.
/// </summary>
public class SoundOptionsView : MonoBehaviour
{
    [Header("UI (0..1)")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private SoundOptionsPresenter _presenter;
    private bool _applyingInitialValues;
    private bool _started;

    [Inject] public void Construct(SoundOptionsPresenter presenter) => _presenter = presenter;

    private void Start()
    {
        _started = true;
        StartCoroutine(InitNextFrame()); // por si este GO se habilita en el mismo frame que los installers
    }

    private IEnumerator InitNextFrame()
    {
        yield return null; // espera 1 frame
        ApplySavedValues();
    }

    private void OnEnable()
    {
        if (_started) ApplySavedValues(); // al reabrir la solapa
        Subscribe();
    }

    private void OnDisable() => Unsubscribe();

    private void ApplySavedValues()
    {
        var vols = _presenter.GetCurrentVolumes();
        _masterSlider.SetValueWithoutNotify(vols.Master);
        _musicSlider.SetValueWithoutNotify(vols.Music);
        _sfxSlider.SetValueWithoutNotify(vols.SFX);
    }

    private void Subscribe()
    {
        _masterSlider.onValueChanged.AddListener(_presenter.SetMaster);
        _musicSlider.onValueChanged.AddListener(_presenter.SetMusic);
        _sfxSlider.onValueChanged.AddListener(_presenter.SetSfx);
    }

    private void Unsubscribe()
    {
        _masterSlider.onValueChanged.RemoveListener(_presenter.SetMaster);
        _musicSlider.onValueChanged.RemoveListener(_presenter.SetMusic);
        _sfxSlider.onValueChanged.RemoveListener(_presenter.SetSfx);
    }
}
