using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Status")]
    public Direction direction;
    [SerializeField] private bool running;
    public bool isGrounded;

    [Header("Options")]
    [SerializeField] private float speed = 3;
    [SerializeField] private float jumpForce = 3;
    [SerializeField] private float maxJumpPos = 1;
    
    [Header("Ground")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    
    //others
    private Vector3 velocity;
    private float velocity_X;
    private float movementLayerWeight;

    public enum Direction
    {
        Right,
        Left
    }

    private Router _router;

    private void Start()
    {
        _router = FindFirstObjectByType<Router>();
    }

    void Update()
    {
        SetVelocity();
        CheckGrounded();
        SetMovementAnim();
        Controller();
    }

    void Controller()
    {
        if (_router.player.combat.death) return;
        
        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.D))
                Move(Direction.Right);
            else if (Input.GetKey(KeyCode.A))
                Move(Direction.Left);
        }

        if (((Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) || (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))) && isGrounded && running)
        {
            velocity = Vector3.zero;
            running = false;
        }
    }

    void Move(Direction direction)
    {
        if (_router.player.climb.climbing) return;

        if (this.direction != direction)
        {
            velocity.x = 0;
            this.direction = direction;
        }
        
        int multiple = direction == Direction.Right ? 1 : -1;

        float speedMultiple = _router.player.combat.mode ? 0.3f : 1;
        velocity = new Vector3(speed * multiple * speedMultiple, velocity.y, velocity.z);
        running = true;

        float scaleAbs = Mathf.Abs(transform.localScale.z);
        SetScaleZ(direction == Direction.Right ? scaleAbs : -scaleAbs);
    }

    void SetVelocity()
    {
        if (_router.player.climb.climbing) return;

        float targetVelocityX = velocity.x + _router.player.combat.force_X;
        float baseLerpSpeed = 7f;
        float lerpSpeed = targetVelocityX > 0 ? baseLerpSpeed : baseLerpSpeed * 2;
        velocity_X = Mathf.Lerp(velocity_X, targetVelocityX, lerpSpeed * Time.deltaTime);
        _router.player.status.rigid.velocity = new Vector3(velocity_X, _router.player.status.rigid.velocity.y, _router.player.status.rigid.velocity.z);
    }
    
    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        _router.player.status.anim.SetBool("isGrounded", isGrounded);
    }
    
    public void Jump()
    {
        _router.player.status.anim.SetTrigger("isGroundedTrigger");
        _router.player.status.rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void SetScaleZ(float value)
    {
        Vector3 scale = transform.localScale;
        scale.z = value;
        transform.localScale = scale;
    }

    void SetMovementAnim()
    {
        float lerpSpeed = 3;
        
        bool movement = (running || !isGrounded) && !_router.player.climb.climbing;
        movementLayerWeight = Mathf.Lerp(movementLayerWeight, movement ? 1 : 0, lerpSpeed * Time.deltaTime);
        
        int layerIndex = _router.player.status.anim.GetLayerIndex("Movement");
        _router.player.status.anim.SetLayerWeight(layerIndex, movementLayerWeight);
    }

    private void OnCollisionEnter(Collision other)
    {
        velocity.x = 0;
    }
}
