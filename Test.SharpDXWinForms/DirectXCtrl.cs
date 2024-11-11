using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;

namespace Test.SharpDXWinForms;

public partial class DirectXCtrl : UserControl
{
	private readonly Int32 _msBasicRenderDriverId = 0x8C;

	private Factory? _dxgiFactory;
	private Adapter? _adapter;
	private SharpDX.Direct3D11.Device? _device;
	private DeviceContext? _deviceContext;
	private SwapChain? _swapChain;
	private RenderTargetView? _backBufferRenderTargetView;
	private DepthStencilView? _depthStencilBufferView;

	public DirectXCtrl()
	{
		this.InitializeComponent();

		// Inizializzazione della classe
		this._dxgiFactory = null;
		this._adapter = null;
		this._device = null;
		this._deviceContext = null;
		this._swapChain = null;
		this._backBufferRenderTargetView = null;
		this._depthStencilBufferView = null;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		// Le operazioni vanno effettuate solo a runtime
		if (this.DesignMode)
			return;

		// Inizializzazione dell'oggetto Factory che permette di leggere le informazioni sul sistema grafico
		this._dxgiFactory = new Factory1();
		if (this._dxgiFactory == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to get DXGI.Factory1 object.");

		// Scelta dell'adapter da utilizzare
		foreach (Adapter adapter in this._dxgiFactory.Adapters)
		{
			// Se l'identificativo dell'adapter corrisponde al "Microsoft Basic Render Driver" (adapter software di Microsoft), passo al prossimo adapter
			if (adapter.Description.DeviceId == this._msBasicRenderDriverId)
				continue;

			// Viene scelto semplicemente il primo adapter che non sia l'implementazione software di Microsoft
			this._adapter = adapter;
			break;
		}
		if (this._adapter == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to find a suitable Direct3D11 hardware accelerated adapter.");

		// Controllo che l'adapter supporti Direct3D11
		FeatureLevel featureLevel = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(this._adapter);
		if (featureLevel != FeatureLevel.Level_11_0)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Adapter does not support feature level 11.");

		// Stampa delle informazioni sull'adattatore grafico trovato
		Trace.WriteLine($"{this._adapter.Description.Description} ({this._adapter.Description.DeviceId})");
		Trace.Indent();
		Trace.WriteLine($"DedicatedSystemMemory: {this._adapter.Description.DedicatedSystemMemory}");
		Trace.WriteLine($"DedicatedVideoMemory:  {this._adapter.Description.DedicatedVideoMemory}");
		Trace.WriteLine($"FeatureLevel:          {SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(this._adapter)}");
		Trace.WriteLine("Outputs");
		Trace.Indent();
		foreach (Output output in this._adapter.Outputs)
			Trace.WriteLine($"DeviceName: {output.Description.DeviceName}");
		if (this._adapter.Outputs.Length == 0)
			Trace.WriteLine("-");
		Trace.Unindent();
		Trace.Unindent();

		// Definizione della swap chain
		SwapChainDescription swapChainDescription = new SwapChainDescription()
		{
			ModeDescription = new ModeDescription(Format.B8G8R8A8_UNorm),
			SampleDescription = new SampleDescription(8, 0),
			Usage = Usage.RenderTargetOutput,
			BufferCount = 2,
			OutputHandle = this.Handle,
			IsWindowed = new RawBool(true),
			SwapEffect = SwapEffect.Discard,
			Flags = SwapChainFlags.None,
		};

		// Creazione del device e della swap chain
		SharpDX.Direct3D11.Device.CreateWithSwapChain(this._adapter, DeviceCreationFlags.Debug, swapChainDescription, out this._device, out this._swapChain);
		if (this._device == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to create Direct3D11 device.");
		if (this._swapChain == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to create Direct3D11 swap chain.");

		// Creazione del device context
		this._deviceContext = this._device.ImmediateContext;
		if (this._deviceContext == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to create Direct3D11 device context.");

		// Creazione della vista sulla destinazione del rendering (il back buffer)
		// Viene richiesta la texture 2D rappresentante il back buffer e questa viene utilizzata per creare la vista
		// Dopo aver creato la vista, la texture 2D non è più necessaria e viene rilasciata
		Texture2D backBufferTexture = this._swapChain.GetBackBuffer<Texture2D>(0);
		this._backBufferRenderTargetView = new RenderTargetView(this._device, backBufferTexture);
		backBufferTexture.Dispose();
		if (this._backBufferRenderTargetView == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to create Direct3D11 render target view for rendering back buffer.");

		// Creazione della vista sul buffer depth/stencil
		// Viene richiesta la texture 2D rappresentante il buffer depth/stencil e questa viene utilizzata per creare la vista
		// Dopo aver creato la vista, la texture 2D non è più necessaria e viene rilasciata
		Texture2D depthStencilBufferTexture = new Texture2D(this._device, new Texture2DDescription()
		{
			Format = Format.D16_UNorm,
			ArraySize = 1,
			MipLevels = 1,
			Width = this.ClientSize.Width,
			Height = this.ClientSize.Height,
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.DepthStencil,
			CpuAccessFlags = CpuAccessFlags.None,
			OptionFlags = ResourceOptionFlags.None
		});
		this._depthStencilBufferView = new DepthStencilView(this._device, depthStencilBufferTexture);
		depthStencilBufferTexture.Dispose();
		if (this._depthStencilBufferView == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnHandleCreated)}: Unable to create Direct3D11 depth/stencil view for depth/stencil buffer.");
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
		if (this._swapChain == null)
			return;

		// Rilascio delle risorse dipendenti dalla risoluzione e precedentemente allocate
		this._backBufferRenderTargetView?.Dispose();

		// Ridimensionamento dei buffer della swap chain
		this._swapChain?.ResizeBuffers(2, this.ClientSize.Width, this.ClientSize.Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

		// Creazione della vista sulla destinazione del rendering (il back buffer)
		// Viene richiesta la texture 2D rappresentante il back buffer e questa viene utilizzata per creare la vista
		// Dopo aver creato la vista, la texture 2D non è più necessaria e viene rilasciata
		Texture2D backBufferTexture = this._swapChain!.GetBackBuffer<Texture2D>(0);
		this._backBufferRenderTargetView = new RenderTargetView(this._device, backBufferTexture);
		backBufferTexture.Dispose();
		if (this._backBufferRenderTargetView == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnResize)}: Unable to create Direct3D11 render target view for rendering back buffer.");

		// Creazione della vista sul buffer depth/stencil
		// Viene richiesta la texture 2D rappresentante il buffer depth/stencil e questa viene utilizzata per creare la vista
		// Dopo aver creato la vista, la texture 2D non è più necessaria e viene rilasciata
		Texture2D depthStencilBufferTexture = new Texture2D(this._device, new Texture2DDescription()
		{
			Format = Format.D16_UNorm,
			ArraySize = 1,
			MipLevels = 1,
			Width = this.ClientSize.Width,
			Height = this.ClientSize.Height,
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.DepthStencil,
			CpuAccessFlags = CpuAccessFlags.None,
			OptionFlags = ResourceOptionFlags.None
		});
		this._depthStencilBufferView = new DepthStencilView(this._device, depthStencilBufferTexture);
		depthStencilBufferTexture.Dispose();
		if (this._depthStencilBufferView == null)
			throw new Exception($"{nameof(DirectXCtrl)}.{nameof(OnResize)}: Unable to create Direct3D11 depth/stencil view for depth/stencil buffer.");
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		base.OnHandleDestroyed(e);

		// Rilascio degli oggetti COM istanziati
		this._depthStencilBufferView?.Dispose();
		this._backBufferRenderTargetView?.Dispose();
		this._swapChain?.Dispose();
		this._deviceContext?.Dispose();
		this._device?.Dispose();
		this._adapter?.Dispose();
		this._dxgiFactory?.Dispose();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		// Se in design mode, viene chiamata solo la versione della classe base
		if (this.DesignMode)
		{
			base.OnPaint(e);
			return;
		}

		// Reset dei buffer
		this._deviceContext?.ClearRenderTargetView(this._backBufferRenderTargetView, new RawColor4(0.1f, 0.1f, 0.1f, 1.0f));
		this._deviceContext?.ClearDepthStencilView(this._depthStencilBufferView, DepthStencilClearFlags.Depth, 1.0f, 0);

		// Rendering del fotogramma

		// Visualizzazione del fotogramma renderizzato
		this._swapChain?.Present(0, PresentFlags.None);
	}
}
