using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform cam;

    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - cam.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 p = transform.position;
        p.y = cam.position.y + offset.y;
        transform.position = p;
    }
}
