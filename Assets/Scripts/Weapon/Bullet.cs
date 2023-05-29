using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public const float AOEDamage = 1f;
    public const float AOERange = 2f;
    public const float ShootDamage = 2f;
    public const float ShootRange = 8f;
    public const float TravelSpeed = 4f;


    public enum BulletMode
    {
        Normal,
        Strengthened
    }
    /// <summary>
    /// The start position of the bullet
    /// </summary>
    private Vector3 _shooterPosition;
    /// <summary>
    /// The direction of the bullet
    /// </summary>
    private Vector3 _shootDirection;
    private float _travelDistance = 0;
    private BulletMode _mode;
    private GameObject _target;

    // Update is called once per frame
    void Update()
    {
        if (this._target == null)
        {
            this.transform.Translate(Time.deltaTime * TravelSpeed * _shootDirection);
        }
        else
        {
            Vector3 shootDirection = (_target.transform.position - this.transform.position).normalized;
            this._shootDirection = shootDirection;
            this.transform.Translate(Time.deltaTime * TravelSpeed * _shootDirection);
            //this.transform.LookAt(this._target.transform);
        }
        this._travelDistance += Time.deltaTime * TravelSpeed;
        // Check if the distance is in the shoot range
        if (_travelDistance > ShootRange)
        {
            Destroy(this.gameObject);
        }
    }

    public void InitializeBullet(Vector3 position, Vector3 direction, BulletMode mode, GameObject target = null)
    {
        this.transform.position = _shooterPosition = position;
        this._shootDirection = direction;
        this._mode = mode;
        this._target = target;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Shoot enemies
        GameObject collisionObject = collision.gameObject;
        if (collisionObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = collisionObject.GetComponent<Enemy>();
            enemy.DoDamage(ShootDamage);
        }


        if (this._mode == BulletMode.Strengthened)
        {
            // AOE damage
            EnemyManager.Instance.DoAOEDamage(this.transform.position, AOERange, AOEDamage);
            // Particle
            Weapon.Instance.StrengthedParticleSystem.transform.position = this.transform.position;
            Weapon.Instance.StrengthedParticleSystem.Play();
        }
        // Destroy bullets
        Destroy(this.gameObject);
    }
}
