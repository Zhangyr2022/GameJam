using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Weapon : MonoBehaviour
{
    public static Weapon Instance;

    public float MaxSpinSpeed = 2160f;
    public float MinThrowSpeed = 20f;
    public float MaxThrowSpeed = 30f;
    [Space]
    public Transform WobbleRoot;
    public float WobbleDuration = 1f;
    public float WobbleSpeed = 20f;
    public float WobbleStrength = 10f;
    public AnimationCurve WobbleCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [Space]
    public Rigidbody WeaponBody;
    public Transform RotateAround;
    public GameObject Blood;
    public float BloodDuration;

    [Space]
    public AudioSource StuckSource;
    public AudioSource PickupSource;
    public AudioItem[] PickupSounds;
    public AudioSource SpinningSource;
    public Vector2 SpinningPitch = new Vector2(0.8f, 1f);



    public enum WeaponState
    {
        Idle,
        Holding,
        Retrieving,
        Throwing,
        Stuck,
    }
    public WeaponState State = WeaponState.Idle;

    public enum WeaponMode
    {
        Normal,
        Strengthened
    }
    public WeaponMode Mode = WeaponMode.Normal;

    public GameObject CurrentWeaponTarget;
    private float _currentThrowSpeed;
    private Vector3 _throwDirection;

    private float lastBloodTime = -100f;

    public const float ShootBulletsInterval = 0.5f;
    private float _lastShootTime;
    private void OnEnable()
    {
        Instance = this;
        this.Blood = GameObject.Find("Particals");
        this.WeaponBody = this.GetComponent<Rigidbody>();
        SetBloodActive(false);
    }

    public void SetBloodActive(bool active)
    {
        foreach (var p in Blood.GetComponentsInChildren<ParticleSystem>())
        {
            if (active && !p.isPlaying)
                p.Play();
            else if (!active && p.isPlaying)
                p.Stop();
        }
    }
    private void FixedUpdate()
    {
        if (Time.time - lastBloodTime > BloodDuration)
        {
            SetBloodActive(false);
        }

        if (State == WeaponState.Throwing)
        {
            SpinningSource.pitch = Mathf.Lerp(SpinningPitch.x, SpinningPitch.y,
                Mathf.InverseLerp(MinThrowSpeed, MaxThrowSpeed, _currentThrowSpeed));
            transform.RotateAround(RotateAround.position, Vector3.up, MaxSpinSpeed * Time.fixedDeltaTime);
            transform.position += _throwDirection * (_currentThrowSpeed * Time.fixedDeltaTime);
            WeaponBody.position = transform.position;
        }
    }

    public void Pickup(Transform holder)
    {
        if (State == WeaponState.Stuck && _wiggleRoutine != null)
        {
            StopCoroutine(_wiggleRoutine);
        }

        if (State is WeaponState.Holding or WeaponState.Retrieving)
            return;

        StartCoroutine(PickupRoutine(holder));
    }

    private IEnumerator PickupRoutine(Transform holder)
    {
        State = WeaponState.Retrieving;
        WeaponBody.detectCollisions = true;
        WeaponBody.isKinematic = true;
        WeaponBody.constraints = RigidbodyConstraints.FreezeAll;
        transform.SetParent(null);
        transform.localScale = Vector3.one;

        transform.LookAt(Player.Instance.transform.position);

        //if (CurrentWeaponTarget != null)
        //{
        //    var st = CurrentWeaponTarget;
        //    CurrentWeaponTarget = null;
        //    st.OnRelease(this);
        //}

        SpinningSource.Play();
        float returnTime = 0f;
        while (Vector3.Distance(transform.position, holder.position) > 1f)
        {
            if (State is not WeaponState.Retrieving)
                yield break;
            SpinningSource.pitch = Mathf.Lerp(SpinningPitch.x, SpinningPitch.y,
                Mathf.InverseLerp(MinThrowSpeed, MaxThrowSpeed, Mathf.Clamp01(returnTime)));
            returnTime += Time.fixedDeltaTime;
            transform.RotateAround(RotateAround.position, Vector3.up, MaxSpinSpeed * Time.fixedDeltaTime);
            transform.position = Vector3.MoveTowards(transform.position, holder.position, Mathf.Clamp01(returnTime) * MaxThrowSpeed * Time.fixedDeltaTime);
            WeaponBody.position = transform.position;
            yield return new WaitForFixedUpdate();
        }
        SpinningSource.Stop();

        if (PickupSounds.Length > 0)
        {
            var sound = PickupSounds[Random.Range(0, PickupSounds.Length)];
            sound.PlayOn(PickupSource);
        }
        State = WeaponState.Holding;

        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        if (State is WeaponState.Throwing or WeaponState.Retrieving)
            return;

        WeaponBody.isKinematic = false;

        //if (CurrentWeaponTarget != null)
        //{
        //    var st = CurrentWeaponTarget;
        //    CurrentWeaponTarget = null;
        //    st.OnRelease(this);
        //}

        WeaponBody.detectCollisions = true;

        transform.SetParent(null);

        WeaponBody.constraints = RigidbodyConstraints.None;
        State = WeaponState.Idle;
    }

    public void Throw(Vector3 direction, float speed)
    {
        //if (CurrentWeaponTarget != null)
        //{
        //    var st = CurrentWeaponTarget;
        //    CurrentWeaponTarget = null;
        //    st.OnRelease(this);
        //}

        SpinningSource.Play();

        transform.SetParent(null);
        WeaponBody.isKinematic = false;
        WeaponBody.detectCollisions = true;
        _throwDirection = direction;
        var throwSpeed = Mathf.Lerp(MinThrowSpeed, MaxThrowSpeed, speed);
        _currentThrowSpeed = throwSpeed;
        WeaponBody.constraints = RigidbodyConstraints.FreezeAll;
        State = WeaponState.Throwing;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var enemyLayer = LayerMask.NameToLayer("Enemy");
        var playerLayer = LayerMask.NameToLayer("Player");
        if (collision.gameObject.layer != playerLayer && State is not WeaponState.Stuck)
        {
            WeaponBody.isKinematic = true;

            var contact = collision.GetContact(0);

            PlayerCamera.Instance.CurrentShakeStrength += 0.3f;

            if (collision.gameObject.layer == enemyLayer)
            {
                SetBloodActive(true);
                lastBloodTime = Time.time;
                if (State is WeaponState.Throwing)
                {
                    WeaponBody.detectCollisions = false;
                    State = WeaponState.Stuck;
                    var enemyRB = collision.transform.GetComponentInParent<Rigidbody>();
                    transform.SetParent(enemyRB.transform);
                    var enemyPosition = collision.transform.position;
                    enemyPosition.y = transform.position.y;
                    var normal = (transform.position - enemyPosition).normalized;
                    transform.position =
                        enemyPosition + normal * (WobbleRoot.position - transform.position).magnitude;
                    transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
                    SpinningSource.Stop();
                }
                else if (State is WeaponState.Retrieving or WeaponState.Holding)
                {
                    // TODO:
                    // kill enemies
                    //var enemy = collision.transform.GetComponentInParent<EnemyControl>();
                    //enemy.Kill();

                    return;
                }
            }
            else
            {
                WeaponBody.detectCollisions = false;
                State = WeaponState.Stuck;
                transform.rotation =
                    Quaternion.LookRotation(contact.normal, Vector3.up);
                transform.position =
                    contact.point + contact.normal * (WobbleRoot.position - transform.position).magnitude;
                SpinningSource.Stop();
                StuckSource.Stop();
                StuckSource.Play();
            }

            //CurrentWeaponTarget = collision.gameObject.GetComponentInParent<IWeaponTarget>();
            //CurrentWeaponTarget?.OnStuck(this);

            if (_wiggleRoutine != null)
                StopCoroutine(_wiggleRoutine);
            _wiggleRoutine = StartCoroutine(WiggleRoutine());
        }
    }

    private Coroutine _wiggleRoutine;
    private IEnumerator WiggleRoutine(bool inverse = false, float speed = 1f)
    {
        var t = 0f;
        while (!Mathf.Approximately(t, 1f))
        {
            t = Mathf.Clamp01(t + Time.deltaTime / WobbleDuration * speed);
            var tt = t;
            if (inverse)
                tt = 1f - t;
            yield return null;
            var angle = Mathf.Sin(tt * WobbleSpeed) * WobbleCurve.Evaluate(tt) * WobbleStrength;
            WobbleRoot.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }
    }

    public void ShootBullets(Vector3 direction)
    {
        if (Time.time - _lastShootTime > ShootBulletsInterval)
        {
            this._lastShootTime = Time.time;
            Bullet.BulletMode bulletMode = Bullet.BulletMode.Normal;
            if (this.Mode == WeaponMode.Strengthened)
            {
                bulletMode = Bullet.BulletMode.Strengthened;
            }
            BulletManager.Instance.CreateBullet(this.transform.position, direction, bulletMode);
        }
    }
}
