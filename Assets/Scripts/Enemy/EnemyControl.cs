using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class EnemyControl : CharacterControl
{
    public float pursueDistance = 4f;
    public float circleSpeed = 0.5f;
    public float knockbackRecoverySpeed = 3f;
    public float knockbackForce = 5f;
    public GameObject top, bottom;
    // public Slingshot slingshot;
    public float shootDelay = 1f;
    public Transform gibletVolume;

    public UnityEvent onStuck;
    [FormerlySerializedAs("onRelease")]
    public UnityEvent onKill;

    private bool _killed;
    private Vector3 _knockbackVelocity;
    private bool _stuck;
    private float _timeSinceShoot;

    private void Start()
    {
        StartCoroutine(ShootRoutine());
    }

    private void OnDisable()
    {
        Movement = Vector3.zero;
    }

    private bool _canShoot;
    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            // if (_canShoot && !_killed)
            //     yield return slingshot.Shoot();
            yield return new WaitForSeconds(shootDelay);
        }
    }

    private void Update()
    {
        _canShoot = false;

        var player = Player.Instance;
        if (player == null)
            return;
        if (Vector3.Distance(player.transform.position, transform.position) > pursueDistance)
        {
            Movement = (player.transform.position - transform.position).normalized;
            LookDirection = Movement;
        }
        else
        {
            _canShoot = true;

            // Vector3 dir2d = player.transform.position - transform.position;
            // dir2d.y = 0;

            // Movement = Vector3.Cross(dir2d, Vector3.up).normalized * circleSpeed;
            Movement = Vector3.Cross((player.transform.position - transform.position).normalized, Vector3.up) *
                       circleSpeed;
            LookDirection = (player.transform.position - transform.position).normalized;
        }


        // _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, knockbackRecoverySpeed * Time.deltaTime);
        // Movement += _knockbackVelocity;
    }

}