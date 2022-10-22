using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float gravity = -9.8f;

    [Space]

    public float moveAmount = 20f;

    public float moveAmountAir = 5f;

    public float jumpAmount = 5f;


    // Custom enumerable type
    public enum PlayerState
    {
        Air, Rail
    }

    // Private vars
    protected Rail currRail;    // represents the rail path we're on

    protected PlayerState state = 0;
    protected Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!rb) return;

        // Enter a method depending on current state
        switch (state)
        {
            case PlayerState.Air:
                _StAirUpdate();
                break;
            case PlayerState.Rail:
                _StRailUpdate();
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other) return;

        // Pickup checks here


        Rail rl = other.GetComponent<Rail>();
        if (rl) OnRailCollision(rl);
    }

    public void OnRailCollision(Rail rl)
    {
        if (state == PlayerState.Rail) return;

        // When we touch a rail, we should "stick" to it
        rb.velocity = new Vector3(rb.velocity.x, 0, 0);
        currRail = rl;
        state = PlayerState.Rail;
    }

    public void OnRailExit()
    {
        currRail = null;
        state = PlayerState.Air;
    }

    protected void _StAirUpdate()
    {
        // Movement
        float h = Input.GetAxisRaw("Horizontal");
        Vector3 move = Vector3.Project(new Vector3(h, 0, 0), Vector3.right).normalized;
        rb.AddForce(move * moveAmountAir, ForceMode.Force);

        // Falling forces
        rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
    }

    protected void _StRailUpdate()
    {
        if(!currRail)
        {
            Debug.LogError("currRail is null while we're in Rail state... that shouldn't be allowed");
            return;
        }

        // Jump
        if(Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpAmount, ForceMode.VelocityChange);
            OnRailExit();
            return;
        }

        // Fall off rail
        if(currRail.PastEdge(rb.position))
        {
            OnRailExit();
            return;
        }

        rb.AddForce(Vector3.up * gravity * 0.5f, ForceMode.Acceleration);   // fall at half amount on rail

        // Movement
        Vector3 f = currRail.Forward();
        float horizontal = Input.GetAxisRaw("Horizontal");
        Vector3 move = Vector3.Project(new Vector3(horizontal, 0, 0), f).normalized;
        rb.AddForce(move * moveAmount, ForceMode.Force);

        Vector3 v = rb.velocity;
        rb.velocity = Vector3.Project(v, f);
    }
}
