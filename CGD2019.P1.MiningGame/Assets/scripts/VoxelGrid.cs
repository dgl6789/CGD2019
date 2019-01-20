using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Gameplay {

    public enum VoxelType { AIR, ROCK }

    [SelectionBase]
    public class VoxelGrid : MonoBehaviour {

        [SerializeField] Vector3Int dimensions;

        [SerializeField] Material debugMaterial;

        Voxel[,,] data;
        public int X { get { return dimensions.x; } }
        public int Y { get { return dimensions.y; } }
        public int Z { get { return dimensions.z; } }

        List<Vector3> newVertices;
        List<int> newTriangles;
        List<Vector2> newUV;

        Mesh mesh;
        MeshRenderer render;
        MeshCollider col;

        int faceCount;

        private void Start() {
            mesh = GetComponent<MeshFilter>().mesh;
            col = GetComponent<MeshCollider>();
            render = GetComponent<MeshRenderer>();

            Generate();
        }

        public void Generate() {
            data = new Voxel[X, Y, Z];

            newVertices = new List<Vector3>();
            newTriangles = new List<int>();
            newUV = new List<Vector2>();

            for (int x = 0; x < X; x++) {
                for (int y = 0; y < Y; y++) {
                    for (int z = 0; z < Z; z++) {
                        // This is where initial shape generation code will go
                        data[x, y, z] = new Voxel(VoxelType.ROCK);
                    }
                }
            }

            UpdateMesh();
        }

        public bool IndexIsValid(int x, int y, int z) {
            return !(x >= X || x < 0 || y >= Y || y < 0 || z >= Z || z < 0);
        }

        public void SetVoxelTypeAtPosition(Vector3Int position, VoxelType type) {
            SetVoxelTypeAtIndex(position.x, position.y, position.z, type);
        }

        public void SetVoxelTypeAtIndex(int x, int y, int z, VoxelType type) {
            if (IndexIsValid(x, y, z)) {
                data[x, y, z] = new Voxel(type);

                UpdateMesh();
            }
        }

        public byte GetData(int x, int y, int z) {
            return IndexIsValid(x, y, z) ? GetType(data[x, y, z].Type) : (byte)0;
        }

        public VoxelType GetType(byte type) { return (VoxelType)type; }

        public byte GetType(VoxelType type) { return (byte)type; }

        #region Geometry Generation
        void CreateVoxel(int x, int y, int z) {
            if (GetData(x, y, z) != 0) {
                if (GetData(x, y + 1, z) == 0) VoxelTop(x, y, z, GetData(x, y, z));
                if (GetData(x, y - 1, z) == 0) VoxelBottom(x, y, z, GetData(x, y, z));
                if (GetData(x + 1, y, z) == 0) VoxelEast(x, y, z, GetData(x, y, z));
                if (GetData(x - 1, y, z) == 0) VoxelWest(x, y, z, GetData(x, y, z));
                if (GetData(x, y, z + 1) == 0) VoxelNorth(x, y, z, GetData(x, y, z));
                if (GetData(x, y, z - 1) == 0) VoxelSouth(x, y, z, GetData(x, y, z));
            }
        }

        void VoxelTop(int x, int y, int z, byte type) {
            newVertices.Add(new Vector3(x, y + 1, z + 1));
            newVertices.Add(new Vector3(x + 1, y + 1, z + 1));
            newVertices.Add(new Vector3(x + 1, y + 1, z));
            newVertices.Add(new Vector3(x, y + 1, z));

            AddTrianglesQuad();
        }

        void VoxelBottom(int x, int y, int z, byte type) {
            newVertices.Add(new Vector3(x, y, z));
            newVertices.Add(new Vector3(x + 1, y, z));
            newVertices.Add(new Vector3(x + 1, y, z + 1));
            newVertices.Add(new Vector3(x, y, z + 1));

            AddTrianglesQuad();
        }

        void VoxelNorth(int x, int y, int z, byte type) {
            newVertices.Add(new Vector3(x + 1, y, z + 1));
            newVertices.Add(new Vector3(x + 1, y + 1, z + 1));
            newVertices.Add(new Vector3(x, y + 1, z + 1));
            newVertices.Add(new Vector3(x, y, z + 1));

            AddTrianglesQuad();
        }

        void VoxelSouth(int x, int y, int z, byte type) {
            newVertices.Add(new Vector3(x, y, z));
            newVertices.Add(new Vector3(x, y + 1, z));
            newVertices.Add(new Vector3(x + 1, y + 1, z));
            newVertices.Add(new Vector3(x + 1, y, z));

            AddTrianglesQuad();
        }

        void VoxelWest(int x, int y, int z, byte type) {
            newVertices.Add(new Vector3(x, y, z + 1));
            newVertices.Add(new Vector3(x, y + 1, z + 1));
            newVertices.Add(new Vector3(x, y + 1, z));
            newVertices.Add(new Vector3(x, y, z));

            AddTrianglesQuad();
        }

        void VoxelEast(int x, int y, int z, byte type) {
            newVertices.Add(new Vector3(x + 1, y, z));
            newVertices.Add(new Vector3(x + 1, y + 1, z));
            newVertices.Add(new Vector3(x + 1, y + 1, z + 1));
            newVertices.Add(new Vector3(x + 1, y, z + 1));

            AddTrianglesQuad();
        }

        void AddTrianglesQuad() {
            newTriangles.Add(faceCount * 4); //1
            newTriangles.Add(faceCount * 4 + 1); //2
            newTriangles.Add(faceCount * 4 + 2); //3
            newTriangles.Add(faceCount * 4); //1
            newTriangles.Add(faceCount * 4 + 2); //3
            newTriangles.Add(faceCount * 4 + 3); //4

            faceCount++;
        }

        void UpdateMesh() {
            for (int x = 0; x < X; x++) {
                for (int y = 0; y < Y; y++) {
                    for (int z = 0; z < Z; z++) {
                        CreateVoxel(x, y, z);
                    }
                }
            }

            render.material = debugMaterial;

            mesh.Clear();
            mesh.vertices = newVertices.ToArray();
            mesh.uv = newUV.ToArray();
            mesh.triangles = newTriangles.ToArray();
            mesh.RecalculateNormals();

            col.sharedMesh = null;
            col.sharedMesh = mesh;

            newVertices.Clear();
            newUV.Clear();
            newTriangles.Clear();

            faceCount = 0;
        }
        #endregion
    }
}
