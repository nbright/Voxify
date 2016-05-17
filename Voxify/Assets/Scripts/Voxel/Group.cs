using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct WorldPos {
	public int X, Y, Z;

	public WorldPos(int x, int y, int z) {
		X = x;
		Y = y;
		Z = z;
	}

	public WorldPos(Vector3 pos) {
		X = Mathf.RoundToInt(pos.x);
		Y = Mathf.RoundToInt(pos.y);
		Z = Mathf.RoundToInt(pos.z);
	}

	public override bool Equals(object obj) {
		if (!(obj is WorldPos))
			return false;

		WorldPos pos = (WorldPos)obj;
		return !(pos.X != X || pos.Y != Y || pos.Z != Z);
	}

	public override int GetHashCode() {
		unchecked {
			int hash = (int)2166136261;

			hash *= 16777619 ^ X.GetHashCode();
			hash *= 16777619 ^ Y.GetHashCode();
			hash *= 16777619 ^ Z.GetHashCode();

			return hash;
		}
	}

	public override string ToString() {
		return string.Format("({0},{1},{2})", X, Y, Z);
	}

	public string ToString(float scale) {
		return string.Format("({0},{1},{2})", X * scale, Y * scale, Z * scale);
	}

	public Vector3 ToVector3() {
		return new Vector3(X, Y, Z);
	}
}

public class Group : MonoBehaviour {
	public Dictionary<WorldPos, Chunk> Chunks = new Dictionary<WorldPos, Chunk>();
	public Group SelectionLayer;

	public GameObject ChunkPrefab;

	public bool Editable = true;
	public bool IsPlatform = false;

	public bool FirstBlock = true;

	public bool OpeningFile = false;

	public string GroupName = "Group";
	
	void Start() {
		int n = 0;
		while (VoxelEditor.Groups.ContainsKey(GroupName)) {
			GroupName = "Group_" + (n++).ToString();
		}

		VoxelEditor.Groups.Add(GroupName, this);

		if (!OpeningFile) {
			CreateStartChunk(0, 0, 0);

			for (int x = -1; x < 1; x++) {
				for (int y = 0; y < 1; y++) {
					for (int z = -1; z < 1; z++) {
						
						if (x == 0 && y == 0 && z == 0)
							continue;

						CreateEmptyChunk(x * Chunk.ChunkSize, y * Chunk.ChunkSize, z * Chunk.ChunkSize);
					}
				}
			}
		}
	}

	void OnDestroy() {
		VoxelEditor.Groups.Remove(GroupName);
	}

	public Chunk CreateEmptyChunk(int x, int y, int z) {
		WorldPos worldPos = new WorldPos(x, y, z);

		GameObject chunkGameObject = (GameObject)Instantiate(ChunkPrefab, new Vector3(x, y, z), Quaternion.identity);
		chunkGameObject.transform.SetParent(transform);

		Chunk chunk = chunkGameObject.GetComponent<Chunk>();

		chunk.Pos = worldPos;
		chunk.Group = this;

		Debug.Log("Creating empty chunk: " + worldPos.ToString());
		Chunks.Add(worldPos, chunk);

		GenerateEmptyChunk(x, y, z);

		return chunk;
	}

	public void CreateStartChunk(int x, int y, int z) {
		WorldPos worldPos = new WorldPos(x, y, z);

		GameObject chunkGameObject = (GameObject)Instantiate(ChunkPrefab, new Vector3(x, y, z), Quaternion.identity);
		chunkGameObject.transform.SetParent(transform);

		Chunk chunk = chunkGameObject.GetComponent<Chunk>();

		chunk.Pos = worldPos;
		chunk.Group = this;

		Debug.Log("Creating chunk: " + worldPos.ToString());
		Chunks.Add(worldPos, chunk);

		if (IsPlatform) {
			GeneratePlatformChunk(x, y, z);
		}
		else {
			GenerateEmptyChunk(x, y, z);
			SetVoxel(0, 0, 0, new Voxel() {
				VoxelColor = new Color(0f, 0f, 1f, .5f)
			});
		}
	}

	public void GenerateChunk(int x, int y, int z) {
		for (int xi = 0; xi < Chunk.ChunkSize; xi++) {
			for (int yi = 0; yi < Chunk.ChunkSize; yi++) {
				for (int zi = 0; zi < Chunk.ChunkSize; zi++) {
					if (FirstBlock && xi == Chunk.ChunkSize - 1 &&
						yi == 0 &&
						zi == Chunk.ChunkSize - 1) {
						SetVoxel(x + xi, y + yi, z + zi, new Voxel() {
							VoxelColor = Color.blue
						});
						FirstBlock = false;
					}
					else {
						SetVoxel(x + xi, y + yi, z + zi, new VoxelAir(), false);
					}
				}
			}
		}
	}

	public void GenerateEmptyChunk(int x, int y, int z) {
		for (int xi = 0; xi < Chunk.ChunkSize; xi++) {
			for (int yi = 0; yi < Chunk.ChunkSize; yi++) {
				for (int zi = 0; zi < Chunk.ChunkSize; zi++) {
					SetVoxel(x + xi, y + yi, z + zi, new VoxelAir(), false);
				}
			}
		}
	}

	public void GeneratePlatformChunk(int x, int y, int z) {
		for (int xi = 0; xi < Chunk.ChunkSize; xi++) {
			for (int yi = 0; yi < Chunk.ChunkSize; yi++) {
				for (int zi = 0; zi < Chunk.ChunkSize; zi++) {
					if (yi < 2) {
						SetVoxel(x + xi, y + yi, z + zi, new Voxel() {
							VoxelColor = Color.grey
						});
					}
					else {
						SetVoxel(x + xi, y + yi, z + zi, new VoxelAir(), false);
					}
				}
			}
		}
	}

	public void DestroyChunk(int x, int y, int z) {
		Chunk chunk = null;
		if (Chunks.TryGetValue(new WorldPos(x, y, z), out chunk)) {
			Destroy(chunk.gameObject);
			Chunks.Remove(new WorldPos(x, y, z));
		}
	}

	public Chunk GetChunk(int x, int y, int z) {
		WorldPos pos = new WorldPos();
		float multiple = Chunk.ChunkSize;
		pos.X = Mathf.FloorToInt(x / multiple) * Chunk.ChunkSize;
		pos.Y = Mathf.FloorToInt(y / multiple) * Chunk.ChunkSize;
		pos.Z = Mathf.FloorToInt(z / multiple) * Chunk.ChunkSize;

		Chunk containerChunk = null;

		Chunks.TryGetValue(pos, out containerChunk);

		return containerChunk;
	}

	public Voxel GetVoxel(WorldPos pos) {
		return GetVoxel(pos.X, pos.Y, pos.Z);
	}

	public Voxel GetVoxel(int x, int y, int z) {
		Chunk containerChunk = GetChunk(x, y, z);

		if (containerChunk != null) {
			Voxel voxel = containerChunk.GetVoxel(x - containerChunk.Pos.X,
					y - containerChunk.Pos.Y,
					z - containerChunk.Pos.Z);

			return voxel;
		}
		else {
			return new VoxelAir();
		}
	}

	public void SetVoxel(WorldPos pos, Voxel voxel, bool count = true) {
		SetVoxel(pos.X, pos.Y, pos.Z, voxel, count);
	}

	public void SetVoxel(int x, int y, int z, Voxel voxel, bool count = true) {
		if (!Editable) {
			return;
		}
		Chunk chunk = GetChunk(x, y, z);

		if (chunk != null && chunk.Editable) {
			chunk.SetVoxel(x - chunk.Pos.X, y - chunk.Pos.Y, z - chunk.Pos.Z, voxel, count);
			chunk.DoUpdate = true;

			UpdateIfEqual(x - chunk.Pos.X, 0, new WorldPos(x - 1, y, z));
			UpdateIfEqual(x - chunk.Pos.X, Chunk.ChunkSize - 1, new WorldPos(x + 1, y, z));
			UpdateIfEqual(y - chunk.Pos.Y, 0, new WorldPos(x, y - 1, z));
			UpdateIfEqual(y - chunk.Pos.Y, Chunk.ChunkSize - 1, new WorldPos(x, y + 1, z));
			UpdateIfEqual(z - chunk.Pos.Z, 0, new WorldPos(x, y, z - 1));
			UpdateIfEqual(z - chunk.Pos.Z, Chunk.ChunkSize - 1, new WorldPos(x, y, z + 1));
		}
	}

	void UpdateIfEqual(int value1, int value2, WorldPos pos) {
		if (value1 == value2) {
			Chunk chunk = GetChunk(pos.X, pos.Y, pos.Z);
			if (chunk != null) {
				chunk.DoUpdate = true;
			}
		}
	}
}