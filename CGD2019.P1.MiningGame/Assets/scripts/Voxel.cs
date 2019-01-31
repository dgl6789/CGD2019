using System;
using UnityEngine;

namespace App.Gameplay {

    [Serializable]
    public class Voxel {

        // Type of this voxel (see VoxelType in VoxelGrid.cs).
        private int type;
        public VoxelType Type { get { return (VoxelType)type; } }

        // Default initializes type to air (nothing).
        public Voxel() { type = (int)VoxelType.AIR; }

        // Initialize with given voxel type.
        public Voxel(VoxelType type) { this.type = (int)type; }

        /// <summary>
        /// Try to destroy this voxel (i.e. replace it with air).
        /// </summary>
        /// <param name="power">Power of the tool performing the attempted destruction.</param>
        /// <returns>Whether the voxel withstands the attempted destruction.</returns>
        public bool CanDestroy(int power) { return power >= type && type > 0; }

        public void DoDestroy() {
            /// TODO: Place per-voxel destruction behavior here.
        }
    }
}
