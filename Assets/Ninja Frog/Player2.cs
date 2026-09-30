using UnityEngine;
using UnityEngine.InputSystem;

public class Player2 : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector2 moveDir;

    private void Update()
    {
        UpdateDirectionFromInput();

        Move();
    }

    private void UpdateDirectionFromInput()
    {
        moveDir = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            moveDir += new Vector2(0, 1);
        if (Keyboard.current.sKey.isPressed)
            moveDir += new Vector2(0, -1);
        if (Keyboard.current.aKey.isPressed)
            moveDir += new Vector2(-1, 0);
        if (Keyboard.current.dKey.isPressed)
            moveDir += new Vector2(1, 0);
    }

    private void Move()
    {
        transform.position += moveSpeed * Time.deltaTime * (Vector3)moveDir.normalized;
    }
}
