using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Trigger event wto enable hands when above map
/// </summary>
public class TriggerEvent : MonoBehaviour
{

    private SkinnedMeshRenderer handRenderer;

    private void Start()
    {
        handRenderer= transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
        handRenderer.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Map"))
        {
            handRenderer.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Map"))
        {
            handRenderer.enabled = false;
        }
    }

}
