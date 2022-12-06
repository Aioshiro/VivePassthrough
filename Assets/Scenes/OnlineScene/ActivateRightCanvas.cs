using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateRightCanvas : MonoBehaviour
{
    [SerializeField]
    Transform playerRig;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 cross = Vector3.Cross(transform.forward, playerRig.forward);
        float dot = Vector3.Dot(cross, transform.up);
        if (dot < 0)
        {
            transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }

}
