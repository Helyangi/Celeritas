using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 메뉴 버튼에 마우스를 올리면 살짝 커지면서 테두리와 글자가 밝게 빛난다.
/// 게임 안의 "빛이 닿으면 밝아진다"(LightSensor) 규칙을 메뉴에 옮겨온 것으로, 마우스를 빛처럼 다룬다.
/// 버튼 전체를 밝히는 대신 테두리(발광용 Image)와 글자(TMP 글로우)만 세진다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MenuButtonGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("커지기")]
    [SerializeField] private float _hoverScale = 1.05f;
    [SerializeField] private float _speed = 12f; // 클수록 빠르게 반응한다

    [Header("테두리 발광")]
    [Tooltip("버튼과 같은 스프라이트를 쓰는 발광용 Image (머티리얼은 Celeritas/UIGlow)")]
    [SerializeField] private Image _glow;
    [SerializeField] private float _glowIntensity = 3f;
    [SerializeField] private Color _glowColor = new Color(1f, 0.95f, 0.75f, 1f);

    [Header("누름")]
    [SerializeField] private float _pressScale = 0.98f; // 누르는 동안의 크기 (호버 크기에서 쑥 들어간다)
    [SerializeField] private float _pressSpeed = 25f;   // 들어가고 나오는 속도

    [Header("클릭 번쩍임")]
    [SerializeField] private float _flashGlowPower = 1.2f; // 번쩍일 때 글자 글로우 세기
    [SerializeField] private float _flashDuration = 0.25f; // 사그라드는 시간

    [Header("글자 발광")]
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private float _labelGlowPower = 0.5f; // 호버했을 때의 글로우 세기 (평소에는 0)
    [SerializeField] private Color _labelGlowColor = new Color(1f, 0.95f, 0.75f, 0.6f);
    [Tooltip("호버/클릭 때 글자 자체를 얼마나 밝힐지 (1보다 크면 HDR이라 Bloom이 반응한다)")]
    [SerializeField] private float _labelFaceBoost = 1.6f;

    [Header("소리")]
    [SerializeField] private bool _playSound = true;
    [SerializeField] private SFXType _hoverSfx = SFXType.UIHover;
    [SerializeField] private SFXType _clickSfx = SFXType.UIClick;

    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private static readonly int GlowPowerId = Shader.PropertyToID("_GlowPower");
    private static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
    private static readonly int FaceColorId = Shader.PropertyToID("_FaceColor");

    private UnityEngine.UI.Button _button; // 눌리지 않는 버튼이면 연출도 쉬어야 한다
    private RectTransform _rect;
    private Vector3 _baseScale;
    private Vector3 _baseGlowScale;
    private Material _glowMaterial;  // 복제본 (원본 .mat과 다른 버튼에 영향을 주지 않는다)
    private Material _labelMaterial; // TMP가 만들어주는 복제본
    private Color _baseFaceColor;    // 평소 글자 색 (여기에 배율을 곱해 밝힌다)
    private bool _hovered;
    private float _t; // 0 = 평소, 1 = 완전히 호버된 상태
    private bool _pressed;
    private float _press;  // 0 = 안 눌림, 1 = 눌림
    private float _flash;  // 1에서 0으로 줄어드는 번쩍임

    private void Awake()
    {
        _button = GetComponent<UnityEngine.UI.Button>();
        _rect = (RectTransform)transform;
        _baseScale = _rect.localScale;

        if (_glow != null)
        {
            _glowMaterial = _glow.material; // Image.material은 복제본을 돌려준다
            _glowMaterial.SetFloat(IntensityId, _glowIntensity);
            _glow.color = new Color(_glowColor.r, _glowColor.g, _glowColor.b, 0f);
            _glow.raycastTarget = false; // 발광이 클릭을 가로채지 않게
            _baseGlowScale = _glow.rectTransform.localScale;
        }

        if (_label != null)
        {
            _labelMaterial = _label.fontMaterial; // 이 글자만의 복제본
            _labelMaterial.SetColor(GlowColorId, _labelGlowColor);
            _baseFaceColor = _labelMaterial.HasProperty(FaceColorId) ? _labelMaterial.GetColor(FaceColorId) : Color.white;
        }

        Apply(0f);
    }

    private void OnDisable()
    {
        // 비활성화되는 사이 마우스가 빠져나가면 호버 해제를 못 받으므로 평소 상태로 되돌린다
        _hovered = false;
        _pressed = false;
        _t = 0f;
        _press = 0f;
        _flash = 0f;
        Apply(0f);
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime; // 메뉴에서는 timeScale의 영향을 받지 않아야 한다

        float hoverTarget = _hovered ? 1f : 0f;
        float pressTarget = _pressed ? 1f : 0f;

        bool changed = false;
        if (!Mathf.Approximately(_t, hoverTarget)) { _t = Mathf.MoveTowards(_t, hoverTarget, _speed * dt); changed = true; }
        if (!Mathf.Approximately(_press, pressTarget)) { _press = Mathf.MoveTowards(_press, pressTarget, _pressSpeed * dt); changed = true; }
        if (_flash > 0f) { _flash = Mathf.MoveTowards(_flash, 0f, dt / Mathf.Max(0.01f, _flashDuration)); changed = true; }

        if (changed) Apply(_t);
    }

    private void Apply(float t)
    {
        float k = t * t * (3f - 2f * t); // 가장자리에서 부드럽게 (smoothstep)

        // 호버로 커진 크기에서 누르는 만큼 다시 들어간다
        float scale = Mathf.Lerp(1f, _hoverScale, k) * Mathf.Lerp(1f, _pressScale, _press);
        _rect.localScale = _baseScale * scale;

        if (_glow != null)
        {
            Color c = _glowColor;
            c.a = _glowColor.a * k;
            _glow.color = c;

            // 발광은 버튼의 자식이 아니라 뒤에 깔린 형제라서, 커지는 것도 따로 맞춰준다
            _glow.rectTransform.localScale = _baseGlowScale * Mathf.Lerp(1f, _hoverScale, k);
        }

        if (_labelMaterial != null)
        {
            // 호버 글로우 위에 번쩍임을 더한다 (둘 중 큰 쪽)
            float glow = Mathf.Max(_labelGlowPower * k, _flashGlowPower * _flash);
            _labelMaterial.SetFloat(GlowPowerId, glow);

            // 글자 자체도 조금 밝힌다. 1을 넘으면 HDR이라 Bloom이 받아준다
            float boost = Mathf.Lerp(1f, _labelFaceBoost, Mathf.Max(k, _flash));
            Color face = _baseFaceColor * boost;
            face.a = _baseFaceColor.a;
            _labelMaterial.SetColor(FaceColorId, face);
        }

    }

    // 버튼이 없으면 항상 반응하고, 있으면 누를 수 있을 때만 반응한다
    private bool Interactable => _button == null || _button.interactable;

    // 메뉴를 떠나는 중이면 상태를 그대로 얼린다.
    // 가림막이 입력을 막으면서 '마우스 벗어남'이 오는데, 그걸 받으면 클릭한 버튼의 호버가 풀려버린다.
    private bool Frozen => MenuFade.Instance != null && MenuFade.Instance.IsLeaving;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Frozen) return;
        if (!Interactable) return;

        _hovered = true;
        PlaySound(_hoverSfx);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Frozen) return;
        _hovered = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Frozen) return;
        if (!Interactable) return;

        _pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Frozen) return;
        _pressed = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 여기서는 얼리기를 검사하지 않는다.
        // 같은 오브젝트의 Button이 먼저 클릭을 처리하면서 씬 이동(떠나기)을 시작하므로,
        // 검사하면 씬을 넘기는 버튼만 소리와 번쩍임이 사라진다.
        // 떠나기 시작하면 가림막이 입력을 막으니, 이때 들어오는 클릭은 떠나기를 일으킨 그 클릭뿐이다.
        if (!Interactable) return;

        PlaySound(_clickSfx);
        _flash = 1f; // 떼는 순간 번쩍인다
    }

    private void PlaySound(SFXType type)
    {
        if (!_playSound || SoundManager.Instance == null) return;

        // SFX는 전부 2D(spatialBlend 0)로 재생되므로 위치는 의미가 없다
        SoundManager.Instance.PlaySFX(type, Vector2.zero);
    }
}
