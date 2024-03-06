using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndMenuBehaviour : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI m_Title;
	[SerializeField] private MainMenuBehaviour m_MainMenu;

	public void ShowEndMenu(string iTitle)
	{
		m_Title.text = iTitle;
		gameObject.SetActive(true);
	}

	public void GoToMainMenu()
	{
		gameObject.SetActive(false);
		m_MainMenu.gameObject.SetActive(true);
	}
}
