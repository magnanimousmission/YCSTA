using UnityEngine;
public class Gravity : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    [SerializeField] PlayerCore player;
    private bool isJumping;
    public LayerMask groundLayer;
    public Transform groundCheck; // Place on character's head
    public Transform feetLevel;
    public float raycastDistance = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player.playerInput == null)
            return;

        isJumping = player.playerInput.jump;

        if (!isJumping)
            StickToGround();
    }

    void StickToGround()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            Vector3 corrected = rb.position;
            corrected.y = Mathf.Lerp(rb.position.y, hit.point.y, Time.deltaTime * 10f);
            rb.MovePosition(corrected);
        }
    }

    void OnAnimatorMove()
    {
        Vector3 velocity = animator.deltaPosition;
        velocity.y = 0f; // Ground sticking handles Y entirely
        rb.MovePosition(rb.position + velocity);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }
}