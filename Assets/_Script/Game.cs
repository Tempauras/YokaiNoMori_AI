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
	END,
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
	private PlayerOwnership _currentPlayerTurn = PlayerOwnership.BOTTOM;

	public event Action OnInit;
	public event Action OnMovement;
	public event Action<int> OnEnd; // 0: draw ; 1: bottom win ; 2: top win

	private bool _isBottomWinningNextTurn = false;
	private bool _isTopWinningNextTurn = true;

	void Start()
	{
		DispatchPieces();
	}

	public void DecodeBoardStateString(string BoardStateString)
	{
		Array.Clear(_gameBoard, 0, _gameBoard.Length);
		_currentPlayerTurn = PlayerOwnership.BOTTOM;
		_isBottomWinningNextTurn = false;
		_isTopWinningNextTurn = false;
		int boardNumber = 0;
		DecodeState state = DecodeState.BOARD;
		foreach(char c in BoardStateString)
		{
			int multiplier = 1;
			bool DoNotIncrement = false;
			switch(c)
			{
				case 'w':
					if(state == DecodeState.PLAYERTURN)
					{
						_currentPlayerTurn = PlayerOwnership.TOP;
					}
					break;
				case 'b':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(KitsunePiece, PlayerOwnership.BOTTOM);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						if(state == DecodeState.PLAYERTURN)
						{
							_currentPlayerTurn = PlayerOwnership.BOTTOM;
						}
						else
						{
							_handPiecesBottomPlayer.Add(new Piece(KitsunePiece, PlayerOwnership.BOTTOM));
						}
					}

					break;
				case 'B':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(KitsunePiece, PlayerOwnership.TOP);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(KitsunePiece, PlayerOwnership.TOP));
					}
					break;
				case 'p':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(KodamaPiece, PlayerOwnership.BOTTOM);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						_handPiecesBottomPlayer.Add(new Piece(KodamaPiece, PlayerOwnership.BOTTOM));
					}

					break;
				case 'P':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(KodamaPiece, PlayerOwnership.TOP);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(KodamaPiece, PlayerOwnership.TOP));
					}

					break;
				case 'g':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(KodamaSamuraiPiece, PlayerOwnership.BOTTOM);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						_handPiecesBottomPlayer.Add(new Piece(KodamaSamuraiPiece, PlayerOwnership.BOTTOM));
					}

					break;
				case 'G':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(KodamaSamuraiPiece, PlayerOwnership.TOP);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(KodamaSamuraiPiece, PlayerOwnership.TOP));
					}

					break;
				case 'k':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(KoropokkuruPiece, PlayerOwnership.BOTTOM);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						_handPiecesBottomPlayer.Add(new Piece(KoropokkuruPiece, PlayerOwnership.BOTTOM));
					}

					break;
				case 'K':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(KoropokkuruPiece, PlayerOwnership.TOP);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(KoropokkuruPiece, PlayerOwnership.TOP));
					}

					break;
				case 'r':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(TanukiPiece, PlayerOwnership.BOTTOM);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						_handPiecesBottomPlayer.Add(new Piece(TanukiPiece, PlayerOwnership.BOTTOM));
					}

					break;
				case 'R':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(TanukiPiece, PlayerOwnership.TOP);
						_onGameBoardPieces.Add(_gameBoard[boardNumber]);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(TanukiPiece, PlayerOwnership.TOP));
					}

					break;
				case '/':
					DoNotIncrement = true;
					break;
				case ' ':
					switch(state)
					{
						case DecodeState.BOARD:
							state = DecodeState.PLAYERTURN;
							break;
						case DecodeState.PLAYERTURN:
							state = DecodeState.HAND;
							break;
						case DecodeState.HAND:
							return;
					}
					break;
				case '\0':
					break;
				default:
					int number = (int)char.GetNumericValue(c);
					if(number != -1)
					{
						multiplier = number;
					}
					break;
			}
			if(!DoNotIncrement)
			{
				boardNumber = boardNumber + (1 * multiplier);
			}
		}
	}

	public void DispatchPieces()
	{
		DecodeBoardStateString(GameStartingString);
		OnInit?.Invoke();
	}

	public bool MovePieces(Piece pieceMoving, int PositionToMoveTo)
	{
		PlayerOwnership ownerOfPiece = pieceMoving.GetPlayerOwnership();
		if (ownerOfPiece == PlayerOwnership.TOP && _isTopWinningNextTurn )
		{
			if (_isTopWinningNextTurn)
			{
                OnEnd.Invoke(2);
            }
		}
		else
		{
			if (_isBottomWinningNextTurn)
			{
				OnEnd.Invoke(1);
			}
		}
		if (ownerOfPiece != _currentPlayerTurn)
		{
			return false;
		}
		if(pieceMoving.GetPieceSO().pieceType == PieceType.KOROPOKKURU &&
			((pieceMoving.GetPlayerOwnership() == PlayerOwnership.TOP) ? (PositionToMoveTo <= 2) : (PositionToMoveTo >= 9)))
		{
			List<int> allowedEnemyMove = new List<int>();
			foreach(Piece piece in _gameBoard)
			{
				if(piece.GetPlayerOwnership() != ownerOfPiece)
				{
					allowedEnemyMove.AddRange(AllowedMove(piece));
				}
			}
			if(!allowedEnemyMove.Contains(PositionToMoveTo))
			{
				OnEnd.Invoke(ownerOfPiece == PlayerOwnership.TOP ? 2 : 1);
			}
			else
			{
				if (ownerOfPiece == PlayerOwnership.TOP)
				{
					_isTopWinningNextTurn = true;
				}
				else
				{
					_isBottomWinningNextTurn = true;
				}
			}
		}
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
			if(_gameBoard[PositionToMoveTo].GetPieceSO().pieceType == PieceType.KOROPOKKURU)
			{
				OnEnd.Invoke(ownerOfPiece == PlayerOwnership.TOP ? 2 : 1);
			}
			if(ownerOfPiece == PlayerOwnership.TOP)
			{
				_handPiecesTopPlayer.Add(_gameBoard[PositionToMoveTo]);
			}
			else
			{
				_handPiecesBottomPlayer.Add(_gameBoard[PositionToMoveTo]);
			}
			_gameBoard[PositionToMoveTo].SetPlayerOwnership(ownerOfPiece);
			if(_gameBoard[PositionToMoveTo].GetPieceSO().pieceType == PieceType.KODAMA_SAMURAI)
				_gameBoard[PositionToMoveTo].SetPieceSO(KodamaPiece);
		}

		_gameBoard[indexOfMovingPiece] = null;
		_gameBoard[PositionToMoveTo] = pieceMoving;

		if(pieceMoving.GetPieceSO().pieceType == PieceType.KODAMA &&
			((ownerOfPiece == PlayerOwnership.TOP) ? (PositionToMoveTo <= 2) : (PositionToMoveTo >= 9)))
		{
			pieceMoving.SetPieceSO(KodamaSamuraiPiece);
		}

		OnMovement?.Invoke();
		ChangeTurn();
		return true;
	}

	public bool ParachutePiece(Piece pieceParachuting, int PositionToParachuteTo)
	{
		PlayerOwnership ownerOfPiece = pieceParachuting.GetPlayerOwnership();
		if(ownerOfPiece != _currentPlayerTurn)
		{
			return false;
		}
		//Check if the piece exists in its owner hand
		if(ownerOfPiece == PlayerOwnership.TOP ? _handPiecesTopPlayer.Exists(x => x == pieceParachuting) : _handPiecesBottomPlayer.Exists(x => x == pieceParachuting))
		{
			//Check if the position is empty
			if(_gameBoard[PositionToParachuteTo] == null || _gameBoard[PositionToParachuteTo].GetPieceSO() == null)
			{
				_gameBoard[PositionToParachuteTo] = pieceParachuting;
				if(ownerOfPiece == PlayerOwnership.TOP)
				{
					_handPiecesTopPlayer.Remove(pieceParachuting);
				}
				else
				{
					_handPiecesBottomPlayer.Remove(pieceParachuting);
				}
				OnMovement?.Invoke();
				ChangeTurn();
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

	public PlayerOwnership GetCurrentPlayer()
	{
		return _currentPlayerTurn;
	}

	private void ChangeTurn()
	{
		switch(_currentPlayerTurn)
		{
			case PlayerOwnership.TOP:
				_currentPlayerTurn = PlayerOwnership.BOTTOM;

				break;
			case PlayerOwnership.BOTTOM:
				_currentPlayerTurn = PlayerOwnership.TOP;
				break;
			default:
				break;
		}
	}
}
