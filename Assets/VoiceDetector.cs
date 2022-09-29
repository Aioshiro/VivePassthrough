using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceDetector : MonoBehaviour
{
    bool isTalking = false;
    static UnityEngine.Events.UnityEvent startedTalking;
    static UnityEngine.Events.UnityEvent stoppedTalking;

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
