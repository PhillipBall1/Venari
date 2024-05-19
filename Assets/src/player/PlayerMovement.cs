using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    [Header("Physics")]
    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;
    public float jumpHeight = 3f;
    public float airControl = 0.6f;
    public float slopeSlideSpeed = 1f;
    [Header("Speed")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 2.5f;
    [Header("Ground Checks")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public float groundCheckRadius = 0.4f;
    [Header("Footsteps")]
    public float minTimeBetweenFootsteps = 0.25f;
    public float maxTimeBetweenFootsteps = 0.5f;

    [Header("Smoothing")]
    public float crouchSmooth = 5f;

    private Vector3 velocity;
    private bool isGrounded;
    private float speed;
    private float originalHeight;
    private float targetHeight;

    private Vector3 move;
    private float footstepTimer = 0f;


    void Start()
    {
        originalHeight = controller.height;
        targetHeight = originalHeight;
        speed = walkSpeed;
    }

    void Update()
    {
        Movement();
        Crouching();
        Jumping();
        FootSteps();
    }

    private void Movement()
    {
        // Ground check using the new method
        isGrounded = IsGrounded();

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        move = transform.right * horizontal + transform.forward * vertical;

        

        // Movement handling
        if (isGrounded || (!isGrounded && move != Vector3.zero))
        {
            controller.Move(move * speed * Time.deltaTime);
        }
        else
        {
            // Air control
            controller.Move(move * speed * airControl * Time.deltaTime);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void FootSteps()
    {
        // Update the footstep timer
        footstepTimer += Time.deltaTime;

        // Check for movement to play footstep sounds
        if (IsGrounded() && move.magnitude > 0.1f) // Check for significant movement
        {
            float timeBetweenFootsteps = CalculateFootstepInterval();

            if (footstepTimer >= timeBetweenFootsteps)
            {
                footstepTimer = 0f; // Reset timer
                PlayFootstepSound();
            }
        }
    }

    private float CalculateFootstepInterval()
    {
        // Normalize the speed to a range of 0 (min speed) to 1 (max speed)
        float normalizedSpeed = (speed - crouchSpeed) / (runSpeed - crouchSpeed);
        // Interpolate between min and max intervals based on speed
        return Mathf.Lerp(maxTimeBetweenFootsteps, minTimeBetweenFootsteps, normalizedSpeed);
    }

    private void PlayFootstepSound()
    {
        RaycastHit hit;
        if (Physics.Raycast(groundCheck.position, -Vector3.up, out hit, groundDistance))
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if(terrain != null)
            {
                Texture2D terrainTexture = GetMainTexture(hit.point, terrain);
                switch(terrainTexture.name)
                {
                    case "PWT_Grass_03_Dark_D": AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.footstepAudio, "Dirt", transform.position); break;
                    case "PW_Grass_Mountain_01_D": AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.footstepAudio, "Dirt", transform.position); break;
                    case "PW_Forest_Path_D": AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.footstepAudio, "Dirt", transform.position); break;
                    case "PW_Ground_Forest_Sequia_01_D": AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.footstepAudio, "Dirt", transform.position); break;
                    case "PW_Stone_Grey_01_D Cliff Shore": AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.footstepAudio, "Stone", transform.position); break;
                    case "PW_Stone_Grey_02_D Cliff Moss Detail": AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.footstepAudio, "Stone", transform.position); break;
                }
            }
            else
            {
                AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.footstepAudio, LayerMask.LayerToName(hit.collider.gameObject.layer), transform.position);
            }
            
        }
    }

    private void Jumping()
    {
        if (velocity.y < 0)
        {
            // Apply increased gravity when falling
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            // Apply normal gravity otherwise
            velocity.y += gravity * Time.deltaTime;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Crouching()
    {
        // Smooth crouching transition
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchSmooth);
    }

    public bool IsGrounded()
    {
        Vector3[] pointsToCheck = {
        groundCheck.position,
        groundCheck.position + groundCheck.right * groundCheckRadius,
        groundCheck.position - groundCheck.right * groundCheckRadius,
        groundCheck.position + groundCheck.forward * groundCheckRadius,
        groundCheck.position - groundCheck.forward * groundCheckRadius
        };

        RaycastHit hit;
        foreach (Vector3 point in pointsToCheck)
        {
            if (Physics.Raycast(point, -Vector3.up, out hit, groundDistance))
            {
                return true;
            }
        }
        return false;
    }

    public float GetForwardVelocity()
    {
        return move.magnitude * speed;
    }

    private Texture2D GetMainTexture(Vector3 worldPosition, Terrain terrain)
    {
        // Convert the world position to terrain coordinates
        Vector3 terrainPosition = worldPosition - terrain.transform.position;
        Vector2 normalizedPosition = new Vector2(terrainPosition.x / terrain.terrainData.size.x, terrainPosition.z / terrain.terrainData.size.z);

        // Get the texture index at the normalized position
        int x = Mathf.FloorToInt(normalizedPosition.x * terrain.terrainData.alphamapWidth);
        int y = Mathf.FloorToInt(normalizedPosition.y * terrain.terrainData.alphamapHeight);
        float[,,] alphaMap = terrain.terrainData.GetAlphamaps(x, y, 1, 1);
        int textureIndex = 0;
        float maxAlpha = 0;
        for (int i = 0; i < alphaMap.GetLength(2); i++)
        {
            if (alphaMap[0, 0, i] > maxAlpha)
            {
                maxAlpha = alphaMap[0, 0, i];
                textureIndex = i;
            }
        }

        // Return the corresponding texture
        return terrain.terrainData.splatPrototypes[textureIndex].texture;
    }

    #region Inputs

    public void Jump()
    {
        if (isGrounded)
        {
            // Initial velocity spike for jump
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Crouch()
    {
        if (isGrounded)
        {
            // Initial velocity spike for jump
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void StartCrouching()
    {
        targetHeight = originalHeight / 2;
        speed = crouchSpeed * 0.9f;
        groundCheck.transform.localPosition = new Vector3(0, 0.46f, 0);
    }

    public void StopCrouching()
    {
        targetHeight = originalHeight;
        speed = walkSpeed;
        groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);
    }

    public void StartSprinting()
    {
        if (targetHeight == originalHeight) speed = runSpeed;
        else speed = walkSpeed;
    }

    public void StopSprinting()
    {
        if (targetHeight == originalHeight) speed = walkSpeed;
        else speed = crouchSpeed * 0.9f;
    }
    #endregion
}
