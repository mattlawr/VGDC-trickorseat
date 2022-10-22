using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns stuff as we move along the world X coordinates...
/// </summary>
public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public Rigidbody target;

    [Space]

    public float frequency = 1f;    // per... 10 units?
    public float randYOffset = 20f;
    public float randZRot = 40f;

    float lastX = -100f;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 p = transform.position;
        p.z = 0f;
        transform.position = p;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.x - lastX > 10f / frequency)
        {
            // Spawn
            float y = Random.Range(-randYOffset, randYOffset);

            if(target)
            {
                y += target.velocity.y; // try to catch player
            }

            GameObject go = Instantiate(prefab, transform.position + Vector3.up * y, Quaternion.identity);
            go.transform.Rotate(new Vector3(0f, 0f, Random.Range(-randZRot, randZRot)));

            Destroy(go, 10f);

            lastX = transform.position.x;
        }
    }
}
