using YokaiNoMori.Enumeration;

namespace YokaiNoMori.Interface
{
    public interface ICompetitor
    {
        /// <summary>
        /// Called by the GameManager for init competitor and get the GameManager (for AI)
        /// </summary>
        /// <param name="igameManager"></param>
        /// <param name="timerForAI">Allows my Game Manager to set the time for your AI search</param>
        /// <param name="currentCamp">Allows my tournament manager to change the camp at the start of the game</param>
        public void Init(IGameManager igameManager, float timerForAI, ECampType currentCamp);


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
        /// Used before StartTurn() for getting data from the board
        /// </summary>
        public void GetDatas();

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
