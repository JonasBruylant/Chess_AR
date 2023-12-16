using System;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardInputHandler : MonoBehaviour, IInputHandler
{
    private Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }

    public void ProcessInput(Vector3 inputPosition, GameObject selectedGameObject, Action callback)
    {
        board.OnSquareSelected(inputPosition);
    }
}
