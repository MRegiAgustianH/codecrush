using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.5f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Suara pop ringan saat ubin cocok.
    /// </summary>
    public void PlayMatch()
    {
        PlayTone(600f, 0.1f, 0.4f);
    }

    /// <summary>
    /// Suara geser saat ubin bertukar posisi.
    /// </summary>
    public void PlaySwap()
    {
        PlayTone(400f, 0.08f, 0.25f);
    }

    /// <summary>
    /// Suara combo dengan nada naik sesuai level combo.
    /// </summary>
    public void PlayCombo(int comboLevel)
    {
        float baseFreq = 500f + (comboLevel * 150f);
        PlayTone(baseFreq, 0.15f, 0.5f);
    }

    /// <summary>
    /// Suara sedih menurun saat game over.
    /// </summary>
    public void PlayGameOver()
    {
        StartCoroutine(PlayGameOverSequence());
    }

    private System.Collections.IEnumerator PlayGameOverSequence()
    {
        PlayTone(400f, 0.2f, 0.4f);
        yield return new WaitForSeconds(0.2f);
        PlayTone(300f, 0.2f, 0.4f);
        yield return new WaitForSeconds(0.2f);
        PlayTone(200f, 0.35f, 0.5f);
    }

    /// <summary>
    /// Generate dan mainkan AudioClip prosedural (sine wave).
    /// </summary>
    private void PlayTone(float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("sfx", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration); // Fade out linear
            envelope = envelope * envelope; // Fade out eksponensial
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
        }

        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip, volume);
    }
}
