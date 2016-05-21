using UnityEngine;
using System.Collections;
using System;

public struct Tile {
	public int x;
	public int y;
}

public enum Direction {
	North,
	East,
	South,
	West,
	Up,
	Down
}

public class Voxel {

	public Color ModColor = new Color(0.25f, 0.25f, 1f, 0.5f);
	public Color VoxelColor = Color.gray;

	public Color RealColor {
		get {
			if (Selected) return VoxelEditor.Instance.GetModifiedColor(this);

			return VoxelColor;
		}
	}

	public const float TileSize = 1f;

	public bool Selected = false;

	public Voxel() {

	}

	public virtual Tile TexturePosition(Direction direction) {
		Tile tile = new Tile()
			{
			x = 0,
			y = 0
		};

		return tile;
	}

	public virtual Vector2[] GetFaceUVs(Direction direction) {
		Vector2[] UVs = new Vector2[4];
		Tile tilePos = TexturePosition(direction);

		UVs[0] = new Vector2(TileSize * tilePos.x + TileSize, TileSize * tilePos.y);

		UVs[1] = new Vector2(TileSize * tilePos.x + TileSize, TileSize * tilePos.y + TileSize);

		UVs[2] = new Vector2(TileSize * tilePos.x, TileSize * tilePos.y + TileSize);

		UVs[3] = new Vector2(TileSize * tilePos.x, TileSize * tilePos.y);

		return UVs;
	}

	public virtual MeshData VoxelData(Chunk chunk, int x, int y, int z, MeshData meshData) {
		if (!chunk.GetVoxel(x, y + 1, z).IsSolid(Direction.Down)) {
			meshData = SetFaceDataUp(chunk, x, y, z, meshData);
		}

		if (!chunk.GetVoxel(x, y - 1, z).IsSolid(Direction.Up)) {
			meshData = SetFaceDataDown(chunk, x, y, z, meshData);
		}

		if (!chunk.GetVoxel(x, y, z + 1).IsSolid(Direction.South)) {
			meshData = SetFaceDataNorth(chunk, x, y, z, meshData);
		}

		if (!chunk.GetVoxel(x, y, z - 1).IsSolid(Direction.North)) {
			meshData = SetFaceDataSouth(chunk, x, y, z, meshData);
		}

		if (!chunk.GetVoxel(x + 1, y, z).IsSolid(Direction.West)) {
			meshData = SetFaceDataEast(chunk, x, y, z, meshData);
		}

		if (!chunk.GetVoxel(x - 1, y, z).IsSolid(Direction.East)) {
			meshData = SetFaceDataWest(chunk, x, y, z, meshData);
		}

		return meshData;
	}

	public virtual bool IsSolid(Direction direction) {
		return true;
	}

	protected virtual MeshData SetFaceDataUp(Chunk chunk, int x, int y, int z, MeshData meshData) {
		meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), RealColor);

		meshData.AddQuadTriangles();

		meshData.UV.AddRange(GetFaceUVs(Direction.Up));

		return meshData;
	}

	protected virtual MeshData SetFaceDataDown(Chunk chunk, int x, int y, int z, MeshData meshData) {
		meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), RealColor);

		meshData.AddQuadTriangles();

		meshData.UV.AddRange(GetFaceUVs(Direction.Down));

		return meshData;
	}

	protected virtual MeshData SetFaceDataNorth(Chunk chunk, int x, int y, int z, MeshData meshData) {
		meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), RealColor);

		meshData.AddQuadTriangles();

		meshData.UV.AddRange(GetFaceUVs(Direction.North));

		return meshData;
	}

	protected virtual MeshData SetFaceDataEast(Chunk chunk, int x, int y, int z, MeshData meshData) {
		meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), RealColor);

		meshData.AddQuadTriangles();

		meshData.UV.AddRange(GetFaceUVs(Direction.East));

		return meshData;
	}

	protected virtual MeshData SetFaceDataSouth(Chunk chunk, int x, int y, int z, MeshData meshData) {
		meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), RealColor);

		meshData.AddQuadTriangles();

		meshData.UV.AddRange(GetFaceUVs(Direction.South));

		return meshData;
	}

	protected virtual MeshData SetFaceDataWest(Chunk chunk, int x, int y, int z, MeshData meshData) {
		meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), RealColor);
		meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), RealColor);

		meshData.AddQuadTriangles();

		meshData.UV.AddRange(GetFaceUVs(Direction.West));

		return meshData;
	}
}

public class VoxelAir : Voxel {

	public VoxelAir() {
		VoxelColor = Color.clear;
	}

	public override MeshData VoxelData(Chunk chunk, int x, int y, int z, MeshData meshData) {

		if (Selected) return base.VoxelData(chunk, x, y, z, meshData);

		return meshData;
	}

	public override bool IsSolid(Direction direction) {
		return false;
	}
}