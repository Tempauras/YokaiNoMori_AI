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
	[SerializeField] private HUDBehaviour m_HUD;

	[SerializeField] private PieceView m_PiecePrefab;
	[SerializeField] private ToggleGroup m_PiecesGroup;

	[SerializeField] private EndMenuBehaviour m_EndMenu;

	private List<CellView> m_CellsViews = new List<CellView>();
	private List<PieceView> m_PiecesViews = new List<PieceView>();
	private Dictionary<Piece, PieceView> m_PieceToView = new Dictionary<Piece, PieceView>();

	private bool m_IsSingleplayer = true; // if false, local 2 player
	private bool m_IsBottomPlayerTurn = true;
	private bool m_IsEnded = false;

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

		m_GameModel.OnMovement += _OnMovement;
		m_GameModel.OnEnd += _OnEnd;
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

	public void Clear()
	{
		foreach(PieceView pieceView in m_PiecesViews)
			Destroy(pieceView.gameObject);
		m_PiecesViews.Clear();
		m_PieceToView.Clear();
		m_SelectedPiece = null;
		m_IsBottomPlayerTurn = true;
		m_IsEnded = false;
		m_HUD.gameObject.SetActive(false);

		ClearInteractableCells();
	}

	private void _InitGame()
	{
		Clear();

		m_GameModel.DispatchPieces();

		for(int cellIdx = 0; cellIdx < 12; cellIdx++)
			_InitPiece(m_GameModel.GetCell(cellIdx), m_CellsViews[cellIdx].transform);

		foreach(Piece bottomHandPiece in m_GameModel.GetBottomHand())
			_InitPiece(bottomHandPiece, m_BottomHand);
		foreach(Piece topHandPiece in m_GameModel.GetTopHand())
			_InitPiece(topHandPiece, m_TopHand);

		m_HUD.gameObject.SetActive(true);
	}

	private void _InitPiece(Piece iPiece, Transform iParent)
	{
		if(iPiece == null)
			return;

		PieceView pieceView = Instantiate(m_PiecePrefab);
		m_PiecesViews.Add(pieceView);
		m_PieceToView.Add(iPiece, pieceView);
		pieceView.InitPiece(this, iPiece, m_PiecesGroup);
		_UpdatePieceView(iPiece, iParent);
	}

	private void _OnMovement()
	{
		m_IsBottomPlayerTurn = !m_IsBottomPlayerTurn;
		_UpdateBoardView();
	}

	private void _OnEnd(int iEndCode)
	{
		m_IsEnded = true;
		m_HUD.gameObject.SetActive(false);
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
				m_IsBottomPlayerTurn = true;
				break;
			case 2:
				if(m_IsSingleplayer)
					m_EndMenu.ShowEndMenu("You lost.");
				else
					m_EndMenu.ShowEndMenu("Player 2 wins.");
				m_IsBottomPlayerTurn = true;
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
		{
			m_HUD.SetText(m_IsBottomPlayerTurn ? "Your turn" : "AI's turn");
			return;
		}

		m_HUD.SetText($"Player {(m_IsBottomPlayerTurn ? 1 : 2)}'s turn");

		if(!m_IsEnded)
			gameObject.transform.rotation = m_IsBottomPlayerTurn ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
	}

	private void _UpdatePieceView(Piece iPiece, Transform iParent)
	{
		if(iPiece == null || !m_PieceToView.ContainsKey(iPiece))
			return;

		PieceView pieceView = m_PieceToView[iPiece];
		pieceView.transform.SetParent(iParent, false);
		pieceView.UpdateView();

		if(m_IsEnded)
		{
			pieceView.SetInteractable(false);
			return;
		}

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
