using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Player : MonoBehaviour
{

    [Header("Serialized References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioClip deathSoundClip;
    [SerializeField] private AudioClip footstepSoundClip;
    [SerializeField] private GameObject bloodPoolPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("Movement Settings")]
    [SerializeField] private float moveStopDistance = 5f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveAcceleration = 5f;
    [SerializeField] private float movementTargetError = 0.3f;

    [Header("World Settings")]
    [SerializeField] private float cameraMaximumY = 10f;
    [SerializeField] private float cameraMinimumY = -1f;
    [SerializeField] private float cameraFollowSpeed = 3f;
    [SerializeField] private float cameraYOffset = 0f;

    public static Player Instance { get; private set; }

    private Game game;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector2 previousPosition;
    private Vector2 targetPosition;
    public Vector2 movementVector;
    new private Collider2D collider;
    public bool isMoving = false;
    private bool wasSlipping = false;
    private bool lastMoveWasHorizontal = false;
    private bool hadVerticalKeyLastFrame = false;
    private bool hadHorizontalKeyLastFrame = false;
    public struct PlayerRewindSnapshot {
        public Vector3 position;
        public Vector2 direction;
        public Vector2 velocity;
        public bool isMoving;
        public bool wasSlipping;
        public Vector2 previousPosition;
        public Vector2 targetPosition;

        public PlayerRewindSnapshot(Vector3 position, Vector2 direction, Vector2 velocity, bool isMoving, bool wasSlipping, Vector2 previousPosition, Vector2 targetPosition) {
            this.position = position;
            this.direction = direction;
            this.velocity = velocity;
            this.isMoving = isMoving;
            this.wasSlipping = wasSlipping;
            this.previousPosition = previousPosition;
            this.targetPosition = targetPosition;
        }
    }
    public List<PlayerRewindSnapshot> snapshots = new List<PlayerRewindSnapshot>();
    private float snapshotTimer = 0f;

    // Start is called before the first frame update

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }
    void Start()
    {
        game = Game.Instance;
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();

        audioSource = this.gameObject.AddComponent<AudioSource>();
    }

    void UpdateCameraPosition() {
        Vector3 cameraPosition = playerCamera.transform.position;
        cameraPosition.y = Mathf.Clamp(transform.position.y + cameraYOffset, cameraMinimumY, cameraMaximumY);
        playerCamera.transform.position = cameraPosition;
    }

    Vector2 GetMoveDirection(float horizontalInput, float verticalInput) {
        Vector2 newMoveDirection = new Vector2(horizontalInput, verticalInput);
        bool hasHorizontalInput = horizontalInput != 0;
        bool hasVerticalInput = verticalInput != 0;
        //no inputs are pressed.
        if (!hasHorizontalInput && !hasVerticalInput) {
            return new Vector2(0,0);
        }
        // only one input is pressed.
        if (!hasHorizontalInput || !hasVerticalInput) {
            return newMoveDirection;
        }
        // both inputs are pressed at the exact same time.
        if (!hadVerticalKeyLastFrame && !hadHorizontalKeyLastFrame) {
            return new Vector2(0,0);
        }
        // two inputs are pressed at the same time, but we had one input last frame.
        // discard the old input in favour of the newly pressed one.
        if (!hadHorizontalKeyLastFrame) {
            newMoveDirection.y = 0;
            return newMoveDirection;
        } else if (!hadVerticalKeyLastFrame) {
            newMoveDirection.x = 0;
            return newMoveDirection;
        }
        // if we're holding both inputs, continue moving in the current direction.
        if (lastMoveWasHorizontal) {
            newMoveDirection.y = 0;
            return newMoveDirection;
        } else {
            newMoveDirection.x = 0;
            return newMoveDirection;
        }
    }

    public void SetSpriteOrientation(Vector2 direction) {
        if (direction.Equals(Vector2.zero)) {
            return;
        } else if (direction.Equals(Vector2.up)) {
            spriteRenderer.sprite = upSprite;
        } else if (direction.Equals(Vector2.down)) {
            spriteRenderer.sprite = downSprite;
        } else if (direction.Equals(Vector2.left)) {
            spriteRenderer.sprite = leftSprite;
        } else if (direction.Equals(Vector2.right)) {
            spriteRenderer.sprite = rightSprite;
        }
    }

    void StartMovement() {
       // player moves in segments, pokemon style.
       // player can only move in one direction at a time.
       // if the player holds two keys down, they will move in the direction of the last key pressed.
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        // get the direction the player wants to move in.
        // Debug.Log("horizontalInput: " + horizontalInput + ", verticalInput: " + verticalInput);
        Vector2 newMovementVector = GetMoveDirection(horizontalInput, verticalInput);
        //Debug.Log("newMovementVector: " + newMovementVector);
        // record inputs for next frame.
        hadVerticalKeyLastFrame = verticalInput != 0;
        hadHorizontalKeyLastFrame = horizontalInput != 0;

        if (newMovementVector.Equals(Vector2.zero)) {
            return;
        }




        targetPosition = new Vector2(transform.position.x, transform.position.y) + (newMovementVector * moveStopDistance);
        //Debug.Log("num hits " + numHits);
        lastMoveWasHorizontal = newMovementVector.x != 0;
        isMoving = true;
        movementVector = newMovementVector;
        SetSpriteOrientation(movementVector);
        Debug.Log("Begin movement towards: " + targetPosition);
    }

    bool InContactWithIce() {
        return Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Ice"));
    }

    // move the player towards the target position.
    // if they have reached it, set isMoving to false.
    void UpdateMovement(bool dontRecursivelyCall = false) {
        float currentMoveSpeed = Mathf.Abs(rigidBody.velocity.y);
        float distance = Vector2.Distance(transform.position, targetPosition);
        bool hasReachedTarget = distance < movementTargetError;
        if (lastMoveWasHorizontal) {
            currentMoveSpeed = Mathf.Abs(rigidBody.velocity.x);
            distance = Mathf.Abs(transform.position.x - targetPosition.x);
            hasReachedTarget = distance < movementTargetError;
            //Debug.Log("Moving horizontally at speed: " + currentMoveSpeed + " with remaining distance to target of: " + distance);
        } else {
            //Debug.Log("Moving vertically at speed: " + currentMoveSpeed + " with remaining distance to target of: " + distance);
        }

        // cast the players' collider to check if they can move in the direction they want to.
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Wall"));
        int numHits = collider.Cast(movementVector, filter, hits, moveStopDistance);
        if (numHits > 0) {
            Debug.Log("Hit a wall.");
            isMoving = false;
            wasSlipping = false;
            rigidBody.velocity = new Vector2(0,0);
            return;
        }
        bool isOnIce = InContactWithIce();
        if (isOnIce) {
            //Debug.Log("Player is on ice.");
            wasSlipping = true;
        } 
        if ((hasReachedTarget && !isOnIce) || (!isOnIce && wasSlipping)) {
            Debug.Log("Reached target position: " + targetPosition);
            isMoving = false;
            wasSlipping = false;
            // try to immediately start moving again.
            StartMovement();
            if (isMoving && !dontRecursivelyCall) {
                UpdateMovement(true);
                return;
            }
            //TODO decelerate instead of stopping instantly.
            rigidBody.velocity = new Vector2(0,0);
            if (footstepSoundClip != null) {
                audioSource.clip = footstepSoundClip;
                audioSource.Play();
            }
            return;
        }
        
        currentMoveSpeed = Mathf.Min(currentMoveSpeed + (moveAcceleration * Time.deltaTime), moveSpeed);
        rigidBody.velocity = movementVector * currentMoveSpeed;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (game.gameIsOver) {
            return;
        }
        if (isMoving) {
            UpdateMovement();
        } else {
            StartMovement();
        }
        UpdateCameraPosition();
        
        if (snapshotTimer <= 0) {
            TakeRewindSnapshot();
            snapshotTimer = game.snapShotInterval;
        } else {
            snapshotTimer -= Time.deltaTime;
        }
    }

    public IEnumerator Rewind() {
        Debug.Log("Rewinding player...");
        int numSnapshots = (int) Mathf.Min( snapshots.Count, game.rewindTime / game.snapShotInterval);
        Debug.Log("Number of snapshots: " + numSnapshots);
        for (int i = snapshots.Count - 1 ; i >= snapshots.Count - numSnapshots; i--) {
            PlayerRewindSnapshot snapshot = snapshots[i];
            Debug.Log("Rewinding to snapshot: " + i);
            Debug.Log("Rewinding to position: " + snapshot.position + " with turning: " + snapshot.direction.x + ", " + snapshot.direction.y);
            this.gameObject.transform.position = snapshot.position;
            movementVector = snapshot.direction;
            SetSpriteOrientation(movementVector);
            rigidBody.velocity = Vector2.zero;
            isMoving = false;
            wasSlipping = false;
            previousPosition = snapshot.position;
            targetPosition = snapshot.position;
            
            yield return new WaitForSeconds(game.snapShotInterval);
        }
        Debug.Log("Finished rewinding.");
        for (int i = 0; i < numSnapshots; i++) {
            snapshots.RemoveAt(snapshots.Count - 1);
        }
    }

    void TakeRewindSnapshot() {
        float snapShotInterval = game.snapShotInterval;
        float rewindTime = game.rewindTime;
        int numSnapshots = (int)(rewindTime / snapShotInterval)*3;
        if (snapshots.Count >= numSnapshots) {
            snapshots.RemoveAt(0);
        }
        snapshots.Add(new PlayerRewindSnapshot(transform.position, movementVector, rigidBody.velocity, isMoving, wasSlipping, previousPosition, targetPosition));
    }

}
