using Proton.Graphics.Types;

namespace Proton.Graphics.Base.Abstractions;

/// <summary>
/// Interfaccia che definisce il comportamento degli oggetti che gestiscono il controllo destinatario dell'output
/// </summary>
public interface IProOutputControlManager : IDisposable
{
	/// <summary>
	/// Effettua l'inizializzazione del controllo destinatario dell'output
	/// </summary>
	void Init(ProGraphicEnvironmentOptions gfxEnvOptions);

	/// <summary>
	/// Gestisce il ridimensionamento del controllo destinatario dell'output
	/// </summary>
	void ManageResize();
}
