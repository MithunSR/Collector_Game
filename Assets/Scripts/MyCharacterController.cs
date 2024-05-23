 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MyCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float turnSpeed = 200f;
    public float jumpForce = 5f;

    [Header("UI Elements")]
    public TextMeshProUGUI countText;
    public TextMeshProUGUI timerText;
    public GameObject winText;
    public GameObject lostText;
     public GameObject restartButton;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Timer Settings")]
    public float timeLeft;
    private Animator animator;
    private CharacterController characterController;
    private Vector3 velocity;
    private int count;
    private bool isGrounded;
    private bool timerOn = false;

    [Header("Audio Settings")]
    public AudioSource walkingAudioSource;
    public AudioSource backgroundMusicAudioSource;
    public AudioSource coinAudioSource;
    public AudioSource winAudioSource;
    public AudioSource lostAudioSource;


    void Start()
    {
        // Initialize components
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // Initialize UI elements
        count = 0;
        SetCountText();
        winText.SetActive(false);
        lostText.SetActive(false);
         restartButton.SetActive(false); 
        timerOn = true;
    }

    void Update()
    {
        HandleTimer();
        HandleMovement();
        CheckWinLoseCondition();
    }

    void HandleTimer()
    {
        if (timerOn)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                UpdateTimer(timeLeft);
            }
            else
            {
                Debug.Log("Timer is Up!");
                timeLeft = 0;
                timerOn = false;
              CheckWinLoseCondition();
            }
        }
    }

    void UpdateTimer(float currentTime)
    {
        currentTime += 1;
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("Time left {0:00}:{1:00}", minutes, seconds);
    }

    void HandleMovement()
    {
        // Check if the character is grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        Debug.Log("IsGrounded: " + isGrounded);
        Debug.Log("GroundCheck Position: " + groundCheck.position);
        Debug.Log("GroundCheck Scale: " + groundCheck.localScale);
        Debug.Log("GroundMask: " + groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to ensure the character sticks to the ground
            animator.SetBool("isJumping", false);
        }

        // Initialize movement variables
        float moveVertical = 0f;
        float moveHorizontal = 0f;

        // Get input for movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveVertical = 0.5f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveVertical = - 0.5f;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveHorizontal = - 0.5f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveHorizontal =  0.5f;
        }

        // Update animator parameters for running
        bool isWalking = moveVertical != 0 || moveHorizontal != 0;
        animator.SetBool("isWalking", isWalking);
        animator.SetFloat("Speed", Mathf.Abs(moveVertical));

         Debug.Log("isWalking: " + isWalking);
        Debug.Log("Speed: " + animator.GetFloat("Speed"));


        // Play walking sound if moving
        if (isWalking)
        {
            if (!walkingAudioSource.isPlaying)
            {
                walkingAudioSource.Play();
            }
        }
        else
        {
            if (walkingAudioSource.isPlaying)
            {
                walkingAudioSource.Stop();
            }
        }

        // Move the character
        Vector3 move = transform.forward * moveVertical * speed * Time.deltaTime;
        characterController.Move(move);

        // Turn the character
        transform.Rotate(0, moveHorizontal * turnSpeed * Time.deltaTime, 0);

        // Handle jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            animator.SetBool("isJumping", true);
        }

        // Apply gravity
        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    
}

    void SetCountText()
    {
        countText.text = "Your Score: " + count.ToString();
        if (count >= 10)
        {
            winText.SetActive(true);
        }
    }
void CheckWinLoseCondition()
    {
        if (count >= 10)
        {
            winText.SetActive(true);
            backgroundMusicAudioSource.Stop();
             winAudioSource.Play();
            timerOn = false; // Stop the timer
            speed = 0; // Stop the player movement
            restartButton.SetActive(true); // Show the restart button
        }
        else if (timeLeft <= 0)
        {
            lostText.SetActive(true);
            backgroundMusicAudioSource.Stop();
             winAudioSource.Play();
            lostAudioSource.Play();
            timerOn = false; // Stop the timer
            speed = 0; // Stop the player movement
            restartButton.SetActive(true); // Show the restart button
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            other.gameObject.SetActive(false);
            count += 1;
            SetCountText();
            CheckWinLoseCondition();
    // Play coin collection sound
            if (coinAudioSource != null)
            {
                coinAudioSource.Play();
            }

        }
    }

    // Method to restart the game
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
