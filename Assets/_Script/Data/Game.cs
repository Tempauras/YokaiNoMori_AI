using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DecodeState
{
    BOARD,
    PLAYERTURN,
    HAND,
    END,
}


public struct MoveData
{
    public Piece piece;
    public int startPos;
    public int endPos;
    public Piece pieceEaten;

    public MoveData(Piece piece, int StartPos, int EndPos)
    {
        this.piece = piece;
        this.startPos = StartPos;
        this.endPos = EndPos;
        this.pieceEaten = null;
    }

    public MoveData(Piece piece, int StartPos, int EndPos, Piece pieceEaten)
    {
        this.piece = piece;
        this.startPos = StartPos;
        this.endPos = EndPos;
        this.pieceEaten = pieceEaten;
    }
}

public class ClonePieceEnumerator : IEnumerator<Piece>
{
    private IEnumerator<Piece> m_PieceEnumerator;
    private Piece m_Current = null;

    public Piece Current => m_Current;

    object IEnumerator.Current => m_Current;

    public ClonePieceEnumerator(IEnumerator<Piece> iPieceEnumerator)
    {
        m_PieceEnumerator = iPieceEnumerator;
        _Clone();
    }

    private void _Clone()
    {
        if (m_PieceEnumerator != null)
            m_Current = new Piece(m_PieceEnumerator.Current.GetPieceData(), m_PieceEnumerator.Current.GetPlayerOwnership());
        else
            m_Current = null;
    }

    public void Dispose()
    {
        m_PieceEnumerator.Dispose();
    }

    public bool MoveNext()
    {
        bool res = m_PieceEnumerator.MoveNext();
        if (!res)
            _Clone();
        return res;
    }

    public void Reset()
    {
        m_PieceEnumerator.Reset();
        _Clone();
    }
}

public class ClonePieceEnumerable : IEnumerable<Piece>
{
    private IEnumerable<Piece> m_Pieces;
    private ClonePieceEnumerator m_PieceEnumerator;

    public ClonePieceEnumerable(IEnumerable<Piece> iPieces)
    {
        m_Pieces = iPieces;
        m_PieceEnumerator = new ClonePieceEnumerator(m_Pieces.GetEnumerator());
    }

    public IEnumerator<Piece> GetEnumerator()
    {
        return m_PieceEnumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class Game
{
    private string _defaultGameStartingString = "bkr/1p1/1P1/RKB b";

    private Piece[] _gameBoard = new Piece[12];
    private List<Piece> _handPiecesTopPlayer = new List<Piece>();
    private List<Piece> _handPiecesBottomPlayer = new List<Piece>();
    private PlayerOwnership _currentPlayerTurn = PlayerOwnership.BOTTOM;

    public event Action OnInit;
    public event Action OnMovement;
    public event Action<int> OnEnd; // 0: draw ; 1: bottom win ; 2: top win

    private bool _isBottomWinningNextTurn = false;
    private bool _isTopWinningNextTurn = false;

    private List<MoveData> _movesData = new List<MoveData>();
    private int _nbRepeatedMoves = 2;

    public Game()
    {
    }

    public Game(Game copy)
    {
        // #TODO: clone piece instances
        _gameBoard = new List<Piece>(new ClonePieceEnumerable(copy._gameBoard)).ToArray();
        _handPiecesTopPlayer = new List<Piece>(new ClonePieceEnumerable(copy._handPiecesTopPlayer));
        _handPiecesBottomPlayer = new List<Piece>(new ClonePieceEnumerable(copy._handPiecesBottomPlayer));
        _currentPlayerTurn = copy._currentPlayerTurn;
        _isBottomWinningNextTurn = copy._isBottomWinningNextTurn;
        _isTopWinningNextTurn = copy._isTopWinningNextTurn;
        _nbRepeatedMoves = copy._nbRepeatedMoves;

        // cloning move data
        Dictionary<Piece, Piece> toNewPiece = new Dictionary<Piece, Piece>();
        foreach ((Piece newPiece, Piece oldPiece) in Enumerable.Zip(_gameBoard, copy._gameBoard, Tuple.Create))
            toNewPiece.Add(oldPiece, newPiece);
        foreach ((Piece newPiece, Piece oldPiece) in Enumerable.Zip(_handPiecesTopPlayer, copy._handPiecesTopPlayer, Tuple.Create))
            toNewPiece.Add(oldPiece, newPiece);
        foreach ((Piece newPiece, Piece oldPiece) in Enumerable.Zip(_handPiecesBottomPlayer, copy._handPiecesBottomPlayer, Tuple.Create))
            toNewPiece.Add(oldPiece, newPiece);

        foreach (MoveData moveData in copy._movesData)
        {
            _movesData.Add(new MoveData(
                toNewPiece[moveData.piece], moveData.startPos, moveData.endPos, toNewPiece.GetValueOrDefault(moveData.pieceEaten, null)));
        }
    }

    public void DecodeBoardStateString(string BoardStateString)
    {
        Array.Clear(_gameBoard, 0, _gameBoard.Length);
        _handPiecesBottomPlayer.Clear();
        _handPiecesTopPlayer.Clear();
        _currentPlayerTurn = PlayerOwnership.BOTTOM;
        _isBottomWinningNextTurn = false;
        _isTopWinningNextTurn = false;
        int boardNumber = 0;
        DecodeState state = DecodeState.BOARD;
        foreach (char c in BoardStateString)
        {
            int multiplier = 1;
            bool DoNotIncrement = false;
            switch (c)
            {
                case 'w':
                    if (state == DecodeState.PLAYERTURN)
                    {
                        _currentPlayerTurn = PlayerOwnership.TOP;
                    }
                    break;
                case '?':
                    if (state == DecodeState.PLAYERTURN)
                    {
                        _currentPlayerTurn = UnityEngine.Random.Range(0, 2) == 0 ? PlayerOwnership.BOTTOM : PlayerOwnership.TOP;
                    }
                    break;
                case 'b':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.Bishop, PlayerOwnership.BOTTOM);
                    }
                    else
                    {
                        if (state == DecodeState.PLAYERTURN)
                        {
                            _currentPlayerTurn = PlayerOwnership.BOTTOM;
                        }
                        else
                        {
                            _handPiecesBottomPlayer.Add(new Piece(PieceData.Bishop, PlayerOwnership.BOTTOM));
                        }
                    }

                    break;
                case 'B':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.Bishop, PlayerOwnership.TOP);
                    }
                    else
                    {
                        _handPiecesTopPlayer.Add(new Piece(PieceData.Bishop, PlayerOwnership.TOP));
                    }
                    break;
                case 'p':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.Pawn, PlayerOwnership.BOTTOM);
                    }
                    else
                    {
                        _handPiecesBottomPlayer.Add(new Piece(PieceData.Pawn, PlayerOwnership.BOTTOM));
                    }

                    break;
                case 'P':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.Pawn, PlayerOwnership.TOP);
                    }
                    else
                    {
                        _handPiecesTopPlayer.Add(new Piece(PieceData.Pawn, PlayerOwnership.TOP));
                    }

                    break;
                case 'g':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.PromotedPawn, PlayerOwnership.BOTTOM);
                    }
                    else
                    {
                        _handPiecesBottomPlayer.Add(new Piece(PieceData.PromotedPawn, PlayerOwnership.BOTTOM));
                    }

                    break;
                case 'G':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.PromotedPawn, PlayerOwnership.TOP);
                    }
                    else
                    {
                        _handPiecesTopPlayer.Add(new Piece(PieceData.PromotedPawn, PlayerOwnership.TOP));
                    }

                    break;
                case 'k':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.King, PlayerOwnership.BOTTOM);
                    }
                    else
                    {
                        _handPiecesBottomPlayer.Add(new Piece(PieceData.King, PlayerOwnership.BOTTOM));
                    }

                    break;
                case 'K':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.King, PlayerOwnership.TOP);
                    }
                    else
                    {
                        _handPiecesTopPlayer.Add(new Piece(PieceData.King, PlayerOwnership.TOP));
                    }

                    break;
                case 'r':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.Rook, PlayerOwnership.BOTTOM);
                    }
                    else
                    {
                        _handPiecesBottomPlayer.Add(new Piece(PieceData.Rook, PlayerOwnership.BOTTOM));
                    }

                    break;
                case 'R':
                    if (state == DecodeState.BOARD)
                    {
                        _gameBoard[boardNumber] = new Piece(PieceData.Rook, PlayerOwnership.TOP);
                    }
                    else
                    {
                        _handPiecesTopPlayer.Add(new Piece(PieceData.Rook, PlayerOwnership.TOP));
                    }

                    break;
                case '/':
                    DoNotIncrement = true;
                    break;
                case ' ':
                    switch (state)
                    {
                        case DecodeState.BOARD:
                            state = DecodeState.PLAYERTURN;
                            break;
                        case DecodeState.PLAYERTURN:
                            state = DecodeState.HAND;
                            break;
                        case DecodeState.HAND:
                            return;
                    }
                    break;
                case '\0':
                    break;
                default:
                    int number = (int)char.GetNumericValue(c);
                    if (number != -1)
                    {
                        multiplier = number;
                    }
                    break;
            }
            if (!DoNotIncrement)
            {
                boardNumber = boardNumber + (1 * multiplier);
            }
        }
    }

    public bool Rewind()
    {
        if (_movesData.Count == 0)
            return false;
        MoveData lastMoveData = _movesData[_movesData.Count - 1];
        _movesData.Remove(lastMoveData);
        PlayerOwnership playerOwnership = lastMoveData.piece.GetPlayerOwnership();
        if (lastMoveData.startPos == -1)
        {
            _gameBoard[lastMoveData.endPos] = null;
            if (playerOwnership == PlayerOwnership.TOP)
                _handPiecesTopPlayer.Add(lastMoveData.piece);
            else
                _handPiecesBottomPlayer.Add(lastMoveData.piece);
        }
        else
        {
            _gameBoard[lastMoveData.startPos] = lastMoveData.piece;
            if (lastMoveData.pieceEaten != null)
            {
                if (playerOwnership == PlayerOwnership.TOP)
                {
                    _handPiecesTopPlayer.Remove(lastMoveData.pieceEaten);
                    lastMoveData.pieceEaten.SetPlayerOwnership(PlayerOwnership.BOTTOM);
                }
                else
                {
                    _handPiecesBottomPlayer.Remove(lastMoveData.pieceEaten);
                    lastMoveData.pieceEaten.SetPlayerOwnership(PlayerOwnership.TOP);
                }
                _gameBoard[lastMoveData.endPos] = lastMoveData.pieceEaten;
            }
            else
            {
                _gameBoard[lastMoveData.endPos] = null;
            }
        }
        ChangeTurn();
        OnMovement?.Invoke();
        return true;
    }

    public void DispatchPieces(string BoardStateString)
    {
        DecodeBoardStateString(BoardStateString);
        _movesData.Clear();
        _nbRepeatedMoves = 2;
        OnInit?.Invoke();
    }

    public void DispatchPieces()
    {
        DispatchPieces(_defaultGameStartingString);
    }

    public bool MovePieces(Piece pieceMoving, int PositionToMoveTo, bool IsRecordingMovement = true)
    {
        if (pieceMoving == null)
            return false;

        PlayerOwnership ownerOfPiece = pieceMoving.GetPlayerOwnership();
        PieceType pieceType = pieceMoving.GetPieceData().pieceType;
        if (ownerOfPiece != _currentPlayerTurn)
            return false;

        int indexOfMovingPiece = Array.IndexOf(_gameBoard, pieceMoving);
        if (indexOfMovingPiece == -1)
            return ParachutePiece(pieceMoving, PositionToMoveTo, IsRecordingMovement);

        if (!pieceMoving.GetNeighbour(indexOfMovingPiece).Contains(PositionToMoveTo))
        {
            Debug.Log("[GameManager - MovePieces] Invalid movement, wtf happened");
            return false;
        }

        Piece prevPiece = _gameBoard[PositionToMoveTo];
        _gameBoard[indexOfMovingPiece] = null;
        _gameBoard[PositionToMoveTo] = pieceMoving;

        // pawn promotion
        if (pieceType == PieceType.KODAMA &&
            ((ownerOfPiece == PlayerOwnership.TOP) ? (PositionToMoveTo <= 2) : (PositionToMoveTo >= 9)))
        {
            pieceMoving.SetPieceType(PieceData.PromotedPawn);
        }

        // managing piece that got taken if any
        if (prevPiece != null)
        {
            PieceType prevPieceType = prevPiece.GetPieceData().pieceType;
            if (ownerOfPiece == PlayerOwnership.TOP)
                _handPiecesTopPlayer.Add(prevPiece);
            else
                _handPiecesBottomPlayer.Add(prevPiece);
            prevPiece.SetPlayerOwnership(ownerOfPiece);

            if (prevPieceType == PieceType.KODAMA_SAMURAI)
                prevPiece.SetPieceType(PieceData.Pawn);

            if (prevPieceType == PieceType.KOROPOKKURU)
            {
                OnEnd?.Invoke(ownerOfPiece == PlayerOwnership.TOP ? 2 : 1);
                return true;
            }
        }

        // opponent king had been placed on last row and we didn't capture king, lose
        if (ownerOfPiece == PlayerOwnership.BOTTOM && _isTopWinningNextTurn)
        {
            OnEnd?.Invoke(2);
            return true;
        }
        if (ownerOfPiece == PlayerOwnership.TOP && _isBottomWinningNextTurn)
        {
            OnEnd?.Invoke(1);
            return true;
        }

        // managing win condition with king on last row
        if (pieceType == PieceType.KOROPOKKURU &&
            ((ownerOfPiece == PlayerOwnership.TOP) ? (PositionToMoveTo <= 2) : (PositionToMoveTo >= 9)))
        {
            List<int> allowedEnemyMove = new List<int>();
            foreach (Piece piece in _gameBoard)
            {
                if (piece == null)
                    continue;
                if (piece.GetPlayerOwnership() != ownerOfPiece)
                    allowedEnemyMove.AddRange(AllowedMove(piece));
            }

            // can't get taken, win
            if (!allowedEnemyMove.Contains(PositionToMoveTo))
            {
                OnEnd?.Invoke(ownerOfPiece == PlayerOwnership.TOP ? 2 : 1);
                return true;
            }

            // end reported to next turn, either the king is captured, either we win
            if (ownerOfPiece == PlayerOwnership.TOP)
                _isTopWinningNextTurn = true;
            else
                _isBottomWinningNextTurn = true;
        }
        if (IsRecordingMovement)
            RecordMove(pieceMoving, indexOfMovingPiece, PositionToMoveTo, prevPiece);
        ChangeTurn();
        OnMovement?.Invoke();
        return true;
    }

    public bool ParachutePiece(Piece pieceParachuting, int PositionToParachuteTo, bool IsRecordingMovement)
    {
        PlayerOwnership ownerOfPiece = pieceParachuting.GetPlayerOwnership();
        if (ownerOfPiece != _currentPlayerTurn)
            return false;

        //Check if the piece exists in its owner hand
        List<Piece> playerHand = (ownerOfPiece == PlayerOwnership.TOP ? _handPiecesTopPlayer : _handPiecesBottomPlayer);
        bool pieceExistsInHand = playerHand.Exists(x => x == pieceParachuting);
        if (!pieceExistsInHand)
        {
            Debug.Log("[GameManager - ParachutePiece] Piece does not exist in its owner hand, wtf happened");
            return false;
        }

        //Check if the position is empty
        if (_gameBoard[PositionToParachuteTo] != null)
        {
            Debug.Log("[GameManager - ParachutePiece] Position is not empty.");
            return false;
        }

        _gameBoard[PositionToParachuteTo] = pieceParachuting;
        playerHand.Remove(pieceParachuting);
        if (IsRecordingMovement)
        {
            RecordMove(pieceParachuting, -1, PositionToParachuteTo, null);
        }
        ChangeTurn();
        OnMovement?.Invoke();
        return true;
    }

    private void RecordMove(Piece piece, int StartPos, int EndPos, Piece pieceEaten)
    {
        RecordMove(new MoveData(piece, StartPos, EndPos, pieceEaten));
    }

    private void RecordMove(MoveData move)
    {
        _movesData.Add(move);

        if (_movesData.Count <= 4)
            return;
        if (_movesData[_movesData.Count - 1].Equals(_movesData[_movesData.Count - 5]))
            _nbRepeatedMoves++;
        else
            _nbRepeatedMoves = 0;

        if (_nbRepeatedMoves >= 10)
            OnEnd?.Invoke(0);
    }

    public List<int> AllowedMove(Piece piece)
    {
        int indexOfMovingPiece = Array.IndexOf(_gameBoard, piece);
        PlayerOwnership player = piece.GetPlayerOwnership();
        List<int> availableSpace = new List<int>();
        if (indexOfMovingPiece == -1)
        {
            for (int i = 0; i < 12; i++)
            {
                Piece boardPiece = _gameBoard[i];
                if (boardPiece != null)
                    continue;
                availableSpace.Add(i);
            }
            return availableSpace;
        }

        List<int> neighbourSpaces = piece.GetNeighbour(indexOfMovingPiece);
        foreach (int neighbourSpace in neighbourSpaces)
        {
            if (_gameBoard[neighbourSpace] == null || _gameBoard[neighbourSpace].GetPlayerOwnership() != piece.GetPlayerOwnership())
            {
                availableSpace.Add(neighbourSpace);
            }
        }

        return availableSpace;
    }

    public Piece GetCell(int cellIdx)
    {
        if (cellIdx < 0 || cellIdx >= 12)
            return null;
        return _gameBoard[cellIdx];
    }

    public List<Piece> GetTopHand()
    {
        return _handPiecesTopPlayer;
    }

    public List<Piece> GetBottomHand()
    {
        return _handPiecesBottomPlayer;
    }

    public PlayerOwnership GetCurrentPlayer()
    {
        return _currentPlayerTurn;
    }

    public void SetCurrentPlayer(PlayerOwnership CurrentPlayer)
    {
        _currentPlayerTurn = CurrentPlayer;
    }

    private void ChangeTurn()
    {
        switch (_currentPlayerTurn)
        {
            case PlayerOwnership.TOP:
                _currentPlayerTurn = PlayerOwnership.BOTTOM;

                break;
            case PlayerOwnership.BOTTOM:
                _currentPlayerTurn = PlayerOwnership.TOP;
                break;
            default:
                break;
        }
    }
}
