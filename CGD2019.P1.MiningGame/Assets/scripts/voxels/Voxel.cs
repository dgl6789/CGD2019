using System;
using UnityEngine;

namespace App.Gameplay {

    [Serializable]
    public class Voxel {

        // Type of this voxel (see VoxelType in VoxelGrid.cs).
        private int type;
        public VoxelType Type { get { return (VoxelType)type; } }

        // Toughness of this voxel, e.g. how powerful of a tool
        // must be used to break it?
        // For now, this is just the type of the voxel.
        public int Toughness { get { return type; } }

        Vector3Int position;
        public Vector3Int Position { get { return position; } }

        // Default initializes type to air (nothing).
        public Voxel() { type = (int)VoxelType.AIR; }

        // Initialize with given voxel type.
        public Voxel(VoxelType type, Vector3Int position) {
            this.type = (int)type;
            this.position = position;
        }

        /// <summary>
        /// Try to destroy this voxel (i.e. replace it with air).
        /// </summary>
        /// <param name="power">Power of the tool performing the attempted destruction.</param>
        /// <returns>Whether the voxel withstands the attempted destruction.</returns>
        public bool TryDestroy(int power) {
            if (Toughness < 0) return false;

            bool destroyed = power >= Toughness;
            
            if (destroyed) OnDestroyed();
            else OnFailDestroyed();

            return destroyed;
        }

        void OnDestroyed() {
            // Per-voxel destruction behavior.
            VoxelGrid.Instance.Volume--;
            RockManager.Instance.OnBreakVoxel(InventoryManager.Instance.ActiveTool, (VoxelType)type);

            type = (int)VoxelType.AIR;

            FXManager.Instance.SpawnDebrisParticles(position, type);
        }

        void OnFailDestroyed() {
            FXManager.Instance.SpawnSparkParticles(position);
        }
    }
}
