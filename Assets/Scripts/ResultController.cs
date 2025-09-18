using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultController : MonoBehaviour
{
    public void OnClickStartButton()
    {
        SceneManager.LoadScene("Title");
    }
}
