using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Add this to manage scenes

public class MenuScript : MonoBehaviour
{
    // This function will be called when the "New Game" button is pressed
    public void StartNewGame()
    {
        // Load the game scene, assuming the name of the game scene is "GameScene"
        SceneManager.LoadScene("GameScene");
    }

    public void Settings(){

        SceneManager.LoadScene("Settings");
    }
}
