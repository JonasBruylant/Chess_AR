using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/PuzzleChecker")]
public class PuzzleChecker : ScriptableObject
{
   public List<BoardLayout> allPuzzles = new List<BoardLayout>();
}
