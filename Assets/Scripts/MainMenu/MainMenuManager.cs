using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour {

	public void LoadLevel (int lvlNumber)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(lvlNumber);
    }
}
