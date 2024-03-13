using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBehaviour : MonoBehaviour
{
	[SerializeField] private BoardView m_Game;
	[SerializeField] private GameSettingsMenuBehaviour m_ConfigurationPanel;

	private void Start()
	{
		ShowMainMenu();
	}

	public void ShowMainMenu()
	{
		m_Game.Clear();
		gameObject.SetActive(true);
	}

	public void StartPlayerVsMachine()
	{
		m_Game.SetSinglePlayer();
		ConfigureGame();
	}

	public void StartPlayerVsPlayer()
	{
		m_Game.SetDuel();
		ConfigureGame();
	}

	private void ConfigureGame()
	{
		gameObject.SetActive(false);
		m_ConfigurationPanel.gameObject.SetActive(true);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
