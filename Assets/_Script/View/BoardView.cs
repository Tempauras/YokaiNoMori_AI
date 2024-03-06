using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
	[SerializeField] private Game m_GameModel;
	[SerializeField] private Transform m_CellsContainer;
	[SerializeField] private Transform m_BottomHand;
	[SerializeField] private Transform m_TopHand;

	[SerializeField] private PieceView m_PiecePrefab;
	[SerializeField] private ToggleGroup m_PiecesGroup;

	[SerializeField] private EndMenuBehaviour m_EndMenu;

	private List<CellView> m_CellsViews = new List<CellView>();
	private List<PieceView> m_PiecesViews = new List<PieceView>();
	private Dictionary<Piece, PieceView> m_PieceToView = new Dictionary<Piece, PieceView>();

	private bool m_IsSingleplayer = true; // if false, local 2 player
	private bool m_IsBottomPlayerTurn = true;

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
			cellView.InitIdx(this, cellIdx);

			cellIdx++;
		}
	}

	private void OnEnable()
	{
		m_GameModel.OnMovement += _OnMovement;
		m_GameModel.OnEnd += _OnEnd;
	}

	private void OnDisable()
	{
		m_GameModel.OnMovement -= _OnMovement;
		m_GameModel.OnEnd -= _OnEnd;
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
		m_IsBottomPlayerTurn = true;

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

			PieceView pieceView = Instantiate(m_PiecePrefab);
			m_PiecesViews.Add(pieceView);
			m_PieceToView.Add(piece, pieceView);
			pieceView.InitPiece(this, piece, m_PiecesGroup);
			_UpdatePieceView(piece, m_CellsViews[cellIdx].transform);
		}
	}

	private void _OnMovement()
	{
		m_IsBottomPlayerTurn = !m_IsBottomPlayerTurn;
		_UpdateBoardView();
	}

	private void _OnEnd(int iEndCode)
	{
		switch(iEndCode)
		{
			case 0:
				m_EndMenu.ShowEndMenu("Draw.");
				break;
			case 1:
				if(m_IsSingleplayer)
					m_EndMenu.ShowEndMenu("You win!");
				else
					m_EndMenu.ShowEndMenu("Player 1 wins.");
				break;
			case 2:
				if(m_IsSingleplayer)
					m_EndMenu.ShowEndMenu("You lost.");
				else
					m_EndMenu.ShowEndMenu("Player 2 wins.");
				break;
			default:
				Debug.LogError("Unsupported end game code");
				break;
		}
	}

	private void _UpdateBoardView()
	{
		ClearInteractableCells();
		for(int cellIdx = 0; cellIdx < 12; cellIdx++)
			_UpdatePieceView(m_GameModel.GetCell(cellIdx), m_CellsViews[cellIdx].transform);

		foreach(Piece piece in m_GameModel.GetBottomHand())
			_UpdatePieceView(piece, m_BottomHand);
		foreach(Piece piece in m_GameModel.GetTopHand())
			_UpdatePieceView(piece, m_TopHand);

		if(m_IsSingleplayer)
			return;

		gameObject.transform.rotation = m_IsBottomPlayerTurn ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
	}

	private void _UpdatePieceView(Piece iPiece, Transform iParent)
	{
		if(iPiece == null || !m_PieceToView.ContainsKey(iPiece))
			return;

		PieceView pieceView = m_PieceToView[iPiece];
		pieceView.transform.SetParent(iParent, false);
		pieceView.UpdateView();

		if(m_IsSingleplayer)
			pieceView.SetInteractable((iPiece.GetPlayerOwnership() == PlayerOwnership.BOTTOM) && m_IsBottomPlayerTurn);
		else
			pieceView.SetInteractable((iPiece.GetPlayerOwnership() == PlayerOwnership.BOTTOM) == m_IsBottomPlayerTurn);
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
		if(iPiece != null)
			SetInteractableCells(m_GameModel.AllowedMove(iPiece));
		else
			ClearInteractableCells();
	}

	public void MoveTo(int iCellIdx)
	{
		m_GameModel.MovePieces(m_SelectedPiece, iCellIdx);
	}
}
