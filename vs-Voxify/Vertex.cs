using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Voxify {
	public struct Vertex {

		public Vector3 position;
		public Color4 color;

		public Vertex(Vector3 pos, Color col) {
			position = pos;
			color = col;
		}

		public Vertex(Vector3 pos, Color4 col) {
			position = pos;
			color = col;
		}

		public override string ToString() {
			return string.Format("Vert: ({0}, {1}, {2}) Col: ({3}, {4}, {5}, {6})", position.X, position.Y, position.Z, color.Red, color.Green, color.Blue, color.Alpha);
		}
	}
}
