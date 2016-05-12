using System;
using System.Collections.Generic;
//using System.Drawing;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Windows;

using D3D11 = SharpDX.Direct3D11;

namespace Voxify {
	public class Editor : IDisposable {
		private RenderForm renderForm;

		private int formWidth = 1280;
		private int formHeight = 720;

		private Color backgroundColor = Color.CornflowerBlue;//new Color(204, 204, 216);

		private Viewport viewport;

		private D3D11.VertexShader vertexShader;
		private D3D11.PixelShader pixelShader;

		private ShaderSignature inputSignature;
		private D3D11.InputLayout inputLayout;
		private D3D11.InputElement[] inputElements = new D3D11.InputElement[]
		{
			new D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, D3D11.InputClassification.PerVertexData, 0),
			new D3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
		};

		private ModeDescription backBufferDescription;
		private SwapChainDescription swapChainDescription;

		private D3D11.Device device;
		private D3D11.DeviceContext deviceContext;
		private SwapChain swapChain;

		private D3D11.RenderTargetView renderTargetView;

		private List<Entity> entities = new List<Entity>();

		private D3D11.Buffer vertexBuffer;

		private int vertexCount = 0;

		private string editorVersion {
			get {
				return System.Reflection.Assembly.GetExecutingAssembly()
					.GetName()
					.Version
					.ToString();
			}
		}

		private void RenderCallback() {
			Update();

			Draw();
		}

		private void Update() {

			#region Update Index and Vertex Buffers

			//HashSet<Vector3> verts = new HashSet<Vector3>();
			//HashSet<int> tris = new HashSet<int>();

			HashSet<Vertex> verts = new HashSet<Vertex>();
			
			if (entities.Count > 0) {

				for (int i = 0; i < entities.Count; i++) {
					NonIndexMesh entMesh = entities[i].mesh;

					// we add the previous size of the vert array to ensure the indeces match.
					//foreach (int tri in entMesh.triangles) tris.Add(tri + verts.Count);

					foreach (Vertex vert in entities[i].vertices) verts.Add(vert);
				}

				Vertex[] vmap = new Vertex[verts.Count];
				verts.CopyTo(vmap);

				Console.WriteLine("vmap length: {0}", vmap.Length);
				for (int i = 0; i < vmap.Length; i++) Console.WriteLine("vmap[{0}] = {1}", i, vmap[i].ToString());

				vertexBuffer = Util.CreateBuffer(device, D3D11.BindFlags.VertexBuffer, vmap);

				//int[] tmap = new int[tris.Count];
				//tris.CopyTo(tmap);
				//indexBuffer = D3D11.Buffer.Create(device, D3D11.BindFlags.IndexBuffer, tmap);

				//indexCount = tris.Count;
				vertexCount = verts.Count;
			}
			else {
				vertexBuffer = null;
			}

			#endregion
		}

		private void Draw() {
			deviceContext.ClearRenderTargetView(renderTargetView, backgroundColor);

			// Set vertex and index buffers, then draw on the device
			if (vertexBuffer != null) {

				deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0));
				//deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

				//deviceContext.DrawIndexed(indexCount, 0, 0);
				deviceContext.Draw(vertexCount, 0);
			}

			swapChain.Present(1, PresentFlags.None);
		}

		public void Run() {
			RenderLoop.Run(renderForm, RenderCallback);
		}

		private void InitializeDeviceResources() {
			backBufferDescription = new ModeDescription(formWidth, formHeight, new Rational(60, 1), Format.R8G8B8A8_UNorm);

			swapChainDescription = new SwapChainDescription() {
				ModeDescription = backBufferDescription,
				SampleDescription = new SampleDescription(1, 0),
				Usage = Usage.RenderTargetOutput,
				BufferCount = 1,
				OutputHandle = renderForm.Handle,
				IsWindowed = true
			};

			D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None, swapChainDescription, out device, out swapChain);
			deviceContext = device.ImmediateContext;

			viewport = new Viewport(0, 0, formWidth, formHeight);
			deviceContext.Rasterizer.SetViewport(viewport);

			using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0)) {
				renderTargetView = new D3D11.RenderTargetView(device, backBuffer);
			}

			deviceContext.OutputMerger.SetRenderTargets(renderTargetView);
		}

		private void InitializeShaders() {
			using (CompilationResult vertexShaderByteCode = ShaderBytecode.CompileFromFile("shader.hlsl", "vsMain", "vs_4_0", ShaderFlags.Debug)) {
				inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

				vertexShader = new D3D11.VertexShader(device, vertexShaderByteCode);
			}

			using (CompilationResult pixelShaderByteCode = ShaderBytecode.CompileFromFile("shader.hlsl", "psMain", "ps_4_0", ShaderFlags.Debug)) {
				pixelShader = new D3D11.PixelShader(device, pixelShaderByteCode);
			}

			deviceContext.VertexShader.Set(vertexShader);
			deviceContext.PixelShader.Set(pixelShader);

			deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			inputLayout = new D3D11.InputLayout(device, inputSignature, inputElements);
			deviceContext.InputAssembler.InputLayout = inputLayout;
		}

		public void AddEntity(Entity entity) {
			if (!entities.Contains(entity)) {

				Console.WriteLine("Adding entity");
				entities.Add(entity);
			}
		}

		public D3D11.Device _GetDevice() => device;

		public void Dispose() {
			entities.Clear();
			inputLayout.Dispose();
			inputSignature.Dispose();
			vertexShader.Dispose();
			pixelShader.Dispose();
			renderTargetView.Dispose();
			swapChain.Dispose();
			device.Dispose();
			deviceContext.Dispose();
			renderForm.Dispose();
		}

		public static D3D11.Device GetDevice() => instance.device;

		#region construction

		private static Editor _instance = null;

		public static Editor instance {
			get {
				if (_instance == null) {
					_instance = new Editor();
				}
				return _instance;
			}
		}

		public Editor(int width = 1280, int height = 720) {

			formWidth = width;
			formHeight = height;

			renderForm = new RenderForm(string.Format("Voxify Editor - v{0}", editorVersion));
			//renderForm = new RenderForm(string.Format("{0}", editorVersion));
			renderForm.ClientSize = new System.Drawing.Size(formWidth, formHeight);
			renderForm.AllowUserResizing = true;
			renderForm.Icon = new System.Drawing.Icon("icon.ico");

			InitializeDeviceResources();
			InitializeShaders();

			AddEntity(new Entity() {
				mesh = new NonIndexMesh() {
					vertices = new List<Vertex>() {
						new Vertex(new Vector3(-0.5f, 0.5f, 0.0f), Color.Red),
						new Vertex(new Vector3(0.5f, 0.5f, 0.0f), Color.White),
						new Vertex(new Vector3(0.0f, -0.5f, 0.0f), Color.Blue)
					}
				}
			});
		}

		public Editor(Color bgColor, int width = 1280, int height = 720) : this(width, height) {
			backgroundColor = bgColor;
		}

		#endregion
	}
}
