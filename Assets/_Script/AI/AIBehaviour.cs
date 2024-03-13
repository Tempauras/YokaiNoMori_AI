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

		private PieceSO m_KitsunePiece;
		private PieceSO m_KoropokkuruPiece;
		private PieceSO m_KodamaPiece;
		private PieceSO m_KodamaSamuraiPiece;
		private PieceSO m_TanukiPiece;

		private Game m_GameModel;

		private ECampType m_Camp = ECampType.NONE;

		public AIBehaviour(PieceSO iKitsunePiece, PieceSO iKoropokkuruPiece, PieceSO iKodamaPiece, PieceSO iKodamaSamuraiPiece, PieceSO iTanukiPiece)
		{
			m_KitsunePiece = iKitsunePiece;
			m_KoropokkuruPiece = iKoropokkuruPiece;
			m_KodamaPiece = iKodamaPiece;
			m_KodamaSamuraiPiece = iKodamaSamuraiPiece;
			m_TanukiPiece = iTanukiPiece;
		}

		#region Competitor functions
		/*public ECampType GetCamp()
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
			throw new System.NotImplementedException();
		}

		public void StopTurn()
		{
			throw new System.NotImplementedException();
		}*/
		#endregion Competitor functions

		public void SetCamp(PlayerOwnership iPlayerOwnership)
		{
			m_Camp = (iPlayerOwnership == PlayerOwnership.BOTTOM) ? ECampType.PLAYER_ONE : ECampType.PLAYER_TWO;
		}



		// read game
		// compute best move
		// send move
	}
}
