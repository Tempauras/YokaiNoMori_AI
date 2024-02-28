using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        //Save these so we don't have to do the same check multiple time
        bool canMoveForward = false;
        bool canMoveBackward = false;
        bool canMoveRight = false;
        bool canMoveLeft = false;
        if (_pieceType.movementType.HasFlag(MovementType.FORWARD))
        {
            if ((_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 3 >= 0 : CurrentPos + 3 <= 11))
            {
                canMoveForward = true;
                results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 3 : CurrentPos + 3);
            }
        }
        if (_pieceType.movementType.HasFlag(MovementType.BACKWARD))
        {
            if ((_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 3 <= 11 : CurrentPos - 3 >= 0))
            {
                canMoveBackward = true;
                results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 3 : CurrentPos - 3);
            }
        }
        if (_pieceType.movementType.HasFlag(MovementType.RIGHT))
        {
            if ((_playerOwnership == PlayerOwnership.TOP ? CurrentPos % 3 != 0 : CurrentPos % 3 != 2))
            {
                canMoveRight = true;
                results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 1 : CurrentPos + 1);
            }
        }
        if (_pieceType.movementType.HasFlag(MovementType.LEFT))
        {
            if ((_playerOwnership == PlayerOwnership.TOP ? CurrentPos % 3 != 2 : CurrentPos % 3 != 0))
            {
                canMoveLeft = true;
                results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 1 : CurrentPos + 1);
            }
        }
        if (_pieceType.movementType.HasFlag(MovementType.FORWARD_RIGHT))
        {
            if (canMoveForward && canMoveRight)
            {
                results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 4 : CurrentPos + 4);
            }
        }
        if (_pieceType.movementType.HasFlag(MovementType.FORWARD_LEFT))
        {
            if (canMoveForward && canMoveLeft)
            {
                results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 2 : CurrentPos + 2);
            }
        }
        if (_pieceType.movementType.HasFlag(MovementType.BACKWARD_RIGHT))
        {
            if (canMoveBackward && canMoveRight)
            {
                results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 4 : CurrentPos - 4);
            }
        }
        if (_pieceType.movementType.HasFlag(MovementType.BACKWARD_LEFT))
        {
            if (canMoveBackward && canMoveLeft)
            {
                results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 2 : CurrentPos - 2);
            }
        }
        return results;
    }
}
