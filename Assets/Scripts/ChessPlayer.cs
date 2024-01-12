using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChessPlayer
{
    public TeamColor teamColor { get; set; }
    public Board board { get; set; }
    public List<Piece> activePieces { get; set; }

    public ChessPlayer(TeamColor teamColor, Board board)
    {
        this.teamColor = teamColor;
        this.board = board;
        activePieces = new List<Piece>();
    }

    public void AddPiece(Piece piece)
    {
        if (!activePieces.Contains(piece))
            activePieces.Add(piece);
    }

    public void RemovePiece(Piece piece)
    {
        if (activePieces.Contains(piece))
            activePieces.Remove(piece);
    }

    public void RemoveAllPieces()
    {
        for(int i = 0; i < activePieces.Count; i++)
        {
            if (activePieces[i] != null)
                GameObject.Destroy(activePieces[i].gameObject);
        }
        activePieces.Clear();
    }

    public void GenerateAllPossibleMoves()
    {
        foreach (var piece in activePieces)
        {
            if (board.HasPiece(piece))
                piece.SelectAvailableSquares();
        }
    }

    public Piece[] GetPiecesAttackingOppositePieceOfType<T>() where T : Piece
    {
        return activePieces.Where(p => p.IsAttackingPieceOfType<T>()).ToArray();
    }

    public Piece[] GetPiecesOfType<T>() where T : Piece
    {
        return activePieces.Where(p => p is T).ToArray();
    }

    public void RemoveMovesEnablingAttackOnPiece<T>(ChessPlayer opponent, Piece selectedPiece) where T : Piece
    {
        List<Vector2Int> coordsToRemove = new List<Vector2Int>();

        foreach (var coords in selectedPiece.availableMoves)
        {
            Piece pieceOnSquare = board.GetPieceOnSquare(coords);
            board.UpdateBoardOnPieceMove(coords, selectedPiece.occupiedSquare, selectedPiece, null);

            opponent.GenerateAllPossibleMoves();

            if (opponent.CheckIfIsAttackingPiece<T>())
                coordsToRemove.Add(coords);

            board.UpdateBoardOnPieceMove(selectedPiece.occupiedSquare, coords, selectedPiece, pieceOnSquare);
        }

        foreach (var coords in coordsToRemove)
        {
            selectedPiece.availableMoves.Remove(coords);
        }
    }

    public bool CheckIfIsAttackingPiece<T>() where T : Piece
    {
        foreach (var piece in activePieces)
        {
            if (board.HasPiece(piece) && piece.IsAttackingPieceOfType<T>())
                return true;
        }

        return false;
    }

    internal bool CanHidePieceFromAttack<T>(ChessPlayer opponent) where T : Piece
    {
        foreach (var piece in activePieces)
        {
            foreach(var coords in piece.availableMoves)
            {
                Piece pieceOnCoords = board.GetPieceOnSquare(coords);
                board.UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
                opponent.GenerateAllPossibleMoves();
                if(!opponent.CheckIfIsAttackingPiece<T>())
                {
                    board.UpdateBoardOnPieceMove(piece.occupiedSquare, coords, piece, pieceOnCoords);
                    return true;
                }
                board.UpdateBoardOnPieceMove(piece.occupiedSquare, coords, piece, pieceOnCoords);
            }
        }
        return false;
    }
}