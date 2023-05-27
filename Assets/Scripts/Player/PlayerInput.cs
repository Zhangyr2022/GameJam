using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : CharacterControl
{
    private new PlayerCamera _camera => PlayerCamera.Instance;
    private Player _player => Player.Instance;

    private void Update()
    {
        if (!_player)
            return;

        var inputPlane = new Plane(Vector3.up, Vector3.zero);

        Movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Movement = Vector3.ClampMagnitude(Movement, 1f);

        if (Input.GetMouseButtonDown(0))
            _player.PickUpWeapon();
        if (Input.GetMouseButtonUp(0))
            _player.DropWeapon();

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
