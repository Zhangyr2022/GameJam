using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-2)]

public class PlayerInput : CharacterControl
{

    public static PlayerInput Instance;

    private PlayerCamera _camera => PlayerCamera.Instance;
    private Player _player => Player.Instance;
    private Weapon _weapon => Weapon.Instance;

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (!_player)
            return;

        var inputPlane = new Plane(Vector3.up, Vector3.zero);

        // Move
        Movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Movement = Vector3.ClampMagnitude(Movement, 1f);

        // Press mouse left
        if (Input.GetMouseButtonDown(0))
        {
            switch (_weapon.State)
            {
                case Weapon.WeaponState.Holding:
                    _player.Shoot();
                    break;
                default:
                    _player.PickUpWeapon();
                    break;
            }
        }
        // Press space to drop the weapon
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Weapon.Instance.State == Weapon.WeaponState.Holding)
                _player.DropWeapon();
            else if (Weapon.Instance.State == Weapon.WeaponState.Stuck || Weapon.Instance.State == Weapon.WeaponState.Idle)
            {
                _player.PickUpWeapon();
            }

        }

        var inputRay = _camera.Camera.ScreenPointToRay(Input.mousePosition);
        if (inputPlane.Raycast(inputRay, out var distance))
        {
            var hitPoint = inputRay.GetPoint(distance);
            var playerPosition = _player.transform.position;
            playerPosition.y = 0f;
            LookDirection = (hitPoint - playerPosition).normalized;
        }
    }
}
