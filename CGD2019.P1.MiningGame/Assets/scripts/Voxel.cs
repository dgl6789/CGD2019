using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using App;

namespace App.Gameplay {

    [Serializable]
    public class Voxel {

        private byte type;
        public VoxelType Type { get { return (VoxelType)type; } }

        public Voxel() {

        }

        public Voxel(VoxelType type) {
            this.type = (byte)type;
        }
    }
}
