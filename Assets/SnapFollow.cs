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
            Vector3 loc = transform.TransformPoint(Vector3.right);
            float currX = Mathf.Floor(targetX.position.x / loc.x);

            Vector3 v = transform.position;
            v.x = currX;
            transform.position = v;
        }
    }
}
