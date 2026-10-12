using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MotorPrototype
{
    /// <summary>Simple arcade Rigidbody handling; no wheel colliders or external assets.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeCarController : MonoBehaviour
    {
        [Header("Driving")]
        [SerializeField] private float acceleration = 21f;
        [SerializeField] private float reverseAcceleration = 10f;
        [SerializeField] private float maxSpeed = 27f;
        [SerializeField] private float steeringDegreesPerSecond = 105f;
        [SerializeField] private float sidewaysGrip = 8f;

        private Rigidbody body;
        private LapTimeTrial race;
        public float SpeedMetersPerSecond => body == null ? 0f :
            Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;

        private void Awake() { body = GetComponent<Rigidbody>(); }
        public void SetRace(LapTimeTrial game) { race = game; }

        private void Update()
        {
            if (ResetPressed() && race != null) race.Restart();
        }

        private void FixedUpdate()
        {
            float throttle = (ForwardHeld() ? 1f : 0f) - (BackwardHeld() ? 1f : 0f);
            float steer = (RightHeld() ? 1f : 0f) - (LeftHeld() ? 1f : 0f);
            Vector3 planarVelocity = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
            float forwardSpeed = Vector3.Dot(planarVelocity, transform.forward);
            float sidewaysSpeed = Vector3.Dot(planarVelocity, transform.right);

            if (throttle != 0f)
            {
                float power = throttle > 0f ? acceleration : reverseAcceleration;
                body.AddForce(transform.forward * (throttle * power), ForceMode.Acceleration);
            }
            else body.AddForce(-planarVelocity * 0.95f, ForceMode.Acceleration);

            body.AddForce(-transform.right * (sidewaysSpeed * sidewaysGrip), ForceMode.Acceleration);
            if (BrakeHeld())
                body.AddForce(-planarVelocity * 5f, ForceMode.Acceleration);

            float speedFactor = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 4f);
            float reverseDirection = forwardSpeed < -0.5f ? -1f : 1f;
            float degrees = steer * steeringDegreesPerSecond * speedFactor *
                            reverseDirection * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0, degrees, 0));

            Vector3 nextPlanar = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
            if (nextPlanar.magnitude > maxSpeed)
                body.linearVelocity = nextPlanar.normalized * maxSpeed +
                                      Vector3.up * body.linearVelocity.y;
        }

        public void ResetPosition(Vector3 position, Quaternion rotation)
        {
            body.position = position;
            body.rotation = rotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(position, rotation);
        }

        private static bool ForwardHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null) return k.wKey.isPressed || k.upArrowKey.isPressed;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
#else
            return false;
#endif
        }

        private static bool BackwardHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null) return k.sKey.isPressed || k.downArrowKey.isPressed;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
#else
            return false;
#endif
        }

        private static bool LeftHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null) return k.aKey.isPressed || k.leftArrowKey.isPressed;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
#else
            return false;
#endif
        }

        private static bool RightHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null) return k.dKey.isPressed || k.rightArrowKey.isPressed;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
#else
            return false;
#endif
        }

        private static bool BrakeHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null) return k.spaceKey.isPressed;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.Space);
#else
            return false;
#endif
        }

        private static bool ResetPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null) return k.rKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.R);
#else
            return false;
#endif
        }
    }
}
