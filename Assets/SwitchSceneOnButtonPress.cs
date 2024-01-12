using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchSceneOnButtonPress : MonoBehaviour
{
    [SerializeField] string SceneName = "";
    
    public void SwitchScene()
    {
        SceneManager.LoadScene(SceneName);
    }
}

