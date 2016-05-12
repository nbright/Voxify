using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Voxify {
	public class NonIndexMesh : IDisposable {
		
		public List<Vertex> vertices = new List<Vertex>();
		
		public void Dispose() {
			vertices.Clear();
			//colors.Clear();
		}

		public NonIndexMesh() {

		}
	}
}
