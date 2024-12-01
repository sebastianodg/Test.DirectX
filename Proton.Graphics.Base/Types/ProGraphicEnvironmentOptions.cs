namespace Proton.Graphics.Types;

/// <summary>
/// Classe contenente le opzioni di creazione del dell'ambiente grafico
/// </summary>
public class ProGraphicEnvironmentOptions
{
	/// <summary>
	/// Restituisce o imposta il flag che stabilisce se la modalità di debug è attiva
	/// </summary>
	public Boolean DebugModeOn { get; set; }

	/// <summary>
	/// Restituisce o imposta il numero dei buffer da utilizzare nella swap chain
	/// </summary>
	public Byte SwapChainBuffersCount { get; set; }

	/// <summary>
	/// Restituisce o imposta il numero dei passaggi per la generazione dell'anti aliasing
	/// </summary>
	public Byte AntiAliasingSamples { get; set; }

	/// <summary>
	/// Costruttore
	/// </summary>
	public ProGraphicEnvironmentOptions()
	{
		this.DebugModeOn = true;
		this.SwapChainBuffersCount = 2;
		this.AntiAliasingSamples = 4;
	}
}
