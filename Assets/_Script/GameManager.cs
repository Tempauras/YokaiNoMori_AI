using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
	public PieceSO KitsunePiece;
	public PieceSO KoropokkuruPiece;
	public PieceSO KodamaPiece;
	public PieceSO KodamaSamuraiPiece;
	public PieceSO TanukiPiece;

	private Piece[] _gameBoard = new Piece[12];
	private List<Piece> _onGameBoardPieces = new List<Piece>();
	private List<Piece> _handPiecesTopPlayer = new List<Piece>();
	private List<Piece> _handPiecesBottomPlayer = new List<Piece>();
	// Start is called before the first frame update
	void Start()
	{
		DispatchPieces();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void DispatchPieces()
	{
		//Populate gameboard array and create pieces
		_gameBoard[0] = new Piece(KitsunePiece, PlayerOwnership.BOTTOM);
		_onGameBoardPieces.Add(_gameBoard[0]);

		_gameBoard[1] = new Piece(KoropokkuruPiece, PlayerOwnership.BOTTOM);
		_onGameBoardPieces.Add(_gameBoard[1]);

		_gameBoard[2] = new Piece(TanukiPiece, PlayerOwnership.BOTTOM);
		_onGameBoardPieces.Add(_gameBoard[2]);

		_gameBoard[4] = new Piece(KodamaPiece, PlayerOwnership.BOTTOM);
		_onGameBoardPieces.Add(_gameBoard[4]);

		_gameBoard[7] = new Piece(KodamaPiece, PlayerOwnership.TOP);
		_onGameBoardPieces.Add(_gameBoard[7]);

		_gameBoard[9] = new Piece(TanukiPiece, PlayerOwnership.TOP);
		_onGameBoardPieces.Add(_gameBoard[9]);

		_gameBoard[10] = new Piece(KoropokkuruPiece, PlayerOwnership.TOP);
		_onGameBoardPieces.Add(_gameBoard[10]);

		_gameBoard[11] = new Piece(KitsunePiece, PlayerOwnership.TOP);
		_onGameBoardPieces.Add(_gameBoard[11]);
	}

	public bool MovePieces(Piece pieceMoving, int PositionToMoveTo)
	{
		int indexOfMovingPiece = Array.IndexOf(_gameBoard, pieceMoving);
		if(indexOfMovingPiece == -1)
		{
			Debug.Log("[GameManager - MovePieces] Pieces not in gameboard, wtf happened");
			return false;
		}

		if(pieceMoving.GetNeighbour(indexOfMovingPiece).Count <= 0)
		{
			Debug.Log("[GameManager - MovePieces] No valid movement, wtf happened");
			return false;
		}
		if(_gameBoard[PositionToMoveTo].GetPieceSO())
		{
			if(pieceMoving.GetPlayerOwnership() == PlayerOwnership.TOP)
			{
				_handPiecesTopPlayer.Add(_gameBoard[PositionToMoveTo]);
			}
			else
			{
				_handPiecesBottomPlayer.Add(_gameBoard[PositionToMoveTo]);
			}
			_gameBoard[indexOfMovingPiece] = null;
			_gameBoard[PositionToMoveTo] = pieceMoving;
		}
		else
		{
			_gameBoard[indexOfMovingPiece] = null;
			_gameBoard[PositionToMoveTo] = pieceMoving;
		}
		return true;
	}

	public bool ParachutePiece(Piece pieceParachuting, int PositionToParachuteTo)
	{
		//Check if the piece exists in its owner hand
		if(pieceParachuting.GetPlayerOwnership() == PlayerOwnership.TOP ? _handPiecesTopPlayer.Exists(x => x == pieceParachuting) : _handPiecesBottomPlayer.Exists(x => x == pieceParachuting))
		{
			//Check if the position is empty
			if(!_gameBoard[PositionToParachuteTo].GetPieceSO())
			{
				_gameBoard[PositionToParachuteTo] = pieceParachuting;
				if(pieceParachuting.GetPlayerOwnership() == PlayerOwnership.TOP)
				{
					_handPiecesTopPlayer.Remove(pieceParachuting);
				}
				else
				{
					_handPiecesBottomPlayer.Remove(pieceParachuting);
				}
				return true;
			}
			else
			{
				Debug.Log("[GameManager - ParachutePiece] Position is not empty.");
			}
		}
		else
		{
			Debug.Log("[GameManager - ParachutePiece] Piece does not exist in its owner hand, wtf happened");
		}
		return false;
	}

	public List<int> AllowedMove(Piece piece)
	{
		int indexOfMovingPiece = Array.IndexOf(_gameBoard, piece);
		if(indexOfMovingPiece == -1)
		{
			Debug.Log("[GameManager - MovePieces] Pieces not in gameboard, wtf happened");
			return new List<int>();
		}
		List<int> availableSpace = new List<int>();
		List<int> NeighbourSpaces = piece.GetNeighbour(indexOfMovingPiece);
		foreach(int NeighbourSpace in NeighbourSpaces)
		{
			if(!_gameBoard[NeighbourSpace].GetPieceSO() || _gameBoard[NeighbourSpace].GetPlayerOwnership() != piece.GetPlayerOwnership())
			{
				availableSpace.Add(NeighbourSpace);
			}
		}

		return availableSpace;
	}

	public Piece GetCell(int cellIdx)
	{
		if(cellIdx < 0 || cellIdx >= 12)
			return null;
		return _gameBoard[cellIdx];
	}
}
