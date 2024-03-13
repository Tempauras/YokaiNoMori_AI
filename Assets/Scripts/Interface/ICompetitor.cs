using YokaiNoMori.Enumeration;

namespace YokaiNoMori.Interface
{

    public interface ICompetitor
    {
        /// <summary>
        /// Used by my UI
        /// </summary>
        /// <returns>Returns the name of this competitor's creator group</returns>
        public string GetName();

        /// <summary>
        /// Recovering the competitor's camp
        /// </summary>
        /// <returns></returns>
        public ECampType GetCamp();

        /// <summary>
        /// Allows my tournament manager to change the camp at the start of the game
        /// </summary>
        /// <param name="camp"></param>
        public void SetCamp(ECampType camp);


        /// <summary>
        /// Called by the Game Manager to warn the competitor that it's his turn.
        /// </summary>
        public void StartTurn();

        /// <summary>
        /// Called by the Game Manager to warn the competitor that his turn is over.
        /// </summary>
        public void StopTurn();
    }

}
