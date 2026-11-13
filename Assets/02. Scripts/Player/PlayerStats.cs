using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float MoveSpeed = 5f;
    public float JumpForce = 5f;
    public float RespawnTime;

    [Header("빛 이동")]
    [Tooltip("거울을 거치지 않는 직선 발사에 걸리는 시간(초). 0이면 아래의 고정 속도 방식으로 돌아간다.")]
    public float LightMoveBaseDuration = 0.1f;
    [Tooltip("거울에서 꺾일 때마다 더해지는 시간(초). 꺾임이 많은 경로일수록 충분히 보여진다.")]
    public float LightMoveDurationPerReflect = 0.04f;
    [Tooltip("시간 고정 모드에서 속도가 너무 느려지거나 빨라지지 않게 가두는 범위")]
    public float MinLightMoveSpeed = 20f;
    public float MaxLightMoveSpeed = 400f;
    [Tooltip("LightMoveDuration이 0일 때 쓰는 고정 속도 (예전 방식)")]
    public float LightMoveSpeed = 35;

    [Header("빛 이동 히트스톱 (초, 0이면 끔)")]
    [Tooltip("발사하는 순간 멈추는 시간")]
    public float LightShotHitstop = 0.05f;
    [Tooltip("빛 오브에 도착하는 순간 멈추는 시간")]
    public float LightArriveHitstop = 0.08f;
}
