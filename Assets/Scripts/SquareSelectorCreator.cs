using System.Collections.Generic;
using UnityEngine;

public class SquareSelectorCreator : MonoBehaviour
{
    [SerializeField] private Material freeSquareMaterial;
    [SerializeField] private Material opponentSquareMaterial;
    [SerializeField] private GameObject selectorPrefab;
    [SerializeField] private Transform boardScale;    

    private List<GameObject> instantiatedSelectors = new List<GameObject>();

    public void ShowSelection(Dictionary<Vector3, bool> squareData)
    {
        ClearSelection();

        foreach(var data in squareData)
        {
            GameObject selector = Instantiate(selectorPrefab, data.Key, Quaternion.identity);
            selector.transform.localScale = boardScale.localScale;

            instantiatedSelectors.Add(selector);

            foreach(var setter in selector.GetComponentsInChildren<MaterialSetter>())
            {
                setter.SetSingleMaterial(data.Value ? freeSquareMaterial : opponentSquareMaterial);
            }
        }
    }

    public void ClearSelection()
    {
        foreach(var selector in instantiatedSelectors)
        {
            Destroy(selector.gameObject);
        }
        instantiatedSelectors.Clear();
    }

}
