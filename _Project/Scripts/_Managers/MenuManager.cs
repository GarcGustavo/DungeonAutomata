using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [SerializeField] private TMP_Dropdown dropdown;
    private int _sceneCount;
    private int _currentScene;
    string[] _scenes;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PopulateSceneDropdown();
    }

    private void GetAllScenes()
    {
        _sceneCount = SceneManager.sceneCountInBuildSettings;
        _scenes = new string[_sceneCount];
        for (int i = 0; i < _sceneCount; i++)
        {
            _scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
        }
    }
    
    private void PopulateSceneDropdown()
    {
        var optionDataList = new List<TMP_Dropdown.OptionData>();
 
        for(int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i) 
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            optionDataList.Add(new TMP_Dropdown.OptionData(name));
        }
 
        dropdown.ClearOptions();
        dropdown.AddOptions(optionDataList);
        dropdown.onValueChanged.AddListener(delegate { OnSceneSelected(dropdown); });
    }

    private void OnSceneSelected(TMP_Dropdown tmpDropdown)
    {
        SceneManager.LoadScene(tmpDropdown.value);
        Debug.Log("Selected: " + tmpDropdown.options[tmpDropdown.value].text);
    }
}
