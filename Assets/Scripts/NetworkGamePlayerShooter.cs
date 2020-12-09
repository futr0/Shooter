using Mirror;

public class NetworkGamePlayerShooter : NetworkBehaviour
{
	[SyncVar]
	private string displayName = "Loading...";

	private NetworkManagerShooter room;

	private NetworkManagerShooter Room
	{
		get
		{
			if (room != null) { return room; }
			return room = NetworkManager.singleton as NetworkManagerShooter;
		}
	}

	public override void OnStartClient()
	{
		DontDestroyOnLoad(gameObject);
		Room.GamePlayers.Add(this);
	}

	public override void OnStopClient()
	{
		Room.GamePlayers.Remove(this);
	}

	[Server]
	public void SetDisplayName(string displayName)
	{
		this.displayName = displayName;
	}
}
