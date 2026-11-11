using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 월드 스페이스에 안내 글자를 띄우는 표시 담당 클래스. 규칙(언제 보여주고 숨길지)은 모른다.
// 나타날 때: 글자가 한 글자씩 차례로 아래에서 떠오르며 선명해지고, 그 자리에서 빛 가루가 생긴다.
// 사라질 때: 한 글자씩 차례로 흐려지고, 그 자리에서 빛 가루가 아래로 떨어진다.
public class TutorialHint : MonoBehaviour
{
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private ParticleSystem _dust; // 글자 위치에 직접 생성하는 빛 가루 (Emission은 꺼두고 코드로만 생성)

    [Header("나타날 때")]
    [SerializeField] private float _showStagger = 0.02f;   // 글자 사이 시작 간격
    [SerializeField] private float _showCharTime = 0.3f;   // 글자 하나가 나타나는 데 걸리는 시간
    [SerializeField] private float _riseDistance = 0.25f;  // 아래에서 떠오르는 거리

    [Header("사라질 때")]
    [SerializeField] private float _hideStagger = 0.012f;
    [SerializeField] private float _hideCharTime = 0.25f;
    [SerializeField] private float _hideRise = 0.12f;      // 사라지면서 위로 살짝 떠오르는 거리

    [Header("나타나 있는 동안")]
    [SerializeField] private float _idleAmplitude = 0.04f;  // 글자가 위아래로 흔들리는 크기
    [SerializeField] private float _idleSpeed = 2f;         // 흔들리는 속도
    [SerializeField] private float _idlePhase = 0.35f;      // 글자마다 시작 시점이 달라지는 정도 (물결 모양)
    [SerializeField] private Vector2 _dripInterval = new Vector2(0.15f, 0.4f); // 빛 가루가 한 알씩 떨어지는 간격(초)

    [Header("조건 달성 연출")]
    [SerializeField] private Color _successColor = Color.white; // 글자가 순간 이 색으로 밝아진다
    [SerializeField] private float _successStagger = 0.012f;
    [SerializeField] private float _successCharTime = 0.3f;
    [SerializeField] private int _successDustPerChar = 2;

    [Header("빛 가루")]
    [SerializeField] private int _dustPerChar = 1;
    [SerializeField] private float _dustSpread = 0.15f;

    private TMP_TextInfo _info;
    private Vector3[][] _baseVertices;       // 메시별 원본 정점 (글자를 움직여도 기준이 되는 위치)
    private readonly List<int> _visible = new List<int>(); // 실제로 그려지는 글자 번호 (공백 제외)
    private bool[] _emitted;
    private int _token; // 새 연출이 시작되면 이전 연출이 스스로 멈추도록 하는 번호 (나타나는 도중에 숨기기 등이 겹쳐도 안전)

    public IEnumerator ShowRoutine(string text)
    {
        int token = ++_token;
        Prepare(text);
        yield return Animate(_showStagger, _showCharTime, true, token);
        if (token == _token && _visible.Count > 0) StartCoroutine(IdleRoutine(token)); // 다 나타나면 살짝 움직이며 빛 가루를 흘린다
    }

    public IEnumerator HideRoutine()
    {
        if (_info == null || _visible.Count == 0) yield break;

        int token = ++_token;
        yield return Animate(_hideStagger, _hideCharTime, false, token);
        if (token != _token) yield break; // 그 사이에 다른 연출이 시작됐다

        _text.text = string.Empty;
        _info = null;
    }

    // 글자가 다 나타난 뒤부터 사라지기 전까지: 물결치듯 살짝 움직이고, 가끔 빛 가루가 떨어진다
    private IEnumerator IdleRoutine(int token)
    {
        float nextDrip = Random.Range(_dripInterval.x, _dripInterval.y);
        float time = 0f;

        while (token == _token && _info != null)
        {
            time += Time.deltaTime;

            for (int n = 0; n < _visible.Count; n++)
            {
                TMP_CharacterInfo ch = _info.characterInfo[_visible[n]];
                float y = Mathf.Sin(time * _idleSpeed + n * _idlePhase) * _idleAmplitude;
                Vector3[] vertices = _info.meshInfo[ch.materialReferenceIndex].vertices;
                Vector3[] baseVertices = _baseVertices[ch.materialReferenceIndex];
                for (int k = 0; k < 4; k++)
                {
                    vertices[ch.vertexIndex + k] = baseVertices[ch.vertexIndex + k] + new Vector3(0f, y, 0f);
                }
            }
            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            nextDrip -= Time.deltaTime;
            if (nextDrip <= 0f)
            {
                nextDrip = Random.Range(_dripInterval.x, _dripInterval.y);
                EmitDust(_info.characterInfo[_visible[Random.Range(0, _visible.Count)]]);
            }

            yield return null;
        }
    }

    // 조건을 달성했을 때: 글자가 왼쪽부터 차례로 밝아졌다 돌아오며 빛 가루가 터진다
    public IEnumerator SuccessRoutine()
    {
        if (_info == null || _visible.Count == 0) yield break;

        int token = _token; // 다른 연출(숨기기 등)이 시작되면 스스로 멈춘다
        float total = (_visible.Count - 1) * _successStagger + _successCharTime;
        float elapsed = 0f;
        bool[] burst = new bool[_visible.Count];

        while (elapsed < total)
        {
            if (token != _token) yield break;
            ApplySuccess(elapsed, burst);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (token != _token) yield break;
        ApplySuccess(total, burst); // 마지막은 원래 색으로 되돌린다
    }

    private void ApplySuccess(float elapsed, bool[] burst)
    {
        Color baseColor = _text.color;
        for (int n = 0; n < _visible.Count; n++)
        {
            TMP_CharacterInfo ch = _info.characterInfo[_visible[n]];
            float start = n * _successStagger;
            float t = Mathf.Clamp01((elapsed - start) / _successCharTime);
            float pulse = Mathf.Sin(t * Mathf.PI); // 0 -> 1 -> 0

            Color32 color = Color.Lerp(baseColor, _successColor, pulse);
            Color32[] colors = _info.meshInfo[ch.materialReferenceIndex].colors32;
            for (int k = 0; k < 4; k++) colors[ch.vertexIndex + k] = color;

            if (!burst[n] && elapsed >= start)
            {
                burst[n] = true;
                EmitBurst(ch);
            }
        }
        _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private void EmitBurst(TMP_CharacterInfo ch)
    {
        if (_dust == null) return;

        Vector3 center = _text.transform.TransformPoint((ch.bottomLeft + ch.topRight) * 0.5f);
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        for (int i = 0; i < _successDustPerChar; i++)
        {
            emitParams.position = center + (Vector3)(Random.insideUnitCircle * 0.1f);
            emitParams.velocity = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.2f, 0.9f), 0f);
            _dust.Emit(emitParams, 1);
        }
    }

    private void Prepare(string text)
    {
        _text.text = text;
        _text.ForceMeshUpdate();
        _info = _text.textInfo;

        _baseVertices = new Vector3[_info.meshInfo.Length][];
        for (int i = 0; i < _info.meshInfo.Length; i++)
        {
            _baseVertices[i] = (Vector3[])_info.meshInfo[i].vertices.Clone();
        }

        _visible.Clear();
        for (int c = 0; c < _info.characterCount; c++)
        {
            if (_info.characterInfo[c].isVisible) _visible.Add(c);
        }
        _emitted = new bool[_visible.Count];
    }

    private IEnumerator Animate(float stagger, float charTime, bool showing, int token)
    {
        float total = (_visible.Count - 1) * stagger + charTime;
        float elapsed = 0f;

        while (elapsed < total)
        {
            if (token != _token) yield break;

            Apply(elapsed, stagger, charTime, showing);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (token != _token) yield break;
        Apply(total, stagger, charTime, showing); // 마지막 프레임은 완료 상태로 맞춘다
    }

    private void Apply(float elapsed, float stagger, float charTime, bool showing)
    {
        for (int n = 0; n < _visible.Count; n++)
        {
            TMP_CharacterInfo ch = _info.characterInfo[_visible[n]];
            float start = n * stagger;
            float t = Mathf.Clamp01((elapsed - start) / charTime);
            float eased = t * t * (3f - 2f * t); // smoothstep

            float alpha = showing ? eased : 1f - eased;
            float yOffset = showing ? -(1f - eased) * _riseDistance : eased * _hideRise;

            int mesh = ch.materialReferenceIndex;
            int vertex = ch.vertexIndex;
            Color32[] colors = _info.meshInfo[mesh].colors32;
            Vector3[] vertices = _info.meshInfo[mesh].vertices;
            Vector3[] baseVertices = _baseVertices[mesh];
            byte a = (byte)(alpha * 255f);

            for (int k = 0; k < 4; k++)
            {
                colors[vertex + k].a = a;
                vertices[vertex + k] = baseVertices[vertex + k] + new Vector3(0f, yOffset, 0f);
            }

            // 이 글자의 차례가 되는 순간 한 번만 빛 가루를 만든다
            if (!_emitted[n] && elapsed >= start)
            {
                _emitted[n] = true;
                EmitDust(ch);
            }
        }

        _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32 | TMP_VertexDataUpdateFlags.Vertices);
    }

    private void EmitDust(TMP_CharacterInfo ch)
    {
        if (_dust == null) return;

        Vector3 center = _text.transform.TransformPoint((ch.bottomLeft + ch.topRight) * 0.5f);
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        for (int i = 0; i < _dustPerChar; i++)
        {
            emitParams.position = center + (Vector3)(Random.insideUnitCircle * _dustSpread);
            emitParams.velocity = new Vector3(Random.Range(-0.2f, 0.2f), 0f, 0f); // 아래로 떨어지는 건 Gravity Modifier
            _dust.Emit(emitParams, 1);
        }
    }

    private void OnDisable()
    {
        // 숨기는 도중 꺼지면 글자가 반쯤 보이는 채로 남지 않도록 정리한다
        if (_text != null) _text.text = string.Empty;
        _info = null;
    }
}
