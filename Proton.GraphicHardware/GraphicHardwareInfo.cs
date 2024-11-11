using SharpDX.DXGI;
using SharpDX.Direct3D11;
using System.Diagnostics;
using SharpDX.Mathematics.Interop;

namespace Proton.GraphicHardware;

/// <summary>
/// Classe che si occupa di ottenere le informazioni sull'hardware grafico
/// </summary>
public class GraphicHardwareInfo : IDisposable
{
	private Factory _dxgiFactory;

	public GraphicHardwareInfo()
	{
		// Inizializzazione dell'oggetto Factory che permette di leggere le informazioni sul sistema grafico
		this._dxgiFactory = new Factory1();
		if (this._dxgiFactory == null)
			throw new Exception($"{nameof(GraphicHardwareInfo)}.{nameof(GraphicHardwareInfo)}: Unable to get DXGI.Factory1 object.");

		// Enumerazione degli adattaori grafici disponibili nel sistema
		foreach (Adapter adapter in this._dxgiFactory.Adapters)
		{
			// Stampa delle informazioni sull'adattatore grafico
			Trace.WriteLine($"{adapter.Description.Description} ({adapter.Description.DeviceId})");
			Trace.Indent();
			Trace.WriteLine($"DedicatedSystemMemory: {adapter.Description.DedicatedSystemMemory}");
			Trace.WriteLine($"DedicatedVideoMemory:  {adapter.Description.DedicatedVideoMemory}");
			Trace.WriteLine($"FeatureLevel:          {SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(adapter)}");
			Trace.WriteLine("Outputs");
			Trace.Indent();
			foreach (Output output in adapter.Outputs)
				Trace.WriteLine($"DeviceName: {output.Description.DeviceName}");
			if (adapter.Outputs.Length == 0)
				Trace.WriteLine("-");
			Trace.Unindent();
			Trace.Unindent();

			SwapChainDescription swapChainDescription = new SwapChainDescription()
			{
				ModeDescription = new ModeDescription(Format.B8G8R8A8_UNorm),
				//SampleDescription = new SampleDescription(8, 0),
				Usage = Usage.RenderTargetOutput,
				BufferCount = 2,
				OutputHandle = (nint)IntPtr.Zero,
				IsWindowed = new RawBool(true),
				SwapEffect = SwapEffect.Discard,
				Flags = SwapChainFlags.None,
			};
		}
	}

	public void Dispose()
	{
		this._dxgiFactory.Dispose();
	}
}
