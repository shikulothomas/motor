using UnityEngine;

namespace MotorPrototype
{
    /// <summary>Smooth third-person chase camera.</summary>
    public sealed class ArcadeFollowCamera : MonoBehaviour
    {
        public Transform Target;
        [SerializeField] private float followDistance = 9f;
        [SerializeField] private float followHeight = 5.2f;
        [SerializeField] private float smoothing = 7f;

        private bool initialized;

        private void LateUpdate()
        {
            if (Target == null) return;
            Vector3 desiredPosition = Target.position - Target.forward * followDistance +
                                      Vector3.up * followHeight;
            if (!initialized)
            {
                transform.position = desiredPosition;
                initialized = true;
            }
            else
            {
                float alpha = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
                transform.position = Vector3.Lerp(transform.position, desiredPosition, alpha);
            }
            Vector3 lookPoint = Target.position + Target.forward * 4f + Vector3.up * 0.65f;
            Quaternion targetRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                1f - Mathf.Exp(-smoothing * Time.deltaTime));
        }
    }
}
