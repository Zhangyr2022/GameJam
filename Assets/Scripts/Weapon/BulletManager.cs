using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance;

    private GameObject _bulletPrefab;

    /// <summary>
    /// Create a bullet
    /// </summary>
    /// <param name="position">The shooter position</param>
    /// <param name="direction">The direction of the new bullet</param>
    /// <param name="mode">The bullet mode</param>
    public void CreateBullet(Vector3 position, Vector3 direction, Bullet.BulletMode mode)
    {
        GameObject newBulletObject = GameObject.Instantiate(_bulletPrefab);
        Bullet bullet = newBulletObject.AddComponent<Bullet>();
        // To avoid colliding with player, the initial position of the bullet should be a little away from player
        Vector3 correctedDirection = Vector3.Normalize(new Vector3(direction.x, direction.y * 0.1f, direction.z));
        position += correctedDirection;
        bullet.InitializeBullet(position, correctedDirection, mode);
    }

    private void Awake()
    {
        Instance = this;
        this._bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullet");
    }
}
