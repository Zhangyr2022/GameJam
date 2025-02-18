using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

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
    public AudioSource SpinningSource;
    public AudioSource DropSource;
    public AudioSource ExplodeSource;

    public AudioItem[] PickupSounds;
    public AudioItem DropSound;
    public Vector2 SpinningPitch = new Vector2(0.8f, 1f);

    public float AOERange = 2;
    public float AOEDamage = 1;
    public float FlyingDamage = 3f;
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

    public float ShootBulletsInterval = 0.5f;
    private float _lastShootTime;

    public float LastExplodeTime = 0.0f;
    public float ExplosionInterval = 10;
    public float ExplosionRange = 3f;
    public float ExplosionDamage = 10;
    public float ExplosionShakeStrength = 2.0f;

    public Image _energyImage;
    private GameObject _energyObject;
    private AudioItem _explodeAudio;

    public ParticleSystem CycleParticleSystem;
    public ParticleSystem ExplosionParticleSystem;
    public ParticleSystem StrengthedParticleSystem;
    private void OnEnable()
    {
        Instance = this;
        this.Blood = GameObject.Find("Particals");
        this.WeaponBody = this.GetComponent<Rigidbody>();
        SetBloodActive(false);
        this.WobbleRoot = this.transform.Find("WobbleRoot");
        this.RotateAround = this.transform.Find("WobbleRoot/RotateAround");
        // Audio sources
        AudioSource[] audioSources = this.GetComponentsInChildren<AudioSource>();
        this.StuckSource = audioSources[0];
        this.PickupSource = audioSources[1];
        this.SpinningSource = audioSources[2];
        this.DropSource = audioSources[3];
        this.ExplodeSource = audioSources[4];
        // Pickup Sounds
        List<AudioItem> pickupSoundsList = new();
        for (int i = 1; i <= 3; i++)
        {
            pickupSoundsList.Add(new AudioItem(Resources.Load<AudioClip>($"Audio/flying{i}")));
        }
        this.PickupSounds = pickupSoundsList.ToArray();

        // Drop sound
        this.DropSound = new AudioItem(Resources.Load<AudioClip>($"Audio/placeCane"));
        _energyImage = GameObject.Find("Canvas/Explosion/Energy").GetComponent<Image>();
        _energyObject = GameObject.Find("Canvas/Explosion");
        this._explodeAudio = new AudioItem(Resources.Load<AudioClip>("Audio/overload"));

        this.CycleParticleSystem = this.transform.Find("CycleParticleSystem").GetComponent<ParticleSystem>();
        this.ExplosionParticleSystem = this.transform.Find("ExplosionParticleSystem").GetComponent<ParticleSystem>();
        this.StrengthedParticleSystem = this.transform.Find("StrengthenedParticleSystem").GetComponent<ParticleSystem>();
    }

    public void SetBloodActive(bool active)
    {
        // TODO:
        // Effects when damaged

        //foreach (var p in Blood.GetComponentsInChildren<ParticleSystem>())
        //{
        //    if (active && !p.isPlaying)
        //        p.Play();
        //    else if (!active && p.isPlaying)
        //        p.Stop();
        //}
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

        float deltaTime = Time.time - this.LastExplodeTime;
        if (deltaTime > ExplosionInterval)
        {
            Explode();
            this.LastExplodeTime = Time.time;
        }

        // Update energy image
        _energyImage.fillAmount = deltaTime / ExplosionInterval;
        if (_energyImage.fillAmount > 0.999f)
        {
            _energyImage.fillAmount = 0;
            _energyObject.transform.eulerAngles = Vector3.zero;
        }
        else if (_energyImage.fillAmount > 0.8f)
        {
            // Shaking
            _energyObject.transform.eulerAngles = new Vector3(0, 0, Random.Range(-10, 10));
        }
        else
        {
        }


        if (WeaponState.Idle == State || WeaponState.Stuck == State || WeaponState.Throwing == State)
        {
            if (CycleParticleSystem.gameObject.activeSelf == false)
            {
                CycleParticleSystem.gameObject.SetActive(true);

                PlayAllParticleSystems(CycleParticleSystem);
                //Debug.Log("open");
            }
        }
        else if (WeaponState.Holding == State || WeaponState.Retrieving == State)
        {
            if (CycleParticleSystem.gameObject.activeSelf == true)
            {
                CycleParticleSystem.gameObject.SetActive(false);
                //StopAllParticleSystems(CycleParticleSystem);
                //Debug.Log("close");
            }
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

        StopAllParticleSystems(CycleParticleSystem);
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

        transform.SetParent(FindTransformInChildren(holder, "WeaponContainer")[0]);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    public void Drop()
    {
        if (State is WeaponState.Throwing or WeaponState.Retrieving)
            return;
        // Play sound
        this.DropSound.PlayOn(this.DropSource);

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
                    collision.gameObject.GetComponent<Enemy>().DoDamage(FlyingDamage);

                    return;
                }
            }
            else
            {
                WeaponBody.detectCollisions = false;
                State = WeaponState.Stuck;
                transform.rotation =
                    Quaternion.LookRotation(contact.normal, Vector3.up);
                transform.eulerAngles = Vector3.zero;
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

    public void ShootBullets(Vector3 direction, GameObject target = null)
    {
        if (Time.time - _lastShootTime > ShootBulletsInterval)
        {
            this._lastShootTime = Time.time;
            Bullet.BulletMode bulletMode = Bullet.BulletMode.Normal;
            if (this.Mode == WeaponMode.Strengthened)
            {
                bulletMode = Bullet.BulletMode.Strengthened;
                // Only shoot one strengthened bullet
                this.Mode = WeaponMode.Normal;
            }
            BulletManager.Instance.CreateBullet(this.transform.position, direction, bulletMode, target);
        }
    }
    /// <summary>
    /// AOE method
    /// </summary>
    public void AOEShoot()
    {
        if (Time.time - _lastShootTime > ShootBulletsInterval)
        {
            this._lastShootTime = Time.time;
            EnemyManager.Instance.DoAOEDamage(this.transform.position, AOERange, AOEDamage);
            StrengthedParticleSystem.gameObject.transform.localPosition = Vector3.zero;
            StrengthedParticleSystem.Play();
        }
    }

    private void Explode()
    {
        EnemyManager.Instance.DoAOEDamage(this.transform.position, ExplosionRange, ExplosionDamage);
        // Check if the player is in the range of explosion
        if (Vector3.Distance(Player.Instance.transform.position, this.transform.position) < ExplosionRange)
        {
            Player.Instance.Damage();
        }
        // Play Sound
        _explodeAudio.PlayOn(ExplodeSource);

        PlayerCamera.Instance.CurrentShakeStrength += ExplosionShakeStrength;
        ExplosionParticleSystem.Play();
    }

    private List<Transform> FindTransformInChildren(Transform parent, string targetName)
    {
        List<Transform> foundTransforms = new List<Transform>();

        int childCount = parent.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = parent.GetChild(i);

            // 检查当前子物体的名称是否匹配目标名称
            if (child.name == targetName)
            {
                // 找到了符合条件的Transform，添加到列表中
                foundTransforms.Add(child);
            }

            // 继续递归查找子物体的子物体，并将结果添加到列表中
            foundTransforms.AddRange(FindTransformInChildren(child, targetName));
        }

        return foundTransforms;
    }

    private void StopAllParticleSystems(ParticleSystem particleSystem)
    {
        ParticleSystem[] particleSystems = particleSystem.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Pause();
        }
    }
    private void PlayAllParticleSystems(ParticleSystem particleSystem)
    {
        ParticleSystem[] particleSystems = particleSystem.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Play();
        }
    }
}
