using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXObject : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    private Queue<SFXObject> poolQueue;
    public void Init(Queue<SFXObject> pool)
    {
        poolQueue = pool;
        ReturnToPool();
    }
    public void Play(SFXData data, Vector3 pos)
    {
        transform.position = pos;
        _audioSource.clip = data.Clip;

        gameObject.SetActive(true);
        _audioSource.Play();
        StartCoroutine(DelayReturnToPool(_audioSource.clip.length / _audioSource.pitch));
    }
    private void ReturnToPool()
    {
        poolQueue.Enqueue(this);
    }
    private IEnumerator DelayReturnToPool(float duration)
    {
        yield return new WaitForSeconds(duration);

        _audioSource.Stop();
        gameObject.SetActive(false);
        ReturnToPool();
    }
}