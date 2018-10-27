using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnButton : MonoBehaviour {

    public void OnClickStart()
    {
        SceneManager.LoadScene("Test");
    }

}
