using System.Collections;
using UnityEngine;

namespace App {
    /// <summary>
    /// CameraEffects handles effects that specifically come from the camera, such as
    /// screen shake. Any other such effects should be here only!
    /// </summary>
    public class CameraEffects : MonoBehaviour {

        /// Singleton instance.
        public static CameraEffects Instance;

        private Camera cam;

        bool shaking;

        [SerializeField] float[] handShakeAmounts;
        [SerializeField] float[] handShakeDurations;

        /// <summary>
        /// Singleton initialization and reference setup.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);

            if (this) cam = Camera.main;
        }

        public void ShakeFromHand(float intensity) {
            float mSize = Mathf.Round(intensity / 0.33f) * 0.33f;
            int index = Mathf.FloorToInt(mSize * 3);
            
            if(!shaking) StartCoroutine(ShakeCamera(handShakeAmounts[index], handShakeDurations[index]));
        }

        /// <summary>
        /// Shakes the screen asynchronously.
        /// </summary>
        /// <param name="amount">Amount to shake (in maximum unity units per frame).</param>
        /// <param name="duration">Duration of the effect in seconds.</param>
        /// <returns></returns>
        public IEnumerator ShakeCamera(float amount, float duration) {
            if (shaking) yield break;

            shaking = true;

            float maxDuration = duration;
            float maxAmount = amount;
            Vector3 initialCameraPosition = cam.transform.position;

            while(duration > 0) {
                duration -= Time.deltaTime;

                cam.transform.position = initialCameraPosition + (Vector3)Random.insideUnitCircle * amount;
                amount = maxAmount * (duration / maxDuration);

                yield return null;
            }

            shaking = false;
        }
    }
}
