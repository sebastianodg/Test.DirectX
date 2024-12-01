using Proton.Graphics.Types;

namespace Proton.Graphics.Abstractions;

/// <summary>
/// Interfaccia che definisce il comportamento degli oggetti che gestiscono il controllo destinatario dell'output
/// </summary>
internal interface IPtnOutputControlManager : IDisposable
{
	/// <summary>
	/// Effettua l'inizializzazione del controllo destinatario dell'output
	/// </summary>
	void Init(PtnGraphicEnvironmentOptions gfxEnvOptions);

	/// <summary>
	/// Gestisce il ridimensionamento del controllo destinatario dell'output
	/// </summary>
	void ManageResize();
}
