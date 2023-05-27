using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Player : MonoBehaviour
{
    public static Player Instance;

    public float WalkSpeed = 5f;
    public float WalkResponse = 20f;
    public float StopResponse = 10f;
    public float TurnResponse = 10f;
    public Rigidbody rb;
    [Space]
    public Transform rig;

    public float CycleSpeed = 3.5f;
    public float AccelerationTilt = 0.5f;
    public float AccelerationTiltResponse = 3f;
    public float Sway = 2f;
    public float Bob = 0.05f;
    public float Squash = 0.06f;

    public AudioSource FootstepSource;
    public AudioItem[] Footsteps;

    private bool _playedFootstep = true;
    private int _playedFootstepIndex = -1;
    private int _lastFootstep = -1;

    public float CurrentSpeed => rb.velocity.magnitude;

    public PlayerInput PlayerInputHandler;
    public CharacterControl CharacterInput => PlayerInputHandler;

    private Vector3 _previousVelocity;
    private float _cycle;

    /// <summary>
    /// Health
    /// </summary>
    public const float MaxHealth = 10;
    public bool IsDead
    {
        get
        {
            return _health < 1e-6;
        }
    }
    private float _health;

    /// <summary>
    /// Get world position of mouse
    /// </summary>
    private Vector3 _lastMouseWorldPosition;

    public void Damage()
    {
        this._health = 0;
    }

    public void PickUpWeapon()
    {
        Weapon.Instance.Pickup(this.transform);
    }
    public void DropWeapon()
    {
        Weapon.Instance.Drop();
    }
    public void Shoot()
    {
        // Compute bullet direction
        Vector3 direction = GetMouseWorldPosition() - this.transform.position;

        // Whether the mouse touches the enemy
        GameObject mouseRaycastObject = GetMouseRaycastObject();
        if (mouseRaycastObject is not null)
        {
            if (mouseRaycastObject.layer == LayerMask.NameToLayer("enemy"))
            {
                // Shoot
                Weapon.Instance.ShootBullets(direction);
            }
            else
            {
                // AOE
                Weapon.Instance.AOEShoot();
            }
        }
    }



    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        this._health = MaxHealth;
        this.PlayerInputHandler = PlayerInput.Instance;
        this.rig = this.transform;
        this.rb = this.GetComponent<Rigidbody>();
        // FootStep Sound
        FootstepSource = this.GetComponent<AudioSource>();
        List<AudioItem> footstepsSoundList = new();
        footstepsSoundList.Add(new AudioItem(Resources.Load<AudioClip>("Audio/FootStep")));
        this.Footsteps = footstepsSoundList.ToArray();
    }
    private void FixedUpdate()
    {
        var targetRotation = rb.rotation;
        if (CharacterInput.LookDirection.sqrMagnitude > 0f)
            targetRotation = Quaternion.LookRotation(CharacterInput.LookDirection);
        //rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, TurnResponse * Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, TurnResponse * Time.fixedDeltaTime));

        var targetVelocity = CharacterInput.Movement * WalkSpeed;
        var response = targetVelocity.magnitude >= CurrentSpeed ? WalkResponse : StopResponse;
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, response * Time.fixedDeltaTime);
        Debug.Log(targetVelocity);
        UpdateRig();
    }
    private void LateUpdate()
    {
        _lastMouseWorldPosition = GetMouseWorldPosition();
    }

    private void UpdateRig()
    {
        _cycle += Time.deltaTime * CycleSpeed * CurrentSpeed;

        var acceleration = (rb.velocity - _previousVelocity) / Time.deltaTime;
        var axis = Vector3.Cross(acceleration, Vector3.up);

        // tilt
        rig.rotation = Quaternion.Slerp
        (
            rig.rotation,
            Quaternion.AngleAxis(AccelerationTilt * acceleration.magnitude, axis) * transform.rotation,
            AccelerationTiltResponse * Time.deltaTime
        );

        // Sway
        rig.localRotation *= Quaternion.Euler(0f, 0f, Mathf.Sin(_cycle) * Sway * CurrentSpeed / WalkSpeed);

        // Bob
        var h = Mathf.Sin(_cycle * 2f);
        if (h < 0f)
        {
            if (!_playedFootstep && Footsteps.Length > 0)
            {
                _playedFootstep = true;
                var newSoundIndex = _playedFootstepIndex;
                while (newSoundIndex == _playedFootstepIndex && Footsteps.Length > 1)
                {
                    newSoundIndex = Random.Range(0, Footsteps.Length);
                }
                newSoundIndex = Mathf.Max(0, newSoundIndex);
                var sound = Footsteps[newSoundIndex];
                FootstepSource.pitch = 1f + Random.Range(-sound.pitchVariance / 2f, sound.pitchVariance / 2f);
                FootstepSource.PlayOneShot(sound.clip, sound.volume);

                _playedFootstepIndex = newSoundIndex;
            }
        }
        else
        {
            _playedFootstep = false;
        }
        rig.localPosition = Vector3.up * (Mathf.Max(h, 0f) * Bob * CurrentSpeed) / WalkSpeed;

        // Squash
        var s = Mathf.Sin(_cycle * 2f) * CurrentSpeed / WalkSpeed * Squash;
        SetSquash(s);

        _previousVelocity = rb.velocity;
    }

    public void SetSquash(float s)
    {
        rig.localScale = new Vector3(1f / (1f + s), 1f + s, 1f / (1f + s));
    }

    // Start is called before the first frame update

    private Vector3 GetMouseWorldPosition()
    {
        var ray = PlayerCamera.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float hitDistance))
        {
            return ray.GetPoint(hitDistance);
        }

        return default;
    }

    private GameObject GetMouseRaycastObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.gameObject;
        }

        return null;
    }
}
