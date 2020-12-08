﻿ using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private NetworkManagerShooter networkManager = null;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;

    public void HostLobby()
	{
        networkManager.StartHost();
        landingPagePanel.SetActive(false);
	}
}
