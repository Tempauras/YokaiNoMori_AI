using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YokaiNoMori.Coffee
{

	public enum DecodeState
	{
		BOARD,
		PLAYERTURN,
		HAND,
		END,
	}

	public struct MoveData
	{
		public PieceData piece;
		public int startPos;
		public int endPos;
		public PieceData? pieceEaten;

		public MoveData(PieceData piece, int StartPos, int EndPos)
		{
			this.piece = piece;
			this.startPos = StartPos;
			this.endPos = EndPos;
			this.pieceEaten = null;
		}

		public MoveData(PieceData piece, int StartPos, int EndPos, PieceData? pieceEaten)
		{
			this.piece = piece;
			this.startPos = StartPos;
			this.endPos = EndPos;
			this.pieceEaten = pieceEaten;
		}
	}

	public class ClonePieceEnumerator : IEnumerator<Piece>
	{
		private IEnumerator<Piece> m_PieceEnumerator;
		private bool m_IsValid = false;
		private Piece m_Current = null;

		public Piece Current => m_Current;

		object IEnumerator.Current => m_Current;

		public ClonePieceEnumerator(IEnumerator<Piece> iPieceEnumerator)
		{
			m_PieceEnumerator = iPieceEnumerator;
		}

		private void _Clone()
		{
			if(m_IsValid && m_PieceEnumerator.Current != null)
				m_Current = new Piece(m_PieceEnumerator.Current.GetPieceData(), m_PieceEnumerator.Current.GetPlayerOwnership());
			else
				m_Current = null;
		}

		public void Dispose()
		{
			m_PieceEnumerator.Dispose();
		}

		public bool MoveNext()
		{
			m_IsValid = m_PieceEnumerator.MoveNext();
			if(m_IsValid)
				_Clone();
			return m_IsValid;
		}

		public void Reset()
		{
			m_PieceEnumerator.Reset();
		}
	}

	public class ClonePieceEnumerable : IEnumerable<Piece>
	{
		private IEnumerable<Piece> m_Pieces;
		private ClonePieceEnumerator m_PieceEnumerator;

		public ClonePieceEnumerable(IEnumerable<Piece> iPieces)
		{
			m_Pieces = iPieces;
			m_PieceEnumerator = new ClonePieceEnumerator(m_Pieces.GetEnumerator());
		}

		public IEnumerator<Piece> GetEnumerator()
		{
			return m_PieceEnumerator;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
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

		private LinkedList<MoveData> _movesData = new LinkedList<MoveData>();
		private int _nbRepeatedMoves = 0;

		private long _gameHash = 0;

		public Game()
		{
		}

		public Game(Game copy, out Dictionary<Piece, Piece> newToOld)
		{
			_gameBoard = new List<Piece>(new ClonePieceEnumerable(copy._gameBoard)).ToArray();
			_handPiecesTopPlayer = new List<Piece>(new ClonePieceEnumerable(copy._handPiecesTopPlayer));
			_handPiecesBottomPlayer = new List<Piece>(new ClonePieceEnumerable(copy._handPiecesBottomPlayer));
			_currentPlayerTurn = copy._currentPlayerTurn;
			_isBottomWinningNextTurn = copy._isBottomWinningNextTurn;
			_isTopWinningNextTurn = copy._isTopWinningNextTurn;
			_nbRepeatedMoves = copy._nbRepeatedMoves;

			// cloning move data
			Dictionary<Piece, Piece> toNewPiece = new Dictionary<Piece, Piece>();
			newToOld = new Dictionary<Piece, Piece>();
			foreach((Piece newPiece, Piece oldPiece) in Enumerable.Zip(_gameBoard, copy._gameBoard, KeyValuePair.Create))
			{
				if(oldPiece != null)
				{
					toNewPiece.Add(oldPiece, newPiece);
					newToOld.Add(newPiece, oldPiece);
				}
			}
			foreach((Piece newPiece, Piece oldPiece) in Enumerable.Zip(_handPiecesTopPlayer, copy._handPiecesTopPlayer, KeyValuePair.Create))
			{
				toNewPiece.Add(oldPiece, newPiece);
				newToOld.Add(newPiece, oldPiece);
			}
			foreach((Piece newPiece, Piece oldPiece) in Enumerable.Zip(_handPiecesBottomPlayer, copy._handPiecesBottomPlayer, KeyValuePair.Create))
			{
				toNewPiece.Add(oldPiece, newPiece);
				newToOld.Add(newPiece, oldPiece);
			}

			_movesData = new LinkedList<MoveData>(_movesData);
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

		public bool Rewind()
		{
			if(_movesData.Count == 0)
				return false;
			MoveData lastMoveData = _movesData.Last.Value;
			_movesData.RemoveLast();
			PlayerOwnership playerOwnership = _currentPlayerTurn == PlayerOwnership.TOP ? PlayerOwnership.BOTTOM : PlayerOwnership.TOP ;
			if(lastMoveData.startPos == -1)
			{
				if(playerOwnership == PlayerOwnership.TOP)
					_handPiecesTopPlayer.Add(_gameBoard[lastMoveData.endPos]);
				else
					_handPiecesBottomPlayer.Add(_gameBoard[lastMoveData.endPos]);
                _gameBoard[lastMoveData.endPos] = null;
            }
			else
			{
				Piece piece = _gameBoard[lastMoveData.endPos];
				piece.SetPieceType(lastMoveData.piece);
				_gameBoard[lastMoveData.startPos] = piece;
				if(lastMoveData.pieceEaten != null)
				{
					Piece pieceEaten;
					if(playerOwnership == PlayerOwnership.TOP)
					{
                        pieceEaten = _handPiecesTopPlayer[_handPiecesTopPlayer.Count -1];
                        pieceEaten.SetPlayerOwnership(PlayerOwnership.BOTTOM);
                        _handPiecesTopPlayer.RemoveAt(_handPiecesTopPlayer.Count - 1);
					}
					else
					{
                        pieceEaten = _handPiecesBottomPlayer[_handPiecesBottomPlayer.Count -1];
                        pieceEaten.SetPlayerOwnership(PlayerOwnership.TOP);
                        _handPiecesBottomPlayer.RemoveAt(_handPiecesBottomPlayer.Count - 1);
					}
                    pieceEaten.SetPieceType(lastMoveData.pieceEaten.Value);
					_gameBoard[lastMoveData.endPos] = pieceEaten;
                }
				else
				{
					_gameBoard[lastMoveData.endPos] = null;
				}
			}
			ChangeTurn();
			OnMovement?.Invoke();
			return true;
		}

		public void DispatchPieces(string BoardStateString)
		{
			DecodeBoardStateString(BoardStateString);
			_movesData.Clear();
			_nbRepeatedMoves = 0;
			OnInit?.Invoke();
		}

		public void DispatchPieces()
		{
			DispatchPieces(_defaultGameStartingString);
		}

		public bool MovePieces(Piece pieceMoving, int PositionToMoveTo, bool IsRecordingMovement = true)
		{
			if(pieceMoving == null)
				return false;

			PlayerOwnership ownerOfPiece = pieceMoving.GetPlayerOwnership();
			PieceType pieceType = pieceMoving.GetPieceData().pieceType;
			if(ownerOfPiece != _currentPlayerTurn)
				return false;

			int indexOfMovingPiece = Array.IndexOf(_gameBoard, pieceMoving);
			if(indexOfMovingPiece == -1)
				return ParachutePiece(pieceMoving, PositionToMoveTo, IsRecordingMovement);

			if(!pieceMoving.GetNeighbour(indexOfMovingPiece).Contains(PositionToMoveTo))
			{
				Debug.Log("[GameManager - MovePieces] Invalid movement, wtf happened");
				return false;
			}

			Piece prevPiece = _gameBoard[PositionToMoveTo];
			_gameBoard[indexOfMovingPiece] = null;
			_gameBoard[PositionToMoveTo] = pieceMoving;

			// pawn promotion
			bool WasPromote = false;
			if(pieceType == PieceType.KODAMA &&
				((ownerOfPiece == PlayerOwnership.TOP) ? (PositionToMoveTo <= 2) : (PositionToMoveTo >= 9)))
			{
				pieceMoving.SetPieceType(PieceData.PromotedPawn);
				WasPromote = true;
			}

			if(IsRecordingMovement)
			{
                if (WasPromote)
                    RecordMove(PieceData.Pawn, indexOfMovingPiece, PositionToMoveTo, prevPiece?.GetPieceData());
				else
					RecordMove(pieceMoving.GetPieceData(), indexOfMovingPiece, PositionToMoveTo, prevPiece?.GetPieceData());
            }

			// managing piece that got taken if any
			if(prevPiece != null)
			{
				PieceType prevPieceType = prevPiece.GetPieceType();
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
					ChangeTurn();
					return true;
				}
			}

			// opponent king had been placed on last row and we didn't capture king, lose
			if(ownerOfPiece == PlayerOwnership.BOTTOM && _isTopWinningNextTurn)
			{
				OnEnd?.Invoke(2);
				ChangeTurn();
				return true;
			}
			if(ownerOfPiece == PlayerOwnership.TOP && _isBottomWinningNextTurn)
			{
				OnEnd?.Invoke(1);
				ChangeTurn();
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
					ChangeTurn();
					return true;
				}

				// end reported to next turn, either the king is captured, either we win
				if(ownerOfPiece == PlayerOwnership.TOP)
					_isTopWinningNextTurn = true;
				else
					_isBottomWinningNextTurn = true;
			}

			ChangeTurn();
			OnMovement?.Invoke();
			return true;
		}

		public bool ParachutePiece(Piece pieceParachuting, int PositionToParachuteTo, bool IsRecordingMovement)
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

			if(IsRecordingMovement)
				RecordMove(pieceParachuting.GetPieceData(), -1, PositionToParachuteTo);
			ChangeTurn();
			OnMovement?.Invoke();
			return true;
		}

		private void RecordMove(PieceData piece, int StartPos, int EndPos, PieceData? pieceEaten = null)
		{
			RecordMove(new MoveData(piece, StartPos, EndPos, pieceEaten));
		}

		private void RecordMove(MoveData move)
		{
			_movesData.AddLast(move);

			HashCurrentGame();

			if(_movesData.Count < 3)
				return;

			MoveData prevMove = _movesData.Last.Previous.Previous.Value; // move the same player did in the previous turn
			if(move.piece.pieceType == prevMove.piece.pieceType && move.endPos == prevMove.startPos)
				_nbRepeatedMoves++;
			else
				_nbRepeatedMoves = 0;

			if(_nbRepeatedMoves >= 10)
				OnEnd?.Invoke(0);
		}

		private void HashCurrentGame()
		{
			byte[] data = new byte[8];
			long pieceIdx = 0;

			int cellIdx = -1;
			foreach(Piece piece in _gameBoard)
			{
				cellIdx++;

				if(piece == null)
					continue;

				data[pieceIdx] = HashPiece(piece, cellIdx, piece.GetPlayerOwnership() == PlayerOwnership.TOP);
				pieceIdx++;
			}

			List<Piece> HandPieceTopPlayerClone = new List<Piece>(_handPiecesTopPlayer);
			HandPieceTopPlayerClone.Sort((x,y) =>
			{
				
				return x.GetPieceType().CompareTo(y.GetPieceType());
			});
			List<Piece> HandPieceBottomPlayerClone = new List<Piece>(_handPiecesBottomPlayer);
            HandPieceBottomPlayerClone.Sort((x, y) =>
            {

                return x.GetPieceType().CompareTo(y.GetPieceType());
            });
            foreach (Piece piece in HandPieceBottomPlayerClone)
			{
				data[pieceIdx] = HashPiece(piece, 15, false);
				pieceIdx++;
			}
			foreach(Piece piece in HandPieceTopPlayerClone)
			{
				data[pieceIdx] = HashPiece(piece, 15, true);
				pieceIdx++;
			}

			_gameHash = BitConverter.ToInt64(data);
		}

		private byte HashPiece(Piece piece, int Position, bool IsTop)
		{
			byte hash = 0b_0000_0000;
			hash |= (byte)piece.GetPieceType();
			hash |= (byte)(Position << 3);
			if(piece.GetPlayerOwnership() == PlayerOwnership.TOP)
				hash |= 1 << 7;
			return hash;
		}

		public long GetHash()
		{
			return _gameHash;
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
}