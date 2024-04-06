using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

namespace YokaiNoMori.Coffee
{
	using Random = UnityEngine.Random;
	using RawMoveData = KeyValuePair<Piece, int>;

	public class AIBehaviour : MonoBehaviour, ICompetitor
	{
		private static string s_Name = "YoshihAIruHabu (Groupe Pause Café)";

		private IGameManager m_RealGameManager;
		private Game m_RealGame;
		private Game m_GameModel;
		private Dictionary<Piece, IPawn> m_RealGameToGameManager;
		private Dictionary<Piece, Piece> m_InternToRealGame;

		private ECampType m_Camp = ECampType.NONE;

		private bool m_LastMoveEnded = false;
		private int m_LastEndCode;

		private int m_SearchDepth = 8;
		private int m_MaxComputeTime = 5000; // in millisecondss

		private int m_NbMoveExplored = 0;
		// private string m_DebugString = "";

		private bool m_IsFirstTurn = true;

		private struct Result
		{
			public RawMoveData? BestMove;
			public int BestMoveEval;
		}

		private CancellationTokenSource m_EndComputation = null;
		private bool m_DoMove = true;

		public AIBehaviour()
		{
		}

		#region Competitor functions
		public void Init(IGameManager iGameManager, float iMaxTime, ECampType iCamp)
		{
			m_RealGameManager = iGameManager;
			m_Camp = iCamp;
			m_MaxComputeTime = Mathf.FloorToInt(iMaxTime * 1000f);
			m_SearchDepth = Mathf.Max(Mathf.FloorToInt(Mathf.Log(m_MaxComputeTime) - 1), 2);
			m_IsFirstTurn = true;
		}

		public string GetName()
		{
			return s_Name;
		}

		public ECampType GetCamp()
		{
			return m_Camp;
		}

		public void GetDatas()
		{
			if(m_RealGameManager != null)
			{
				if(m_IsFirstTurn)
				{
					m_RealGame = new Game(m_RealGameManager, out m_RealGameToGameManager);
					if(m_Camp == ECampType.PLAYER_TWO)
						m_RealGame.SetCurrentPlayer(PlayerOwnership.TOP);
					m_IsFirstTurn = false;
				}
				else
					m_RealGame.ForwardLastAction(m_RealGameManager, m_RealGameToGameManager);
			}

			m_GameModel = new Game(m_RealGame, out m_InternToRealGame);
			m_GameModel.OnEnd += _OnModelEnd;
		}

		public async void StartTurn()
		{
			m_EndComputation = new CancellationTokenSource();
			m_EndComputation.CancelAfter(m_MaxComputeTime);
			m_DoMove = true;

			// debug
			// float startTime = Time.time;
			// m_DebugString = "";
			m_NbMoveExplored = 0;

			Result result;
			result.BestMove = null;
			result.BestMoveEval = int.MinValue;
			try
			{
				result = await Negamax();
			}
			catch(OperationCanceledException)
			{
				Debug.LogWarning("Computation canceled");
			}
			catch(Exception iException)
			{
				Debug.LogError("An error occured in AI computation:");
				Debug.LogException(iException);
			}
			finally
			{
				if(m_DoMove)
				{
					if(result.BestMove.HasValue)
					{
						//debug
						// Debug.Log($"Number of evaluated positions: {m_NbMoveExplored}");
						// float timeSpan = Time.time - startTime;
						// Debug.Log($"Computation time: {timeSpan}s ({(timeSpan != 0 ? m_NbMoveExplored / timeSpan : 0)} pos/seconds)");
						//Debug.Log($"{s_Name}: {result.BestMove.Value.Key.GetPieceType()} -> {Game.CellIdxToVector(result.BestMove.Value.Value)}");
						// Debug.Log($"Move evaluation: {result.BestMoveEval}");
						// Debug.Log($"Debug string:\n{m_DebugString}");
						// Write the string array to a new file named "WriteLines.txt".
						/*using(StreamWriter outputFile = new StreamWriter(Path.Combine(Application.dataPath, "_AI_Debug.txt")))
						{
							outputFile.Write(m_DebugString);
						}*/

						Move(result.BestMove.Value);
					}
					else
						MoveRandom();
				}
				else
					Debug.Log("Aborted");
			}
		}

		public void StopTurn()
		{
			m_EndComputation?.Cancel();
		}
		#endregion Competitor functions

		public void AbortTurn()
		{
			m_DoMove = false;
			m_EndComputation?.Cancel();
		}

		public void Init(Game iGame, float iMaxTime, PlayerOwnership iPlayerOwnership)
		{
			Init(null, iMaxTime, iPlayerOwnership == PlayerOwnership.BOTTOM ? ECampType.PLAYER_ONE : ECampType.PLAYER_TWO);
			m_RealGame = iGame;
		}

		private void MoveRandom()
		{
			GetDatas(); // resetting game model to real game
			List<RawMoveData> moves = GetAllMoves(m_GameModel);
			if(moves.Count == 0)
			{
				Debug.LogError("No moves available");
				return;
			}
			Move(moves[Random.Range(0, moves.Count)]);
		}

		private void _OnModelEnd(int iEndCode)
		{
			m_LastMoveEnded = true;
			m_LastEndCode = iEndCode;
		}

		private Task<Result> Negamax()
		{
			return Task.Run(() => _Negamax(m_GameModel, m_SearchDepth - 1));
		}

		private Result _Negamax(Game iGame, int iDepth)
		{
			Result result = new Result();
			result.BestMoveEval = int.MinValue;

			int curMoveVal = int.MinValue;
			PlayerOwnership curPlayer = iGame.GetCurrentPlayer();
			List<RawMoveData> moves = GetAllMoves(iGame);
			/*if(moves.Count == 0)
			{
				Debug.LogError("No move to visit");
				return result;
			}*/

			/*long initHash = iGame.GetHash();
			int moveIdx = 0;*/
			foreach(RawMoveData move in moves)
			{
				m_NbMoveExplored++;

				/*if(iDepth == m_SearchDepth - 1)
					Debug.Log($"Progress: {moveIdx++}/{moves.Count}");*/

				if(!iGame.MovePieces(move.Key, move.Value))
				{
					Debug.LogError("Failed move");
					continue;
				}

				if(m_LastMoveEnded)
				{
					int multiplier = 0;
					switch(m_LastEndCode)
					{
						case 0:
							break;
						case 1:
							multiplier = (curPlayer == PlayerOwnership.BOTTOM ? 1 : -1);
							break;
						case 2:
							multiplier = (curPlayer == PlayerOwnership.BOTTOM ? -1 : 1);
							break;
						default:
							Debug.LogError("End code not supported");
							break;
					}
					curMoveVal = multiplier * (10000 + iDepth);
					m_LastMoveEnded = false;
				}
				else
				{
					if(result.BestMoveEval == 10000 + iDepth - 1 || result.BestMoveEval == 10000 + iDepth - 2)
						curMoveVal = int.MinValue; // a win has been found, we only need to search for a shorter win, so we don't explore further
					else
					{
						if(iDepth <= 1)
							curMoveVal = -_Evaluate(iGame);
						else
						{
							Result res = _Negamax(iGame, iDepth - 1);
							curMoveVal = -res.BestMoveEval;
						}
					}
				}

				/*m_DebugString += $"{iDepth} - {move.Key.GetPieceType()}->{move.Value}: {curMoveVal}\n";*/
				iGame.Rewind();
				/*
								if(iGame.GetCurrentPlayer() != curPlayer)
									Debug.LogError("Rewind error : player turn changed");
								if(initHash != iGame.GetHash())
									Debug.LogError("Rewind error : hash changed");*/

				if(curMoveVal > result.BestMoveEval)
				{
					result.BestMoveEval = curMoveVal;
					result.BestMove = move;

					if(result.BestMoveEval >= 10000 + iDepth)
						return result;
				}

				if(m_EndComputation.IsCancellationRequested)
				{
					/*Debug.LogWarning("Computation cancelled");*/
					return result;
				}
			}

			return result;
		}

		private int _Evaluate(Game iGame)
		{
			int value = 0;
			PlayerOwnership curPlayer = iGame.GetCurrentPlayer();

			for(int cellIdx = 0; cellIdx < 12; cellIdx++)
			{
				Piece piece = iGame.GetCell(cellIdx);
				if(piece == null)
					continue;

				PlayerOwnership pieceCamp = piece.GetPlayerOwnership();
				PieceType pieceType = piece.GetPieceType();
				int campMultiplier = pieceCamp == curPlayer ? 1 : -1;
				value += campMultiplier * GetPieceValue(pieceType) * 2;
				//King Position on board
				if(pieceType == PieceType.KOROPOKKURU)
				{
					if(pieceCamp == PlayerOwnership.BOTTOM && cellIdx >= 6)
					{
						value += campMultiplier * 25;
						if(cellIdx >= 9)
							value += campMultiplier * 25;
					}
					if(pieceCamp == PlayerOwnership.TOP && cellIdx < 6)
					{
						value += campMultiplier * 25;
						if(cellIdx < 3)
							value += campMultiplier * 25;
					}
				}
				//End King Position on board
			}

			int handCamp = curPlayer == PlayerOwnership.BOTTOM ? 1 : -1;
			foreach(Piece piece in iGame.GetBottomHand())
				value += handCamp * GetPieceValue(piece.GetPieceType());
			foreach(Piece piece in iGame.GetTopHand())
				value += -handCamp * GetPieceValue(piece.GetPieceType());

			return value;
		}

		private static int GetPieceValue(PieceType iPieceType)
		{
			switch(iPieceType)
			{
				case PieceType.KODAMA:
					return 5;
				case PieceType.KODAMA_SAMURAI:
					return 30;
				case PieceType.TANUKI:
					return 25;
				case PieceType.KITSUNE:
					return 15;
				default:
					return 0;
			}
		}

		private static List<RawMoveData> GetAllMoves(Game iGame)
		{
			List<RawMoveData> moves = new List<RawMoveData>();
			PlayerOwnership curPlayer = iGame.GetCurrentPlayer();
			for(int cellIdx = 0; cellIdx < 12; cellIdx++)
			{
				Piece piece = iGame.GetCell(cellIdx);
				if(piece == null || piece.GetPlayerOwnership() != curPlayer)
					continue;
				moves.AddRange(GetAllMovesForPiece(iGame, piece));
			}
			foreach(Piece piece in (curPlayer == PlayerOwnership.BOTTOM ? iGame.GetBottomHand() : iGame.GetTopHand()))
				moves.AddRange(GetAllMovesForPiece(iGame, piece));

			return moves;
		}

		private static List<RawMoveData> GetAllMovesForPiece(Game iGame, Piece iPiece)
		{
			List<RawMoveData> moves = new List<RawMoveData>();
			foreach(int pos in iGame.AllowedMove(iPiece))
				moves.Add(KeyValuePair.Create(iPiece, pos));

			return moves;
		}

		private void Move(RawMoveData iMove)
		{
			Move(iMove.Key, iMove.Value);
		}

		private void Move(Piece iPiece, int iPos)
		{
			// iPiece is assumed to be a Piece from m_GameModel
			Piece realPiece = m_InternToRealGame.GetValueOrDefault(iPiece);
			if(realPiece == null)
			{
				Debug.LogError("No piece found");
				return;
			}

			m_RealGame.MovePieces(realPiece, iPos);

			if(m_RealGameManager != null)
			{
				IPawn pawn = m_RealGameToGameManager.GetValueOrDefault(realPiece);
				if(pawn == null)
				{
					Debug.LogError("No pawn found, AI died.");
					return;
				}

				Move(pawn, Game.CellIdxToVector(iPos));
			}
		}

		private void Move(IPawn iPawn, Vector2Int iPos)
		{
			if(iPawn == null || m_RealGameManager == null)
				return;

			Vector2Int pawnPos = iPawn.GetCurrentPosition();
			EActionType action = (pawnPos.x < 0 || pawnPos.y < 0) ? EActionType.PARACHUTE : EActionType.MOVE;
			m_RealGameManager.DoAction(iPawn, iPos, action);
		}

		// read game
		// compute best move
		// send move
	}
}