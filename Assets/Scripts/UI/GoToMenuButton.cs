using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoToMenuButton : MonoBehaviour
{
    public Button button;
    private void Awake()
    {
        button.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }
}
