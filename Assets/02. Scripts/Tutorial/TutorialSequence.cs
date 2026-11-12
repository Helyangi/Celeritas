using System;
using System.Collections;
using UnityEngine;

// 튜토리얼 안내를 순서대로 보여주는 진행 담당 클래스.
// 플레이어가 트리거 범위에 처음 들어오면 시작하고, 단계마다 (글자 나타남 -> 조건 달성 -> 글자 사라짐) 순으로 진행한다.
// - 표시 시간(MinDisplayTime / DisplayDuration)은 플레이어가 범위 밖에 있어도 계속 흐른다.
// - 조건 달성 여부는 플레이어가 범위 안에 있을 때만 판정한다. (범위 밖에서 한 행동은 인정하지 않는다)
// - 단계는 자기 조건 또는 이후 단계의 조건이 달성되면 완료된다. (안내를 건너뛰고 앞서 간 경우 바로 넘어간다)
// - 끝낸 튜토리얼은 저장 데이터(ProgressData)에 기록돼서 다시 나오지 않는다. (새 게임에서는 초기화)
[RequireComponent(typeof(Collider2D))]
public class TutorialSequence : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        [TextArea(1, 3)] public string Text;
        public TutorialCondition Condition;
        public float MinDisplayTime = 1.2f; // 조건을 빨리 달성(또는 뒤 단계를 건너뛰어야 할 때)해도 이 시간 동안은 글자를 보여준다
        public float DisplayDuration = 3f;  // 조건이 없는(null) 단계일 때: 건너뛸 필요가 없으면 이 시간 동안 보여주고 사라진다
    }

    [SerializeField] private string _id = "Stage1_Aim"; // 저장 데이터에 기록되는 ID. 튜토리얼마다 달라야 한다
    [SerializeField] private TutorialHint _hint;
    [SerializeField] private Step[] _steps;
    [SerializeField] private ParticleSystem[] _activeEffects; // 플레이어가 범위 안에 있는 동안만 재생되는 연출 (예: 벽 반짝임)

    private Coroutine _routine;
    private PlayerController _player;
    private Collider2D _trigger;
    private Collider2D[] _playerColliders;
    private bool _inside;       // 지금 플레이어가 범위 안에 있는지
    private int _index;         // 지금 진행 중인 단계
    private int _latchedMet;    // 범위 안에서 달성한 가장 뒤 단계 번호 (없으면 -1)
    private bool _completed;

    private void Awake()
    {
        _trigger = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.IsTutorialCompleted(_id))
        {
            _completed = true;
            _trigger.enabled = false;
        }
        SetEffects(false);
    }

    // 처음 들어왔을 때만 시작한다. 이후에는 멈추지 않고 계속 진행된다.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_completed || _routine != null) return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        _player = player;
        _playerColliders = player.GetComponentsInChildren<Collider2D>();
        _inside = true;
        _routine = StartCoroutine(Run());
        SetEffects(true);
    }

    private void OnDisable()
    {
        // 꺼지면 코루틴이 멈추므로, 다시 켜졌을 때 처음부터 시작할 수 있게 진행 상태를 정리한다
        if (_routine == null) return;

        _routine = null;
        _inside = false;
        EndConditions(_index);
        SetEffects(false);
    }

    private IEnumerator Run()
    {
        _index = 0;

        while (_index < _steps.Length)
        {
            Step step = _steps[_index];
            _latchedMet = -1;
            BeginConditions(_index);

            yield return StartCoroutine(_hint.ShowRoutine(step.Text));

            // 최소 표시 시간이 지나고 조건이 달성될 때까지 기다린다 (이후 단계의 조건이 먼저 달성돼도 완료)
            // 조건이 null인 단계는 DisplayDuration 동안만 보여준다
            float shown = 0f;
            int met;
            bool achieved = false; // 조건을 달성해서 끝났는지 (시간이 다 돼서 끝난 게 아니라)
            while (true)
            {
                shown += Time.deltaTime; // 시간은 범위와 상관없이 흐른다
                UpdateInside();
                // 조건 판정은 범위 안에서만 한다 (범위를 나가는 것이 조건인 경우는 예외)
                int current = HighestMetIndex(_index, _inside);
                if (current > _latchedMet) _latchedMet = current;

                met = _latchedMet;
                if (met >= 0 && shown >= step.MinDisplayTime)
                {
                    achieved = true;
                    break;
                }

                // 조건이 없는 단계: 뒤 단계 조건이 이미 달성됐으면 위에서 최소 시간 후 넘어가고,
                // 아니면 DisplayDuration 동안 보여준 뒤 스스로 완료된다
                if (step.Condition == null && shown >= step.DisplayDuration)
                {
                    met = Mathf.Max(met, _index);
                    break;
                }
                yield return null;
            }

            EndConditions(_index);
            if (achieved && step.Condition != null) yield return StartCoroutine(_hint.SuccessRoutine()); // 달성 피드백
            yield return StartCoroutine(_hint.HideRoutine());

            _index = met + 1; // 앞서 간 단계가 있으면 그 단계 다음으로 바로 넘어간다
        }

        Complete();
    }

    // 플레이어가 범위 안에 있는지 확인하고, 드나드는 순간을 처리한다
    private void UpdateInside()
    {
        bool inside = IsPlayerInside();
        if (inside == _inside) return;

        _inside = inside;
        SetEffects(inside);

        // 다시 들어왔을 때는 범위 밖에서 쌓인 조건 상태를 지우고 새로 판정을 시작한다
        if (inside) BeginConditions(_index);
    }

    private bool IsPlayerInside()
    {
        foreach (Collider2D col in _playerColliders)
        {
            if (col != null && col.enabled && _trigger.Distance(col).isOverlapped) return true; // 접촉 정보가 아니라 도형 겹침을 직접 계산한다
        }
        return false;
    }

    // from 이후 단계 중 가장 뒤에서 달성된 단계 번호 (없으면 -1)
    private int HighestMetIndex(int from, bool inside)
    {
        for (int i = _steps.Length - 1; i >= from; i--)
        {
            TutorialCondition condition = _steps[i].Condition;
            if (condition != null && condition.IsMet && (inside || condition.CountsOutsideZone)) return i;
        }
        return -1;
    }

    private void BeginConditions(int from)
    {
        for (int i = from; i < _steps.Length; i++)
        {
            if (_steps[i].Condition != null) _steps[i].Condition.Begin(_player);
        }
    }

    private void EndConditions(int from)
    {
        for (int i = from; i < _steps.Length; i++)
        {
            if (_steps[i].Condition != null) _steps[i].Condition.End();
        }
    }

    private void Complete()
    {
        _completed = true;
        _routine = null;
        _trigger.enabled = false;
        SetEffects(false);

        if (SaveManager.Instance != null) SaveManager.Instance.MarkTutorialCompleted(_id);
    }

    private void SetEffects(bool on)
    {
        if (_activeEffects == null) return;
        foreach (ParticleSystem effect in _activeEffects)
        {
            if (effect == null) continue;
            if (on) { if (!effect.isPlaying) effect.Play(); }
            else effect.Stop(true, ParticleSystemStopBehavior.StopEmitting); // 남은 입자는 자연스럽게 사라진다
        }
    }
}
