using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private PuzzleChecker BoardPuzzles;
    [SerializeField] private GameObject promotionCanvas;
    [SerializeField] private GameObject puzzleCanvas;
    [SerializeField] private Text MoveText;
    [SerializeField] private GameObject boardPrefab;
    [SerializeField] private GameObject currentPuzzleUI;

    private int currentPuzzle = 0;
    private int currentCorrectMove = 0;
    public bool IsComputerTurn = false;
    [SerializeField] private int ComputerTurnTime = 1;

    private Vector3 boardPosition;
    private Quaternion boardRotation;

    private Board board;
    private PieceCreator pieceCreator;
    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer ActivePlayer;
    private GameState gameState;
    private Pawn pawnToPromote;

    private Text CurrentPuzzleText;
    private Color Invisible = new Color(1, 1, 1, 0);
    private Color CorrectMoveColor = new Color(0, 1, 0, 1);
    private Color WrongMoveColor = new Color(1, 0, 0, 1);


    public static ChessGameController Instance;


    private void Awake()
    {
        Instance = this;
        promotionCanvas.SetActive(false);
        puzzleCanvas.SetActive(false);
        MoveText.color = Invisible;
        SetDependencies();
        CurrentPuzzleText = currentPuzzleUI.GetComponent<Text>();
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
            if (board != null)
            {
                CurrentPuzzleText.text = (currentPuzzle + 1).ToString();
                SetUpBoard();
                puzzleCanvas.SetActive(true);
            }

            return;
        }


    }

    private void SetUpBoard()
    {
        CreatePlayers();
        board.SetDependencies(this);
        CreatePiecesFromLayout(BoardPuzzles.allPuzzles[currentPuzzle]);

        ActivePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(ActivePlayer);

        SetGameState(GameState.Play);
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
    private void ClearPieces()
    {
        board.ClearGrid();
    }
    private void CreatePiecesFromLayout(BoardLayout startingBoardLayout)
    {
        ClearPieces();

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
        {
            EndGame();
        }
        else
        {
            var correctMoves = BoardPuzzles.allPuzzles[currentPuzzle].correctMoves;
            if (correctMoves[currentCorrectMove].newPosition.position == board.lastMove.newPosition.position + new Vector2Int(1, 1)
                && correctMoves[currentCorrectMove].oldPosition.position == board.lastMove.oldPosition.position + new Vector2Int(1, 1)
                && correctMoves[currentCorrectMove].oldPosition.pieceType == board.lastMove.oldPosition.pieceType)
            {
                ChangeActiveTeam();
                //Enemy moves
                if (correctMoves.Count > currentCorrectMove + 1)
                {
                    StartCoroutine(DoComputerMove(correctMoves));
                }
                else
                {
                    EndGame();
                }
            }
            else
            {
                WrongMove();
            }
        }
    }

    private IEnumerator DoComputerMove(List<BoardLayout.CorrectMove> correctMoves)
    {
        IsComputerTurn = true;

        yield return new WaitForSeconds(ComputerTurnTime);
        board.ComputerMove(correctMoves[currentCorrectMove + 1]);
        ++currentCorrectMove;

        IsComputerTurn = false;
        ChangeActiveTeam();

    }
    private void WrongMove()
    {
        MoveText.text = "Wrong move!";
        MoveText.color = WrongMoveColor;
        SetGameState(GameState.Finished);
    }
    
    public void SpawnNextPuzzle()
    {
        SetGameState(GameState.Init);
        board.DeselectPiece();
        ++currentPuzzle;

        if (currentPuzzle == BoardPuzzles.allPuzzles.Count)
        {
            currentPuzzle = BoardPuzzles.allPuzzles.Count - 1;
            SetGameState(GameState.Play);
            return;
        }
        int puzzleUIDisplay = currentPuzzle + 1;
        CurrentPuzzleText.text = puzzleUIDisplay.ToString();
        MoveText.color = Invisible;
        currentCorrectMove = 0;

        Destroy(board.gameObject);

        board = Instantiate(boardPrefab, boardPosition, boardRotation).GetComponent<Board>();

        whitePlayer.RemoveAllPieces();
        blackPlayer.RemoveAllPieces();
        //board.transform.LookAt(Camera.main.transform);
        SetUpBoard();
    }

    public void RedoPuzzle()
    {
        SetGameState(GameState.Init);
        board.DeselectPiece();

        MoveText.color = Invisible;
        currentCorrectMove = 0;

        Destroy(board.gameObject);
        board = Instantiate(boardPrefab, boardPosition, boardRotation).GetComponent<Board>();

        whitePlayer.RemoveAllPieces();
        blackPlayer.RemoveAllPieces();
        //board.transform.LookAt(Camera.main.transform);
        SetUpBoard();
    }

    public void SpawnPreviousPuzzle()
    {
        SetGameState(GameState.Init);
        board.DeselectPiece();
        --currentPuzzle;

        if (currentPuzzle == -1)
        {
            currentPuzzle = 0;
            SetGameState(GameState.Play);
            return;
        }

        int puzzleUIDisplay = currentPuzzle + 1;
        CurrentPuzzleText.text = puzzleUIDisplay.ToString();
        MoveText.color = Invisible;
        currentCorrectMove = 0;

        Destroy(board.gameObject);
        board = Instantiate(boardPrefab, boardPosition, boardRotation).GetComponent<Board>();

        whitePlayer.RemoveAllPieces();
        blackPlayer.RemoveAllPieces();
        //board.transform.LookAt(Camera.main.transform);
        SetUpBoard();
    }

    private bool CheckIfGameIsFinished()
    {
        Piece[] kingAttackingPieces = ActivePlayer.GetPiecesAttackingOppositePieceOfType<King>();

        if (kingAttackingPieces.Length > 0)
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
        MoveText.text = "Correct move!";
        MoveText.color = CorrectMoveColor;
        SetGameState(GameState.Finished);
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
        ActivePlayer.RemoveMovesEnablingAttackOnPiece<T>(GetOpponentToPlayer(ActivePlayer), piece);
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

    public void SetBoardTransform(Vector3 position, Quaternion rotation)
    {
        boardPosition = position;
        boardRotation = rotation;

    }
}
