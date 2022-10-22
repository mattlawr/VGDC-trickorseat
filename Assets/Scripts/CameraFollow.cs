using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform plyr;

    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - plyr.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = plyr.position + offset;
    }

    
}
