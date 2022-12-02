using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
/// <summary>
/// Script to quit application on escape press
/// </summary>
public class Quit : MonoBehaviour
{
    void Update()
    {
        if (NetworkServer.active)
        {
            return;
        }
        var keyboard = Keyboard.current;
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("Force quit !");
            Application.Quit();
        }
    }
}
