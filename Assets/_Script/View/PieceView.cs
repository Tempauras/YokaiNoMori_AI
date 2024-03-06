using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceView : MonoBehaviour
{
	[SerializeField] private Image m_Image;
	[SerializeField] private Toggle m_SelectPiece;
	[SerializeField] private PiecesSprites m_PiecesSprites;

	private BoardView m_BoardView;
	private Piece m_PieceModel;

	// Start is called before the first frame update
	void Start()
	{
		m_SelectPiece.onValueChanged.AddListener(_OnSelect);
	}

	private void _OnSelect(bool iSelect)
	{
		if(!iSelect)
		{
			m_BoardView.SelectPiece(null);
			return;
		}

		m_BoardView.SelectPiece(m_PieceModel);
	}

	public void InitPiece(BoardView iBoard, Piece iPiece)
	{
		m_BoardView = iBoard;
		m_PieceModel = iPiece;
		UpdateView();
	}

	public void UpdateView()
	{
		m_SelectPiece.isOn = false;

		if(m_PieceModel == null)
		{
			m_Image.enabled = false;
			return;
		}

		m_Image.enabled = true;

		PieceType pieceType = m_PieceModel.GetPieceSO().pieceType;
		m_Image.sprite = m_PiecesSprites.Assets.Find(x => x.Piece == pieceType).Sprite;
		transform.rotation = m_PieceModel.GetPlayerOwnership() == PlayerOwnership.BOTTOM ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
	}

	public void SetInteractable()
	{

	}
}
