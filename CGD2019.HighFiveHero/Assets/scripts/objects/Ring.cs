using App.UI;
using UnityEngine;

namespace App {
    public class Ring : MonoBehaviour {

        [SerializeField] Vector2 angularVelocityRange;
        [SerializeField] float hitStrengthModifier;

        Rigidbody rb;
        float yMin;

        /// <summary>
        /// Sets the ring object's initial physics state.
        /// </summary>
        /// <param name="hitVector">Vector between the hit position and the hand, times the strength of the hit.</param>
        /// <param name="size">Size of the hand.</param>
        public void Initialize(Vector2 hitVector, float size) {
            rb = GetComponent<Rigidbody>();

            yMin = -Camera.main.orthographicSize - 5f;

            transform.localScale = new Vector3(size, size, size) * 2f;

            RunManager.Instance.ActiveRings.Add(this);

            // Fling the object along the hit vector, and set a random angular velocity
            rb.AddForce(hitVector * hitStrengthModifier);

            float rand = Random.Range(angularVelocityRange.x, angularVelocityRange.y);

            rb.angularVelocity = new Vector3(
                Random.Range(-rand, rand),
                Random.Range(-rand, rand),
                Random.Range(-rand, rand));
        }

        /// <summary>
        /// Checks if the ring is off-screen and removes it if so.
        /// </summary>
        private void Update() {
            if (transform.position.y < yMin) {
                RunManager.Instance.ActiveRings.Remove(this);
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// To be executed when the ring is collected.
        /// </summary>
        public void OnCollect() {
            RunManager.Instance.AddCurrency();

            HandManager.Instance.SpawnRingIndicator(transform);
        }
    }
}
