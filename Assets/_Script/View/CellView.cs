using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellView : MonoBehaviour
{
	[SerializeField] private Button m_MoveButton;
	[SerializeField] private Image m_ButtonView;

	private BoardView m_BoardView;
	private int m_CellIdx = -1;

	private void Start()
	{
		m_MoveButton.onClick.AddListener(Move);
		SetInteractable(false);
	}

	public void InitIdx(BoardView iBoard, int iIdx)
	{
		m_BoardView = iBoard;
		m_CellIdx = iIdx;
	}

	public void SetInteractable(bool iInteractable)
	{
		m_MoveButton.enabled = iInteractable;
		m_ButtonView.enabled = iInteractable;
	}

	public bool IsInteractable()
	{
		return m_MoveButton.enabled && m_ButtonView.enabled;
	}

	public void Move()
	{
		m_BoardView.MoveTo(m_CellIdx);
	}
}
