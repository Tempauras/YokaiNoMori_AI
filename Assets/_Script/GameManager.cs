using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public PieceSO KitsunePiece;
    public PieceSO KoropokkuruPiece;
    public PieceSO KodamaPiece;
    public PieceSO KodamaSamuraiPiece;
    public PieceSO TanukiPiece;

    private Piece[] _gameBoard = new Piece[12];
    private List<Piece> _onGameBoardPieces = new List<Piece>();
    private List<Piece> _handPiecesTopPlayer = new List<Piece>();
    private List<Piece> _handPiecesBottomPlayer = new List<Piece>();
    // Start is called before the first frame update
    void Start()
    {
        DispatchPieces();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DispatchPieces()
    {
        //Populate gameboard array and create pieces
        _gameBoard[0] = new Piece(KitsunePiece, PlayerOwnership.BOTTOM);
        _onGameBoardPieces.Add(_gameBoard[0]);

        _gameBoard[1] = new Piece(KoropokkuruPiece, PlayerOwnership.BOTTOM);
        _onGameBoardPieces.Add(_gameBoard[1]);

        _gameBoard[2] = new Piece(TanukiPiece, PlayerOwnership.BOTTOM);
        _onGameBoardPieces.Add(_gameBoard[2]);

        _gameBoard[4] = new Piece(KodamaPiece, PlayerOwnership.BOTTOM);
        _onGameBoardPieces.Add(_gameBoard[4]);

        _gameBoard[7] = new Piece(KodamaPiece, PlayerOwnership.TOP);
        _onGameBoardPieces.Add(_gameBoard[7]);

        _gameBoard[9] = new Piece(TanukiPiece, PlayerOwnership.TOP);
        _onGameBoardPieces.Add(_gameBoard[9]);

        _gameBoard[10] = new Piece(KoropokkuruPiece, PlayerOwnership.TOP);
        _onGameBoardPieces.Add(_gameBoard[10]);

        _gameBoard[11] = new Piece(KitsunePiece, PlayerOwnership.TOP);
        _onGameBoardPieces.Add(_gameBoard[11]);
    }

    private void MovePieces(Piece pieceMoving, int PositionToMoveTo)
    {
        if (_gameBoard[PositionToMoveTo].GetPieceSO())
        {
            if (pieceMoving.GetPlayerOwnership() == PlayerOwnership.TOP)
            {
                _handPiecesTopPlayer.Add(_gameBoard[PositionToMoveTo]);
            }
            else
            {
                _handPiecesBottomPlayer.Add(_gameBoard[PositionToMoveTo]);
            }
            int indexOfMovingPiece = Array.IndexOf(_gameBoard, pieceMoving);
            if (indexOfMovingPiece != -1) 
            {
                _gameBoard[indexOfMovingPiece] = new Piece();
            }
            _gameBoard[PositionToMoveTo] = pieceMoving;

        }
    }
}
