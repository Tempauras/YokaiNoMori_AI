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

public struct MoveData
{
	Piece piece;
	int startPos;
	int endPos;
	Piece? pieceEaten;

	public MoveData(Piece piece, int StartPos, int EndPos)
	{
		this.piece = piece;
		this.startPos = StartPos;
		this.endPos = EndPos;
		this.pieceEaten = null;
	}

	public MoveData(Piece piece, int StartPos, int EndPos, Piece pieceEaten)
	{
        this.piece = piece;
        this.startPos = StartPos;
        this.endPos = EndPos;
        this.pieceEaten = pieceEaten;
    }
}

public class Game
{
	private string _defaultGameStartingString = "bkr/1p1/1P1/RKB b";

	private Piece[] _gameBoard = new Piece[12];
	private List<Piece> _handPiecesTopPlayer = new List<Piece>();
	private List<Piece> _handPiecesBottomPlayer = new List<Piece>();
	private PlayerOwnership _currentPlayerTurn = PlayerOwnership.BOTTOM;

	public event Action OnInit;
	public event Action OnMovement;
	public event Action<int> OnEnd; // 0: draw ; 1: bottom win ; 2: top win

	private bool _isBottomWinningNextTurn = false;
	private bool _isTopWinningNextTurn = false;

	private List<MoveData> _movesData = new List<MoveData>();

	private List<KeyValuePair<Piece, int>> _moves = new List<KeyValuePair<Piece, int>>();
	private int _nbRepeatedMoves = 2;

	public Game()
	{
	}

	public Game(Game copy)
	{
		// #TODO: clone piece instances
		Array.Copy(copy._gameBoard, _gameBoard, 12);
		_handPiecesTopPlayer = new List<Piece>(copy._handPiecesTopPlayer);
		_handPiecesBottomPlayer = new List<Piece>(copy._handPiecesBottomPlayer);
		_currentPlayerTurn = copy._currentPlayerTurn;
		_isBottomWinningNextTurn = copy._isBottomWinningNextTurn;
		_isTopWinningNextTurn = copy._isTopWinningNextTurn;
		_moves = new List<KeyValuePair<Piece, int>>(copy._moves);
		_nbRepeatedMoves = copy._nbRepeatedMoves;
	}

	public void DecodeBoardStateString(string BoardStateString)
	{
		Array.Clear(_gameBoard, 0, _gameBoard.Length);
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
						_gameBoard[boardNumber] = new Piece(PieceData.Bishop, PlayerOwnership.BOTTOM);
					}
					else
					{
						if(state == DecodeState.PLAYERTURN)
						{
							_currentPlayerTurn = PlayerOwnership.BOTTOM;
						}
						else
						{
							_handPiecesBottomPlayer.Add(new Piece(PieceData.Bishop, PlayerOwnership.BOTTOM));
						}
					}

					break;
				case 'B':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(PieceData.Bishop, PlayerOwnership.TOP);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(PieceData.Bishop, PlayerOwnership.TOP));
					}
					break;
				case 'p':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(PieceData.Pawn, PlayerOwnership.BOTTOM);
					}
					else
					{
						_handPiecesBottomPlayer.Add(new Piece(PieceData.Pawn, PlayerOwnership.BOTTOM));
					}

					break;
				case 'P':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(PieceData.Pawn, PlayerOwnership.TOP);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(PieceData.Pawn, PlayerOwnership.TOP));
					}

					break;
				case 'g':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(PieceData.PromotedPawn, PlayerOwnership.BOTTOM);
					}
					else
					{
						_handPiecesBottomPlayer.Add(new Piece(PieceData.PromotedPawn, PlayerOwnership.BOTTOM));
					}

					break;
				case 'G':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(PieceData.PromotedPawn, PlayerOwnership.TOP);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(PieceData.PromotedPawn, PlayerOwnership.TOP));
					}

					break;
				case 'k':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(PieceData.King, PlayerOwnership.BOTTOM);
					}
					else
					{
						_handPiecesBottomPlayer.Add(new Piece(PieceData.King, PlayerOwnership.BOTTOM));
					}

					break;
				case 'K':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(PieceData.King, PlayerOwnership.TOP);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(PieceData.King, PlayerOwnership.TOP));
					}

					break;
				case 'r':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(PieceData.Rook, PlayerOwnership.BOTTOM);
					}
					else
					{
						_handPiecesBottomPlayer.Add(new Piece(PieceData.Rook, PlayerOwnership.BOTTOM));
					}

					break;
				case 'R':
					if(state == DecodeState.BOARD)
					{
						_gameBoard[boardNumber] = new Piece(PieceData.Rook, PlayerOwnership.TOP);
					}
					else
					{
						_handPiecesTopPlayer.Add(new Piece(PieceData.Rook, PlayerOwnership.TOP));
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
		DispatchPieces(_defaultGameStartingString);
	}

	public bool MovePieces(Piece pieceMoving, int PositionToMoveTo)
	{
		if(pieceMoving == null)
			return false;

		PlayerOwnership ownerOfPiece = pieceMoving.GetPlayerOwnership();
		PieceType pieceType = pieceMoving.GetPieceData().pieceType;
		if(ownerOfPiece != _currentPlayerTurn)
			return false;

		int indexOfMovingPiece = Array.IndexOf(_gameBoard, pieceMoving);
        if (indexOfMovingPiece == -1)
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
			pieceMoving.SetPieceType(PieceData.PromotedPawn);
		}

		// managing piece that got taken if any
		if(prevPiece != null)
		{
			PieceType prevPieceType = prevPiece.GetPieceData().pieceType;
			if(ownerOfPiece == PlayerOwnership.TOP)
				_handPiecesTopPlayer.Add(prevPiece);
			else
				_handPiecesBottomPlayer.Add(prevPiece);
			prevPiece.SetPlayerOwnership(ownerOfPiece);

			if(prevPieceType == PieceType.KODAMA_SAMURAI)
				prevPiece.SetPieceType(PieceData.Pawn);

			if(prevPieceType == PieceType.KOROPOKKURU)
			{
				OnEnd?.Invoke(ownerOfPiece == PlayerOwnership.TOP ? 2 : 1);
				return true;
			}
		}

		// opponent king had been placed on last row and we didn't capture king, lose
		if(ownerOfPiece == PlayerOwnership.BOTTOM && _isTopWinningNextTurn)
		{
			OnEnd?.Invoke(2);
			return true;
		}
		if(ownerOfPiece == PlayerOwnership.TOP && _isBottomWinningNextTurn)
		{
			OnEnd?.Invoke(1);
			return true;
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
			{
				OnEnd?.Invoke(ownerOfPiece == PlayerOwnership.TOP ? 2 : 1);
				return true;
			}

			// end reported to next turn, either the king is captured, either we win
			if(ownerOfPiece == PlayerOwnership.TOP)
				_isTopWinningNextTurn = true;
			else
				_isBottomWinningNextTurn = true;
		}
		RecordMove(pieceMoving, indexOfMovingPiece, PositionToMoveTo, prevPiece);
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
		if(_gameBoard[PositionToParachuteTo] != null)
		{
			Debug.Log("[GameManager - ParachutePiece] Position is not empty.");
			return false;
		}

		_gameBoard[PositionToParachuteTo] = pieceParachuting;
		playerHand.Remove(pieceParachuting);

        RecordMove(pieceParachuting, -1, PositionToParachuteTo, null);
        OnMovement?.Invoke();
		ChangeTurn();
		return true;
	}

	private void RecordMove(Piece piece, int StartPos, int EndPos, Piece pieceEaten)
	{
		RecordMove(new MoveData(piece, StartPos, EndPos, pieceEaten));
	}

	private void RecordMove(MoveData move)
	{
		_movesData.Add(move);

		if (_movesData.Count <= 4)
			return;
		if (_movesData[_movesData.Count - 1].Equals(_movesData[_movesData.Count - 5]))
			_nbRepeatedMoves++;
		else
			_nbRepeatedMoves = 0;

		if (_nbRepeatedMoves >= 10)
			OnEnd?.Invoke(0);
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
				if(boardPiece != null)
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
