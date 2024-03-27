using UnityEngine;


namespace YokaiNoMori.Interface
{
    public interface IBoardCase
    {
        /// <summary>
        /// Retrieves its position in a two-dimensional array.
        /// [0,0] being the first cell at bottom left.
        /// </summary>
        /// <returns></returns>
        public Vector2Int GetPosition();


        /// <summary>
        /// Retrieves the pawn on this case
        /// </summary>
        /// <returns></returns>
        public IPawn GetPawnOnIt();
    }
}
