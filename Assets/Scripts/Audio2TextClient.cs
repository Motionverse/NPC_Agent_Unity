using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using System;
using Microsoft.CognitiveServices.Speech.Audio;
using System.IO;
using MotionverseSDK;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Diagnostics.Tracing;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
#if PLATFORM_IOS
using UnityEngine.iOS;
using System.Collections;
#endif

public class Audio2TextClient : Singleton<Audio2TextClient>
{
    private bool micPermissionGranted = false;
    SpeechRecognizer recognizer;
    SpeechConfig config;
    AudioConfig audioInput;
    PushAudioInputStream pushStream;

    private object threadLocker = new object();
    private bool recognitionStarted = false;
    private string message = null;
    private string recognizedStr = null;

    int lastSample = 0;
    AudioSource audioSource;

    public string subscriptionKey;
    public string region;
    public string Language;
    public Text TextBG;
    public Toggle MicState;
    [SerializeField]
    private Image micAmount;

    private Player player = null;
    [SerializeField]
    private ChatGPTClient chatGPTClient;

#if PLATFORM_ANDROID || PLATFORM_IOS
    private Microphone mic;
#endif

    void Start()
    {
#if PLATFORM_ANDROID
            // Request to use the microphone, cf.
            // https://docs.unity3d.com/Manual/android-RequestingPermissions.html
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#elif PLATFORM_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }
#else

#endif
        micPermissionGranted = true;
        config = SpeechConfig.FromSubscription(subscriptionKey, region);
        if (Language!="")
        {
            Debug.Log("Language");
            config.SpeechSynthesisLanguage = Language;
            config.SpeechRecognitionLanguage = Language;
        }
 

        pushStream = AudioInputStream.CreatePushStream();
        audioInput = AudioConfig.FromStreamInput(pushStream);
        recognizer = new SpeechRecognizer(config, audioInput);
        recognizer.Recognizing += RecognizingHandler;
        recognizer.Recognized += RecognizedHandler;
        recognizer.Canceled += CanceledHandler;

        foreach (var device in Microphone.devices)
        {
            Debug.Log("DeviceName: " + device);
        }
        audioSource = GetComponent<AudioSource>();



    }

    public void OnClick(Player _player)
    {
        OnSwitchMic(MicState.isOn, _player);
    }
    public async void OnSwitchMic(bool isOpen, Player _player = null)
    {
        player = _player;
        if (Microphone.devices.Length < 1)
            return;

        MicState.isOn = !isOpen;
        if (isOpen && !Microphone.IsRecording(Microphone.devices[0]))
        {

            audioSource.clip = Microphone.Start(Microphone.devices[0], true, 200, 16000);
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            lock (threadLocker)
            {
                recognitionStarted = true;
            }

        }
        else if (!isOpen && recognitionStarted)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(true);

            if (Microphone.IsRecording(Microphone.devices[0]))
            {
                Debug.Log("Microphone.End: " + Microphone.devices[0]);
                Microphone.End(null);
                lastSample = 0;
            }

            lock (threadLocker)
            {
                recognitionStarted = false;
                Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
            }
        }
    }

    private byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
    {
        MemoryStream dataStream = new MemoryStream();
        int x = sizeof(Int16);
        Int16 maxValue = Int16.MaxValue;
        int i = 0;
        while (i < data.Length)
        {
            dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * maxValue)), 0, x);
            ++i;
        }
        byte[] bytes = dataStream.ToArray();
        dataStream.Dispose();
        return bytes;
    }

    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        message = e.Result.Text;
        //Debug.Log($"Recognizing {e.Result.Text}");
    }

    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        recognizedStr = e.Result.Text;
        Debug.Log($"Recognized {message}");
        MainThreadDispatcher.Enqueue(() =>
        {
            if (recognizedStr.Length > 2)
            {
                chatGPTClient.GetDialogue(recognizedStr, player?.GetComponent<NPC>());
            }
            
        });
       

    }

    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        lock (threadLocker)
        {
            message = e.ErrorDetails.ToString();
            Debug.Log("CanceledHandler: " + message);

        }
    }
    void Disable()
    {
        recognizer.Recognizing -= RecognizingHandler;
        recognizer.Recognized -= RecognizedHandler;
        recognizer.Canceled -= CanceledHandler;
        pushStream.Close();
        recognizer.Dispose();
    }
    void FixedUpdate()
    {
#if PLATFORM_ANDROID
        if (!micPermissionGranted && Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            micPermissionGranted = true;

        }
#elif PLATFORM_IOS
        if (!micPermissionGranted && Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            micPermissionGranted = true;
        }
#endif
        lock (threadLocker)
        {
            if (TextBG != null && recognizedStr != curMessage)
            {
                curMessage = recognizedStr;
                ShowText(recognizedStr);
            }
        }
        if (Microphone.devices.Length > 0 && Microphone.IsRecording(Microphone.devices[0]) && recognitionStarted == true)
        {
            int pos = Microphone.GetPosition(Microphone.devices[0]);
            int diff = pos - lastSample;

            if (diff > 0)
            {
                float[] samples = new float[diff * audioSource.clip.channels];
                audioSource.clip.GetData(samples, lastSample);
                byte[] ba = ConvertAudioClipDataToInt16ByteArray(samples);
                if (ba.Length != 0)
                {
                    pushStream.Write(ba);
                }


                float[] sam = new float[128];
                float levelMax = 0;
                int startPosition = Microphone.GetPosition(null) - (128 + 1);
                audioSource.clip.GetData(sam, startPosition);
                for (int i = 0; i < 128; i++)
                {
                    float wavePeak = sam[i];
                    if (levelMax < wavePeak)
                    {
                        levelMax = wavePeak;
                    }
                }
                micAmount.fillAmount = levelMax;

            }
            lastSample = pos;
        }
    }


    string curMessage = null;
    public async void ShowText(string text)
    {

        TextBG.gameObject.SetActive(true);
        TextBG.text = text;
        await Task.Delay(3000);
        TextBG.gameObject.SetActive(false);
    }

    public void OnClick(string text)
    {
        chatGPTClient.GetDialogue(text, player?.GetComponent<NPC>());
    }
}

