using UnityEngine;
using UnityEngine.UI;

public class GetCoordinatesOnGrid : MonoBehaviour
{
    [SerializeField] GameObject ChessBoard;
    [SerializeField] Text CoordsText;

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
        if (!Input.GetMouseButtonUp(0))
            return;

        CheckGridOnMousePosition();
    }

    void CheckGridOnMousePosition()
    {
        for (int CurrentRow = 0; CurrentRow < BoardSize; ++CurrentRow)
        {
            for (int CurrentColumn = 0; CurrentColumn < BoardSize; ++CurrentColumn)
            {
                Vector2 bottomLeftSquare = Grid[CurrentRow, CurrentColumn];

                if (Input.mousePosition.x < bottomLeftSquare.x || Input.mousePosition.x > bottomLeftSquare.x + SquareSize)
                    continue;

                if (Input.mousePosition.y < bottomLeftSquare.y || Input.mousePosition.y > bottomLeftSquare.y + SquareSize)
                    continue;

                SelectedCoords = Coordinates[CurrentColumn, CurrentRow];

                CoordsText.text = SelectedCoords.ToString();
            }
        }
    }
}
