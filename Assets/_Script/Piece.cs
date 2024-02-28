using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum PlayerOwnership
{
    TOP,
    BOTTOM
}

public class Piece
{
    private PieceSO _pieceType;
    private PlayerOwnership _playerOwnership;

    public Piece(PieceSO pieceType, PlayerOwnership playerOwnership)
    {
        _pieceType = pieceType;
        _playerOwnership = playerOwnership;
    }

    public Piece()
    {
        _pieceType = null;
    }

    public PieceSO GetPieceSO()
    {
        return _pieceType;
    }
    public PlayerOwnership GetPlayerOwnership()
    {
        return _playerOwnership;
    }

    public void SetPieceSO(PieceSO newPieceType)
    {
        _pieceType = newPieceType;
    }

    public void SetPlayerOwnership(PlayerOwnership newPlayerOwnership)
    {
        _playerOwnership = newPlayerOwnership;
    }

    public List<int> GetNeightbour(int CurrentPos)
    {
        List<int> results = new List<int>();
        if (_playerOwnership == PlayerOwnership.TOP)
        {
            if (_pieceType.movementType.HasFlag(MovementType.FORWARD))
            {
                if (CurrentPos - 3 >= 0)
                {

                }
            }
            if (CurrentPos >= 9)
            {

            }
        }
        //Check is first or last row

        return results;
    }
}
