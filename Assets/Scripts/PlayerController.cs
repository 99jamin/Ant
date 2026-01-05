using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private Animator _animator;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }
    
    private void Update()
    {
        UpdateAnimationState();
    }
    
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        _rb.velocity = _moveInput * moveSpeed;

        _sr.flipX = _moveInput.x switch
        {
            < 0 => true,
            > 0 => false,
            _ => _sr.flipX
        };
    }
    
    private void UpdateAnimationState()
    {
        float currentSpeed = _moveInput.magnitude;
        _animator.SetFloat(SpeedHash, currentSpeed);
    }
}