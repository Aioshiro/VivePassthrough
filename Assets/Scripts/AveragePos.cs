using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AveragePos : MonoBehaviour
{
    [SerializeField] Transform object1;
    [SerializeField] Transform object2;


    // Update is called once per frame
    void Update()
    {
        transform.position = (object1.position + object2.position) / 2;
    }
}
