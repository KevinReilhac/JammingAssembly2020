﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Manager<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance = null;
    private static bool m_ShuttingDown = false;
    private static object m_Lock = new object();

    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static T instance
    {
        get
        {
            //Prevent instance creation on application quit when previous instance was already destroyed
            if (m_ShuttingDown)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed. Returning null.");
                return null;
            }

            lock (m_Lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (_instance == null)
                    {
                        _instance = new GameObject(GetGameObjectName(typeof(T)), typeof(T)).GetComponent<T>();
                        _instance.transform.SetSiblingIndex(1);
                    }
                }

                return _instance;
            }
        }
    }

    private static string GetGameObjectName(System.Type classType)
    {
        return (ObjectNames.NicifyVariableName(classType.ToString()));
    }


    private void OnApplicationQuit()
    {
        m_ShuttingDown = true;
    }

    private void OnDestroy()
    {
        m_ShuttingDown = true;
        _instance = null;
    }
}
