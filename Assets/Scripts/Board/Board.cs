using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SquareSelectorCreator))]
public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;


    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;
    CameraZoom cameraZoom;

    public BoardLayout.CorrectMove lastMove;
    
    private Piece[,] grid;
    private Piece selectedPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;

    private void Awake()
    {
        //squareSize *= transform.localScale.x;
        squareSelector = GetComponent<SquareSelectorCreator>();

        cameraZoom = GameObject.Find("CameraZoom").GetComponent<CameraZoom>();
        cameraZoom.SetBoardReference(this.gameObject);
        CreateGrid();
    }

    public void SetDependencies(ChessGameController chessGameController)
    {
        this.chessController = chessGameController;
    }

    void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    public Vector3 CalculatePositionFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize * transform.localScale.x, 0f, coords.y * squareSize * transform.localScale.z);
    }

    public void OnSquareSelected(Vector3 inputPosition)
    {
        if (!chessController.IsGameInProgress())
            return;

        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);

        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
                DeselectPiece();
            else if (piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team))
            {
                SelectPiece(piece);
                lastMove.oldPosition.position = coords;
                lastMove.oldPosition.teamColor = selectedPiece.team;
                lastMove.oldPosition.pieceType = GetPieceType(selectedPiece);
            }
            else if (selectedPiece.CanMoveTo(coords))
            {
                lastMove.newPosition.position = coords;
                lastMove.newPosition.teamColor = selectedPiece.team;
                lastMove.newPosition.pieceType = GetPieceType(selectedPiece);

                OnSelectedPieceMoved(coords, selectedPiece);
            }
        }
        else
        {
            if (piece != null && chessController.IsTeamTurnActive(piece.team))
            {
                SelectPiece(piece);
                lastMove.oldPosition.position = coords;
                lastMove.oldPosition.teamColor = selectedPiece.team;
                lastMove.oldPosition.pieceType = GetPieceType(selectedPiece);
            }
        }
    }

    private void OnSelectedPieceMoved(Vector2Int coords, Piece piece)
    {
        TryToTakeOppositePiece(coords);
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(coords);

        DeselectPiece();

        EndTurn();
    }

    private void TryToTakeOppositePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece != null && !selectedPiece.IsFromSameTeam(piece))
            TakePiece(piece);
    }

    private void TakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            chessController.OnPieceRemoved(piece);
        }
    }

    private void EndTurn()
    {
        chessController.EndTurn();
    }

    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newCoords.x, newCoords.y] = newPiece;
    }

    private void SelectPiece(Piece piece)
    {
        chessController.RemoveMovesEnablingAttackOnPieceOfType<King>(piece);
        selectedPiece = piece;
        List<Vector2Int> selection = selectedPiece.availableMoves;

        ShowSelectionSquares(selection);
    }

    private void ShowSelectionSquares(List<Vector2Int> selection)
    {
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();

        for (int i = 0; i < selection.Count; ++i)
        {
            Vector3 position = CalculatePositionFromCoords(selection[i]);
            bool isSquareFree = GetPieceOnSquare(selection[i]) == null;

            squaresData.Add(position, isSquareFree);
        }

        squareSelector.ShowSelection(squaresData);
    }

    public void DeselectPiece()
    {
        selectedPiece = null;
        squareSelector.ClearSelection();
    }

    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        if (CheckIfCoordinatesAreOnBoard(coords))
            return grid[coords.x, coords.y];

        return null;
    }

    public bool CheckIfCoordinatesAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
            return false;

        return true;
    }

    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition)
    {
        int x = 7 - (Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).x / squareSize) + BOARD_SIZE / 2);
        int y = 7 - (Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).z / squareSize) + BOARD_SIZE / 2);

        return new Vector2Int(x, y);
    }

    public bool HasPiece(Piece piece)
    {
        for (int i = 0; i < BOARD_SIZE; ++i)
        {
            for (int j = 0; j < BOARD_SIZE; ++j)
            {
                if (grid[i, j] == piece)
                    return true;
            }
        }
        return false;
    }

    internal void SetPieceOnBoard(Vector2Int squareCoords, Piece newPiece)
    {
        if (CheckIfCoordinatesAreOnBoard(squareCoords))
            grid[squareCoords.x, squareCoords.y] = newPiece;
    }

    public void PromotePiece(Pawn piece, PieceType pieceType)
    {
        TakePiece(piece);
        switch (pieceType)
        {
            case PieceType.Queen:
                {
                    chessController.CreateAndInitialize(piece.occupiedSquare, piece.team, typeof(Queen));
                    break;
                }
            case PieceType.Knight:
                {
                    chessController.CreateAndInitialize(piece.occupiedSquare, piece.team, typeof(Knight));
                    break;
                }
            case PieceType.Rook:
                {
                    chessController.CreateAndInitialize(piece.occupiedSquare, piece.team, typeof(Rook));
                    break;
                }
            case PieceType.Bishop:
                {
                    chessController.CreateAndInitialize(piece.occupiedSquare, piece.team, typeof(Bishop));
                    break;
                }
            default:
                {
                    chessController.CreateAndInitialize(piece.occupiedSquare, piece.team, typeof(Queen));
                    break;
                }
        }
    }

    PieceType GetPieceType(Piece piece)
    {
        Rook rook = piece as Rook;
        if (rook != null)
            return PieceType.Rook;

        Queen queen = piece as Queen;
        if (queen != null)
            return PieceType.Queen;

        Bishop bishop = piece as Bishop;
        if (bishop != null)
            return PieceType.Bishop;

        Knight knight = piece as Knight;
        if (knight != null)
            return PieceType.Knight;

        Pawn pawn = piece as Pawn;
        if (pawn != null)
            return PieceType.Pawn;

        King king = piece as King;
        if (king != null)
            return PieceType.King;

        return PieceType.Pawn;
    }

    public void ComputerMove(BoardLayout.CorrectMove correctMove)
    {
        var coords = correctMove.oldPosition.position - new Vector2Int(1, 1);
        var newCoords = correctMove.newPosition.position - new Vector2Int(1, 1);

        Piece piece = GetPieceOnSquare(coords);
        SelectPiece(piece);

        TryToTakeOppositePiece(newCoords);
        UpdateBoardOnPieceMove(newCoords, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(newCoords);

        DeselectPiece();


        if (correctMove.newPosition.pieceType != correctMove.oldPosition.pieceType)
        {
            PromotePiece(piece as Pawn, correctMove.newPosition.pieceType);
        }
    }

    public void ClearGrid()
    {
        for (int i = 0; i < BOARD_SIZE; ++i)
        {
            for (int j = 0; j < BOARD_SIZE; ++j)
            {
                if (grid[i,j] != null)
                    chessController.OnPieceRemoved(grid[i,j]);
            }
        }
    }
}
