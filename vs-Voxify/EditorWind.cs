using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace Voxify {
	public partial class EditorWind : Form {

		public SwapChain swapChain;
		public SharpDX.Direct3D11.Device device;
		public DeviceContext deviceContext;
		
		public void InitD3D(IntPtr hwnd) {
			SwapChainDescription scd = new SwapChainDescription();

			scd.BufferCount = 1;
			scd.ModeDescription.Format = Format.R8G8B8A8_UNorm;
			scd.Usage = Usage.RenderTargetOutput;
			scd.OutputHandle = hwnd;
			scd.SampleDescription.Count = 4;
			scd.IsWindowed = true;

			SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Null, DeviceCreationFlags.Debug, scd, out device, out swapChain);
		}

		public void CleanD3D() {
			
		}

		public EditorWind() {
			InitializeComponent();
			
		}
	}
}
