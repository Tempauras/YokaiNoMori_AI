using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	public string GameStartingString;

	private Piece[] _gameBoard = new Piece[12];
	private List<Piece> _onGameBoardPieces = new List<Piece>();
	private List<Piece> _handPiecesTopPlayer = new List<Piece>();
	private List<Piece> _handPiecesBottomPlayer = new List<Piece>();
	private PlayerOwnership _currentPlayerTurn = PlayerOwnership.BOTTOM;

	public event Action OnInit;
	public event Action OnMovement;
	public event Action<int> OnEnd; // 0: draw ; 1: bottom win ; 2: top win

	private bool _isBottomWinningNextTurn = false;
	private bool _isTopWinningNextTurn = false;

	private List<KeyValuePair<Piece, int>> _moves = new List<KeyValuePair<Piece, int>>();
	private int _nbRepeatedMoves = 2;

	public void DecodeBoardStateString(string BoardStateString)
	{
		Array.Clear(_gameBoard, 0, _gameBoard.Length);
		_onGameBoardPieces.Clear();
		_handPiecesBottomPlayer.Clear();
		_handPiecesTopPlayer.Clear();
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
				case '?':
					if(state == DecodeState.PLAYERTURN)
					{
						_currentPlayerTurn = UnityEngine.Random.Range(0, 2) == 0 ? PlayerOwnership.BOTTOM : PlayerOwnership.TOP;
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

	public void DispatchPieces(string BoardStateString)
	{
		DecodeBoardStateString(BoardStateString);
		_moves.Clear();
		_nbRepeatedMoves = 2;
		OnInit?.Invoke();
	}

	public void DispatchPieces()
	{
		DispatchPieces(GameStartingString);
	}

	public bool MovePieces(Piece pieceMoving, int PositionToMoveTo)
	{
		if(pieceMoving == null || pieceMoving.GetPieceSO() == null)
			return false;

		PlayerOwnership ownerOfPiece = pieceMoving.GetPlayerOwnership();
		PieceType pieceType = pieceMoving.GetPieceSO().pieceType;
		if(ownerOfPiece != _currentPlayerTurn)
			return false;

		int indexOfMovingPiece = Array.IndexOf(_gameBoard, pieceMoving);
		if(indexOfMovingPiece == -1)
			return ParachutePiece(pieceMoving, PositionToMoveTo);

		if(!pieceMoving.GetNeighbour(indexOfMovingPiece).Contains(PositionToMoveTo))
		{
			Debug.Log("[GameManager - MovePieces] Invalid movement, wtf happened");
			return false;
		}

		Piece prevPiece = _gameBoard[PositionToMoveTo];
		_gameBoard[indexOfMovingPiece] = null;
		_gameBoard[PositionToMoveTo] = pieceMoving;

		// pawn promotion
		if(pieceType == PieceType.KODAMA &&
			((ownerOfPiece == PlayerOwnership.TOP) ? (PositionToMoveTo <= 2) : (PositionToMoveTo >= 9)))
		{
			pieceMoving.SetPieceSO(KodamaSamuraiPiece);
		}

		// managing piece that got taken if any
		bool hasTakenKing = false;
		if(prevPiece != null && prevPiece.GetPieceSO() != null)
		{
			PieceType prevPieceType = prevPiece.GetPieceSO().pieceType;
			if(ownerOfPiece == PlayerOwnership.TOP)
				_handPiecesTopPlayer.Add(prevPiece);
			else
				_handPiecesBottomPlayer.Add(prevPiece);
			prevPiece.SetPlayerOwnership(ownerOfPiece);

			if(prevPieceType == PieceType.KODAMA_SAMURAI)
				prevPiece.SetPieceSO(KodamaPiece);

			if(prevPieceType == PieceType.KOROPOKKURU)
				OnEnd?.Invoke(ownerOfPiece == PlayerOwnership.TOP ? 2 : 1);
		}

		// opponent king had been placed on last row and we didn't capture him, lose
		if(!hasTakenKing)
		{
			if(ownerOfPiece == PlayerOwnership.BOTTOM && _isTopWinningNextTurn)
				OnEnd?.Invoke(2);
			if(ownerOfPiece == PlayerOwnership.TOP && _isBottomWinningNextTurn)
				OnEnd?.Invoke(1);
		}

		// managing win condition with king on last row
		if(pieceType == PieceType.KOROPOKKURU &&
			((ownerOfPiece == PlayerOwnership.TOP) ? (PositionToMoveTo <= 2) : (PositionToMoveTo >= 9)))
		{
			List<int> allowedEnemyMove = new List<int>();
			foreach(Piece piece in _gameBoard)
			{
				if(piece == null)
					continue;
				if(piece.GetPlayerOwnership() != ownerOfPiece)
					allowedEnemyMove.AddRange(AllowedMove(piece));
			}

			// can't get taken, win
			if(!allowedEnemyMove.Contains(PositionToMoveTo))
				OnEnd?.Invoke(ownerOfPiece == PlayerOwnership.TOP ? 2 : 1);
			else
			{
				if(ownerOfPiece == PlayerOwnership.TOP)
					_isTopWinningNextTurn = true;
				else
					_isBottomWinningNextTurn = true;
			}
		}

		RecordMove(pieceMoving, PositionToMoveTo);
		OnMovement?.Invoke();
		ChangeTurn();
		return true;
	}

	public bool ParachutePiece(Piece pieceParachuting, int PositionToParachuteTo)
	{
		PlayerOwnership ownerOfPiece = pieceParachuting.GetPlayerOwnership();
		if(ownerOfPiece != _currentPlayerTurn)
			return false;

		//Check if the piece exists in its owner hand
		List<Piece> playerHand = (ownerOfPiece == PlayerOwnership.TOP ? _handPiecesTopPlayer : _handPiecesBottomPlayer);
		bool pieceExistsInHand = playerHand.Exists(x => x == pieceParachuting);
		if(!pieceExistsInHand)
		{
			Debug.Log("[GameManager - ParachutePiece] Piece does not exist in its owner hand, wtf happened");
			return false;
		}

		//Check if the position is empty
		if(_gameBoard[PositionToParachuteTo] != null && _gameBoard[PositionToParachuteTo].GetPieceSO() != null)
		{
			Debug.Log("[GameManager - ParachutePiece] Position is not empty.");
			return false;
		}

		_gameBoard[PositionToParachuteTo] = pieceParachuting;
		playerHand.Remove(pieceParachuting);

		RecordMove(pieceParachuting, PositionToParachuteTo);
		OnMovement?.Invoke();
		ChangeTurn();
		return true;
	}

	private void RecordMove(Piece piece, int Position)
	{
		RecordMove(KeyValuePair.Create(piece, Position));
	}

	private void RecordMove(KeyValuePair<Piece, int> move)
	{
		_moves.Add(move);

		if(_moves.Count <= 4)
			return;

		if(_moves[_moves.Count - 1].Equals(_moves[_moves.Count - 5]))
			_nbRepeatedMoves++;
		else
			_nbRepeatedMoves = 0;

		if(_nbRepeatedMoves >= 10)
			OnEnd.Invoke(0);
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

	public void SetCurrentPlayer(PlayerOwnership CurrentPlayer)
	{
		_currentPlayerTurn = CurrentPlayer;
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
