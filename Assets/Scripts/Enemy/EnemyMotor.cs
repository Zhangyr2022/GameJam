// public class EnemyMotor : CharacterMotor
// {
//     public EnemyControl enemyControl;

//     public override CharacterControl CharacterInput => enemyControl;
// }


using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyMotor : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float walkResponse = 20f;
    public float stopResponse = 10f;
    public float turnResponse = 10f;
    private Rigidbody _rb;
    [Space]
    public Transform rig;

    public float cycleSpeed = 3.5f;
    public float accelerationTilt = 0.5f;
    public float accelerationTiltResponse = 3f;
    public float sway = 2f;
    public float bob = 0.05f;
    public float squash = 0.06f;

    public bool CanMove = false;

    private int _lastFootstep = -1;

    public float CurrentSpeed => _rb.velocity.magnitude;
    public EnemyControl CharacterInput;

    private Vector3 _previousVelocity;
    private float _cycle;


    void FixedUpdate()
    {
        if (!CanMove)
            return;

        var targetRotation = _rb.rotation;
        if (CharacterInput.LookDirection.sqrMagnitude > 0f)
            targetRotation = Quaternion.LookRotation(CharacterInput.LookDirection);
        //rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, turnResponse * Time.fixedDeltaTime);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, turnResponse * Time.fixedDeltaTime));

        var targetVelocity = CharacterInput.Movement * walkSpeed;
        var response = targetVelocity.magnitude >= CurrentSpeed ? walkResponse : stopResponse;
        _rb.velocity = Vector3.Lerp(_rb.velocity, targetVelocity, response * Time.fixedDeltaTime);

        UpdateRig();
    }

    private bool _playedFootstep = true;
    private int _playedFootstepIndex = -1;
    private void UpdateRig()
    {
        _cycle += Time.deltaTime * cycleSpeed * CurrentSpeed;

        var acceleration = (_rb.velocity - _previousVelocity) / Time.deltaTime;
        var axis = Vector3.Cross(acceleration, Vector3.up);

        // tilt
        rig.rotation = Quaternion.Slerp
        (
            rig.rotation,
            Quaternion.AngleAxis(accelerationTilt * acceleration.magnitude, axis) * transform.rotation,
            accelerationTiltResponse * Time.deltaTime
        );

        // sway
        rig.localRotation *= Quaternion.Euler(0f, 0f, Mathf.Sin(_cycle) * sway * CurrentSpeed / walkSpeed);

        //bob
        var h = Mathf.Sin(_cycle * 2f);
        rig.localPosition = Vector3.up * (Mathf.Max(h, 0f) * bob * CurrentSpeed) / walkSpeed;

        // squash
        var s = Mathf.Sin(_cycle * 2f) * CurrentSpeed / walkSpeed * squash;
        SetSquash(s);

        _previousVelocity = _rb.velocity;
    }


    public void SetScale(float s)
    {
        rig.localScale = new Vector3(s, s, s);
    }

    public void SetSquash(float s)
    {
        rig.localScale = new Vector3(1f / (1f + s), 1f + s, 1f / (1f + s));
    }

    void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        CharacterInput = gameObject.GetComponent<EnemyControl>();
    }
}