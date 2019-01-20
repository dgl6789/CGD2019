using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App;

namespace App.Gameplay {

    public enum VoxelType { AIR = -1, ROCK = 1 }

    [SelectionBase]
    public class VoxelGrid : MonoBehaviour {

        [SerializeField] bool showDebugCells;

        [SerializeField] float Surface;

        [SerializeField] Vector3Int dimensions;

        [SerializeField] Material material;

        [SerializeField] int maxVertsPerMesh; // Must be divisible by 3 (for even triangles)

        Voxel[,,] data;
        public int X { get { return dimensions.x; } }
        public int Y { get { return dimensions.y; } }
        public int Z { get { return dimensions.z; } }

        List<Vector3> vertexList;
        List<int> indexList;

        List<GameObject> meshes = new List<GameObject>();

        private void Start() {
            Generate();
        }

        public void Generate() {
            data = new Voxel[X, Y, Z];

            vertexList = new List<Vector3>();
            indexList = new List<int>();

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

        public int GetData(int x, int y, int z) {
            return IndexIsValid(x, y, z) ? GetType(data[x, y, z].Type) : -1;
        }

        public VoxelType GetType(int type) { return (VoxelType)type; }

        public int GetType(VoxelType type) { return (int)type; }

        #region Geometry Generation
        
        void UpdateMesh() {
            // Marching
            vertexList = new List<Vector3>();
            indexList = new List<int>();

            // Need to march a grid of cubes that is 1 unit larger on each axis than the maximum dimensions of the rock.
            for (int x = -1; x < X; x++) {
                for (int y = -1; y < Y; y++) {
                    for (int z = -1; z < Z; z++) {

                        // March each virtual cell (see March(int x, int y, int z))
                        March(x, y, z);
                    }
                }
            }

            GenerateFinalMeshes();
        }

        void GenerateFinalMeshes() {
            // Unity has a vertex limit of ~65000 for a mesh. 
            // Our vert list could end up with more than that, so we gotta split up the meshes in those cases.

            // First clear the current mesh(es).
            foreach(GameObject m in meshes) { Destroy(m); }
            meshes = new List<GameObject>();

            int numMeshes = vertexList.Count / maxVertsPerMesh + 1;

            for (int i = 0; i < numMeshes; i++) {

                List<Vector3> splitVerts = new List<Vector3>();
                List<int> splitIndices = new List<int>();

                for (int j = 0; j < maxVertsPerMesh; j++) {
                    int index = i * maxVertsPerMesh + j;

                    if (index < vertexList.Count) {
                        splitVerts.Add(vertexList[index]);
                        splitIndices.Add(j);
                    }
                }

                if (splitVerts.Count == 0) continue;

                Mesh mesh = new Mesh();
                mesh.SetVertices(splitVerts);
                mesh.SetTriangles(splitIndices, 0);
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                GameObject meshObject = new GameObject("Mesh " + (i + 1));
                meshObject.transform.parent = transform;
                meshObject.AddComponent<MeshFilter>();
                meshObject.AddComponent<MeshRenderer>();
                meshObject.GetComponent<Renderer>().material = material;
                meshObject.GetComponent<MeshFilter>().mesh = mesh;

                meshObject.AddComponent<MeshCollider>();

                meshes.Add(meshObject);
            }
        }

        /// <summary>
        /// Perform a marching cubes step on one voxel.
        /// 
        /// This implementation is adapted from https://github.com/Scrawk/Marching-Cubes. 
        /// See license in MarchingCubes.cs
        /// </summary>
        /// <param name="x">X position of the voxel to march.</param>
        /// <param name="y">Y position of the voxel to march.</param>
        /// <param name="z">Z position of the voxel to march.</param>
        void March(int x, int y, int z) {
            // Construct the virtual cell that will provide this march's surface information.
            float[] cell = new float[8];
            for(int i = 0; i < 8; i++) {
                cell[i] = GetData(
                    x + MarchingCubes.VertexOffset[i, 0], 
                    y + MarchingCubes.VertexOffset[i, 1], 
                    z + MarchingCubes.VertexOffset[i, 2]);
            }

            // Draw the cells if flagged to
            if(showDebugCells) {
                for(int i = 0; i < 12; i++) {
                    Debug.DrawLine(
                        new Vector3(
                            x + MarchingCubes.VertexOffset[MarchingCubes.EdgeConnection[i, 0], 0], 
                            y + MarchingCubes.VertexOffset[MarchingCubes.EdgeConnection[i, 0], 1], 
                            z + MarchingCubes.VertexOffset[MarchingCubes.EdgeConnection[i, 0], 2]),
                        new Vector3(
                            x + MarchingCubes.VertexOffset[MarchingCubes.EdgeConnection[i, 1], 0],
                            y + MarchingCubes.VertexOffset[MarchingCubes.EdgeConnection[i, 1], 1],
                            z + MarchingCubes.VertexOffset[MarchingCubes.EdgeConnection[i, 1], 2]
                        ), Color.green, 3);
                }
            }

            int flagIndex = 0;

            // Determine which vertices of this cell are inside the surface and which are not.
            for (int i = 0; i < 8; i++) if (cell[i] <= Surface) flagIndex |= 1 << i;

            int edgeFlags = MarchingCubes.CubeEdgeFlags[flagIndex];

            if (edgeFlags == 0) return; // no intersections with the surface, thus no geometry is generated.

            // Find the marched vertices of this cell. There are a maximum of 12 (one on each edge).
            Vector3[] EdgeVertex = new Vector3[12];

            for (int i = 0; i < 12; i++) {
                if ((edgeFlags & (1 << i)) != 0) {
                    // There is an intersection with the surface on this edge
                    float offset = GetOffset(cell[MarchingCubes.EdgeConnection[i, 0]], cell[MarchingCubes.EdgeConnection[i, 1]]);

                    EdgeVertex[i].x = x + (MarchingCubes.VertexOffset[MarchingCubes.EdgeConnection[i, 0], 0] + offset * MarchingCubes.EdgeDirection[i, 0]);
                    EdgeVertex[i].y = y + (MarchingCubes.VertexOffset[MarchingCubes.EdgeConnection[i, 0], 1] + offset * MarchingCubes.EdgeDirection[i, 1]);
                    EdgeVertex[i].z = z + (MarchingCubes.VertexOffset[MarchingCubes.EdgeConnection[i, 0], 2] + offset * MarchingCubes.EdgeDirection[i, 2]);
                }
            }

            int vert, index;

            // Determine the index configuration of the edge vertices
            // (These are listed in MarchingCubes.TriangleConnectionTable).
            for (int i = 0; i < 5; i++) {
                if (MarchingCubes.TriangleConnectionTable[flagIndex, i * 3] < 0) break;

                index = vertexList.Count;

                for (int j = 0; j < 3; j++) {
                    vert = MarchingCubes.TriangleConnectionTable[flagIndex, i * 3 + j];
                    indexList.Add(index + j); // This might be index + (2 - j); depending on the winding order
                    vertexList.Add(EdgeVertex[vert]);
                }
            }
        }

        float GetOffset(float v1, float v2) {
            float delta = v2 - v1;
            return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
        }
        #endregion
    }
}
