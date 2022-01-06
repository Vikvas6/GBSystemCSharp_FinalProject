using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class SolarBlink : MonoBehaviour
{
    [SerializeField] private Material _material;
    private Color _emissionColor;

    private float _currentCoeff = 1.0f;
    private float _currentDirect = 1;

    private float _highRange = 2.0f;
    private float _lowRange = 0.3f;
    private float _step = 0.1f;
    private float _frequency = 1.0f;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Start()
    {
        _emissionColor = _material.GetColor(EmissionColor);
        StartCoroutine(nameof(Blink));
    }

    IEnumerator Blink()
    {
        while (true)
        {
            _material.SetColor(EmissionColor, _emissionColor * _currentCoeff);
            
            if (_currentCoeff >= _highRange || _currentCoeff <= _lowRange)
            {
                _currentDirect *= -1;
            }

            _currentCoeff += _step * _currentDirect;
            yield return new WaitForSeconds(_frequency);
        }
    }

    private void OnDestroy()
    {
        _material.SetColor(EmissionColor, _emissionColor);
    }
}
