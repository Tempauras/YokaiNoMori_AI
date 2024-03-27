using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDBehaviour : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI m_Text;

	private BoardView m_Game;

	public void InitHUD(BoardView iGame)
	{
		m_Game = iGame;
	}

	public void SetText(string iText)
	{
		m_Text.text = iText;
	}

	public void Rewind()
	{
		m_Game.Rewind();
	}
}
