using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBehaviour : MonoBehaviour
{
	[SerializeField] private BoardView m_Game;

	public void StartPlayerVsMachine()
	{
		m_Game.PlaySingle();
		gameObject.SetActive(false);
	}

	public void StartPlayerVsPlayer()
	{
		m_Game.PlayDuel();
		gameObject.SetActive(false);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
