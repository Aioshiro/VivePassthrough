using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField]
    float maxDist;

    Rigidbody buttonTop;

    public UnityEngine.Events.UnityEvent OnPushEvent;
    bool hasCalledEvent = false;

    [SerializeField]
    List<Material> materialsColors;
    int currentMaterial = 0;
    [SerializeField]
    Renderer renderer;
    
    // Start is called before the first frame update
    void Start()
    {
        buttonTop = transform.GetChild(2).GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonTop.transform.localPosition.z < maxDist/2)
        {
            if (!hasCalledEvent)
            {
                OnPushEvent.Invoke();
                hasCalledEvent = true;
                Debug.Log("calling event");
            }

            if (buttonTop.transform.localPosition.z < maxDist)
            {
                buttonTop.transform.localPosition = new Vector3(buttonTop.transform.localPosition.x, buttonTop.transform.localPosition.y, maxDist);
            }
            
        }
        else
        {
            hasCalledEvent = false;
        }
    }

    public void ToggleMaterials()
    {
        currentMaterial = (currentMaterial + 1) % 2;
        renderer.material = materialsColors[currentMaterial];
    }
}
