using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

public class MicrophonePitchDetection : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI frequencyText;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Button muteOpenButton;

    private AudioSource audioSource;
    private string micDevice;
    private int sampleRate = 44100;
    private const int sampleSize = 1024;
    private float[] spectrumData = new float[sampleSize];
    private bool isAudioEnabled = true;

    void Start()
    {
        if (Microphone.devices != null)
        {
            micDevice = Microphone.devices[0];
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = Microphone.Start(micDevice, true, 1, sampleRate);
            audioSource.loop = true;

            audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master")[0];

            muteOpenButton.onClick.AddListener(SetAudioFeedback);

            audioSource.Play();
            Debug.Log("Microphone: " + micDevice);
        }
        else
        {
            Debug.LogError("No microphone detected");
        }
    }

    void Update()
    {
        if (audioSource != null)
        {
            audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);
            float frequency = GetFrequency(spectrumData);
            frequencyText.text = "Frequency: " + frequency.ToString() + " Hz";
        }
    }

    private float GetFrequency(float[] spectrum)
    {
        float maxVal = 0f;
        int maxIndex = 0;

        for (int i = 0; i < spectrum.Length; i++)
        {
            if (spectrum[i] > maxVal)
            {
                maxVal = spectrum[i];
                maxIndex = i;
            }
        }

        if (maxIndex > 0 && maxIndex < spectrum.Length - 1)
        {
            float leftVal = spectrum[maxIndex - 1];
            float rightVal = spectrum[maxIndex + 1];
            float delta = 0.5f * (rightVal - leftVal) / (2 * maxVal - leftVal - rightVal);
            maxIndex += (int)delta;
        }

        float freqResolution = (float)sampleRate / 2 / spectrum.Length;
        return maxIndex * freqResolution;
    }

    private void SetAudioFeedback()
    {
        isAudioEnabled = !isAudioEnabled;

        audioMixer.SetFloat("Master", isAudioEnabled ? 0f : -80f);
    }
}