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

    void OnRailCollision(Rail rl)
    {
        if (state == PlayerState.Rail) return;

        // When we touch a rail, we should "stick" to it
        Vector3 f = rl.Forward();

        float h = Input.GetAxisRaw("Horizontal");
        if (h != 0)
        {
            // Tiny boost when landing
            rb.AddForce(f * moveAmount * 2f * h, ForceMode.VelocityChange);
        }

        Vector3 v = rb.velocity;
        v.y = 0f;
        rb.velocity = Vector3.Project(v, f);

        currRail = rl;
        state = PlayerState.Rail;
    }

    void OnRailExit()
    {
        currRail = null;
        state = PlayerState.Air;
    }

    /// <summary>
    /// Let the player do a jump action.
    /// Types:
    /// - Normal jump
    /// - Trick jump: right at a rail edge!
    ///   * directional input should cause directional jumps:
    ///   Backwards: Indy grab (extra backwards control)
    ///   Neutral: Ollie?
    ///   Forwards: Shove-it, or kickflip (gain speed)
    /// </summary>
    void Jump()
    {
        Vector3 v = rb.velocity;
        v.y = Mathf.Max(v.y, 0f);
        rb.velocity = v;    // cancel out downwards velocity

        rb.AddForce(Vector3.up * jumpAmount, ForceMode.VelocityChange);
        OnRailExit();
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
            Jump();
            return;
        }

        // Fall off rail
        if(currRail.PastEdge(rb.position, rb.velocity))
        {
            OnRailExit();
            return;
        }

        Vector3 f = currRail.Forward();

        rb.AddForce(Vector3.Project(Vector3.up * gravity * 0.5f, f), ForceMode.Acceleration);   // slide down rail

        // Movement
        float horizontal = Input.GetAxisRaw("Horizontal");
        Vector3 move = Vector3.Project(new Vector3(horizontal, 0, 0), f).normalized;
        rb.AddForce(move * moveAmount, ForceMode.Force);

        Vector3 v = rb.velocity;
        rb.velocity = Vector3.Project(v, f);
    }
}
