using UnityEngine;

public class RandomPitch : MonoBehaviour
{
    public AudioSource audioSource;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayWithRandomPitch()
    {
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.Play();
    }
}
