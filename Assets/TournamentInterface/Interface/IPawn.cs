using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

namespace YokaiNoMori.Interface
{
    public interface IPawn
    {
        /// <summary>
        /// Recovers all moves of the pawn concerned
        /// </summary>
        /// <returns>All possible directions (e.g. [0.1] for forward)</returns>
        public List<Vector2Int> GetDirections();

        /// <summary>
        /// Retrieves the owner of this pawn
        /// </summary>
        /// <returns>1 : Player 1 - 2 : Player 2</returns>
        public ICompetitor GetCurrentOwner();

        /// <summary>
        /// Retrieve the square on which the pawn is located
        /// </summary>
        /// <returns>null is no case</returns>
        public IBoardCase GetCurrentBoardCase();


        /// <summary>
        /// Retrieve pawn type
        /// </summary>
        /// <returns></returns>
        public EPawnType GetPawnType();

    }

}