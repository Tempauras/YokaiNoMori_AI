using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Struct;


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

        /// <summary>
        /// Retrieve all pawns of a player in the graveyard
        /// </summary>
        /// <param name="campType"></param>
        /// <returns></returns>
        public List<IPawn> GetReservePawnsByPlayer(ECampType campType);

        /// <summary>
        /// Retrieve all pawns active of a player on the board (Graveyard not included)
        /// </summary>
        /// <param name="campType"></param>
        /// <returns></returns>
        public List<IPawn> GetPawnsOnBoard(ECampType campType);

        /// <summary>
        /// Retrieve the last action
        /// </summary>
        public SAction GetLastAction();
    }
}
