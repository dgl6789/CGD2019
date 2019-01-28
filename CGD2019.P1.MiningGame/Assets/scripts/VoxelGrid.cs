using System.Collections.Generic;
using UnityEngine;

namespace App.Gameplay {

    /// <summary>
    /// Types that a voxel can have. 0 is intentionally unused.
    /// </summary>
    public enum VoxelType { AIR = -1, ROCK = 1 }

    [SelectionBase]
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class VoxelGrid : MonoBehaviour {

        public static VoxelGrid Instance;

        [Range(0, 1)]
        public float spawnRate;
        public float xMin;
        public float yMin;
        public float zMin;

        // Rock bounds
        [SerializeField] Vector3 rockBounds;
        public float XBounds { get { return rockBounds.x; } }
        public float YBounds { get { return rockBounds.y; } }
        public float ZBounds { get { return rockBounds.z; } }

        // Cutoff for the surface in marching cubes.
        [SerializeField] float Surface;

        [SerializeField] Vector3Int dimensions;

        [SerializeField] Material material;

        // Must be divisible by 3 (for even triangles)
        [SerializeField] int maxVertsPerMesh;

        // Voxel data layer
        Voxel[,,] data;
        public int X { get { return dimensions.x; } }
        public int Y { get { return dimensions.y; } }
        public int Z { get { return dimensions.z; } }

        // Geometry lists
        List<Vector3> vertexList;
        List<int> indexList;

        int faceCount;

        // Mesh collider components
        MeshCollider meshCollider;
        Mesh collisionMesh;

        List<GameObject> meshes = new List<GameObject>();

        public Vector3 Center {
            get {
                return ((Vector3)dimensions / 2);
            }
        }

        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        /// Initialization
        private void Start() {
            // Initialize the vertex and index lists
            vertexList = new List<Vector3>();
            indexList = new List<int>();

            // Initialize the components of the mesh collider
            meshCollider = GetComponent<MeshCollider>();
            collisionMesh = GetComponent<MeshFilter>().mesh;

            // Initial generation of the rock
            Generate();
        }

        /// <summary>
        /// Generates a rock.
        /// </summary>
        public void Generate() {

            // Initialize the data layer.
            data = new Voxel[X, Y, Z];

            // Clear geometry lists.
            vertexList.Clear();
            indexList.Clear();

            // Populate the data layer.
            // This is where initial shape generation code will go
            shapedRock();

            // Generate gems
            Gemeration.Instance.GenerateGems();

            // Generate the initial visual mesh and collider
            UpdateVisualMesh();
            UpdateCollisionMesh();
        }
        
        private void shapedRock()
        {
            float temp = Random.value;

            float xBounds = Random.Range(xMin, (X / 2)-2);
            float yBounds = Random.Range(yMin, (Y / 2)-2);
            float zBounds = Random.Range(zMin, (Z / 2)-2);

            rockBounds.x = xBounds;
            rockBounds.y = yBounds;
            rockBounds.z = zBounds;

            if (temp <= .33f)
            {
                yBounds = Random.Range(xBounds - 2, xBounds + 2);
                zBounds = Random.Range(xBounds - 2, xBounds + 2);
            }
            else if (temp <= .66f)
            {
                xBounds = Random.Range(yBounds - 2, yBounds + 2);
                zBounds = Random.Range(yBounds - 2, yBounds + 2);
            }
            else
            {
                xBounds = Random.Range(yBounds - 2, yBounds + 2);
                zBounds = Random.Range(yBounds - 2, yBounds + 2);
            }

            for (int x = 0; x < X; x++)
            {
                for (int y = 0; y < Y; y++)
                {
                    for (int z = 0; z < Z; z++)
                    {
                        if (Mathf.Pow(x-X/2f, 2) / Mathf.Pow(xBounds, 2) + Mathf.Pow(y-Y/2f, 2) / Mathf.Pow(yBounds, 2) + Mathf.Pow(z-Z/2f, 2) / Mathf.Pow(zBounds, 2) <= 1f) {
                            data[x, y, z] = Random.value < spawnRate ? new Voxel(VoxelType.ROCK) : new Voxel(VoxelType.AIR);
                        } else data[x, y, z] = new Voxel(VoxelType.AIR);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the voxel at coordinates to VoxelType type.
        /// </summary>
        /// <param name="x">X position of the voxel to set.</param>
        /// <param name="y">Y position of the voxel to set.</param>
        /// <param name="z">Z position of the voxel to set.</param>
        /// <param name="type">Type to set.</param>
        public void SetVoxelTypeAtIndex(int x, int y, int z, VoxelType type) {
            if (IndexIsValid(x, y, z)) {
                data[x, y, z] = new Voxel(type);

                UpdateVisualMesh();
                UpdateCollisionMesh();
            }
        }

        /// <summary>
        /// Get the integer type of the voxel at coordinates.
        /// </summary>
        /// <param name="x">X position of the voxel whose data to get.</param>
        /// <param name="y">X position of the voxel whose data to get.</param>
        /// <param name="z">X position of the voxel whose data to get.</param>
        /// <returns>Integer type of the voxel, validity-checked.</returns>
        public int GetData(int x, int y, int z) { return IndexIsValid(x, y, z) ? (int)data[x, y, z].Type : -1; }

        /// <summary>
        /// Checks whether the coordinates are within the bounds of the voxel grid.
        /// </summary>
        /// <param name="x">X position to check.</param>
        /// <param name="y">Y position to check.</param>
        /// <param name="z">Z position to check.</param>
        /// <returns>Whether the coordinates are valid.</returns>
        public bool IndexIsValid(int x, int y, int z) { return !(x >= X || x < 0 || y >= Y || y < 0 || z >= Z || z < 0); }

        #region Geometry & Collider Generation

        /// <summary>
        /// Creates a collider for a voxel.
        /// </summary>
        /// <param name="x">X position of the voxel to generate collision for.</param>
        /// <param name="y">Y position of the voxel to generate collision for.</param>
        /// <param name="z">Z position of the voxel to generate collision for.</param>
        void CreateCollisionVoxel(int x, int y, int z) {
            if (GetData(x, y, z) != (int)VoxelType.AIR) {
                if (GetData(x, y + 1, z) == (int)VoxelType.AIR) CollisionVoxelTop(x, y, z);
                if (GetData(x, y - 1, z) == (int)VoxelType.AIR) CollisionVoxelBottom(x, y, z);
                if (GetData(x + 1, y, z) == (int)VoxelType.AIR) CollisionVoxelEast(x, y, z);
                if (GetData(x - 1, y, z) == (int)VoxelType.AIR) CollisionVoxelWest(x, y, z);
                if (GetData(x, y, z + 1) == (int)VoxelType.AIR) CollisionVoxelNorth(x, y, z);
                if (GetData(x, y, z - 1) == (int)VoxelType.AIR) CollisionVoxelSouth(x, y, z);
            }
        }

        /// Create the up-facing geometry for a cube voxel.
        void CollisionVoxelTop(int x, int y, int z) {
            vertexList.Add(new Vector3(x, y + 1, z + 1));
            vertexList.Add(new Vector3(x + 1, y + 1, z + 1));
            vertexList.Add(new Vector3(x + 1, y + 1, z));
            vertexList.Add(new Vector3(x, y + 1, z));

            AddCollisionTriangles();
        }

        /// Create the down-facing geometry for a cube voxel.
        void CollisionVoxelBottom(int x, int y, int z) {
            vertexList.Add(new Vector3(x, y, z));
            vertexList.Add(new Vector3(x + 1, y, z));
            vertexList.Add(new Vector3(x + 1, y, z + 1));
            vertexList.Add(new Vector3(x, y, z + 1));

            AddCollisionTriangles();
        }

        /// Create the up-facing geometry for a cube voxel.
        void CollisionVoxelNorth(int x, int y, int z) {
            vertexList.Add(new Vector3(x + 1, y, z + 1));
            vertexList.Add(new Vector3(x + 1, y + 1, z + 1));
            vertexList.Add(new Vector3(x, y + 1, z + 1));
            vertexList.Add(new Vector3(x, y, z + 1));

            AddCollisionTriangles();
        }

        /// Create the up-facing geometry for a cube voxel.
        void CollisionVoxelSouth(int x, int y, int z) {
            vertexList.Add(new Vector3(x, y, z));
            vertexList.Add(new Vector3(x, y + 1, z));
            vertexList.Add(new Vector3(x + 1, y + 1, z));
            vertexList.Add(new Vector3(x + 1, y, z));

            AddCollisionTriangles();
        }

        /// Create the up-facing geometry for a cube voxel.
        void CollisionVoxelWest(int x, int y, int z) {
            vertexList.Add(new Vector3(x, y, z + 1));
            vertexList.Add(new Vector3(x, y + 1, z + 1));
            vertexList.Add(new Vector3(x, y + 1, z));
            vertexList.Add(new Vector3(x, y, z));

            AddCollisionTriangles();
        }

        /// Create the up-facing geometry for a cube voxel.
        void CollisionVoxelEast(int x, int y, int z) {
            vertexList.Add(new Vector3(x + 1, y, z));
            vertexList.Add(new Vector3(x + 1, y + 1, z));
            vertexList.Add(new Vector3(x + 1, y + 1, z + 1));
            vertexList.Add(new Vector3(x + 1, y, z + 1));

            AddCollisionTriangles();
        }

        /// Add a quad to the index list.
        void AddCollisionTriangles() {
            indexList.Add(faceCount * 4);
            indexList.Add(faceCount * 4 + 1);
            indexList.Add(faceCount * 4 + 2);
            indexList.Add(faceCount * 4); 
            indexList.Add(faceCount * 4 + 2);
            indexList.Add(faceCount * 4 + 3);

            faceCount++;
        }

        /// <summary>
        /// Updates the collision mesh for the voxel grid.
        /// </summary>
        void UpdateCollisionMesh() {

            // Clear the vertex and index lists, and reset the face count.
            vertexList.Clear();
            indexList.Clear();

            faceCount = 0;

            for (int x = 0; x < X; x++) {
                for (int y = 0; y < Y; y++) {
                    for (int z = 0; z < Z; z++) {
                        // Create a collision voxel (maybe) at each array position.
                        CreateCollisionVoxel(x, y, z);
                    }
                }
            }

            // Apply the new vertices and indices to the mesh collider.
            collisionMesh.Clear();

            collisionMesh.SetVertices(vertexList);
            collisionMesh.SetTriangles(indexList, 0);
            collisionMesh.RecalculateNormals();
            collisionMesh.RecalculateBounds();

            meshCollider.sharedMesh = collisionMesh;
        }
        
        /// <summary>
        /// Updates the visual component of the voxel mesh.
        /// </summary>
        void UpdateVisualMesh() {
            // Clear the vertex and index lists
            vertexList.Clear();
            indexList.Clear();

            // Re-march relevant cubes
            // Need to march a grid of cubes that is 1 unit larger on each axis than the maximum dimensions of the rock.
            for (int x = -1; x < X; x++) {
                for (int y = -1; y < Y; y++) {
                    for (int z = -1; z < Z; z++) {

                        // March each virtual cell (see March(int x, int y, int z))
                        /// TODO: Optimize this to re-march only affected cubes when removing voxels from the grid.
                        March(x, y, z);
                    }
                }
            }

            // Apply the marched vertex and index lists to a number of final visual meshes.
            GenerateFinalVisualMesh();
        }

        /// <summary>
        /// Generate the final marched cubes mesh.
        /// If the vertex count is too high for a single mesh, it is split into multiple meshes.
        /// 
        /// This implementation is adapted from https://github.com/Scrawk/Marching-Cubes. 
        /// See license in MarchingCubes.cs
        /// </summary>
        void GenerateFinalVisualMesh() {
            // Unity has a vertex limit of ~65000 for a mesh. 
            // Our vert list could end up with more than that, so we gotta split up the meshes in those cases.

            // First clear the current mesh(es).
            foreach(GameObject m in meshes) { Destroy(m); }
            meshes = new List<GameObject>();

            // Determine how many meshes need to be made.
            int numMeshes = vertexList.Count / maxVertsPerMesh + 1;

            // Create each mesh.
            for (int i = 0; i < numMeshes; i++) {

                List<Vector3> splitVerts = new List<Vector3>();
                List<int> splitIndices = new List<int>();

                // Make new geometry lists that contain a portion of the total geometry.
                for (int j = 0; j < maxVertsPerMesh; j++) {
                    int index = i * maxVertsPerMesh + j;

                    if (index < vertexList.Count) {
                        splitVerts.Add(vertexList[index]);
                        splitIndices.Add(j);
                    }
                }

                if (splitVerts.Count == 0) continue;

                // Apply geometry from the split lists to a new mesh.
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

                // Align the visual mesh to the mesh collider
                meshObject.transform.localPosition += new Vector3(0.5f, 0.5f, 0.5f);

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

        /// <summary>
        /// Get the offset of the surface of the mesh between two virtual vertices.
        /// 
        /// This implementation is adapted from https://github.com/Scrawk/Marching-Cubes. 
        /// See license in MarchingCubes.cs
        /// </summary>
        /// <param name="v1">First vertex.</param>
        /// <param name="v2">Second vertex.</param>
        /// <returns>Offset from first to second vertex of the surface of the mesh.</returns>
        float GetOffset(float v1, float v2) {
            float delta = v2 - v1;
            return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
        }

        #endregion
    }
}
