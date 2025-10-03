using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingControl : MonoBehaviour
{
    [SerializeField] private GameObject _menu;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            _menu.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
