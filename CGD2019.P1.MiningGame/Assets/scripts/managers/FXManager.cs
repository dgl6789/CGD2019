using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.Gameplay;
using App.UI;

namespace App {
    public class FXManager : MonoBehaviour {

        public static FXManager Instance;
        
        [SerializeField] GameObject DebrisParticle;
        [SerializeField] GameObject SparksParticle;

        [SerializeField] Vector2Int debrisParticleCount;
        [SerializeField] float destroyShakeAmount;
        [SerializeField] float destroyShakeDuration;
        
        [SerializeField] float rockGenMoveSmoothing;

        Camera cam;

        void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);

            cam = Camera.main;
        }

        /// <summary>
        /// Spawn the rock-breaking particle system.
        /// </summary>
        /// <param name="position">Position at which to spawn particles.</param>
        /// <param name="type">Type of the rock from which the particles come.</param>
        public void SpawnDebrisParticles(Vector3 position, int type = 1) {
            // Shake the camera
            CameraShake(destroyShakeAmount, destroyShakeDuration);

            /// TODO: spawn a poof of dust.

            // Spawn a number of rock hunks.
            int r = Random.Range(debrisParticleCount.x, debrisParticleCount.y + 1);
            for (int i = 0; i < r; i++) Instantiate(DebrisParticle, position, Quaternion.identity, transform).GetComponent<DebrisParticle>().Initialize(type);
        }

        /// <summary>
        /// Spawn the spark particles to indicate a rock that is too hard to be broken with the current tool.
        /// </summary>
        /// <param name="position">Position at which to spawn particles.</param>
        public void SpawnSparkParticles(Vector3 position) {
            Destroy(Instantiate(SparksParticle, position, Quaternion.identity, transform), 0.5f);
        }

        /// <summary>
        /// Shake the game camera (wrapper for IEnumerator in FXManager.cs)
        /// </summary>
        /// <param name="amount">Amount to shake. Higher = more vigorous.</param>
        /// <param name="duration">Amount of time to shake for.</param>
        public void CameraShake(float amount, float duration) {
            StartCoroutine(DoCameraShake(amount, duration));
        }

        /// <summary>
        /// Shake the game camera, typically to show impact.
        /// </summary>
        /// <param name="amount">Amount to shake. Higher = more vigorous.</param>
        /// <param name="duration">Amount of time to shake for.</param>
        /// <returns>N/A.</returns>
        IEnumerator DoCameraShake(float amount, float duration) {
            float initialDuration = duration;

            while (duration > 0) {
                duration -= Time.deltaTime;

                float durationFactor = duration / initialDuration;

                // Set the camera's position to a random value modulated by the duration.
                // It returns to its actual position as the duration nears 0.
                cam.transform.position += (Random.insideUnitSphere * durationFactor);

                yield return null;
            }
        }

        public void GetNewRock() { StartCoroutine(DoNewRockRoutine()); }

        /// <summary>
        /// Spawn a new rock for the player.
        /// </summary>
        /// <returns>N/A.</returns>
        public IEnumerator DoNewRockRoutine() {
            // Lock input and new rock button.
            InputManager.Instance.LockInput();
            UIManager.Instance.SetButtonEnabled(UIManager.Instance.newRockButton, false);

            // Save initial cam position
            Vector3 camInitPos = cam.transform.position;

            // Lerp move camera off to right side.
            Vector3 desiredCamPos = cam.transform.right * Mathf.Max(VoxelGrid.Instance.X, VoxelGrid.Instance.Y, VoxelGrid.Instance.Z) * 2f;

            while (Vector3.Distance(desiredCamPos, cam.transform.position) > 0.05f) {
                cam.transform.position = Vector3.Lerp(cam.transform.position, desiredCamPos, Time.deltaTime * rockGenMoveSmoothing);
                yield return null;
            }

            // Destroy the old rock and regenerate a new one.
            VoxelGrid.Instance.Generate();

            // Move camera directly to left side of new rock.
            cam.transform.position = camInitPos - cam.transform.right * Mathf.Max(VoxelGrid.Instance.X, VoxelGrid.Instance.Y, VoxelGrid.Instance.Z) * 2f;

            // Lerp move camera to original position.
            desiredCamPos = camInitPos;

            while (Vector3.Distance(desiredCamPos, cam.transform.position) > 0.05f) {
                cam.transform.position = Vector3.Lerp(cam.transform.position, desiredCamPos, Time.deltaTime * rockGenMoveSmoothing);
                yield return null;
            }

            // Unlock input.
            InputManager.Instance.LockInput(false);
            UIManager.Instance.SetButtonEnabled(UIManager.Instance.newRockButton, true);
            yield return null;
        }
    }
}
