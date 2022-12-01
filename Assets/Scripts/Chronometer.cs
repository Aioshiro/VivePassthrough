using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chronometer script to measure time
/// </summary>
public class Chronometer
{
    private float initialTime;

    /// <summary>
    /// Starting chronometer
    /// </summary>
    public void StartChronometer()
    {
        initialTime = Time.time;
    }

    /// <summary>
    /// Returning chronometer current time
    /// </summary>
    /// <returns></returns>
    public float StopChronometer()
    {
        return Time.time - initialTime;
    }

}
