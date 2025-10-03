using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateStartTower : MonoBehaviour
{
    [SerializeField] private List<Tower> _startTowers = new List<Tower>();
    [SerializeField] private TowerPlaces _towerPlace;

    private void Start()
    {
        Time.timeScale = 1;
        int i = Random.Range(0, _startTowers.Count);
        Instantiate(_startTowers[i], _towerPlace.transform.position, Quaternion.identity);
    }
}
