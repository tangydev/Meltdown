using UnityEngine;

public class TitleScreenManager : MonoBehaviour
{
    public void OnStartButtonPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level01");
    }
}
