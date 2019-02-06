using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.Gameplay;
using App.UI;

namespace App
{
    public class RockManager : MonoBehaviour
    {
        public static RockManager Instance;

        [SerializeField] float rockGenMoveSmoothing;

        Camera cam;

        // How much integrity per solid voxel does a rock have?
        [SerializeField] float baseIntegrityFactor;

        // How much integrity should be lost per voxel destoryed?
        [SerializeField] float baseIntegrityLossFactor;
        float maxIntegrity;
        float currentIntegrity;
        public float Integrity { get { return currentIntegrity / maxIntegrity; } }

        public int Volume { get { return VoxelGrid.Instance.Volume; } }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            cam = Camera.main;
        }

        /// <summary>
        /// Set the rock's integrity.
        /// </summary>
        /// <param name="integrity">Integrity to set to.</param>
        void SetRockIntegrity(float integrity) {
            currentIntegrity = integrity;

            UIManager.Instance.SetRockIntegrity(Integrity);
        }

        /// <summary>
        /// Adjust the rock's integrity (additive).
        /// </summary>
        /// <param name="adj">Amount to add to the rock's integrity.</param>
        void AdjustRockIntegrity(float adj) {
            currentIntegrity += adj;

            UIManager.Instance.SetRockIntegrity(Integrity);

            if(currentIntegrity <= 0) {
                // The rock broke.
                OnRockBreak();
            }
        }

        /// <summary>
        /// Adjust the rock's integrity by multiplying its base integrity by a value.
        /// </summary>
        /// <param name="adj">Amount (0 - 1) to multiply the base integrity by.</param>
        public void AdjustRockIntegrityPercentage(float adj) {
            currentIntegrity = maxIntegrity * Mathf.Clamp01(adj);

            if (currentIntegrity <= 0) {
                // The rock broke.
                OnRockBreak();
            }
        }

        /// <summary>
        /// Run when a voxel is broken.
        /// </summary>
        /// <param name="tool">Tool used to break the voxel.</param>
        public void OnBreakVoxel(ToolItem tool) {
            AdjustRockIntegrity(-tool.Precision * baseIntegrityLossFactor);
        }

        /// <summary>
        /// Perform functionality upon breaking the rock.
        /// </summary>
        public void OnRockBreak() {
            VoxelGrid.Instance.DestroyAllVoxels();
            VoxelGrid.Instance.DestroyAllGems();

            FXManager.Instance.SpawnRockBreakParticles();
        }

        /// <summary>
        /// Set the initial integrity of a rock.
        /// </summary>
        public void SetupNewRockIntegrity() {
            maxIntegrity = Volume * baseIntegrityFactor;
            SetRockIntegrity(maxIntegrity);
        }

        /// <summary>
        /// Spawn a new rock for the player.
        /// </summary>
        public void GetNewRock() { StartCoroutine(DoNewRockRoutine()); }

        /// <summary>
        /// Spawn a new rock for the player.
        /// </summary>
        /// <returns>N/A.</returns>
        public IEnumerator DoNewRockRoutine()
        {
            // Lock input and new rock button.
            InputManager.Instance.LockInput();
            UIManager.Instance.SetButtonEnabled(UIManager.Instance.newRockButton, false);

            // Save initial cam position
            Vector3 camInitPos = cam.transform.position;

            // Lerp move camera off to right side.
            Vector3 desiredCamPos = cam.transform.right * Mathf.Max(VoxelGrid.Instance.X, VoxelGrid.Instance.Y, VoxelGrid.Instance.Z) * 2f;

            while (Vector3.Distance(desiredCamPos, cam.transform.position) > 0.05f)
            {
                cam.transform.position = Vector3.Lerp(cam.transform.position, desiredCamPos, Time.deltaTime * rockGenMoveSmoothing);
                yield return null;
            }

            // Destroy the old rock and regenerate a new one.
            VoxelGrid.Instance.Generate();

            // Move camera directly to left side of new rock.
            cam.transform.position = camInitPos - cam.transform.right * Mathf.Max(VoxelGrid.Instance.X, VoxelGrid.Instance.Y, VoxelGrid.Instance.Z) * 2f;

            // Lerp move camera to original position.
            desiredCamPos = camInitPos;

            while (Vector3.Distance(desiredCamPos, cam.transform.position) > 0.05f)
            {
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
