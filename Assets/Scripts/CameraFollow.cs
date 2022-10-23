using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Rigidbody plyr;

    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - plyr.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 vf = plyr.position + offset + Vector3.ClampMagnitude(plyr.velocity * 0.1f, 5f);
        transform.position = Vector3.Lerp(transform.position, vf, Time.deltaTime * 15f);
    }

    
}
