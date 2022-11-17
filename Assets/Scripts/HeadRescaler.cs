using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadRescaler : MonoBehaviour
{
    [SerializeField] float initialHeadLength = 0.17f;


    private void Start()
    {
        Rescaler(GameManager.Instance.otherPersonHeadLength);
    }

    void Rescaler(float realLifeHeadLength)
    {
        transform.localScale = Vector3.one * (realLifeHeadLength / initialHeadLength);
    }
}
