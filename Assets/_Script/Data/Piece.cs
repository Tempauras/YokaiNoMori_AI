using System.Collections.Generic;

public enum PlayerOwnership
{
	TOP,
	BOTTOM
}

public class Piece
{
	private PieceData _pieceType;
	private PlayerOwnership _playerOwnership;

	public Piece(PieceData pieceType, PlayerOwnership playerOwnership)
	{
		_pieceType = pieceType;
		_playerOwnership = playerOwnership;
	}
	public Piece(Piece iCopy)
	{
		_pieceType = iCopy._pieceType;
		_playerOwnership = iCopy._playerOwnership;
	}

	public PieceData GetPieceData()
	{
		return _pieceType;
	}

	public PieceType GetPieceType()
	{
		return _pieceType.pieceType;
	}

	public PlayerOwnership GetPlayerOwnership()
	{
		return _playerOwnership;
	}

	public void SetPieceType(PieceData newPieceType)
	{
		_pieceType = newPieceType;
	}

	public void SetPlayerOwnership(PlayerOwnership newPlayerOwnership)
	{
		_playerOwnership = newPlayerOwnership;
	}

	public List<int> GetNeighbour(int CurrentPos)
	{
		List<int> results = new List<int>();
		//Save these so we don't have to do the same check multiple time
		bool canMoveForward = false;
		bool canMoveBackward = false;
		bool canMoveRight = false;
		bool canMoveLeft = false;

		if(_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 3 >= 0 : CurrentPos + 3 <= 11)
		{
			canMoveForward = true;

			if(_pieceType.movementType.HasFlag(MovementType.FORWARD))
			{
				results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 3 : CurrentPos + 3);
			}
		}
		if(_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 3 <= 11 : CurrentPos - 3 >= 0)
		{
			canMoveBackward = true;
			if(_pieceType.movementType.HasFlag(MovementType.BACKWARD))
			{
				results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 3 : CurrentPos - 3);
			}
		}
		if(_playerOwnership == PlayerOwnership.TOP ? CurrentPos % 3 != 0 : CurrentPos % 3 != 2)
		{
			canMoveRight = true;
			if(_pieceType.movementType.HasFlag(MovementType.RIGHT))
			{
				results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 1 : CurrentPos + 1);
			}
		}
		if(_playerOwnership == PlayerOwnership.TOP ? CurrentPos % 3 != 2 : CurrentPos % 3 != 0)
		{
			canMoveLeft = true;
			if(_pieceType.movementType.HasFlag(MovementType.LEFT))
			{
				results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 1 : CurrentPos - 1);
			}
		}
		if(_pieceType.movementType.HasFlag(MovementType.FORWARD_RIGHT))
		{
			if(canMoveForward && canMoveRight)
			{
				results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 4 : CurrentPos + 4);
			}
		}
		if(_pieceType.movementType.HasFlag(MovementType.FORWARD_LEFT))
		{
			if(canMoveForward && canMoveLeft)
			{
				results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos - 2 : CurrentPos + 2);
			}
		}
		if(_pieceType.movementType.HasFlag(MovementType.BACKWARD_RIGHT))
		{
			if(canMoveBackward && canMoveRight)
			{
				results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 2 : CurrentPos - 2);
			}
		}
		if(_pieceType.movementType.HasFlag(MovementType.BACKWARD_LEFT))
		{
			if(canMoveBackward && canMoveLeft)
			{
				results.Add(_playerOwnership == PlayerOwnership.TOP ? CurrentPos + 4 : CurrentPos - 4);
			}
		}
		return results;
	}
}
