using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Script to quit application on escape press
/// </summary>
public class Quit : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
