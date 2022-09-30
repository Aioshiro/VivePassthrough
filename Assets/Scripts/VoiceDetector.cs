using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceDetector : MonoBehaviour
{
    static bool isTalking = false;
    public static UnityEngine.Events.UnityEvent startedTalking;
    public static UnityEngine.Events.UnityEvent stoppedTalking;

    [SerializeField] private float loudnessThreshold = 10e-4f;

    private void Start()
    {
        startedTalking = new UnityEngine.Events.UnityEvent();
        stoppedTalking = new UnityEngine.Events.UnityEvent();


        startedTalking.AddListener(DebugStartTalking);
        stoppedTalking.AddListener(DebugStopTalking);
    }


    // Update is called once per frame
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
