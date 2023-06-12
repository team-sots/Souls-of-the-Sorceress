﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fes_EmergencyReset : MonoBehaviour
{
    List<KeyCode> keys = new List<KeyCode> { KeyCode.E, KeyCode.N, KeyCode.D };

    bool allPushedm1;

    static Fes_EmergencyReset instance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;
        allPushedm1 = false;
    }
    // Update is called once per frame
    void Update() // FIXME: FInd alternative way to realize this by InputSystem.
    {
        // if(keys.Count == 0)
        // {
        //     return;
        // }

        // bool allPushed = keys.TrueForAll(k => Input.GetKey(k));
        // if(!allPushedm1 && allPushed)
        // {
        //     // SaveData.Instance.Init();
        //     // SceneTransitionManager.Initialize();
        //     // UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName.title);
        //     SotS.GameCommonInterface.Instance.ResetEntireGame();
        // }
        // allPushedm1 = allPushed;
    }

}
