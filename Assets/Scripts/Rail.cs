using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
{
    protected Collider col;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool PastEdge(Vector3 position, Vector3 velocity)
    {
        if (position.x > col.bounds.max.x)
        {
            if(velocity.x > 0)
                return true;
        }
        else if (position.x < col.bounds.min.x)
        {
            if (velocity.x < 0)
                return true;
        }

        return false;
    }

    public bool NearEdge(float worldAmount, Vector3 position)
    {
        if (position.x > col.bounds.max.x - worldAmount)
        {
            return true;
        }
        else if (position.x < col.bounds.min.x + worldAmount)
        {
            return true;
        }

        return false;
    }

    public Vector3 Forward()
    {
        return transform.right;
    }
}
