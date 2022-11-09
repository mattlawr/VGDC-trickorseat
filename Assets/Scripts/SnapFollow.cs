using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapFollow : MonoBehaviour
{
    public Transform targetX;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(targetX)
        {
            Vector3 loc = transform.parent.TransformPoint(Vector3.right);
            float currX = Mathf.Floor(targetX.position.x / loc.x);

            //print($"{targetX.position.x} / {loc.x} = {currX}");

            Vector3 v = transform.localPosition;
            v.x = currX;
            transform.localPosition = v;
        }
    }
}
