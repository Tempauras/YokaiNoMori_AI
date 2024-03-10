using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsMenuBehaviour : MonoBehaviour
{
	[SerializeField] MainMenuBehaviour m_MainMenu;
	[SerializeField] BoardView m_Game;

	[SerializeField] TMP_InputField m_InitialTimeInput;
	[SerializeField] TMP_InputField m_TimeIncrementInput;
	[SerializeField] TMP_Dropdown m_StartingPlayerOption;
	[SerializeField] Toggle m_EnableAdvancedSettings;
	[SerializeField] Transform m_AdvancedSettingsContainer;
	[SerializeField] TMP_InputField m_CustomPosInput;

	private void Start()
	{
		m_EnableAdvancedSettings.onValueChanged.AddListener((bool _) => _UpdateSettingsDisplay());
	}

	private void _UpdateSettingsDisplay()
	{
		m_AdvancedSettingsContainer.gameObject.SetActive(m_EnableAdvancedSettings.isOn);
		m_StartingPlayerOption.interactable = !m_EnableAdvancedSettings.isOn;
	}

	public void StartGame()
	{
		gameObject.SetActive(false);
		m_Game.SetupTimer(int.Parse(m_InitialTimeInput.text), int.Parse(m_TimeIncrementInput.text));

		if(m_EnableAdvancedSettings.isOn)
		{
			if(m_CustomPosInput.text.Length > 0)
				m_Game.StartGame(m_CustomPosInput.text);
			else
				m_Game.StartGame();
		}
		else
		{
			switch(m_StartingPlayerOption.value)
			{
				case 0:
					m_Game.SetFirstPlayer(true);
					break;
				case 1:
					m_Game.SetFirstPlayer(Random.Range(0, 2) == 0);
					break;
				case 2:
					m_Game.SetFirstPlayer(false);
					break;
				default:
					Debug.LogError("First player option not supported.");
					break;
			}
			m_Game.StartGame();
		}
	}

	public void Cancel()
	{
		gameObject.SetActive(false);
		m_MainMenu.ShowMainMenu();
	}
}
