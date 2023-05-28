using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public const float AOEDamage = 1f;
    public const float AOERange = 2f;
    public const float ShootDamage = 2f;
    public const float ShootRange = 8f;
    public const float TravelSpeed = 3f;

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

    private BulletMode _mode;

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(Time.deltaTime * TravelSpeed * _shootDirection);
        // Check if the distance is in the shoot range
        if (Vector3.Distance(this.transform.position, this._shooterPosition) > ShootRange)
        {
            Destroy(this.gameObject);
        }
    }

    public void InitializeBullet(Vector3 position, Vector3 direction, BulletMode mode)
    {
        this.transform.position = _shooterPosition = position;
        this._shootDirection = (direction);
        this._mode = mode;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Shoot enemies
        GameObject collisionObject = collision.gameObject;
        if (collisionObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = collisionObject.GetComponent<Enemy>();
            enemy.DoDamage(ShootDamage);

            if (this._mode == BulletMode.Strengthened)
            {
                // AOE damage
                EnemyManager.Instance.DoAOEDamage(this.transform.position, AOERange, AOEDamage);
            }
        }
        // Destroy bullets
        Destroy(this.gameObject);
    }
}
