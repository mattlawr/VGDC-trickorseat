using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float gravity = -9.8f;
    public float grav_fallAdd = 0f;     // For falling faster
    public float grav_peakScale = 1f;   // For peaking longer
    public float maxFallSpeed = -20f;

    [Space]

    public float moveAmount = 20f;
    public float moveAmountAir = 5f;

    public float jumpAmount = 5f;

    float timeSinceRail = 100f;
    readonly float bufferTime = 0.25f;

    [Space]

    public Animator anim;

    public Transform wheel;

    public Transform scaleWithMovement;

    public ParticleSystem windFX;

    public Resrc.UnityEventToggle onRail;

    public UnityEvent onJump;
    public UnityEvent onTrick;

    public UnityEvent onPickup;

    // Custom enumerable type
    public enum PlayerState
    {
        Air, Rail
    }

    // Private vars
    protected Rail currRail;    // represents the rail path we're on
    Rail railCache = null;

    protected PlayerState state = 0;
    protected Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

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

        // For FX:
        if(scaleWithMovement && rb.velocity.x != 0f)
        {
            float sc = rb.velocity.x / 30f;
            if (Mathf.Abs(sc) < 1f) sc = 1f;
            scaleWithMovement.localScale = new Vector3(1f, 1f, sc);

            Vector3 dir = rb.velocity.normalized;
            scaleWithMovement.rotation = Quaternion.LookRotation(dir, Vector3.up);

        }

        if(windFX)
        {
            float m = rb.velocity.magnitude;
            if (m > 32f)
            {
                windFX.Emit(Mathf.Min(Mathf.FloorToInt(m / 120f), 2));
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other) return;

        // Pickups
        Pickup pk = other.GetComponent<Pickup>();
        if (pk) OnPickup(pk);

        // Rails
        Rail rl = other.GetComponent<Rail>();
        if (rl) OnRailCollision(rl);
    }

    void OnPickup(Pickup p)
    {
        // uh! do something
        // you could support a score var here and print to screen
        onPickup.Invoke();
    }

    void OnRailCollision(Rail rl)
    {
        if (state == PlayerState.Rail) return;

        // Pass through rails...
        if (rl.Forward().y < -0.1f && rb.velocity.y > 2f)
        {
            return;
        }
        if (Input.GetButton("Jump") && rb.velocity.y > 2f)
        {
            return;
        }
        if (rl == railCache && rb.velocity.y > 2f)
        {
            return;
        }

        // When we touch a rail, we should "stick" to it
        Vector3 f = rl.Forward();

        float h = Input.GetAxisRaw("Horizontal");
        if (h != 0)
        {
            // Tiny boost when landing
            rb.AddForce(f * moveAmount * 1.5f * h, ForceMode.VelocityChange);
        }

        Vector3 v = rb.velocity;
        v.y = 0f;
        rb.velocity = Vector3.Project(v, f);

        currRail = rl;
        state = PlayerState.Rail;
        onRail.On();    // shown in inspector!

        anim.SetBool("onrail", true);
    }

    void OnRailExit()
    {
        timeSinceRail = 0f;
        railCache = currRail;
        currRail = null;
        state = PlayerState.Air;
        onRail.Off();

        anim.SetBool("onrail", false);
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
        if (!currRail) currRail = railCache;
        if (!currRail) return;

        float bonus = 1f;

        Vector3 v = rb.velocity;
        v.y = Mathf.Max(v.y, 0f);
        rb.velocity = v;    // cancel out downwards velocity

        if(rb.velocity.magnitude > 2f && currRail.NearEdge(2f, rb.position))
        {
            bonus = 1.5f;
            float h = Input.GetAxisRaw("Horizontal");
            Vector3 f = currRail.Forward();

            rb.AddForce(f * moveAmount * 2f * h, ForceMode.VelocityChange);

            anim.SetTrigger("jumptrick");
            onTrick.Invoke();
        }
        else
        {
            anim.SetTrigger("jump");
        }

        rb.AddForce(Vector3.up * jumpAmount * bonus, ForceMode.VelocityChange);

        onJump.Invoke();

        OnRailExit();
        timeSinceRail = 100f;
    }

    protected void _StAirUpdate()
    {
        // Jump (buffered)
        if (timeSinceRail < bufferTime &&  Input.GetButtonDown("Jump"))
        {
            Jump();
            return;
        }
        timeSinceRail += Time.deltaTime;

        // Movement
        float h = Input.GetAxisRaw("Horizontal");
        Vector3 move = Vector3.Project(new Vector3(h, 0, 0), Vector3.right).normalized;
        rb.AddForce(move * moveAmountAir, ForceMode.Force);

        // Falling forces
        GravityUpdate();
    }

    protected void _StRailUpdate()
    {
        if(!currRail)
        {
            OnRailExit();
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

        PushAlongRail(Vector3.up * gravity * 0.5f, 1f, ForceMode.Acceleration);   // slide down rail

        // Movement
        float horizontal = Input.GetAxisRaw("Horizontal");
        PushAlongRail(new Vector3(horizontal + 0.5f, 0, 0), moveAmount);

        Vector3 v = rb.velocity;
        rb.velocity = Vector3.Project(v, f);

        wheel.Rotate(Vector3.right * rb.velocity.magnitude * 10f * Time.deltaTime * Mathf.Sign(rb.velocity.x));
    }

    private void PushAlongRail(Vector3 v, float amt, ForceMode mode = ForceMode.Force)
    {
        Vector3 f = currRail.Forward();

        Vector3 m = Vector3.Project(v, f).normalized;
        rb.AddForce(m * amt, mode);
    }

    public void StopAndClearParticle(ParticleSystem p)
    {
        p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void GravityUpdate()
    {
        Vector3 v = rb.velocity;
        if (v.y < maxFallSpeed && maxFallSpeed < 0) return;

        Vector3 grav = gravity * Vector3.up;

        // Reduce gravity at peak
        float a = 0.5f;
        if (v.y > -a && v.y < a * 0.1f)
        {
            grav *= grav_peakScale;
        }

        rb.AddForce(grav, ForceMode.Acceleration);

        if (grav_fallAdd > 0f)
        {
            if (v.y < -a)
            {
                Vector3 fall = -grav_fallAdd * Vector3.up;
                rb.AddForce(fall, ForceMode.Acceleration);    // In addition to above force
            }
        }
    }
}
