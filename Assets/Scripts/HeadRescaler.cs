using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rescales the head based on person real head's lengh
/// </summary>
public class HeadRescaler : MonoBehaviour
{
    /// <summary>
    /// Initial head length of the avatar
    /// </summary>
    [SerializeField] float initialHeadLength = 0.17f;


    private void Start()
    {
        Rescaler(GameManager.Instance.otherPersonHeadLength);
    }
    
    /// <summary>
    /// Changes avatar length to fit real size head length
    /// </summary>
    /// <param name="realLifeHeadLength"></param>
    void Rescaler(float realLifeHeadLength)
    {
        transform.localScale = Vector3.one * (realLifeHeadLength / initialHeadLength);
    }
}
