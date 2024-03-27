using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;


namespace YokaiNoMori.Interface
{
    public interface IGameManager
    {
        /// <summary>
        /// Collect all pawns on the board, including the graveyard
        /// </summary>
        /// <returns></returns>
        public List<IPawn> GetAllPawn();

        /// <summary>
        /// Recover all the squares on the board
        /// </summary>
        /// <returns></returns>
        public List<IBoardCase> GetAllBoardCase();


        /// <summary>
        /// Enable a specific type of action on a pawn
        /// </summary>
        /// <param name="pawnTarget">The pawn who must perform the action</param>
        /// <param name="position">The position, in Vector2Int, targeted</param>
        /// <param name="actionType">Type of action performed</param>
        public void DoAction(IPawn pawnTarget, Vector2Int position, EActionType actionType);

    }
}
