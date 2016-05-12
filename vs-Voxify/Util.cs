using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

using D3D11 = SharpDX.Direct3D11;

namespace Voxify {
	public static class Util {

		public static D3D11.Buffer CreateBuffer<T>(D3D11.Device device, D3D11.BindFlags bindFlags, params T[] items)
			where T : struct {

			var len = Utilities.SizeOf(items);
			var stream = new DataStream(len, true, true);
			foreach (var item in items)
				stream.Write(item);
			stream.Position = 0;
			var buffer = new D3D11.Buffer(device, stream, len, D3D11.ResourceUsage.Default,
				bindFlags, D3D11.CpuAccessFlags.None, D3D11.ResourceOptionFlags.None, 0);
			return buffer;
		}
	}
}
