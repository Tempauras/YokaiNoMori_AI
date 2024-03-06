using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDBehaviour : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI m_Text;

	public void SetText(string iText)
	{
		m_Text.text = iText;
	}
}
