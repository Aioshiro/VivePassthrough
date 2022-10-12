using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneDetector : MonoBehaviour
{
    public static float MicLoudness; //Mic sound level

    private string _device;
    AudioClip _clipRecord;
    int _sampleWindow = 1024;

    //mic initialization
    void InitMic()
    {
        if (_device == null) _device = Microphone.devices[0];
        _clipRecord = Microphone.Start(_device, true, 999, AudioSettings.outputSampleRate);
    }

    void StopMicrophone()
    {
        Microphone.End(_device);
    }


    //get data from microphone into audioclip
    float LevelMax()
    {
        float levelMax = 0;
        float[] waveData = new float[_sampleWindow];
        int micPosition = Microphone.GetPosition(null) - (_sampleWindow + 1); // null means the first microphone
        if (micPosition < 0) return 0;
        _clipRecord.GetData(waveData, micPosition);
        // Getting a peak on the last 128 samples
        //for (int i = 0; i < _sampleWindow; i++)
        //{
        //    float wavePeak = waveData[i] * waveData[i];
        //    if (levelMax < wavePeak)
        //    {
        //        levelMax = wavePeak;
        //    }
        //}
        for (int i = 0; i < _sampleWindow; i++)
        {
            levelMax += waveData[i] * waveData[i];
        }
        levelMax = Mathf.Sqrt(levelMax / _sampleWindow); // rms = square root of average

        return levelMax;
    }



    void Update()
    {
        // levelMax equals to the highest normalized value power 2, a small number because < 1
        // pass the value to a static var so we can access it from anywhere
        MicLoudness = LevelMax();
        //Debug.Log(MicLoudness);
    }

    // start mic when scene starts
    void OnEnable()
    {
        InitMic();
        //_isInitialized = true;
    }

    //stop mic when loading a new level or quit application
    void OnDisable()
    {
        StopMicrophone();
    }

    void OnDestroy()
    {
        StopMicrophone();
    }

}