using System;
using System.Text;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;


namespace YokaiNoMori.Struct
{
	public struct SAction
	{
		public ECampType CampType;
		public EPawnType PawnType;
		public Vector2Int StartPosition;
		public Vector2Int NewPosition;
		public IPawn TakedPawn;
		public EActionType ActionType;

		public void SetAction(ECampType camp, EPawnType pawnType, EActionType actionType, Vector2Int startPosition, Vector2Int newPosition, IPawn takedPawn)
		{
			CampType = camp;
			PawnType = pawnType;
			ActionType = actionType;
			StartPosition = startPosition;
			NewPosition = newPosition;
			TakedPawn = takedPawn;
		}

		public void ShowActionDebug()
		{
			StringBuilder debugToShow = new StringBuilder($"Joueur : {CampType} \nAction : {ActionType} \nPawn : {PawnType} \ncurrent position : {StartPosition} \nnew Position : {NewPosition} ");
			if(TakedPawn != null && ActionType == EActionType.MOVE)
				debugToShow.Append($"- Pawn captured : {TakedPawn}");

			Debug.Log(debugToShow);
		}
	}
}
