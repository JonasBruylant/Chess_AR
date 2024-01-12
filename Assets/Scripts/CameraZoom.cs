using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    float initialDistance;
    GameObject currentBoard;

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount == 2 && currentBoard != null)
        {
            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled ||
                touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
                return;

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
            }
            else
            {
                float currentDistance = Vector2.Distance(touchZero.position,touchOne.position);

                if (Mathf.Approximately(initialDistance, 0))
                    return;

                float scaleFactor = currentDistance / initialDistance;

                Vector3 vectorDifference = Camera.main.transform.position - currentBoard.transform.position;

                Vector3 scaledVector = scaleFactor * vectorDifference;

                Camera.main.transform.position = currentBoard.transform.position + scaledVector;
            }
        }
    }

    public void SetBoardReference(GameObject board)
    {
        currentBoard = board;
    }
}
