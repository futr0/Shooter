using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private NetworkManagerShooter networkManager = null;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;

	private void OnEnable()
	{
        NetworkManagerShooter.OnClientConnected += HandleClientConnected;
		NetworkManagerShooter.OnClientDisconnected += HandleClientDisconnected;
	}

	private void OnDisable()
	{
		NetworkManagerShooter.OnClientConnected -= HandleClientConnected;
		NetworkManagerShooter.OnClientDisconnected -= HandleClientDisconnected;
	}

	public void JoinLobby()
	{
		string ipAddress = ipAddressInputField.text;
		networkManager.networkAddress = ipAddress;
		networkManager.StartClient();

		joinButton.interactable = false;

	}

	private void HandleClientConnected()
	{
		joinButton.interactable = true;

		gameObject.SetActive(false);
		landingPagePanel.SetActive(false);
	}

	private void HandleClientDisconnected()
	{
		joinButton.interactable = true;
	}
}
