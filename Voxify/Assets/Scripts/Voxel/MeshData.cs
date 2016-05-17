using UnityEngine;
using System.Collections.Generic;

public class MeshData {
	List<Color> m_ColorVertices = new List<Color>();
	public List<Color> ColorVertices {
		get {
			return m_ColorVertices;
		}
	}

	List<Vector3> m_Vertices = new List<Vector3>();
	public List<Vector3> Vertices {
		get {
			return m_Vertices;
		}
	}
	List<int> m_Triangles = new List<int>();
	public List<int> Triangles {
		get {
			return m_Triangles;
		}
	}
	public List<Vector2> UV = new List<Vector2>();

	List<Vector3> m_ColliderVertices = new List<Vector3>();
	public List<Vector3> ColliderVertices {
		get {
			return m_ColliderVertices;
		}
	}
	List<int> m_ColliderTriangles = new List<int>();
	public List<int> ColliderTriangles {
		get {
			return m_ColliderTriangles;
		}
	}

	public bool UseRenderDataForCollision = true;

	public MeshData() {
	}

	public void AddQuadTriangles() {
		m_Triangles.Add(Vertices.Count - 4);
		m_Triangles.Add(Vertices.Count - 3);
		m_Triangles.Add(Vertices.Count - 2);

		m_Triangles.Add(Vertices.Count - 4);
		m_Triangles.Add(Vertices.Count - 2);
		m_Triangles.Add(Vertices.Count - 1);

		if (UseRenderDataForCollision) {
			ColliderTriangles.Add(m_ColliderVertices.Count - 4);
			ColliderTriangles.Add(m_ColliderVertices.Count - 3);
			ColliderTriangles.Add(m_ColliderVertices.Count - 2);
			ColliderTriangles.Add(m_ColliderVertices.Count - 4);
			ColliderTriangles.Add(m_ColliderVertices.Count - 2);
			ColliderTriangles.Add(m_ColliderVertices.Count - 1);
		}
	}

	public const float VertexScale = 0.03125f;

	public void AddVertex(Vector3 vertex, Color color) {
		m_Vertices.Add(vertex);
		m_ColorVertices.Add(color);

		if (UseRenderDataForCollision) {
			m_ColliderVertices.Add(vertex);
		}
	}

	public void AddTriangle(int tri) {
		m_Triangles.Add(tri);

		if (UseRenderDataForCollision) {
			ColliderTriangles.Add(tri - (m_Vertices.Count - m_ColliderVertices.Count));
		}
	}
}