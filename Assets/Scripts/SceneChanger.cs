using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void ChangeScene(int SceneNumber)
    {
        SceneManager.LoadScene(SceneNumber);
    }
}
