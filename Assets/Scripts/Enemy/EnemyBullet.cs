using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float LiveTime;

    private float _startTime;
    // Start is called before the first frame update
    void Start()
    {
        _startTime = Time.time;
        GetComponent<Rigidbody>().useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - _startTime >= LiveTime)
            GameObject.Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject.Destroy(gameObject);
    }
}
