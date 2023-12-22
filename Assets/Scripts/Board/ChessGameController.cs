using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PieceCreator))]
public class ChessGameController : MonoBehaviour
{
    private enum GameState
    {
        Init,
        Play,
        Finished
    };

    [SerializeField] private List<BoardLayout> Puzzles;
    [SerializeField] private GameObject promotionCanvas;
    private int puzzleListIndex = 0;

    private Board board;
    private PieceCreator pieceCreator;
    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer ActivePlayer;
    private GameState gameState;
    private Pawn pawnToPromote;



    public static ChessGameController Instance;


    private void Awake()
    {
        Instance = this;
        promotionCanvas.SetActive(false);
        SetDependencies();
    }

    private void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }

    private void SetDependencies()
    {
        pieceCreator = GetComponent<PieceCreator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartNewGame();
    }

    private void Update()
    {

        if (board == null)
        {
            GameObject[] chessboards = GameObject.FindGameObjectsWithTag("ChessBoard");
            if (chessboards.Length <= 0)
                return;

            board = GameObject.Find("PF_ChessBoard(Clone)").GetComponent<Board>();
            if(board != null)
                SetUpBoard();

            return;
        }


    }

    private void SetUpBoard()
    {
        CreatePlayers();
        board.SetDependencies(this);
        CreatePiecesFromLayout(Puzzles[puzzleListIndex]);

        ActivePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(ActivePlayer);
    }

    private void GenerateAllPossiblePlayerMoves(ChessPlayer activePlayer)
    {
        activePlayer.GenerateAllPossibleMoves();
    }

    private void StartNewGame()
    {
        SetGameState(GameState.Init);

        SetGameState(GameState.Play);
    }
    public bool IsGameInProgress()
    {
        return gameState == GameState.Play;
    }

    private void SetGameState(GameState state)
    {
        this.gameState = state;
    }

    private void CreatePiecesFromLayout(BoardLayout startingBoardLayout)
    {
        if (startingBoardLayout.GetPiecesCount() > 0)
            startingBoardLayout.ClearBoardSquares();

        for (int i = 0; i < startingBoardLayout.GetPiecesCount(); ++i)
        {
            Vector2Int squareCoords = startingBoardLayout.GetSquareCoordsAtIndex(i);
            TeamColor teamColor = startingBoardLayout.GetSquareTeamColorAtIndex(i);
            string typeName = startingBoardLayout.GetSquarePieceNameAtIndex(i);

            Type type = Type.GetType(typeName);
            CreateAndInitialize(squareCoords, teamColor, type);
        }
    }

    public void CreateAndInitialize(Vector2Int squareCoords, TeamColor teamColor, Type type)
    {
        Piece newPiece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        newPiece.SetData(squareCoords, teamColor, board);

        newPiece.transform.localScale = board.transform.localScale * 5f;

        Material teamMaterial = pieceCreator.GetTeamMaterial(teamColor);
        newPiece.SetMaterial(teamMaterial);

        board.SetPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = teamColor == TeamColor.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }

    public bool IsTeamTurnActive(TeamColor team)
    {
        return ActivePlayer.teamColor == team;
    }

    public void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(ActivePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(ActivePlayer));

        if (CheckIfGameIsFinished())
            EndGame(); //Spawn next puzzle
        else
            ChangeActiveTeam();
    }

    private bool CheckIfGameIsFinished()
    {
        Piece[] kingAttackingPieces = ActivePlayer.GetPiecesAttackingOppositePieceOfType<King>();

        if(kingAttackingPieces.Length > 0)
        {
            ChessPlayer oppositePlayer = GetOpponentToPlayer(ActivePlayer);
            Piece attackedKing = oppositePlayer.GetPiecesOfType<King>().FirstOrDefault();
            oppositePlayer.RemoveMovesEnablingAttackOnPiece<King>(ActivePlayer, attackedKing);

            int availableKingMoves = attackedKing.availableMoves.Count;
            if (availableKingMoves == 0)
            {
                bool CanCoverKing = oppositePlayer.CanHidePieceFromAttack<King>(ActivePlayer);
                if (!CanCoverKing)
                    return true;
            }
        }
        return false;
    }

    private void EndGame()
    {
        ++puzzleListIndex;
        SetUpBoard();


        //Debug.Log("Game Ended");
        //SetGameState(GameState.Finished);
    }

    private void ChangeActiveTeam()
    {
        ActivePlayer = ActivePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer activePlayer)
    {
        return activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    public void RemoveMovesEnablingAttackOnPieceOfType<T>(Piece piece) where T : Piece
    {
        ActivePlayer.RemoveMovesEnablingAttackOnPiece<T>(GetOpponentToPlayer(ActivePlayer),piece);
    }

    public void OnPieceRemoved(Piece piece)
    {
        ChessPlayer pieceOwner = (piece.team == TeamColor.White) ? whitePlayer : blackPlayer;
        pieceOwner.RemovePiece(piece);

        Destroy(piece.gameObject);
    }

    public void PawnWantsToPromote(Pawn piece)
    {
        pawnToPromote = piece;
        promotionCanvas.SetActive(true);
    }

    public void PromoteTo(int pieceType)
    {
        board.PromotePiece(pawnToPromote, (PieceType)pieceType);

        promotionCanvas.SetActive(false);
    }
}
