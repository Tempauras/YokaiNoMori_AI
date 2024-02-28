using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
	[SerializeField] private Transform m_CellsContainer;
	[SerializeField] private Transform m_BottomHand;
	[SerializeField] private Transform m_TopHand;

	[SerializeField] private PieceView m_PiecePrefab;

	private GameManager m_GameModel;
	private List<CellView> m_CellsViews = new List<CellView>();
	private List<PieceView> m_PiecesViews = new List<PieceView>();
	private Dictionary<Piece, PieceView> m_PieceToView = new Dictionary<Piece, PieceView>();

	private bool m_IsSingleplayer = true; // if false, local 2 player

	private Piece m_SelectedPiece = null;

	// Start is called before the first frame update
	private void Start()
	{
		int cellIdx = 0;
		foreach(Transform cell in m_CellsContainer)
		{
			CellView cellView = cell.GetComponent<CellView>();
			if(cellView == null)
			{
				Debug.LogError($"No cell view component on {cell.name}");
				continue;
			}

			m_CellsViews.Add(cellView);
			cellView.InitIdx(cellIdx);

			cellIdx++;
		}
	}

	public void PlaySingle()
	{
		m_IsSingleplayer = true;
		_InitGame();
	}

	public void PlayDuel()
	{
		m_IsSingleplayer = false;
		_InitGame();
	}

	private void _Clear()
	{
		foreach(PieceView pieceView in m_PiecesViews)
			Destroy(pieceView.gameObject);
		m_PiecesViews.Clear();
		m_PieceToView.Clear();
		m_SelectedPiece = null;

		ClearInteractableCells();
	}

	private void _InitGame()
	{
		_Clear();

		m_GameModel.DispatchPieces();

		for(int cellIdx = 0; cellIdx < 12; cellIdx++)
		{
			Piece piece = m_GameModel.GetCell(cellIdx);
			if(piece == null)
				continue;

			PieceView pieceView = Instantiate(m_PiecePrefab, m_CellsViews[cellIdx].transform);
			m_PiecesViews.Add(pieceView);
			m_PieceToView.Add(piece, pieceView);
			pieceView.InitPiece(piece);
		}
	}

	private void _UpdateBoardView()
	{
		for(int cellIdx = 0; cellIdx < 12; cellIdx++)
		{
			Piece piece = m_GameModel.GetCell(cellIdx);
			if(piece == null)
				continue;

			PieceView pieceView = Instantiate(m_PiecePrefab, m_CellsViews[cellIdx].transform);
			m_PiecesViews.Add(pieceView);
			m_PieceToView.Add(piece, pieceView);
			pieceView.InitPiece(piece);
		}
	}

	public void SetInteractableCells(List<int> iCellIndices)
	{
		ClearInteractableCells();
		foreach(int cellIdx in iCellIndices)
			m_CellsViews[cellIdx].SetInteractable(true);
	}

	public void ClearInteractableCells()
	{
		foreach(CellView cell in m_CellsViews)
			cell.SetInteractable(false);
	}

	public void SelectPiece(Piece iPiece)
	{
		m_SelectedPiece = iPiece;
		SetInteractableCells(iPiece.GetNeightbour());
		// SetInteractableCells(m_GameModel.GetAllowedMoves(iPiece));
		// TODO: set interactable cells
	}

	public void MoveTo(int iCellIdx)
	{
		m_GameModel.MovePieces(m_SelectedPiece, iCellIdx);
	}
}
