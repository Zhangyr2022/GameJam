using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MaxHP = 5.0f;
    public int DamageToChunk = 1;
    public float GrowSpeed = 1.0f;

    public GameObject Particle;

    public Transform ShootStart;
    public GameObject Bullet;

    public float MaxShootInterval;

    public float MinShootInterval;

    // public float ShootInterval;

    public float BulletSpeed;

    public GameObject Remain;
    public float RemainDuration;

    private enum State
    {
        Grow,
        Alive,
        Die
    }

    private State _state = State.Grow;
    private float _hp;
    private Vector2Int _prevPos;
    private Vector2Int _currentPos;

    private EnemyMotor _motor;
    private float _size;

    private float _lastShootTime;

    private AudioSource _audioSource;
    private AudioItem _deadAudio;

    public void DoDamage(float damage)
    {
        _hp = Mathf.Max(_hp - damage, 0);
        if (Mathf.Approximately(_hp, 0))
        {
            Die();
        }
    }

    private void GetGridPos()
    {
        _currentPos.x = Mathf.FloorToInt(transform.position.x);
        _currentPos.y = Mathf.FloorToInt(transform.position.z);
    }

    private void Die()
    {
        Game.Instance.AddScore(1);
        EnemyManager.Instance.RemoveEnemy(gameObject);
        GridManager.Instance.CleanChunk(_currentPos);

        // Play dead audio
        this._deadAudio.PlayOn(_audioSource);

        GameObject.Instantiate(Particle, transform.position, Quaternion.identity);
        GameObject remain = GameObject.Instantiate(Remain, transform.position, Quaternion.identity);
        GameObject.Destroy(gameObject);
        GameObject.Destroy(remain, RemainDuration);
    }

    private void Shoot()
    {
        GameObject bullet = GameObject.Instantiate(Bullet, ShootStart.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody>().velocity = (Player.Instance.transform.position - transform.position).normalized * BulletSpeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        GetGridPos();
        _prevPos = _currentPos;
        _hp = MaxHP;
        _motor = gameObject.GetComponent<EnemyMotor>();
        _size = 0;
        _motor.SetScale(_size);

        // GetComponent<Rigidbody>().centerOfMass = transform.Find("foot").localPosition;
        // Try get audioSource
        bool result = TryGetComponent<AudioSource>(out AudioSource audioSource);
        if (result)
        {
            _audioSource = audioSource;
        }
        else
        {
            _audioSource = this.AddComponent<AudioSource>();
        }
        this._deadAudio = new AudioItem(Resources.Load<AudioClip>("Audio/sprout"));
    }

    // Update is called once per frame
    void Update()
    {
        GetGridPos();
        if (_prevPos != _currentPos)
        {
            _prevPos = _currentPos;
            GridManager.Instance.CorruptChunk(_currentPos, DamageToChunk);
        }

        if (_state == State.Grow)
        {
            _size = Mathf.MoveTowards(_size, 1, Time.deltaTime * GrowSpeed);
            _motor.SetScale(_size);

            if (Mathf.Approximately(_size, 1))
            {
                _state = State.Alive;
                _lastShootTime = Time.time;
                _motor.CanMove = true;
            }
        }
        else if (_state == State.Alive)
        {
            float shootInterval = Mathf.Lerp(MinShootInterval, MaxShootInterval, GridManager.Instance.GetEvilRatio());
            if (Time.time - _lastShootTime >= shootInterval)
            {
                _lastShootTime = Time.time;
                Shoot();
            }
        }
    }
}
