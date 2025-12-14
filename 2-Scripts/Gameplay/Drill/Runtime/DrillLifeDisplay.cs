using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillLifeDisplay : MonoBehaviour
{
    [System.Serializable]
    public class LifeSegment
    {
        public GameObject segmentObject;
        public Material onMaterial;
        public Material offMaterial;

        [HideInInspector] public Renderer cachedRenderer;

        public void CacheRenderer()
        {
            if (segmentObject != null)
                cachedRenderer = segmentObject.GetComponent<Renderer>();
        }
    }

    [Header("Segmentos de vida (de izquierda a derecha)")]
    public LifeSegment[] lifeSegments;

    [Header("Configuración de vida")]
    public int maxLife = 500;
    public int lifePerSegment = 100;

    [Header("Parpadeo")]
    public float blinkDuration = 1f;
    public float blinkInterval = 0.15f;

    private int _currentActiveSegments;
    private Coroutine _globalBlinkCoroutine;
    private bool _isBlinkingAll = false;

    private void Awake()
    {
        foreach (var segment in lifeSegments)
        {
            segment.CacheRenderer();

            if (segment.cachedRenderer != null && segment.onMaterial != null)
                segment.cachedRenderer.material = segment.onMaterial;
        }

        _currentActiveSegments = lifeSegments.Length;
    }

    public void UpdateLifeDisplay(int currentLife)
    {
        int segmentsToShow = Mathf.Clamp(currentLife / lifePerSegment, 0, lifeSegments.Length);
        
        // TODO: Hay que probar esto en build por que es lo que se rompe aparentemente
        Debug.Log(currentLife);

        // ⚠️ Activar parpadeo global si la vida baja de 100
        if (currentLife < lifePerSegment)
        {
            if (!_isBlinkingAll)
            {
                _globalBlinkCoroutine = StartCoroutine(BlinkAllSegments());
                _isBlinkingAll = true;
            }
            return; // no apagar más segmentos
        }
        else if (_isBlinkingAll)
        {
            // 🔄 Si la vida volvió a 100 o más, restaurar todos y detener parpadeo
            StopCoroutine(_globalBlinkCoroutine);
            ResetAllToOn();
            _isBlinkingAll = false;
        }

        // 🔽 Si vida bajó normalmente
        while (_currentActiveSegments > segmentsToShow)
        {
            int indexToBlink = _currentActiveSegments - 1;
            StartCoroutine(BlinkAndTurnOff(indexToBlink));
            _currentActiveSegments--;
        }
    }

    private IEnumerator BlinkAndTurnOff(int index)
    {
        if (index < 0 || index >= lifeSegments.Length)
            yield break;

        var segment = lifeSegments[index];
        var rend = segment.cachedRenderer;

        if (rend == null)
            yield break;

        float elapsed = 0f;
        bool on = false;

        while (elapsed < blinkDuration)
        {
            rend.material = on ? segment.onMaterial : segment.offMaterial;
            on = !on;
            elapsed += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        rend.material = segment.offMaterial;
    }

    private IEnumerator BlinkAllSegments()
    {
        bool on = true;

        while (true)
        {
            for (int i = 0; i < _currentActiveSegments; i++)
            {
                if (lifeSegments[i].cachedRenderer != null)
                {
                    lifeSegments[i].cachedRenderer.material = on
                        ? lifeSegments[i].onMaterial
                        : lifeSegments[i].offMaterial;
                }
            }

            on = !on;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void ResetAllToOn()
    {
        for (int i = 0; i < _currentActiveSegments; i++)
        {
            if (lifeSegments[i].cachedRenderer != null)
                lifeSegments[i].cachedRenderer.material = lifeSegments[i].onMaterial;
        }
    }
}
