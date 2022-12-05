using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script associated with a button model, to be able to press it ingame
/// </summary>
public class Button : MonoBehaviour
{
    [Tooltip("Max distance where the button is able to be pushed")]
    [SerializeField]
    float maxDist;

    /// <summary>
    /// Rigidbody of the top of the button
    /// </summary>
    Rigidbody buttonTop;

    [Tooltip("Event called once each time the button is pushed")]
    public UnityEngine.Events.UnityEvent OnPushEvent;

    /// <summary>
    /// Check if the event has been called, to make sure it's not called multiple times
    /// </summary>
    bool hasCalledEvent = false;

    [Tooltip("Materials for the top of the button")]
    [SerializeField]
    List<Material> materialsColors;
    /// <summary>
    /// Current material id
    /// </summary>
    int currentMaterial = 0;
    [Tooltip("Renderer of the button top to change its material")]
    [SerializeField]
    new Renderer renderer;
    
    // Start is called before the first frame update
    void Start()
    {
        buttonTop = transform.GetChild(2).GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        buttonTop.transform.localPosition = new Vector3(0, 0, Mathf.Min(buttonTop.transform.localPosition.z,0));
        if (buttonTop.transform.localPosition.z < maxDist/2)
        {
            if (!hasCalledEvent)
            {
                OnPushEvent.Invoke();
                hasCalledEvent = true;
                //Debug.Log("calling event");
            }

            if (buttonTop.transform.localPosition.z < maxDist)
            {
                buttonTop.transform.localPosition = new Vector3(0, 0, maxDist);
            }
            
        }
        else
        {
            hasCalledEvent = false;
        }
    }

    /// <summary>
    /// Toggling from one material to the other
    /// </summary>
    public void ToggleMaterials()
    {
        currentMaterial = (currentMaterial + 1) % 2;
        renderer.material = materialsColors[currentMaterial];
    }
}
