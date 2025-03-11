using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //////////////////////////////////////////////
    // SERIALIZED SETTINGS (SET IN INSPECTOR)
    //////////////////////////////////////////////
    [Header("Movement Settings")]
    public float moveSpeed = 8f;           // Ground movement speed
    public float jumpForce = 10f;          // Base jump power
    public float gravity = -25f;           // Custom gravity value
    public float airControlMultiplier = 0.5f; // How much control in air (0-1)
    public float horizontalBound = 8f;     // Screen edge limits

    [Header("Combat Settings")]
    public GameObject rocketPrefab;        // Prefab reference
    public float rocketCooldown = 0.5f;    // Time between shots

    [Header("Collision Detection")]
    public float groundCheckDistance = 0.6f; // Ray length for ground check
    public LayerMask groundLayer;          // MUST SET TO PLATFORM LAYER!

    //////////////////////////////////////////////
    // PRIVATE STATE
    //////////////////////////////////////////////
    private Rigidbody2D rb;                // Physics component
    private Vector2 velocity;              // Custom velocity system
    private bool canShoot = true;          // Rocket cooldown flag
    private float maxHeight;               // Score tracking
    private int jumpsRemaining;            // Jump counter
    private bool wasGrounded;              // Ground state tracking

    //////////////////////////////////////////////
    // INITIALIZATION
    //////////////////////////////////////////////
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;               // DISABLE UNITY GRAVITY
        jumpsRemaining = 1;                // Start with single jump
    }

    //////////////////////////////////////////////
    // MAIN UPDATE LOOP
    //////////////////////////////////////////////
    void Update()
    {
        HandleMovement();      // Movement and jumping
        TrackHeight();         // Score calculation
        ShootRocket();         // Firing mechanic
        EnforceBounds();       // Screen constraints
    }

    //////////////////////////////////////////////
    // MOVEMENT SYSTEM (CORE FIXES HERE)
    //////////////////////////////////////////////
    void HandleMovement()
    {
        // 1. HORIZONTAL MOVEMENT (FIXED)
        float moveInput = Input.GetAxisRaw("Horizontal");
        velocity.x = moveInput * moveSpeed * (IsGrounded() ? 1 : airControlMultiplier);

        // 2. VERTICAL MOVEMENT
        velocity.y += gravity * Time.deltaTime; // Apply custom gravity

        // 3. JUMP HANDLING (CRITICAL FIX)
        bool isGrounded = IsGrounded();

        // Reset jumps when FIRST touching ground
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = 1; // Reset to single jump
        }

        // Jump input handling
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpsRemaining > 0)
            {
                velocity.y = jumpForce;
                jumpsRemaining--;

                // If mid-air, consume all jumps
                if (!isGrounded) jumpsRemaining = 0;
            }
        }

        // 4. UPDATE PHYSICS
        rb.velocity = velocity;
        wasGrounded = isGrounded;
    }

    //////////////////////////////////////////////
    // GROUND DETECTION (RAYCAST SYSTEM)
    //////////////////////////////////////////////
    bool IsGrounded()
    {
        float width = GetComponent<BoxCollider2D>().bounds.extents.x;
        Vector2 origin = transform.position;

        // Triple raycast for edge detection
        bool leftHit = Physics2D.Raycast(origin + Vector2.left * width,
                                       Vector2.down,
                                       groundCheckDistance,
                                       groundLayer);

        bool centerHit = Physics2D.Raycast(origin,
                                         Vector2.down,
                                         groundCheckDistance,
                                         groundLayer);

        bool rightHit = Physics2D.Raycast(origin + Vector2.right * width,
                                        Vector2.down,
                                        groundCheckDistance,
                                        groundLayer);

        // Debug visuals
        Debug.DrawRay(origin + Vector2.left * width,
                    Vector2.down * groundCheckDistance,
                    leftHit ? Color.green : Color.red);

        Debug.DrawRay(origin,
                    Vector2.down * groundCheckDistance,
                    centerHit ? Color.green : Color.red);

        Debug.DrawRay(origin + Vector2.right * width,
                    Vector2.down * groundCheckDistance,
                    rightHit ? Color.green : Color.red);

        return leftHit || centerHit || rightHit;
    }

    //////////////////////////////////////////////
    // SCREEN BOUNDARIES
    //////////////////////////////////////////////
    void EnforceBounds()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -horizontalBound, horizontalBound);
        transform.position = pos;
    }

    //////////////////////////////////////////////
    // ROCKET SYSTEM 
    //////////////////////////////////////////////
    void ShootRocket()
    {
        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 shootDirection = (mousePosition - (Vector2)transform.position).normalized;

            Instantiate(rocketPrefab, transform.position, Quaternion.identity)
                .GetComponent<Rocket>().Initialize(shootDirection);

            canShoot = false;
            Invoke(nameof(ResetShoot), rocketCooldown);
        }
    }

    void ResetShoot() => canShoot = true;

    //////////////////////////////////////////////
    // EXPLOSION HANDLING
    //////////////////////////////////////////////
    public void ApplyRocketForce(Vector2 explosionPosition, float force)
    {
        Vector2 direction = ((Vector2)transform.position - explosionPosition).normalized;
        Vector2 addedVelocity = direction * force;
        velocity += addedVelocity;

        // Log the explosion force and added velocity
        Debug.Log($"Explosion Force: {force}");
        Debug.Log($"Added Velocity: {addedVelocity}");
    }

    //////////////////////////////////////////////
    // SCORE SYSTEM
    //////////////////////////////////////////////
    void TrackHeight() => maxHeight = Mathf.Max(maxHeight, transform.position.y);
    public float GetHeight() => maxHeight;
}