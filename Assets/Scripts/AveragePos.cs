using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Setting up an object position as an average of two other objects positions
/// </summary>
public class AveragePos : MonoBehaviour
{
    [Tooltip("First object")]
    [SerializeField] Transform object1;
    [Tooltip("Second object")]
    [SerializeField] Transform object2;


    // Update is called once per frame
    void Update()
    {
        //new object pos is average of the 2 transform poses
        transform.position = (object1.position + object2.position) / 2;
    }
}
