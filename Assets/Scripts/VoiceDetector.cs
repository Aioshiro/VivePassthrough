using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceDetector : MonoBehaviour
{
    static bool isTalking = false;
    public static UnityEngine.Events.UnityEvent startedTalking;
    public static UnityEngine.Events.UnityEvent stoppedTalking;

    [Tooltip("Microphone level at which we considered there is someone talking")]
    [SerializeField] private float loudnessThreshold = 10e-4f;

    private void Awake()
    {
        startedTalking = new UnityEngine.Events.UnityEvent();
        stoppedTalking = new UnityEngine.Events.UnityEvent();


        startedTalking.AddListener(DebugStartTalking);
        stoppedTalking.AddListener(DebugStopTalking);
    }


    void Update()
    {
        //Debug.Log(MicrophoneDetector.MicLoudness);
        if (MicrophoneDetector.MicLoudness > loudnessThreshold)
        {
            if (!isTalking)
            {
                isTalking = true;
                startedTalking.Invoke();
            }
        }
        else
        {
            if (isTalking)
            {
                isTalking = false;
                stoppedTalking.Invoke();
            }
        }
    }

    void DebugStartTalking()
    {
        Debug.Log("Started talking");
    }

    void DebugStopTalking()
    {
        Debug.Log("stopped talking");
    }
}
