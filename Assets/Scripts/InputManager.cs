using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// If we ever expand this project this class will need significant improvements to allow any 
// combination of KB&M / GamePad for the characters.

public class InputManager : MonoBehaviour
{
    [SerializeField] bool isGamepad;
    [SerializeField] float gamepadAimPositionRangeMax = 1.0f;  // How far to put the virtual cursor along the aim vector.
    [SerializeField] Transform aimTracker;  // An optional tracker object that anchors to the aim position.

    [Space, Header("Debugging")]
    [SerializeField] bool debug;

    public struct InputData
    {
        public Vector2 movementInput;
        public Vector2 aimInput;  // The direction of the aim from our current position.
        public Vector2 aimPosition;  // The exact position of the aim.
        public bool primaryButtonDown;
        public bool jumpButtonDown;
    }

    private void Update()
    {
        if (debug)
        {
            InputData data = GetInput();
            Debug.DrawLine(transform.position, (Vector3)data.aimPosition, Color.yellow);
            Debug.DrawLine(transform.position, transform.position + (Vector3)data.movementInput, Color.black);
            Debug.DrawLine(transform.position, transform.position + (Vector3)data.aimInput, Color.grey);
        }
    }

    public InputData GetInput()
    {
        InputData data = isGamepad ? GetGamepadInput() : GetKeyboardInput();
        if (aimTracker)
            aimTracker.position = data.aimPosition;
        return data;
    }

    private InputData GetKeyboardInput()
    {
        InputData data = new InputData();

        data.movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        data.movementInput.Normalize();

        data.aimPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        data.aimInput = data.aimPosition - (Vector2)transform.position;
        data.aimInput.Normalize();

        data.primaryButtonDown = Input.GetButton("Primary");
        data.jumpButtonDown = Input.GetButton("Jump");

        return data;
    }

    private InputData GetGamepadInput()
    {
        InputData data = new InputData();

        data.movementInput = new Vector2(Input.GetAxis("JoystickHorizontal_L"), Input.GetAxis("JoystickVertical_L"));
        if (data.movementInput.magnitude > 1.0f)
            data.movementInput.Normalize();

        data.aimInput = new Vector2(Input.GetAxis("JoystickHorizontal_R"), Input.GetAxis("JoystickVertical_R"));
        if (data.aimInput.magnitude > 1.0f)
            data.aimInput.Normalize();

        data.aimPosition = data.aimInput * gamepadAimPositionRangeMax;

        data.primaryButtonDown = Input.GetButton("JoystickPrimary");
        data.jumpButtonDown = Input.GetButton("JoystickJump");

        return data;
    }
}
