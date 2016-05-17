using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour {
	public Voxel[,,] m_Voxels = new Voxel[ChunkSize, ChunkSize, ChunkSize];
	public static int ChunkSize = 32;
	public bool DoUpdate = true;

	internal MeshFilter Filter;
	internal MeshCollider Collider;

	public Group Group;
	public WorldPos Pos;
	public bool Editable = true;

	// Use this for initialization
	void Start() {
		Filter = GetComponent<MeshFilter>();
		Collider = GetComponent<MeshCollider>();
    }

	// Update is called once per frame
	void Update() {
		if (DoUpdate) {
			//DoUpdate = false;
			UpdateChunk();
		}
	}

	public Voxel GetVoxel(WorldPos pos) {
		return GetVoxel(pos.X, pos.Y, pos.Z);
	}

	public Voxel GetVoxel(int x, int y, int z) {
		if (InRange(x) && InRange(y) && InRange(z)) {
			return m_Voxels[x, y, z];
		}
		return Group.GetVoxel(Pos.X + x, Pos.Y + y, Pos.Z + z);
	}

	public static bool InRange(int index) {
		return !(index < 0 || index >= ChunkSize);
	}

	public void SetVoxel(int x, int y, int z, Voxel voxel, bool count = true) {
		if (InRange(x) && InRange(y) && InRange(z)) {
			m_Voxels[x, y, z] = voxel;
			if (count) {
				if (voxel.GetType() == typeof(VoxelAir)) {
					VoxelEditor.Instance.VoxelCount--;
				}
				else {
					VoxelEditor.Instance.VoxelCount++;
				}
			}
		}
		else {
			Group.SetVoxel(Pos.X + x, Pos.Y + y, Pos.Z + z, voxel);
		}
	}

	public void UpdateChunk() {
		MeshData mData = new MeshData();

		for (int x = 0; x < ChunkSize; x++) {
			for (int y = 0; y < ChunkSize; y++) {
				for (int z = 0; z < ChunkSize; z++) {

					mData = m_Voxels[x, y, z].VoxelData(this, x, y, z, mData);
				}
			}
		}

		RenderMesh(mData);
	}

	public void RenderMesh(MeshData meshData) {
		Filter.mesh.Clear();
		Filter.mesh.vertices = meshData.Vertices.ToArray();
		Filter.mesh.triangles = meshData.Triangles.ToArray();
		Filter.mesh.SetColors(meshData.ColorVertices);

		Filter.mesh.uv = meshData.UV.ToArray();
		Filter.mesh.RecalculateNormals();

		Collider.sharedMesh = null;
		Mesh mesh = new Mesh();
		mesh.vertices = meshData.ColliderVertices.ToArray();
		mesh.triangles = meshData.ColliderTriangles.ToArray();
		mesh.RecalculateNormals();

		Collider.sharedMesh = mesh;
	}
}