using SharpGen.Runtime;
using System.Diagnostics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Test.VorticeWinForms;

public partial class DirectXCtrl : UserControl
{
	private readonly Int32 _msBasicRenderDriverId = 0x8C;

	private IDXGIFactory1? _dxgiFactory1;
	private IDXGIAdapter1? _dxgiAdapter1;
	private ID3D11Device? _d3d11Device;
	private ID3D11DeviceContext? _d3d11DeviceContext;
	private IDXGISwapChain? _d3d11SwapChain;
	private ID3D11Texture2D? _d3d11BackBufferTexture;
	private ID3D11RenderTargetView? _d3d11RenderTargetView;
	private ID3D11Texture2D? _d3d11DepthStencilTexture;
	private ID3D11DepthStencilView? _d3d11DepthStencilView;

	public DirectXCtrl()
	{
		this.InitializeComponent();

		this._dxgiFactory1 = null;
		this._dxgiAdapter1 = null;
		this._d3d11Device = null;
		this._d3d11DeviceContext = null;
		this._d3d11SwapChain = null;
		this._d3d11BackBufferTexture = null;
		this._d3d11RenderTargetView = null;
		this._d3d11DepthStencilTexture = null;
		this._d3d11DepthStencilView = null;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		// Le operazioni vanno effettuate solo a runtime
		if (this.DesignMode)
			return;

		// Inizializzazione dell'oggetto Factory che permette di leggere le informazioni sul sistema grafico
		Result createFactoryResult = DXGI.CreateDXGIFactory1<IDXGIFactory1>(out this._dxgiFactory1);
		if (!createFactoryResult.Success || this._dxgiFactory1 == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to get IDXGIFactory1 interface.");

		// Enumerazione degli adattatori grafici
		IDXGIAdapter1? tempAdapter1 = null;
		UInt32 adapterIndex = 0;
		Result enumAdapterResult;
		do
		{
			// Richiesta dell'interfaccia rappresentante il graphic adapter corrente
			enumAdapterResult = this._dxgiFactory1.EnumAdapters1(adapterIndex, out tempAdapter1);
			if (!enumAdapterResult.Success || tempAdapter1 == null)
				break;

			// Visualizzazione delle informazioni sul graphic adapter corrente
			Trace.WriteLine($"{tempAdapter1.Description.Description} ({tempAdapter1.Description.DeviceId})");

			// Lettura delle modalità di visualizzazione disponibili per il graphic adapter, per il pixel format necessario
			IDXGIOutput adapterOutput;
			Result enumOutputsResult = tempAdapter1.EnumOutputs(0, out adapterOutput);
			ModeDescription[] modeDesriptions = adapterOutput.GetDisplayModeList(Format.B8G8R8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);
			foreach (ModeDescription item in modeDesriptions)
				Trace.WriteLine($"  {item.Width} x {item.Height} @ {item.RefreshRate.Numerator / item.RefreshRate.Denominator}Hz");
			adapterOutput.Release();

			// Salvataggio del primo graphic adapter che non sia quello standard / software di Microsoft
			if (tempAdapter1.Description.DeviceId != this._msBasicRenderDriverId)
			{
				this._dxgiAdapter1 = tempAdapter1;
				break;
			}

			// Prossimo graphic adapter
			adapterIndex++;
			tempAdapter1.Dispose();
		} while (enumAdapterResult.Success);
		if (this._dxgiAdapter1 == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to get IDXGIAdapter1 interface for graphic adapter.");

		// Definizione della swap chain
		SwapChainDescription swapChainDescription = new SwapChainDescription()
		{
			BufferDescription = new ModeDescription((UInt32)this.ClientSize.Width, (UInt32)this.ClientSize.Height, Format.B8G8R8A8_UNorm),
			SampleDescription = new SampleDescription(8, 0),
			BufferUsage = Usage.RenderTargetOutput,
			BufferCount = 2,
			OutputWindow = this.Handle,
			Windowed = new RawBool(true),
			SwapEffect = SwapEffect.Discard,
			Flags = SwapChainFlags.None,
		};

		FeatureLevel[] featureLevels = new FeatureLevel[]
		{
			FeatureLevel.Level_11_0,
			FeatureLevel.Level_11_1,
		};

		// Creazione del device e della swap chain
		Result createDeviceSwapChainResult = D3D11.D3D11CreateDeviceAndSwapChain
		(
			this._dxgiAdapter1,
			DriverType.Unknown,
			DeviceCreationFlags.Debug,
			featureLevels,
			swapChainDescription,
			out this._d3d11SwapChain,
			out this._d3d11Device,
			out FeatureLevel? ft,
			out this._d3d11DeviceContext
		);
		if (!createDeviceSwapChainResult.Success || this._d3d11SwapChain == null || this._d3d11Device == null || this._d3d11DeviceContext == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to get device and swap chain.");

		// Richiesta della texture rappresentante il back buffer
		Result getBackBufferTextureResult = this._d3d11SwapChain.GetBuffer<ID3D11Texture2D>(0, out this._d3d11BackBufferTexture);
		if (!getBackBufferTextureResult.Success || this._d3d11BackBufferTexture == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to get back buffer texture.");

		// Richiesta della render target view per il back buffer
		this._d3d11RenderTargetView = this._d3d11Device.CreateRenderTargetView(this._d3d11BackBufferTexture);
		if (this._d3d11RenderTargetView == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to get back buffer render target view.");

		// Creazione della texture per il buffer di depth / stencil
		Texture2DDescription depthBufferTextureDescription = new Texture2DDescription()
		{
			Width = (UInt32)this.ClientSize.Width,
			Height = (UInt32)this.ClientSize.Height,
			MipLevels = 1,
			ArraySize = 1,
			Format = Format.D24_UNorm_S8_UInt,
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.DepthStencil,
			CPUAccessFlags = CpuAccessFlags.None,
			MiscFlags = ResourceOptionFlags.None,
		};
		this._d3d11DepthStencilTexture = this._d3d11Device.CreateTexture2D(depthBufferTextureDescription);
		if (this._d3d11DepthStencilTexture == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to get depth / stencil buffer texture.");

		// Richiesta della render target view per il buffer di depth / stencil
		DepthStencilViewDescription depthStencilViewDescription = new DepthStencilViewDescription()
		{
			Format = Format.D24_UNorm_S8_UInt,
			ViewDimension = DepthStencilViewDimension.Texture2D,
			Texture2DMS = new Texture2DMultisampledDepthStencilView(),
		};
		this._d3d11DepthStencilView = this._d3d11Device.CreateDepthStencilView(this._d3d11DepthStencilTexture, depthStencilViewDescription);
		if (this._d3d11DepthStencilView == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to get depth / stencil buffer render target view.");
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);

		// Le altre operazioni vanno effettuate solo a runtime
		if (this.DesignMode)
			return;

		// Se la finestra è minimizzata, non ho altro da fare
		if (this.ClientSize.Width == 0 || this.ClientSize.Height == 0)
			return;

		// Se la swap chain non è stata creata, non ho altro da fare
		if (this._d3d11SwapChain == null)
			return;

		// Rilascio delle risorse dipendenti dalla risoluzione e precedentemente allocate
		this._d3d11BackBufferTexture?.Dispose();
		this._d3d11RenderTargetView?.Dispose();

		// Ridimensionamento dei buffer della swap chain
		this._d3d11SwapChain?.ResizeBuffers(2, (UInt32)this.ClientSize.Width, (UInt32)this.ClientSize.Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

		// Creazione della vista sulla destinazione del rendering (il back buffer)
		// Viene richiesta la texture 2D rappresentante il back buffer e questa viene utilizzata per creare la vista
		Result getBackBufferTextureResult = this._d3d11SwapChain!.GetBuffer<ID3D11Texture2D>(0, out this._d3d11BackBufferTexture);
		if (!getBackBufferTextureResult.Success || this._d3d11BackBufferTexture == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnResize)}: Unable to get back buffer texture.");

		// Richiesta della render target view
		this._d3d11RenderTargetView = this._d3d11Device!.CreateRenderTargetView(this._d3d11BackBufferTexture);
		if (this._d3d11RenderTargetView == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnResize)}: Unable to get back buffer render target view.");

		// Creazione della texture per il buffer di depth / stencil
		Texture2DDescription depthBufferTextureDescription = new Texture2DDescription()
		{
			Width = (UInt32)this.ClientSize.Width,
			Height = (UInt32)this.ClientSize.Height,
			MipLevels = 1,
			ArraySize = 1,
			Format = Format.D24_UNorm_S8_UInt,
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.DepthStencil,
			CPUAccessFlags = CpuAccessFlags.None,
			MiscFlags = ResourceOptionFlags.None,
		};
		this._d3d11DepthStencilTexture = this._d3d11Device.CreateTexture2D(depthBufferTextureDescription);
		if (this._d3d11DepthStencilTexture == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnResize)}: Unable to get depth / stencil buffer texture.");

		// Richiesta della render target view per il buffer di depth / stencil
		DepthStencilViewDescription depthStencilViewDescription = new DepthStencilViewDescription()
		{
			Format = Format.D24_UNorm_S8_UInt,
			ViewDimension = DepthStencilViewDimension.Texture2D,
			Texture2DMS = new Texture2DMultisampledDepthStencilView(),
		};
		this._d3d11DepthStencilView = this._d3d11Device.CreateDepthStencilView(this._d3d11DepthStencilTexture, depthStencilViewDescription);
		if (this._d3d11DepthStencilView == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnResize)}: Unable to get depth / stencil buffer render target view.");
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		// Le operazioni vanno effettuate solo a runtime
		if (this.DesignMode)
			return;

		if (this._d3d11DeviceContext == null || this._d3d11RenderTargetView == null || this._d3d11DepthStencilView == null || this._d3d11SwapChain == null)
			return;
		this._d3d11DeviceContext.ClearRenderTargetView(this._d3d11RenderTargetView, new Color4(1.0f, 0.0f, 0.0f));
		this._d3d11DeviceContext.ClearDepthStencilView(this._d3d11DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
		this._d3d11SwapChain.Present(1, 0);
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		base.OnHandleDestroyed(e);

		this._d3d11DepthStencilView?.Dispose();
		this._d3d11DepthStencilTexture?.Dispose();
		this._d3d11RenderTargetView?.Dispose();
		this._d3d11BackBufferTexture?.Dispose();
		this._d3d11SwapChain?.Dispose();
		this._d3d11DeviceContext?.Dispose();
		this._d3d11Device?.Dispose();
		this._dxgiAdapter1?.Dispose();
		this._dxgiFactory1?.Dispose();
	}
}
