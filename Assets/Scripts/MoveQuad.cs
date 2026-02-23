using UnityEngine;

public class MoveQuad : MonoBehaviour
{
    public float speed = 5.0f; // Control the movement speed from the Inspector

    void Update()
    {
        // Get input from the keyboard (Horizontal/Vertical axes)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0);

        // Move the object by the calculated amount
        // Time.deltaTime ensures smooth, frame-rate independent movement
        transform.Translate(movement * speed * Time.deltaTime);
    }
}
