using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Add this to manage scenes


public class Settings : MonoBehaviour
{


    public void BackToMenu(){

        SceneManager.LoadScene("MainMenu");
    }
}
