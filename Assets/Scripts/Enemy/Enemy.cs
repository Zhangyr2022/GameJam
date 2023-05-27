using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MaxHP = 5.0f;
    public int DamageToChunk = 1;
    public float GrowSpeed = 1.0f;

    // public GameObject Remain;

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
        EnemyManager.Instance.RemoveEnemy(gameObject);
        GridManager.Instance.CleanChunk(_currentPos);

        GameObject.Destroy(gameObject);
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
                _motor.CanMove = true;
            }
        }
    }
}
