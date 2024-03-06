using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum DecodeState
{
	BOARD,
	PLAYERTURN,
	HAND,
}

public class Game : MonoBehaviour
{
	public PieceSO KitsunePiece;
	public PieceSO KoropokkuruPiece;
	public PieceSO KodamaPiece;
	public PieceSO KodamaSamuraiPiece;
	public PieceSO TanukiPiece;

	public String GameStartingString;

	private Piece[] _gameBoard = new Piece[12];
	private List<Piece> _onGameBoardPieces = new List<Piece>();
	private List<Piece> _handPiecesTopPlayer = new List<Piece>();
	private List<Piece> _handPiecesBottomPlayer = new List<Piece>();

	public event Action OnInit;
	public event Action OnMovement;
	public event Action<int> OnEnd; // 0: draw ; 1: bottom win ; 2: top win

	void Start()
	{
		DispatchPieces();
	}

	public void DecodeBoardStateString(string BoardStateString)
	{
		int boardNumber = 0;
		foreach (char c in BoardStateString)
		{
			int multiplier = 1;
			bool DoNotIncrement = false;
			switch (c)
			{
				case 'b':
					_gameBoard[boardNumber] = new Piece(KitsunePiece, PlayerOwnership.BOTTOM);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case 'B':
                    _gameBoard[boardNumber] = new Piece(KitsunePiece, PlayerOwnership.TOP);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case 'p':
                    _gameBoard[boardNumber] = new Piece(KodamaPiece, PlayerOwnership.BOTTOM);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case 'P':
                    _gameBoard[boardNumber] = new Piece(KodamaPiece, PlayerOwnership.TOP);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case 'g':
                    _gameBoard[boardNumber] = new Piece(KodamaSamuraiPiece, PlayerOwnership.BOTTOM);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case 'G':
                    _gameBoard[boardNumber] = new Piece(KodamaSamuraiPiece, PlayerOwnership.TOP);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case 'k':
                    _gameBoard[boardNumber] = new Piece(KoropokkuruPiece, PlayerOwnership.BOTTOM);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case 'K':
                    _gameBoard[boardNumber] = new Piece(KoropokkuruPiece, PlayerOwnership.TOP);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case 'r':
                    _gameBoard[boardNumber] = new Piece(TanukiPiece, PlayerOwnership.BOTTOM);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case 'R':
                    _gameBoard[boardNumber] = new Piece(TanukiPiece, PlayerOwnership.TOP);
                    _onGameBoardPieces.Add(_gameBoard[boardNumber]);
                    break;
				case '/':
					DoNotIncrement = true;
					break;
				case ' ':
					break;
				case '\0':
					break;
				default:
					int number = (int)char.GetNumericValue(c); 
					if (number != -1)
					{
						multiplier = number;
					}
					break;
			}
			if (!DoNotIncrement)
			{
                boardNumber = boardNumber + (1 * multiplier);
            }
		}
	}

	public void DispatchPieces()
	{
		//Populate gameboard array and create pieces
		DecodeBoardStateString(GameStartingString);
		//_gameBoard[0] = new Piece(KitsunePiece, PlayerOwnership.BOTTOM);
		//_onGameBoardPieces.Add(_gameBoard[0]);
		//
		//_gameBoard[1] = new Piece(KoropokkuruPiece, PlayerOwnership.BOTTOM);
		//_onGameBoardPieces.Add(_gameBoard[1]);
		//
		//_gameBoard[2] = new Piece(TanukiPiece, PlayerOwnership.BOTTOM);
		//_onGameBoardPieces.Add(_gameBoard[2]);
		//
		//_gameBoard[4] = new Piece(KodamaPiece, PlayerOwnership.BOTTOM);
		//_onGameBoardPieces.Add(_gameBoard[4]);
		//
		//_gameBoard[7] = new Piece(KodamaPiece, PlayerOwnership.TOP);
		//_onGameBoardPieces.Add(_gameBoard[7]);
		//
		//_gameBoard[9] = new Piece(TanukiPiece, PlayerOwnership.TOP);
		//_onGameBoardPieces.Add(_gameBoard[9]);
		//
		//_gameBoard[10] = new Piece(KoropokkuruPiece, PlayerOwnership.TOP);
		//_onGameBoardPieces.Add(_gameBoard[10]);
		//
		//_gameBoard[11] = new Piece(KitsunePiece, PlayerOwnership.TOP);
		//_onGameBoardPieces.Add(_gameBoard[11]);

		OnInit?.Invoke();
	}

	public bool MovePieces(Piece pieceMoving, int PositionToMoveTo)
	{
		int indexOfMovingPiece = Array.IndexOf(_gameBoard, pieceMoving);
		if(indexOfMovingPiece == -1)
			return ParachutePiece(pieceMoving, PositionToMoveTo);

		if(pieceMoving.GetNeighbour(indexOfMovingPiece).Count <= 0)
		{
			Debug.Log("[GameManager - MovePieces] No valid movement, wtf happened");
			return false;
		}

		if(_gameBoard[PositionToMoveTo] != null && _gameBoard[PositionToMoveTo].GetPieceSO() != null)
		{
			if(pieceMoving.GetPlayerOwnership() == PlayerOwnership.TOP)
			{
				_handPiecesTopPlayer.Add(_gameBoard[PositionToMoveTo]);
			}
			else
			{
				_handPiecesBottomPlayer.Add(_gameBoard[PositionToMoveTo]);
			}
			_gameBoard[PositionToMoveTo].SetPlayerOwnership(pieceMoving.GetPlayerOwnership());
			if(_gameBoard[PositionToMoveTo].GetPieceSO().pieceType == PieceType.KODAMA_SAMURAI)
				_gameBoard[PositionToMoveTo].SetPieceSO(KodamaPiece);
		}

		_gameBoard[indexOfMovingPiece] = null;
		_gameBoard[PositionToMoveTo] = pieceMoving;

		if(pieceMoving.GetPieceSO().pieceType == PieceType.KODAMA &&
			((pieceMoving.GetPlayerOwnership() == PlayerOwnership.TOP) ? (PositionToMoveTo <= 2) : (PositionToMoveTo >= 9)))
		{
			pieceMoving.SetPieceSO(KodamaSamuraiPiece);
		}

		OnMovement?.Invoke();
		return true;
	}

	public bool ParachutePiece(Piece pieceParachuting, int PositionToParachuteTo)
	{
		//Check if the piece exists in its owner hand
		if(pieceParachuting.GetPlayerOwnership() == PlayerOwnership.TOP ? _handPiecesTopPlayer.Exists(x => x == pieceParachuting) : _handPiecesBottomPlayer.Exists(x => x == pieceParachuting))
		{
			//Check if the position is empty
			if(_gameBoard[PositionToParachuteTo] == null || _gameBoard[PositionToParachuteTo].GetPieceSO() == null)
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
				OnMovement?.Invoke();
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
		PlayerOwnership player = piece.GetPlayerOwnership();
		List<int> availableSpace = new List<int>();
		if(indexOfMovingPiece == -1)
		{
			for(int i = 0; i < 12; i++)
			{
				Piece boardPiece = _gameBoard[i];
				if(boardPiece != null && boardPiece.GetPieceSO() != null)
					continue;
				availableSpace.Add(i);
			}
			return availableSpace;
		}

		List<int> neighbourSpaces = piece.GetNeighbour(indexOfMovingPiece);
		foreach(int neighbourSpace in neighbourSpaces)
		{
			if(_gameBoard[neighbourSpace] == null || _gameBoard[neighbourSpace].GetPlayerOwnership() != piece.GetPlayerOwnership())
			{
				availableSpace.Add(neighbourSpace);
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

	public List<Piece> GetTopHand()
	{
		return _handPiecesTopPlayer;
	}

	public List<Piece> GetBottomHand()
	{
		return _handPiecesBottomPlayer;
	}
}
