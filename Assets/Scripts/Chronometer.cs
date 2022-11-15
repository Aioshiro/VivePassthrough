using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chronometer
{
    private float initialTime;

    public void StartChronometer()
    {
        initialTime = Time.time;
    }

    public float StopChronometer()
    {
        return Time.time - initialTime;
    }
}
