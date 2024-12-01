using Proton.Graphics.Base.Abstractions;
using Proton.Graphics.Types;

namespace Proton.Graphics.WinForms;

/// <summary>
/// Oggetto che gestisce il controllo destinatario dell'output per WinForms
/// </summary>
internal class ProOutputControlManagerWF : IProOutputControlManager
{
	private Control _outputControl;

	/// <summary>
	/// Costruttore
	/// </summary>
	/// <param name="outputControl">Riferimento al controllo WinForms destinatario dell'output</param>
	public ProOutputControlManagerWF(Control outputControl)
	{
		if (outputControl == null)
			throw new Exception($"{nameof(ProOutputControlManagerWF)}.{nameof(ProOutputControlManagerWF)}: Reference to output control cannot be null.");

		this._outputControl = outputControl;

		// Registrazione agli eventi del controllo
		this._outputControl.HandleCreated += this.OnOutputControlHandleCreated;
		this._outputControl.HandleDestroyed += this.OnOutputControlHandleDestroyed;
		this._outputControl.Resize += this.OnOutputControlResized;
	}

	private void OnOutputControlResized(Object? sender, EventArgs e)
	{
	}

	private void OnOutputControlHandleDestroyed(Object? sender, EventArgs e)
	{
	}

	private void OnOutputControlHandleCreated(Object? sender, EventArgs e)
	{
	}

	public void Init(ProGraphicEnvironmentOptions gfxEnvOptions)
	{
	}

	public void ManageResize()
	{
	}

	public void Dispose()
	{
	}
}
