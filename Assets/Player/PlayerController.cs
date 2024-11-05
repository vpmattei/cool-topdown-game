using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    [SerializeField]
    private float _speed = 5f;
    [SerializeField]
    private int _speedBoost = 3;
    [SerializeField]
    private int _jumpHeight = 10;
    private Vector3 _moveDirection = new Vector3(0, 0, 0);

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnSprintStart()
    {
        _speed *= _speedBoost;
        Move();
    }

    private void OnSprintEnd()
    {
        _speed /= _speedBoost;
        Move();
    }

    private void OnMove(InputValue value)
    {
        Vector2 v2 = value.Get<Vector2>();
        _moveDirection = new Vector3(v2.x, 0, v2.y);
        Move();
    }

    private void Move()
    {
        _rigidbody.linearVelocity = new Vector3(_moveDirection.x * _speed,
                                                _rigidbody.linearVelocity.y,
                                                _moveDirection.z * _speed);
    }

    private void OnJump()
    {
        _rigidbody.AddForce(new Vector3(0, 10, 0) * _jumpHeight);
    }
}
