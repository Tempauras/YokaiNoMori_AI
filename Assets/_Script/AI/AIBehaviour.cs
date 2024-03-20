using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

namespace YokaiNoMori.Coffee
{
	using RawMoveData = KeyValuePair<Piece, int>;

	public class AIBehaviour // : ICompetitor
	{
		private const string s_Name = "YoshihAIruHabu";

		private Game m_RealGame;
		private Game m_GameModel;
		private Dictionary<Piece, Piece> m_InternToRealGame;
		private static Dictionary<Piece, Piece> s_DictDummy;

		private ECampType m_Camp = ECampType.NONE;
		private PlayerOwnership m_PlayerOwnership;

		private bool m_LastMoveEnded = false;
		private int m_LastEndCode;

		private const int m_SearchDepth = 6;
		private static RawMoveData? s_MoveDataDummy;
		private int m_NbMoveExplored = 0;
		private float m_LastMoveEval = 0;
		private Dictionary<long, float> m_AlreadyEvaluated = new Dictionary<long, float>();

		public AIBehaviour()
		{
		}

		#region Competitor functions
		public ECampType GetCamp()
		{
			return m_Camp;
		}

		public string GetName()
		{
			return s_Name;
		}

		public void SetCamp(ECampType iCamp)
		{
			m_Camp = iCamp;
		}

		public void StartTurn()
		{
			MoveBestNegamax();
		}

		public void StopTurn()
		{
		}
		#endregion Competitor functions

		public void SetCamp(PlayerOwnership iPlayerOwnership)
		{
			m_PlayerOwnership = iPlayerOwnership;
		}

		public void Init(Game iGame)
		{
			m_RealGame = iGame;
		}

		private void UpdateData()
		{
			m_GameModel = new Game(m_RealGame, out m_InternToRealGame);
			m_GameModel.OnEnd += _OnModelEnd;
		}

		private void _OnModelEnd(int iEndCode)
		{
			m_LastMoveEnded = true;
			m_LastEndCode = iEndCode;
		}

		private void MoveRandom()
		{
			List<RawMoveData> moves = GetAllMoves(m_RealGame);
			Move(moves[Random.Range(0, moves.Count)]);
		}

		private async void MoveBestNegamax()
		{
			float startTime = Time.time;
			RawMoveData bestMove = await Negamax();
			Debug.Log($"Computation time for {m_SearchDepth} search depth: {Time.time - startTime}s");
			Debug.Log($"Number of evaluated positions: {m_NbMoveExplored}");
			Debug.Log($"Move evaluation: {m_LastMoveEval}");
			Move(bestMove);
		}

		private Task<RawMoveData> Negamax()
		{
			UpdateData();
			return Task.Run(
				() =>
				{
					RawMoveData? bestMove = null;
					m_NbMoveExplored = 0;
					m_LastMoveEval = _Negamax(m_GameModel, m_SearchDepth, ref bestMove);
					bestMove = KeyValuePair.Create(m_InternToRealGame[bestMove.Value.Key], bestMove.Value.Value);
					return bestMove.Value;
				});
		}

		private float _Negamax(Game iGame, int iDepth, ref RawMoveData? oBestMove)
		{
			m_NbMoveExplored++;
			if(iDepth == 0)
				return _Evaluate(iGame);

			float value = float.NegativeInfinity;
			float curMoveVal = float.NegativeInfinity;
			List<RawMoveData> moves = GetAllMoves(iGame);
			foreach(RawMoveData move in moves)
			{
				bool success = iGame.MovePieces(move.Key, move.Value);
				if(!success)
					continue;
				if(m_LastMoveEnded)
				{
					switch(m_LastEndCode)
					{
						case 0:
							curMoveVal = 0;
							break;
						case 1:
							curMoveVal = (iGame.GetCurrentPlayer() == PlayerOwnership.BOTTOM ? -1 : 1) * (1000 + iDepth);
							break;
						case 2:
							curMoveVal = (iGame.GetCurrentPlayer() == PlayerOwnership.BOTTOM ? 1 : -1) * (1000 + iDepth);
							break;
						default:
							Debug.LogError("End code not supported");
							break;
					}
					m_LastMoveEnded = false;
				}
				else
				{
					Game localCopy = new Game(iGame, out s_DictDummy);
					iGame.OnEnd += _OnModelEnd;
					curMoveVal = -_Negamax(localCopy, iDepth - 1, ref s_MoveDataDummy);
				}

				if(curMoveVal > value)
				{
					value = curMoveVal;
					oBestMove = move;
				}

				iGame.Rewind();

				/*if(value >= 1000)
					return value;*/
			}

			return value;
		}

		private float _Evaluate(Game iGame)
		{
			float value = 0;
			PlayerOwnership curPlayer = iGame.GetCurrentPlayer();
			for(int cellIdx = 0; cellIdx < 12; cellIdx++)
			{
				Piece piece = iGame.GetCell(cellIdx);
				if(piece == null)
					continue;

				PlayerOwnership pieceCamp = piece.GetPlayerOwnership();
				PieceType pieceType = piece.GetPieceType();
				float campMultiplier = pieceCamp == curPlayer ? 1 : -1;
				value += campMultiplier * GetPieceValue(pieceType);
				if(pieceType == PieceType.KOROPOKKURU)
				{
					if(pieceCamp == PlayerOwnership.BOTTOM && cellIdx >= 6)
						value += campMultiplier * 25;
					if(pieceCamp == PlayerOwnership.TOP && cellIdx < 6)
						value += campMultiplier * 25;
				}
			}

			int handCamp = curPlayer == PlayerOwnership.BOTTOM ? 1 : -1;
			foreach(Piece piece in iGame.GetBottomHand())
				value += handCamp * GetPieceValue(piece.GetPieceType());
			foreach(Piece piece in iGame.GetTopHand())
				value += -handCamp * GetPieceValue(piece.GetPieceType());

			return value;
		}

		private static float GetPieceValue(PieceType iPieceType)
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
			m_RealGame.MovePieces(iPiece, iPos);
		}

		// read game
		// compute best move
		// send move
	}
}
