using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MaxHP = 5.0f;
    public int DamageToChunk = 1;

    private float _hp;
    private Vector2Int _prevPos;
    private Vector2Int _currentPos;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        GetGridPos();
        _prevPos = _currentPos;
        _hp = MaxHP;

        GetComponent<Rigidbody>().centerOfMass = transform.Find("foot").localPosition;
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
    }
}
