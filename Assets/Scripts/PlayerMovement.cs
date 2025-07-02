using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float moveSpeed = 5f;         // your normal walk speed
    public float sprintSpeed = 10f;      // your sprint speed

    [Header("Drag Settings")]
    public float groundDrag = 5f;        // drag when grounded
    public float sprintDragMultiplier = 1.5f; // extra drag when sprinting

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.5f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;

    [Header("Stamina (optional)")]
    public GameObject stami; 

    [Header("Orientation & Gravity")]
    public Transform orientation;
    public static float globalGravity = -9.81f;
    public float gravityScale = 1.0f;

    // internally used
    private Rigidbody rb;
    private StaminaRegen staminaRegen;
    private bool readyToJump = true;
    private bool isSprinting;
    private bool sprintingLocked = false;
    private float horizontalInput;
    private float verticalInput;
    private bool grounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (stami != null)
            staminaRegen = stami.GetComponent<StaminaRegen>();
    }

    void Update()
    {
        // 1) Check if we’re on the ground
        grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            playerHeight * 0.5f + 0.3f,
            whatIsGround
        );

        // 2) Sprint toggle & stamina drain
        HandleSprint();

        // 3) Read movement & jump input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput   = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // 4) Adjust drag: more drag when sprinting for better control
        rb.linearDamping = grounded ? groundDrag : 0f;
        if (isSprinting && grounded)
            rb.linearDamping = groundDrag * sprintDragMultiplier;
    }

    void FixedUpdate()
    {
        // apply custom gravity
        Vector3 gravity = globalGravity * gravityScale * Vector3.up;
        rb.AddForce(gravity, ForceMode.Acceleration);

        // move & cap speed
        MovePlayer();
        SpeedControl();
    }

    private void HandleSprint()
    {
        if (staminaRegen == null) { isSprinting = false; return; }

        // If not currently sprinting, only allow starting if stamina >= 5 and shift is pressed
        if (!isSprinting && Input.GetKey(sprintKey) && staminaRegen.currentStamina >= 5 && !sprintingLocked)
        {
            isSprinting = true;
        }
        // If currently sprinting, keep sprinting as long as shift is held and stamina > 0
        if (isSprinting && Input.GetKey(sprintKey) && staminaRegen.currentStamina > 0)
        {
            staminaRegen.DrainStamina((int)(10 * Time.deltaTime));
        }
        else
        {
            isSprinting = false;
        }
        // If stamina hits 0, stop sprinting and lock sprinting until stamina >= 5
        if (staminaRegen.currentStamina == 0)
        {
            isSprinting = false;
            sprintingLocked = true;
        }
        // If stamina recovers to 5 or more, unlock sprinting
        if (staminaRegen.currentStamina >= 5)
        {
            sprintingLocked = false;
        }
    }

    private void MovePlayer()
    {
        // pick the correct speed each frame
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // form our move direction
        Vector3 moveDir = orientation.forward * verticalInput
                        + orientation.right   * horizontalInput;
        moveDir.Normalize();

        // slope check
        if (Physics.Raycast(
                transform.position,
                Vector3.down,
                out RaycastHit hit,
                playerHeight * 0.5f + 0.3f
            ) && grounded &&
            Vector3.Angle(Vector3.up, hit.normal) > 0.1f)
        {
            Vector3 slopeDir = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;
            slopeDir.y = 0f;
            rb.AddForce(slopeDir * currentSpeed * 10f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDir * currentSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDir * currentSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        // only clamp horizontal velocity
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float cap = isSprinting ? sprintSpeed : moveSpeed;

        if (flatVel.magnitude > cap)
        {
            Vector3 limited = flatVel.normalized * cap;
            rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
        }
    }

    private void Jump()
    {
        // zero out Y then impulse
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() => readyToJump = true;
}
