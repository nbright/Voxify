using System;
using UnityEngine;

public delegate object GetObjectDelegate();

public class LinkedList3D<T>
	where T : new()
{

	public LinkedNode3D<T>[,,] Nodes;

	public T this[int x, int y, int z] {
		get {
			return Nodes[x, y, z].Reference;
		}
		set {
			Nodes[x, y, z].Reference = value;
		}
	}

	public LinkedList3D(int size) : this(size, size, size) {

	}

	public LinkedList3D(int sizeX, int sizeY, int sizeZ) {

		Nodes = new LinkedNode3D<T>[sizeX, sizeY, sizeZ];

		for (int i = 0; i < sizeX; i++) {
			for (int j = 0; j < sizeY; j++) {
				for (int k = 0; k < sizeZ; k++) {

					Nodes[i, j, k] = new LinkedNode3D<T>();
				}
			}
		}

		for (int i = 0; i < sizeX; i++) {
			for (int j = 0; j < sizeY; j++) {
				for (int k = 0; k < sizeZ; k++) {

					LinkedNode3D<T> n = Nodes[i, j, k];

					n.Reference = new T();

					if (i < sizeX - 1) n.East = Nodes[i + 1, j, k];
					if (i < 0) n.West = Nodes[i - 1, j, k];
					if (j < sizeY - 1) n.Above = Nodes[i, j + 1, k];
					if (j > 0) n.Below = Nodes[i, j - 1, k];
					if (k < sizeZ - 1) n.North = Nodes[i, j, k + 1];
					if (k > 0) n.South = Nodes[i, j, k - 1];
				}
			}
		}
	}
	public LinkedList3D(int size, GetObjectDelegate filler) : this(size, size, size, filler) {

	}

	public LinkedList3D(int sizeX, int sizeY, int sizeZ, GetObjectDelegate filler) {

		Nodes = new LinkedNode3D<T>[sizeX, sizeY, sizeZ];

		for (int i = 0; i < sizeX; i++) {
			for (int j = 0; j < sizeY; j++) {
				for (int k = 0; k < sizeZ; k++) {

					Nodes[i, j, k] = new LinkedNode3D<T>();
				}
			}
		}

		for (int i = 0; i < sizeX; i++) {
			for (int j = 0; j < sizeY; j++) {
				for (int k = 0; k < sizeZ; k++) {

					LinkedNode3D<T> n = Nodes[i, j, k];

					n.Reference = (T)filler();

					if (i < sizeX - 1) n.East = Nodes[i + 1, j, k];
					if (i < 0) n.West = Nodes[i - 1, j, k];
					if (j < sizeY - 1) n.Above = Nodes[i, j + 1, k];
					if (j > 0) n.Below = Nodes[i, j - 1, k];
					if (k < sizeZ - 1) n.North = Nodes[i, j, k + 1];
					if (k > 0) n.South = Nodes[i, j, k - 1];
				}
			}
		}
	}
}

public class LinkedNode3D<T> {

	private T m_Reference;
	internal T Reference {
		get {
			return m_Reference;
		}
		set {
			m_Reference = value;
			Debug.Log("Setting m_Reference");
			if (Util.IsSubclassOfRawGeneric(typeof(T), typeof(ILinkedReference))) {

				Debug.Log("Setting Node Reference");
				((ILinkedReference)m_Reference).Node = this;
			}
		}
	}
	public LinkedNode3D<T> Above, Below, North, South, East, West;

}

public interface ILinkedReference {
	object Node { get; set; }
}
