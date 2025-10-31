using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void SceneChanger(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
