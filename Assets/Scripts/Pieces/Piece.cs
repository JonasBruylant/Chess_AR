using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IObjectTweener))]
[RequireComponent(typeof(MaterialSetter))]
public abstract class Piece : MonoBehaviour
{
    MaterialSetter materialSetter;
    public Board board { protected get; set; }
    public Vector2Int occupiedSquare { get; set; }
    public TeamColor team { get; set; }
    public bool hasMoved { get; private set; }
    public List<Vector2Int> availableMoves;

    private IObjectTweener tweener;

    public abstract List<Vector2Int> SelectAvailableSquares();

    private void Awake()
    {
        availableMoves = new List<Vector2Int>();
        tweener = GetComponent<IObjectTweener>();
        materialSetter = GetComponent<MaterialSetter>();

        hasMoved = false;
    }

    public void SetMaterial(Material material)
    {
        if (!materialSetter)
            materialSetter = GetComponent<MaterialSetter>();


        materialSetter.SetSingleMaterial(material);
    }

    public bool IsFromSameTeam(Piece piece)
    {
        return team == piece.team;
    }
    public bool CanMoveTo(Vector2Int coords)
    {
        return availableMoves.Contains(coords);
    }

    public virtual void MovePiece(Vector2Int coords)
    {
        Vector3 targetPosition = board.CalculatePositionFromCoords(coords);
        occupiedSquare = coords;
        hasMoved = true;
        tweener.MoveTo(transform, targetPosition);
    }

    protected void TryToAddMove(Vector2Int coords)
    {
        availableMoves.Add(coords);
    }

    public void SetData(Vector2Int coords, TeamColor team, Board board)
    {
        this.team = team;
        occupiedSquare = coords;
        this.board = board;
        transform.position = board.CalculatePositionFromCoords(coords);
    }

    public bool IsAttackingPieceOfType<T>() where T : Piece
    {
        foreach (var square in availableMoves)
        {
            if (board.GetPieceOnSquare(square) is T)
                return true;
        }

        return false;
    }

    protected Piece GetPieceInDirection<T>(TeamColor team, Vector2Int direction) where T : Piece
    {
        for (int i = 1; i <= Board.BOARD_SIZE; ++i)
        {
            Vector2Int nextCoords = occupiedSquare + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);

            if (!board.CheckIfCoordinatesAreOnBoard(nextCoords))
                return null;

            if(piece != null)
            {
                if (piece.team != team || !(piece is T))
                    return null;
                else if (piece.team == team && piece is T)
                    return piece;
            }
        }
        return null;
    }
}
