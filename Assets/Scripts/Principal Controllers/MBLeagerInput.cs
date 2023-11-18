﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GInput
{
    public static MBLeagerInput leagerInput;

    public static List<KeyCode> keysDown = new List<KeyCode>();
    public static List<KeyCode> keys = new List<KeyCode>();
    public static List<int> mousesDown = new List<int>();
    public static List<int> mouses = new List<int>();

    public static bool GetKey(KeyCode key)
    {
        return Input.GetKey(key) || keys.Contains(key);
    }

    public static bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key) || keysDown.Contains(key);
    }

    public static bool GetMouseButton(int key)
    {
        return Input.GetMouseButton(key) || mouses.Contains(key);
    }

    public static bool GetMouseButtonDown(int key)
    {
        return Input.GetMouseButtonDown(key) || mousesDown.Contains(key);
    }

    public static void PressKey(KeyCode key)
    {
        if (!keys.Contains(key))
        {
            keys.Add(key);
            leagerInput.KeyDown(key);
        }
    }

    public static void ReleaseKey(KeyCode key)
    {
        keys.Remove(key);
    }

    public static void PressMouse(int key)
    {
        if (!mouses.Contains(key))
        {
            mouses.Add(key);
            leagerInput.MouseDown(key);
        }
    }

    public static void ReleaseMouse(int key)
    {
        mouses.Remove(key);
    }

    public static void Update()
    {
        if(GameManager.gameManagerReference != null)
        {
            GameManager.gameManagerReference.cancelPlacing = keys.Count != 0 || mouses.Count != 0;
        }
    }
}


public class MBLeagerInput : MonoBehaviour
{
    public string platform = "PC";
    public List<KeyCode> keys = new List<KeyCode>();

    void Start()
    {
        if(FindObjectsOfType<MBLeagerInput>().Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            GInput.leagerInput = this;
        }
    }

    void Update()
    {
        keys = GInput.keys;
        GInput.Update();
    }

    public void KeyDown(KeyCode key)
    {
        StartCoroutine(IEKeyDown(key));
    }

    IEnumerator IEKeyDown(KeyCode key)
    {
        GInput.keysDown.Add(key);
        yield return new WaitForEndOfFrame();
        GInput.keysDown.Remove(key);
    }

    public void MouseDown(int key)
    {
        StartCoroutine(IEMouseDown(key));
    }

    IEnumerator IEMouseDown(int key)
    {
        GInput.mousesDown.Add(key);
        yield return new WaitForEndOfFrame();
        GInput.mousesDown.Remove(key);
    }
}
