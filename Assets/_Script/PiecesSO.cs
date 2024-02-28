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

[CreateAssetMenu(fileName = "Pieces", menuName = "ScriptableObjects/Pieces")]
public class PiecesSO : ScriptableObject
{
    public MovementType movementType;
    public PieceType pieceType;
}
