#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PieceSelector : MonoBehaviour
{
    [SerializeField] GameObject PuzzleName;
    [SerializeField] GameObject ChessBoard;
    [SerializeField] GameObject TempImages;
    [SerializeField] GameObject InstantiateObject;
    [SerializeField] Text CoordsText;
    [SerializeField] Sprite pieceSprite;
    [SerializeField] SpriteDictionary spriteDictionary;

    public Text CurrentSelectedPiece;
    public BoardLayout template;

    PieceType pieceType;
    TeamColor teamColor;

    const int BoardSize = 8;
    float SquareSize;
    Vector2[,] Grid;
    Vector2Int[,] Coordinates;
    Vector2Int SelectedCoords;


    // Start is called before the first frame update
    void Start()
    {
        var rectTransform = ChessBoard.GetComponent<RectTransform>();

        Grid = new Vector2[BoardSize, BoardSize];
        Coordinates = new Vector2Int[BoardSize, BoardSize];

        SquareSize = rectTransform.sizeDelta.y / BoardSize;
        float xPosition = rectTransform.position.x - rectTransform.sizeDelta.x * 0.5f;
        float yPosition = rectTransform.position.y - rectTransform.sizeDelta.y * 0.5f;
        Vector2 BottomLeft = new Vector2(xPosition, yPosition);

        for (int CurrentRow = 0; CurrentRow < BoardSize; ++CurrentRow)
        {
            for (int CurrentColumn = 0; CurrentColumn < BoardSize; ++CurrentColumn)
            {
                float squareX = BottomLeft.x + CurrentColumn * SquareSize;
                float squareY = BottomLeft.y + CurrentRow * SquareSize;

                Grid[CurrentRow, CurrentColumn] = new Vector2(squareX, squareY);
                Coordinates[CurrentRow, CurrentColumn] = new Vector2Int(CurrentRow, CurrentColumn);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (template == null)
            return;

        if (Input.GetMouseButtonUp(0))
            CheckGridOnMousePosition();
        else if (Input.GetMouseButtonUp(1))
            ClearPieceAtCoordinate();


    }

    private void ClearPieceAtCoordinate()
    {
        var rectTransform = ChessBoard.GetComponent<RectTransform>();
        float leftPosition = rectTransform.position.x - rectTransform.sizeDelta.x * 0.5f;
        float bottomPosition = rectTransform.position.y - rectTransform.sizeDelta.y * 0.5f;
        Vector2 bottomLeftPosition = new Vector2(leftPosition, bottomPosition);

        int x = (int)((Input.mousePosition.x - bottomLeftPosition.x) / SquareSize);
        var y = (int)((Input.mousePosition.y - bottomLeftPosition.y) / SquareSize);

        //Not on board
        if (x >= BoardSize || x < 0 || y >= BoardSize || y < 0)
            return;

        SelectedCoords = Coordinates[x, y];
        string coordinateName = $"{x} {y}";
        for (int i = 0; i < TempImages.transform.childCount; ++i)
        {
            if (TempImages.transform.GetChild(i).gameObject.name.Equals(coordinateName))
                Destroy(TempImages.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < template.boardSquares.Count; i++)
        {
            if (template.boardSquares[i].position.x == x+1 && template.boardSquares[i].position.y == y+1)
                template.boardSquares.Remove(template.boardSquares[i]);
        }
    }

    void CheckGridOnMousePosition()
    {
        var rectTransform = ChessBoard.GetComponent<RectTransform>();
        float leftPosition = rectTransform.position.x - rectTransform.sizeDelta.x * 0.5f;
        float bottomPosition = rectTransform.position.y - rectTransform.sizeDelta.y * 0.5f;
        Vector2 bottomLeftPosition = new Vector2(leftPosition, bottomPosition);

        float fX = ((Input.mousePosition.x - bottomLeftPosition.x) / SquareSize);
        float fY = ((Input.mousePosition.y - bottomLeftPosition.y) / SquareSize);

        //Not on board
        if (fX >= BoardSize || fX < 0 || fY >= BoardSize || fY < 0)
            return;

        int x = (int)fX;
        int y = (int)fY;
        
        SelectedCoords = Coordinates[x, y];

        CoordsText.text = SelectedCoords.ToString();
        Vector2 canvasPosition = PositionFromCoords(bottomLeftPosition, new Vector2Int(x, y));
        var instantiatedImage = Instantiate(InstantiateObject,
            new Vector3(canvasPosition.x + (SquareSize * 0.5f), canvasPosition.y + (SquareSize * 0.5f), 0),
            Quaternion.identity,
            TempImages.transform);

        string coordinateName = $"{x} {y}";

        for (int i = 0; i < TempImages.transform.childCount; ++i)
        {
            if (TempImages.transform.GetChild(i).gameObject.name.Equals(coordinateName))
                Destroy(TempImages.transform.GetChild(i).gameObject);
        }
        instantiatedImage.name = coordinateName;
        instantiatedImage.GetComponent<Image>().sprite = pieceSprite;

        for (int i = 0; i < template.boardSquares.Count; i++)
        {
            if (template.boardSquares[i].position.x == x + 1 && template.boardSquares[i].position.y == y + 1)
                template.boardSquares.Remove(template.boardSquares[i]);
        }

        template.boardSquares.Add(new BoardLayout.BoardSquareSetup(SelectedCoords + new Vector2Int(1,1), pieceType, teamColor));

    }

    Vector2 PositionFromCoords(Vector2 startPosition, Vector2Int coords)
    {
        return startPosition + (new Vector2(coords.x, coords.y) * SquareSize);
    }

    public void OnPieceSelected(int pieceIndex)
    {
        if (pieceIndex >= 10)
            teamColor = TeamColor.White;
        else
            teamColor = TeamColor.Black;

        pieceType = (PieceType)(pieceIndex % 10);

        CurrentSelectedPiece.text = $"{teamColor} {pieceType}";

        pieceSprite = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>().image.sprite;
    }

    public void SaveLayout()
    {

        TMP_InputField tmpComp;
        if (!PuzzleName.TryGetComponent(out tmpComp))
            return;

        string puzzleText = tmpComp.text;
        
        if (puzzleText == "")
            return;



        for (int i = 0; i < TempImages.transform.childCount; ++i)
        {
            Destroy(TempImages.transform.GetChild(i).gameObject);
        }

        if (template != null)
            EditorUtility.SetDirty(template);


        template = ScriptableObject.CreateInstance<BoardLayout>();
        string puzzleName = puzzleText.Replace(" ", "");
        AssetDatabase.CreateAsset(template, $"Assets/Data/{puzzleName}.asset");
        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
    }

    private void OnValidate()
    {
        if (template == null)
            return;

        LoadBoardLayout(template);
    }

    private void LoadBoardLayout(BoardLayout template)
    {
        for (int i = 0; i < TempImages.transform.childCount; ++i)
        {
            Destroy(TempImages.transform.GetChild(i).gameObject);
        }

        //template.boardSquares.Add(new BoardLayout.BoardSquareSetup(SelectedCoords, pieceType, teamColor));

        var rectTransform = ChessBoard.GetComponent<RectTransform>();
        float leftPosition = rectTransform.position.x - rectTransform.sizeDelta.x * 0.5f;
        float bottomPosition = rectTransform.position.y - rectTransform.sizeDelta.y * 0.5f;
        Vector2 bottomLeftPosition = new Vector2(leftPosition, bottomPosition);
        int x, y;
        
        for (int i = 0; i < template.boardSquares.Count; ++i)
        {
            x = template.boardSquares[i].position.x - 1;
            y = template.boardSquares[i].position.y - 1;

            Vector2 canvasPosition = PositionFromCoords(bottomLeftPosition, new Vector2Int(x, y));
            var instantiatedImage = Instantiate(InstantiateObject,
                new Vector3(canvasPosition.x + (SquareSize * 0.5f), canvasPosition.y + (SquareSize * 0.5f), 0),
                Quaternion.identity,
                TempImages.transform);

            string coordinateName = $"{x} {y}";
            instantiatedImage.name = coordinateName;
            
            Sprite imageSprite = spriteDictionary.GetSprite(template.boardSquares[i].teamColor, template.boardSquares[i].pieceType);
            instantiatedImage.GetComponent<Image>().sprite = imageSprite;
        }
    }

    private void OnDestroy()
    {
        EditorUtility.SetDirty(template);
        AssetDatabase.SaveAssets();
    }

    public void ClearBoard()
    {
        for (int i = 0; i < TempImages.transform.childCount; ++i)
        {
            Destroy(TempImages.transform.GetChild(i).gameObject);
        }

        template.boardSquares.Clear();
    }

}
#endif