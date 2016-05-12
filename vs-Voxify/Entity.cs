using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;

using D3D11 = SharpDX.Direct3D11;

namespace Voxify {
	public class Entity : IDisposable {

		public Vector3 position = Vector3.Zero;
		public Quaternion rotation = Quaternion.Identity;
		public Vector3 scale = Vector3.One;

		public NonIndexMesh mesh = new NonIndexMesh();

		public Vertex[] vertices {
			get {
				Vertex[] verts = new Vertex[mesh.vertices.Count];

				for (int i = 0; i < verts.Length; i++) verts[i] = new Vertex(mesh.vertices[i].position * scale, mesh.vertices[i].color);

				return verts;
			}
		}
		
		public void Dispose() {
			mesh.Dispose();
		}

		public Entity() {
			
		}
	}
}
