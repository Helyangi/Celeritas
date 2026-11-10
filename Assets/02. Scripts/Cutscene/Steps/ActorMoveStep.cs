using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// NPC나 오브젝트를 목표 위치까지 정해진 속도로 옮긴다. (플레이어는 "플레이어 걷기"를 쓴다)
/// 이동 중/도착 후 애니메이션을 지정하면 그 이름의 Animator 상태를 재생한다.
/// </summary>
[Serializable, CutsceneStepMenu("오브젝트 이동")]
public class ActorMoveStep : CutsceneStep
{
    [Tooltip("움직일 대상의 이름표 Id")]
    [SerializeField] private string _actorId;
    [Tooltip("도착할 위치의 이름표 Id (마커 등)")]
    [SerializeField] private string _targetId;
    [Tooltip("초당 이동 거리")]
    [SerializeField, Min(0.01f)] private float _speed = 3f;
    [Tooltip("켜면 좌우로만 움직이고 높이는 그대로 둔다 (땅 위를 걷는 NPC)")]
    [SerializeField] private bool _horizontalOnly = true;
    [Tooltip("움직이는 방향을 바라보게 한다")]
    [SerializeField] private bool _faceMoveDirection = true;

    [Header("애니메이션 (선택, Animator 상태 이름)")]
    [SerializeField] private string _moveAnimation;   // 이동하는 동안
    [SerializeField] private string _arriveAnimation; // 도착한 뒤

    [Tooltip("끄면 움직이는 동안 다음 단계가 동시에 진행된다")]
    [SerializeField] private bool _waitUntilArrive = true;

    public override string Summary => $"{_actorId} → {_targetId}";

    public override IEnumerator Execute(CutsceneManager manager)
    {
        if (CutsceneTargets.IsPlayer(_actorId))
        {
            Debug.LogWarning("[ActorMoveStep] 플레이어는 '플레이어 걷기' 단계로 움직여 주세요.");
            yield break;
        }

        if (_waitUntilArrive) yield return Move();
        else manager.RunInBackground(this, Move());
    }

    private IEnumerator Move()
    {
        if (!CutsceneActor.TryFind(_actorId, out CutsceneActor actor)) yield break;
        Transform target = CutsceneTargets.FindTransform(_targetId);
        if (target == null) yield break;

        Transform mover = actor.transform;
        Animator animator = actor.GetComponentInChildren<Animator>(true);

        // 물리로 움직이는 오브젝트는 이동하는 동안만 물리를 멈춘다 (중력·충돌과 싸우지 않도록)
        Rigidbody2D body = actor.GetComponent<Rigidbody2D>();
        bool wasDynamic = body != null && body.bodyType == RigidbodyType2D.Dynamic;
        if (wasDynamic)
        {
            body.linearVelocity = Vector2.zero;
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        Vector3 goal = GetGoal(mover, target);
        if (_faceMoveDirection && Mathf.Abs(goal.x - mover.position.x) > 0.001f)
        {
            CutsceneTargets.Face(_actorId, goal.x > mover.position.x);
        }
        if (animator != null) CutsceneTargets.PlayAnimation(animator, _moveAnimation);

        while (mover != null && target != null)
        {
            goal = GetGoal(mover, target); // 목표가 움직여도 따라간다
            Vector3 next = Vector3.MoveTowards(mover.position, goal, _speed * Time.deltaTime);
            mover.position = next;

            if ((next - goal).sqrMagnitude < 0.0001f) break;
            yield return null;
        }

        if (wasDynamic && body != null)
        {
            body.bodyType = RigidbodyType2D.Dynamic;
            body.linearVelocity = Vector2.zero;
        }
        if (animator != null) CutsceneTargets.PlayAnimation(animator, _arriveAnimation);
    }

    private Vector3 GetGoal(Transform mover, Transform target)
    {
        Vector3 goal = target.position;
        goal.z = mover.position.z;
        if (_horizontalOnly) goal.y = mover.position.y;
        return goal;
    }
}
