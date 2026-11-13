using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float MoveSpeed = 5f;
    public float JumpForce = 5f;
    public float RespawnTime;

    [Header("빛 이동")]
    [Tooltip("경로 길이와 상관없이 이동에 걸리는 시간(초). 거울이 몇 번 꺾이든 항상 이만큼 걸린다. 0이면 아래의 고정 속도를 쓴다.")]
    public float LightMoveDuration = 0.12f;
    [Tooltip("시간 고정 모드에서 속도가 너무 느려지거나 빨라지지 않게 가두는 범위")]
    public float MinLightMoveSpeed = 20f;
    public float MaxLightMoveSpeed = 400f;
    [Tooltip("LightMoveDuration이 0일 때 쓰는 고정 속도 (예전 방식)")]
    public float LightMoveSpeed = 35;

    [Header("빛 이동 히트스톱 (초, 0이면 끔)")]
    [Tooltip("발사하는 순간 멈추는 시간")]
    public float LightShotHitstop = 0.05f;
    [Tooltip("거울에서 꺾이는 순간 멈추는 시간")]
    public float MirrorReflectHitstop = 0.03f;
    [Tooltip("빛 오브에 도착하는 순간 멈추는 시간")]
    public float LightArriveHitstop = 0.08f;
}
