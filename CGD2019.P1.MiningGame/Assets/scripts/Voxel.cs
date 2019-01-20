using System;

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
    }
}
