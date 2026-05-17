using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{

	public static EventManager Instance { get; private set; }

	[Header("Players")]
	public PlayerMovement jugador1;
	public PlayerMovement jugador2;

	[Header("Victory")]
	public Door_Controller Puerta_Ganadora;
	public string nombreCampeona = string.Empty;
	public bool partidaTerminada = false;

	[Header("Key Found")]
	public bool llaveDoradaEncontrada = false;
	public float tiempoMostrarLlaveEncontrada = 3f;

	private Coroutine llaveEncontradaRoutine;

	[Header("Debug")]
	public bool debugLogs = true;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start()
	{
		RefreshPlayersFromScene();
		SelectWinningDoor();
	}

	public void RegisterPlayer(int playerIndex, PlayerMovement player)
	{
		if (playerIndex <= 0)
			jugador1 = player;
		else
			jugador2 = player;
	}

	public void RefreshPlayersFromScene()
	{
		if (jugador1 != null && jugador2 != null)
			return;

		PlayerMovement[] players = FindObjectsOfType<PlayerMovement>(true);
		if (players == null || players.Length == 0)
			return;

		if (jugador1 == null)
			jugador1 = players.Length > 0 ? players[0] : null;

		if (jugador2 == null)
			jugador2 = players.Length > 1 ? players[1] : null;
	}

	public void SelectWinningDoor()
	{
		Door_Controller[] doors = FindObjectsOfType<Door_Controller>(true);
		if (doors == null || doors.Length == 0)
			return;

		Door_Controller[] goldenDoors = System.Array.FindAll(doors, door => door != null && door.Tipo_de_puerta == 2);
		Door_Controller[] candidates = goldenDoors != null && goldenDoors.Length > 0 ? goldenDoors : doors;

		for (int i = 0; i < doors.Length; i++)
		{
			if (doors[i] != null)
				doors[i].Puerta_Ganadora = false;
		}

		int randomIndex = Random.Range(0, candidates.Length);
		Puerta_Ganadora = candidates[randomIndex];

		if (Puerta_Ganadora != null)
		{
			Puerta_Ganadora.Puerta_Ganadora = true;

			if (debugLogs)
				Debug.Log("EventManager: puerta ganadora seleccionada -> " + Puerta_Ganadora.name);
		}
	}

	public void NotifyGoldenDoorOpened(Door_Controller door, PlayerMovement player)
	{
		if (partidaTerminada || door == null || player == null)
			return;

		if (door != Puerta_Ganadora)
			return;

		partidaTerminada = true;
		nombreCampeona = player.gameObject.name;

		if (debugLogs)
			Debug.Log("EventManager: victoria de " + nombreCampeona + " al abrir " + door.name);
	}

	public void NotifyGoldenKeyFound(PlayerMovement player)
	{
		if (player == null || llaveDoradaEncontrada)
			return;

		llaveDoradaEncontrada = true;

		ShowKeyFoundOverlayForPlayer(jugador1);
		ShowKeyFoundOverlayForPlayer(jugador2);

		if (llaveEncontradaRoutine != null)
		{
			StopCoroutine(llaveEncontradaRoutine);
		}

		llaveEncontradaRoutine = StartCoroutine(HideKeyFoundOverlayAfterDelay());

		if (debugLogs)
			Debug.Log("EventManager: llave dorada encontrada por " + player.gameObject.name);
	}

	private void ShowKeyFoundOverlayForPlayer(PlayerMovement player)
	{
		ShowKeyFoundOverlayForPlayer(player, true);
	}

	private IEnumerator HideKeyFoundOverlayAfterDelay()
	{
		float delay = Mathf.Max(0f, tiempoMostrarLlaveEncontrada);
		yield return new WaitForSeconds(delay);

		SetKeyFoundOverlayVisible(false);
		llaveEncontradaRoutine = null;
	}

	private void SetKeyFoundOverlayVisible(bool visible)
	{
		ShowKeyFoundOverlayForPlayer(jugador1, visible);
		ShowKeyFoundOverlayForPlayer(jugador2, visible);
	}

	private void ShowKeyFoundOverlayForPlayer(PlayerMovement player, bool visible)
	{
		if (player == null)
			return;

		MultijugadorPlayerHUD hud = player.GetComponent<MultijugadorPlayerHUD>();
		if (hud == null)
			return;

		hud.SetLlaveEncontradaVisible(visible);
	}

}
