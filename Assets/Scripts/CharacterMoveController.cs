using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveController : MonoBehaviour
{
    [Header("Movement")]
    public float moveAccel;
    public float maxSpeed;

    [Header("Jump")]
    public float jumpAccel;
    public float jumpAccel2;

    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;

    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;
    private float lastPositionX;


    [Header("GameOver")]
    public GameObject gameOverScreen;
    public float fallPositionY;

    [Header("Camera")]
    public CameraMoveController gameCamera;

    private Rigidbody2D rig;

    private bool isJumping;
    private bool isJumping2;
    private bool isOnGround;
    private int jumpCount = 2;

    private Animator anim;
    private SpriteRenderer sprites;

    private CharacterSoundController sound;

    // Start is called before the first frame update
    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<CharacterSoundController>();
        sprites = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        // read input
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Jumping button clicked");
            if (isOnGround)
            {
                isJumping = true;
                sound.PlayJump();
            }

            else if (jumpCount > 0)
            {
                isJumping2 = true;
                sound.PlayJump();
            }
        }

        // change animation
        anim.SetBool("isOnGround", isOnGround);
        anim.SetBool("isDoubleJump", (jumpCount == 0));

        // calculate score
        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPositionX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);

        if (scoreIncrement > 0)
        {
            score.IncreaseCurrentScore(scoreIncrement);
            lastPositionX += distancePassed;
            //Debug.Log("Current score :" + score.GetCurrentScore());
        }

        // game over
        if (transform.position.y < fallPositionY)
        {
            GameOver();
        }
    }

    private void FixedUpdate()
    {
        // raycast ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if (hit)
        {
            //Debug.Log("Raycast hit");
            if (!isOnGround && rig.velocity.y <= 0)
            {
                isOnGround = true;
                isJumping2 = false;
                jumpCount = 2;
            }
        }
        else
        {
            //Debug.Log("Raycast does not hit");
            isOnGround = false;
        }

        // calculate velocity vector
        Vector2 velocityVector = rig.velocity;

        if (isJumping)
        {
            //Debug.Log("Jumping");
            velocityVector.y += jumpAccel;
            isJumping = false;
            jumpCount = 1;
        }

        if (isJumping2)
        {
            //Debug.Log("Jumping");
            if (velocityVector.y < 0)
            {
                velocityVector.y += (jumpAccel2 + 5);
            }
            else
            {
                velocityVector.y += jumpAccel2;
            }
            isJumping2 = false;
            jumpCount = 0;
        }

        velocityVector.x = Mathf.Clamp(velocityVector.x + moveAccel * Time.deltaTime, 0.0f, maxSpeed);

        rig.velocity = velocityVector;
    }

    private void GameOver()
    {
        // set high score
        score.FinishScoring();

        // stop camera movement
        gameCamera.enabled = false;

        // show gameover
        gameOverScreen.SetActive(true);

        // disable this too
        this.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D anotherCollider)
    {
        if (anotherCollider.CompareTag("Obstacle"))
        {
            sprites.enabled = false;
            GameOver();
        }
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * groundRaycastDistance), Color.white);
    }
}
