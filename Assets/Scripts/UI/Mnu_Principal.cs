using UnityEngine;
using UnityEngine.SceneManagement;

public class Mnu_Principal : MonoBehaviour
{
	[Header("Scenes")]
	[SerializeField] private string escenaVideo = "Comic-Inicio";
	[SerializeField] private string escenaMenu = "MenuPrincipal";

	public void Jugar()
	{
		CargarEscena(escenaVideo);
	}

	public void VolverAlMenu()
	{
		CargarEscena(escenaMenu);
	}

	public void Salir()
	{
		Application.Quit();
	}

	private void CargarEscena(string nombreEscena)
	{
		if (string.IsNullOrWhiteSpace(nombreEscena))
			return;

		SceneManager.LoadScene(nombreEscena);
	}

}
