using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellView : MonoBehaviour
{
	[SerializeField] private Button m_MoveButton;

	private BoardView m_BoardView;
	private int m_CellIdx = -1;

	private void Start()
	{
		m_MoveButton.onClick.AddListener(_Move);
		SetInteractable(false);
	}

	public void InitIdx(BoardView iBoard, int iIdx)
	{
		m_BoardView = iBoard;
		m_CellIdx = iIdx;
	}

	public void SetInteractable(bool iInteractable)
	{
		m_MoveButton.gameObject.SetActive(iInteractable);
	}

	private void _Move()
	{
		m_BoardView.MoveTo(m_CellIdx);
	}
}
