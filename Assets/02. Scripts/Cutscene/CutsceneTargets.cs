using System.Collections;
using UnityEngine;

/// <summary>
/// 컷씬 단계들이 대상(플레이어, NPC, 마커 등)을 찾고 다룰 때 쓰는 도우미.
/// 대상 Id에 "Player"라고 쓰면 플레이어를 자동으로 찾는다. (플레이어에는 이름표를 붙이지 않아도 된다)
/// </summary>
public static class CutsceneTargets
{
    public const string PlayerId = "Player";

    public static bool IsPlayer(string id) => id == PlayerId;

    /// <summary>
    /// 씬의 플레이어를 찾는다 (Player 태그 + PlayerController).
    /// </summary>
    public static PlayerController FindPlayer(bool warn = true)
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        PlayerController player = playerObject != null ? playerObject.GetComponent<PlayerController>() : null;

        if (player == null && warn)
        {
            Debug.LogWarning("[Cutscene] 씬에서 플레이어(Player 태그 + PlayerController)를 찾지 못했습니다.");
        }
        return player;
    }

    /// <summary>
    /// Id로 대상의 Transform을 찾는다. (플레이어 또는 CutsceneActor)
    /// </summary>
    public static Transform FindTransform(string id)
    {
        if (IsPlayer(id))
        {
            PlayerController player = FindPlayer();
            return player != null ? player.transform : null;
        }

        return CutsceneActor.TryFind(id, out CutsceneActor actor) ? actor.transform : null;
    }

    /// <summary>
    /// Id로 대상의 Animator를 찾는다. (플레이어는 모델의 Animator)
    /// </summary>
    public static Animator FindAnimator(string id)
    {
        if (IsPlayer(id))
        {
            PlayerController player = FindPlayer();
            if (player == null) return null;

            if (player.Anim == null) Debug.LogWarning("[Cutscene] 플레이어에 Animator가 연결되어 있지 않습니다.", player);
            return player.Anim;
        }

        return CutsceneActor.FindComponent<Animator>(id);
    }

    /// <summary>
    /// Animator의 상태를 이름으로 재생한다. 그런 상태가 없으면 경고를 남기고 false.
    /// </summary>
    public static bool PlayAnimation(Animator animator, string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName)) return false;

        int hash = Animator.StringToHash(stateName);
        if (!animator.HasState(0, hash))
        {
            Debug.LogWarning($"[Cutscene] '{animator.name}'의 Animator에 '{stateName}' 상태가 없습니다.", animator);
            return false;
        }

        animator.Play(hash, 0, 0f);
        return true;
    }

    /// <summary>
    /// 대상이 오른쪽 또는 왼쪽을 보게 한다.
    /// 플레이어는 평소 이동할 때와 같은 방식(모델 회전)으로, NPC는 이미지가 붙은 오브젝트를 돌린다.
    /// </summary>
    public static void Face(string id, bool faceRight)
    {
        if (IsPlayer(id))
        {
            PlayerController player = FindPlayer();
            if (player == null || player.Renderer == null) return;

            player.Renderer.transform.rotation = faceRight ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
            return;
        }

        if (!CutsceneActor.TryFind(id, out CutsceneActor actor)) return;

        SpriteRenderer sprite = actor.GetComponentInChildren<SpriteRenderer>(true);
        Transform visual = sprite != null ? sprite.transform : actor.transform;

        bool flip = faceRight == actor.SpriteFacesLeft; // 기본 방향과 반대를 봐야 하면 뒤집는다
        Vector3 euler = visual.localEulerAngles;
        euler.y = flip ? 180f : 0f;
        visual.localEulerAngles = euler;
    }

    /// <summary>
    /// 플레이어가 스스로 움직일 수 있는 상태인가 (등장 연출·빛 이동이 끝났고 땅 위에 서 있다)
    /// </summary>
    public static bool IsPlayerReady(PlayerController player)
    {
        if (player == null) return false;
        if (player.RB == null || !player.RB.simulated) return false; // 등장/리스폰 연출 중
        if (player.IsLightMoving) return false;
        return player.GroundChecker != null && player.GroundChecker.IsGround();
    }

    /// <summary>
    /// 플레이어가 움직일 준비가 될 때까지 기다린다. (공중이면 착지할 때까지)
    /// </summary>
    public static IEnumerator WaitUntilPlayerReady(PlayerController player, float timeout = 5f)
    {
        float time = 0f;
        while (player != null && !IsPlayerReady(player))
        {
            time += Time.deltaTime;
            if (time >= timeout)
            {
                Debug.LogWarning("[Cutscene] 플레이어가 착지하지 않아 기다리지 않고 진행합니다.");
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 이동 입력을 비운 뒤 플레이어가 완전히 멈출 때까지 기다린다.
    /// (이동 상태가 물리 프레임에서 한 번 더 움직이며 방향을 되돌리는 것을 기다려, 다음 단계의 "바라보기" 등이 덮이지 않도록)
    /// </summary>
    public static IEnumerator WaitUntilPlayerStops(PlayerController player, float timeout = 0.3f)
    {
        float time = 0f;
        do
        {
            yield return new WaitForFixedUpdate();
            yield return null;
            time += Time.deltaTime;
        }
        while (player != null && player.RB != null && Mathf.Abs(player.RB.linearVelocityX) > 0.01f && time < timeout);
    }
}
