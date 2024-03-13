using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum MovementType
{
	FORWARD = 1 << 0,
	BACKWARD = 1 << 1,
	LEFT = 1 << 2,
	RIGHT = 1 << 3,
	FORWARD_LEFT = 1 << 4,
	FORWARD_RIGHT = 1 << 5,
	BACKWARD_LEFT = 1 << 6,
	BACKWARD_RIGHT = 1 << 7,
}

public enum PieceType
{
	KOROPOKKURU,
	KITSUNE,
	TANUKI,
	KODAMA,
	KODAMA_SAMURAI
}

public struct PieceData
{
	public readonly MovementType movementType;
	public readonly PieceType pieceType;
	private PieceData(PieceType iPieceType, MovementType iMovementType)
	{
		movementType = iMovementType;
		pieceType = iPieceType;
	}

	public static readonly PieceData Pawn;
	public static readonly PieceData PromotedPawn;
	public static readonly PieceData Rook;
	public static readonly PieceData Bishop;
	public static readonly PieceData King;

	static PieceData()
	{
		Pawn = new PieceData(PieceType.KODAMA, MovementType.FORWARD);

		PromotedPawn = new PieceData
		(
			PieceType.KODAMA_SAMURAI,
			MovementType.FORWARD | MovementType.BACKWARD |
				MovementType.LEFT | MovementType.RIGHT |
				MovementType.FORWARD_LEFT | MovementType.FORWARD_RIGHT
		);

		Rook = new PieceData
		(
			PieceType.TANUKI,
			MovementType.FORWARD | MovementType.BACKWARD | MovementType.LEFT | MovementType.RIGHT
		);

		Bishop = new PieceData
		(
			PieceType.KITSUNE,
			MovementType.FORWARD_LEFT | MovementType.FORWARD_RIGHT | MovementType.BACKWARD_LEFT | MovementType.BACKWARD_RIGHT
		);

		King = new PieceData
		(
			PieceType.KOROPOKKURU,
			MovementType.FORWARD | MovementType.BACKWARD | MovementType.LEFT | MovementType.RIGHT |
			MovementType.FORWARD_LEFT | MovementType.FORWARD_RIGHT | MovementType.BACKWARD_LEFT | MovementType.BACKWARD_RIGHT
		);
	}
}

