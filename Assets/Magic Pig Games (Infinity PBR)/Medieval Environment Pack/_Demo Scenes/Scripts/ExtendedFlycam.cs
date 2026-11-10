using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace InfinityPBR
{
    /// <summary>
    /// Demo fly camera. Hold the right mouse button to look; WASD or the arrow keys move, Q/E go down/up, Shift is fast
    /// and Ctrl is slow. Works with the Input System, the legacy Input Manager, or both (the Input System is used when
    /// it's active).
    /// </summary>
    public class ExtendedFlycam : MonoBehaviour
    {
        public float CameraSensitivity = 90;
        public float NormalMoveSpeed = 10;
        public float SlowMoveFactor = 0.25f;
        public float FastMoveFactor = 3;

        // Mouse movement arrives per frame, so it isn't scaled by deltaTime. These turn it into degrees, matching the
        // original feel at 60 fps: legacy axis units are 0.1 of a pixel at default sensitivity.
        private const float DegreesPerAxisUnit = 1f / 60f;
        private const float AxisUnitsPerPixel = 0.1f;

        private float _rotationX;
        private float _rotationY;

        private struct FlyInput
        {
            public bool Look;
            public Vector2 LookDelta; // legacy axis units
            public Vector2 Move;      // x = right, y = forward
            public float Climb;
            public bool Fast;
            public bool Slow;
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _rotationX = transform.eulerAngles.y;
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            FlyInput input = ReadInput();

            if (input.Look)
            {
                _rotationX += input.LookDelta.x * CameraSensitivity * DegreesPerAxisUnit;
                _rotationY += input.LookDelta.y * CameraSensitivity * DegreesPerAxisUnit;
                _rotationY = Mathf.Clamp(_rotationY, -90f, 90f);
            }

            Quaternion targetRotation = Quaternion.AngleAxis(_rotationX, Vector3.up);
            targetRotation *= Quaternion.AngleAxis(_rotationY, Vector3.left);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4f);

            float speedFactor = 1f;
            if (input.Fast) speedFactor = FastMoveFactor;
            if (input.Slow) speedFactor = SlowMoveFactor;

            float step = NormalMoveSpeed * speedFactor * Time.deltaTime;
            transform.position += transform.forward * (input.Move.y * step);
            transform.position += transform.right * (input.Move.x * step);
            transform.position += transform.up * (input.Climb * step);
        }

        private static FlyInput ReadInput()
        {
            var input = new FlyInput();
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                input.Look = true;
                input.LookDelta = mouse.delta.ReadValue() * AxisUnitsPerPixel;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                input.Move.x = Axis(keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed,
                    keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
                input.Move.y = Axis(keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
                    keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);
                input.Climb = 0.5f * Axis(keyboard.eKey.isPressed, keyboard.qKey.isPressed);
                input.Fast = keyboard.shiftKey.isPressed;
                input.Slow = keyboard.ctrlKey.isPressed;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButton(1))
            {
                input.Look = true;
                input.LookDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }

            input.Move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            input.Climb = 0.5f * Axis(Input.GetKey(KeyCode.E), Input.GetKey(KeyCode.Q));
            input.Fast = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            input.Slow = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif
            return input;
        }

        private static float Axis(bool positive, bool negative) => (positive ? 1f : 0f) - (negative ? 1f : 0f);
    }
}
