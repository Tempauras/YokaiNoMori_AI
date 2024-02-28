using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PiecesSprites", menuName = "ScriptableObjects/PiecesSprites")]
public class PiecesSprites : ScriptableObject
{
	[Serializable]
	public struct SpritePiece
	{
		public Sprite Sprite;
		public PieceType Piece;
	}

	public List<SpritePiece> Assets;
}
