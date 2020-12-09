using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkRoomPlayerShooter : NetworkBehaviour
{
	[Header("UI")]
	[SerializeField] private GameObject lobbyUI = null;
	[SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
	[SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
	[SerializeField] private Button startGameButton = null;

	[SyncVar(hook = "HandleDisplayNameChanged")]
	public string DisplayName = "Loading...";
	[SyncVar(hook = "HandleReadyStatusChanged")]
	public bool IsReady = false;

	private bool isLeader;

	public bool IsLeader
	{
		set
		{
			isLeader = value;
			startGameButton.gameObject.SetActive(value);
		}
	}

	private NetworkManagerShooter room;

	private NetworkManagerShooter Room
	{
		get
		{
			if (room!= null) { return room; }
			return room = NetworkManager.singleton as NetworkManagerShooter;
		}
	}

	public override void OnStartAuthority()
	{
		CmdSetDisplayName(PlayerNameInput.DisplayName);
		lobbyUI.SetActive(true);
	}

	public override void OnStartClient()
	{
		Room.RoomPlayers.Add(this);
		UpdateDisplay();
	}

	public override void OnStopClient()
	{
		Room.RoomPlayers.Remove(this);
		UpdateDisplay();
		base.OnStopClient();
	}

	public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

	public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();

	private void UpdateDisplay()
	{
		if(!hasAuthority)
		{
			foreach (var player in Room.RoomPlayers)
			{
				if (player.hasAuthority)
				{
					player.UpdateDisplay();
					break;
				}
			}
			return;
		}

		for (int i = 0; i < playerNameTexts.Length; i++)
		{
			playerNameTexts[i].text = "Waiting For Player...";
			playerReadyTexts[i].text = string.Empty;
		}

		for (int i = 0; i < Room.RoomPlayers.Count; i++)
		{
			playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
			playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ?
				"<color=green>Ready</color>" :
				"<color=red>Not Ready</color>";
		}
	}

	public void HandleReadyToStart(bool readyToStart)
	{
		if (!isLeader) { return; }
		startGameButton.interactable = readyToStart;
	}

	[Command]
	private void CmdSetDisplayName(string displayName)
	{
		DisplayName = displayName;
	}

	[Command]
	public void CmdReadyUp()
	{
		IsReady = !IsReady;
		Room.NotifyPlayersOfReadyState();
	}

	[Command]
	public void CmdStartGame()
	{
		if (Room.RoomPlayers[0].connectionToClient != connectionToClient) { return; }

		// Start Game
	}
}
