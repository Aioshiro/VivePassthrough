using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calculates the bounding box of a set of objects, mainly an hand
/// </summary>
public class CalculateBoundingBox : MonoBehaviour
{
    [Tooltip("Root object of the hand")]
    public GameObject hand;

    [Tooltip("Transforms to include in the bounding box")]
    [SerializeField] Transform[] transformsToInclude;

    [Tooltip("Bounding box gameObject")]
    public GameObject box;

    [Tooltip("How much more bigger should the box be compared to the true bounding box ?")]
    public float extendAmount;

    private void Start()
    {
        transformsToInclude = hand.transform.GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        //Bounds bound = new Bounds(transformsToInclude[0].position, Vector3.zero);
        //foreach( Transform trans in transformsToInclude)
        //{
        //    bound.Encapsulate(trans.position);
        //}
        Vector3[] listOfPos = new Vector3[transformsToInclude.Length];
        for (int i=0;i< transformsToInclude.Length;i++)
        {
            listOfPos[i] = transformsToInclude[i].position;
        }
        //Bounds bound = GeometryUtility.CalculateBounds(listOfPos, Matrix4x4.TRS(Vector3.zero, transformsToInclude[0].rotation, Vector3.one));
        Bounds bound = GeometryUtility.CalculateBounds(listOfPos, Matrix4x4.identity);
        //Bounds bound = meshRenderer.bounds;
        bound.Expand(extendAmount);
        box.transform.position = bound.center;
        //box.transform.localScale = bound.size;
        box.transform.rotation = transformsToInclude[0].rotation;
    }
}
