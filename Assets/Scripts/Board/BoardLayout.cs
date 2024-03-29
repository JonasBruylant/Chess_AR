using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/Board/Layout")]
public class BoardLayout : ScriptableObject
{
    [Serializable]
    public class BoardSquareSetup
    {
        public BoardSquareSetup(Vector2Int coordPos, PieceType type, TeamColor color)
        {
            position = coordPos;
            pieceType = type;
            teamColor = color;
        }
        public Vector2Int position;
        public PieceType pieceType;
        public TeamColor teamColor;
    }

    [Serializable]
    public class CorrectMove
    {
        public BoardSquareSetup oldPosition;
        public BoardSquareSetup newPosition;
    }

    [SerializeField] public List<BoardSquareSetup> boardSquares = new List<BoardSquareSetup>();
    [SerializeField] public List<CorrectMove> correctMoves = new List<CorrectMove>();

    public int GetPiecesCount()
    {
        return boardSquares.Count;
    }

    public void ClearBoardSquares()
    {
        boardSquares.Clear();
    }
    public Vector2Int GetSquareCoordsAtIndex(int index)
    {
        if (index >= boardSquares.Count)
        {
            Debug.LogError("Index of piece is out of range");
            return new Vector2Int(-1, -1);
        }

        return new Vector2Int(boardSquares[index].position.x - 1, boardSquares[index].position.y - 1);
    }

    public string GetSquarePieceNameAtIndex(int index)
    {
        if (index >= boardSquares.Count)
        {
            Debug.LogError("Index of piece is out of range");
            return "";
        }

        return boardSquares[index].pieceType.ToString();
    }

    public TeamColor GetSquareTeamColorAtIndex(int index)
    {
        if (index >= boardSquares.Count)
        {
            Debug.LogError("Index of piece is out of range");
            return TeamColor.Black;
        }

        return boardSquares[index].teamColor;
    }

    [ContextMenu("Add One")]
    public void AddOneToPos()
    {
        foreach (var square in boardSquares)
        {
            square.position += new Vector2Int(1, 1);
        }

        foreach (var correctMove in correctMoves)
        {
            correctMove.oldPosition.position += new Vector2Int(1, 1);
            correctMove.newPosition.position += new Vector2Int(1, 1);
        }
    }
}
