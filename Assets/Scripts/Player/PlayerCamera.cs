using UnityEngine;

[DefaultExecutionOrder(1)]

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Instance;

    public Camera Camera;
    public float FollowDuration = 0.3f;
    public float LookAhead = 1f;
    [Space]
    public float ShakeSpeed = 5f;
    public float ShakeDamping = 5f;
    public float ShakeStrength = 0.3f;

    private Vector3 _initialOffset;
    private Vector3 _positionVelocityRef;
    private float _shakeT;
    public float CurrentShakeStrength;

    private Player _player => Player.Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Camera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        _initialOffset = transform.position - _player.transform.position;
    }

    private void FixedUpdate()
    {
        if (_player)
        {
            var playerTransform = _player.transform;
            var playerPosition = playerTransform.position;
            var targetPosition = playerPosition + _player.PlayerInputHandler.LookDirection * LookAhead;
            targetPosition.y = 0f;

            transform.position =
               Vector3.SmoothDamp(transform.position, targetPosition + _initialOffset, ref _positionVelocityRef,
                  FollowDuration);
        }

        UpdateShake();
    }

    private void UpdateShake()
    {
        _shakeT += Time.fixedDeltaTime * ShakeSpeed;
        CurrentShakeStrength = Mathf.Lerp(CurrentShakeStrength, 0f, ShakeDamping * Time.deltaTime);

        if (CurrentShakeStrength > 0.001f)
        {
            Camera.transform.localPosition = Noisy.Noise3D(_shakeT, 1f, CurrentShakeStrength, 0.5f, 2f, 1);
        }
    }

    private void OnLightningSpawned()
    {
        CurrentShakeStrength += ShakeStrength;
    }
}