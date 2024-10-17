using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Add this to manage scenes

public class cfg : MonoBehaviour
{

public void MainMenu(){
    SceneManager.LoadScene("MainMenu");
}

public void Mute(){
    AudioListener.pause = !AudioListener.pause;
    }

}
