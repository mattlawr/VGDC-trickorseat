using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
{
    protected Collider collider;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool PastEdge(Vector3 position)
    {
        if (position.x > collider.bounds.max.x) return true;

        if (position.x < collider.bounds.min.x) return true;

        return false;
    }

    public Vector3 Forward()
    {
        return transform.right;
    }
}
