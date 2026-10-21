using UnityEngine;
using UnityEngine.Video;

public class IntroVideo : MonoBehaviour
{
    [SerializeField] VideoPlayer _video;
    [SerializeField] PlayerController _controller;

    private void Awake()
    {
        _video.loopPointReached += OnVideoEnd;
    }
    private void Start()
    {
        if (_controller == null) _controller = GameObject.Find("Player").GetComponent<PlayerController>();
        _controller.enabled = false;
    }
    private void OnDestroy()
    {
        _video.loopPointReached -= OnVideoEnd;
    }
    private void OnVideoEnd(VideoPlayer vp)
    {
        Destroy(gameObject);
        _controller.enabled = true;
    }
}
