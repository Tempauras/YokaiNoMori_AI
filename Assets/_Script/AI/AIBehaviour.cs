using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

namespace YokaiNoMori.Coffee
{
	public class AIBehaviour // : ICompetitor
	{
		private const string s_Name = "YoshihAIruHabu";

		private Game m_RealGame;
		private Game m_GameModel;

		private ECampType m_Camp = ECampType.NONE;
		private PlayerOwnership m_PlayerOwnership;

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
			MoveRandom();
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

		private void MoveRandom()
		{
			List<KeyValuePair<Piece, int>> moves = GetAllMoves();
			Move(moves[Random.Range(0, moves.Count)]);
		}

		private float Evaluate()
		{
			return 0;
		}

		private List<KeyValuePair<Piece, int>> GetAllMoves()
		{
			List<KeyValuePair<Piece, int>> moves = new List<KeyValuePair<Piece, int>>();
			for(int cellIdx = 0; cellIdx < 12; cellIdx++)
			{
				Piece piece = m_RealGame.GetCell(cellIdx);
				if(piece == null || piece.GetPlayerOwnership() != m_PlayerOwnership)
					continue;
				moves.AddRange(GetAllMovesForPiece(piece));
			}
			foreach(Piece piece in (m_PlayerOwnership == PlayerOwnership.BOTTOM ? m_RealGame.GetBottomHand() : m_RealGame.GetTopHand()))
				moves.AddRange(GetAllMovesForPiece(piece));

			return moves;
		}

		private List<KeyValuePair<Piece, int>> GetAllMovesForPiece(Piece iPiece)
		{
			List<KeyValuePair<Piece, int>> moves = new List<KeyValuePair<Piece, int>>();
			foreach(int pos in m_RealGame.AllowedMove(iPiece))
				moves.Add(KeyValuePair.Create(iPiece, pos));
			return moves;
		}

		private void Move(KeyValuePair<Piece, int> iMove)
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
