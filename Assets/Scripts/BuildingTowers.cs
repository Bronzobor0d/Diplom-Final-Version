using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BuildingTowers : MonoBehaviour
{
    [SerializeField] private Money _money;
    [SerializeField] private EditTower _editTower;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private GameObject _menu;
    [SerializeField] private List<GameObject> _buttonPanels;
    [SerializeField] private List<GameObject> _panels;
    [SerializeField] private Text _timerStartText;
    [SerializeField] private GameObject _readyPanel;
    [SerializeField] private Text _countReadyPlayersText;
    [SerializeField] private GameObject _buttonReady;
    [SerializeField] private SendWarriors _sendWarriors;
    [SerializeField] private GameObject _selectPathTower;
    [SerializeField] private int _timerStart;
    [SerializeField] private List<Tower> _defenseTowers = new List<Tower>();

    public PhotonView PhotonView;

    private Tower _flyingBuilding;
    private Tower _setPathTower;
    private Camera _mainCamera;
    private EditTower _upgradeYet;
    private Tower _lastTower;
    private bool _isPause;
    private bool _isGameOver;
    private Tower _preLastTower;
    private bool _isStart;
    private int _countReadyPlayers;

    #region MONO

    void Start()
    {
        _mainCamera = Camera.main;
        PhotonView = GetComponent<PhotonView>();
        _sendWarriors = GetComponent<SendWarriors>();
        foreach (var defenseTower in _defenseTowers)
        {
            defenseTower.SendWarriors = _sendWarriors;
            if (PhotonNetwork.LocalPlayer.NickName.Contains(defenseTower.Team))
                defenseTower.IsPlaced = true;
        }
        foreach (var buttonPanel in _buttonPanels)
        {
            buttonPanel.SetActive(false);
        }
        if (PhotonNetwork.IsMasterClient)
            InvokeRepeating("StartTimer", 0, 1);
    }

    #endregion

    #region BODY

    void Update()
    {
        if (_countReadyPlayers == 2 && !_isStart)
        {
            StartGame();
        }
        if (!_isStart)
        {
            _countReadyPlayersText.text = $"{_countReadyPlayers} из 2";
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape) && _flyingBuilding != null && !_isGameOver)
        {
            CancelBuilding();
        }
        else if (_upgradeYet != null && Input.GetKeyDown(KeyCode.Escape) && !_isGameOver)
        {
            CancelUpgrade();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !_isGameOver)
        {
            bool check = false;
            foreach (var panel in _panels)
            {
                if (panel.activeSelf)
                    check = true;
            }
            if (!check)
            {
                if (_menu.activeSelf)
                {
                    _menu.SetActive(false);
                    _isPause = false;
                    foreach (var buttonPanel in _buttonPanels)
                    {
                        buttonPanel.SetActive(true);
                    }
                }
                else
                {
                    _menu.SetActive(true);
                    _isPause = true;
                    foreach (var buttonPanel in _buttonPanels)
                    {
                        buttonPanel.SetActive(false);
                    }
                }
            }
        }
        if (_flyingBuilding != null)
        {
            if (_upgradeYet != null)
            {
                _upgradeYet.TowerObj.transform.GetChild(2).gameObject.SetActive(false);
                Destroy(_upgradeYet.gameObject);
                _upgradeYet = null;
            }
            var groundPlane = new Plane(Vector3.up, -0.200001f);
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float position))
            {
                Vector3 worldPosition = ray.GetPoint(position);
                _flyingBuilding.transform.position = worldPosition;
                _flyingBuilding.SetTransparent(false);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.tag == "TowerPlace")
                    {
                        TowerPlaces place = hit.collider.GetComponent<TowerPlaces>();
                        if (!place.Occupied && place.IsBig == _flyingBuilding.IsBig && PhotonNetwork.LocalPlayer.NickName.Contains(place.Team))
                        {
                            _flyingBuilding.transform.position = hit.transform.position;
                            _flyingBuilding.SetTransparent(true);
                        }
                        else
                        {
                            _flyingBuilding.SetTransparent(false);
                        }
                        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && !place.Occupied && place.IsBig == _flyingBuilding.IsBig && PhotonNetwork.LocalPlayer.NickName.Contains(place.Team))
                        {
                            _money.CoinMinus(_flyingBuilding.Cost);
                            Tower towerNew = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Towers", _flyingBuilding.name.Remove(_flyingBuilding.name.Length - 7)), _flyingBuilding.transform.position, Quaternion.identity).GetComponent<Tower>();
                            PhotonView.RPC("RPC_SetSendWarriors", RpcTarget.Others, _flyingBuilding.transform.position);
                            towerNew.IsPlaced = true;
                            place.Occupied = true;
                            place.Tower = towerNew;
                            towerNew.TowerPlace = place;
                            towerNew.Team = place.Team;
                            towerNew.RemoveCost = _flyingBuilding.Cost / 2;
                            towerNew.GetComponent<Collider>().isTrigger = false;
                            towerNew.ZoneTrigger.gameObject.SetActive(true);
                            towerNew.ZoneVisibility.gameObject.SetActive(false);
                            towerNew.SendWarriors = _sendWarriors;
                            Destroy(_flyingBuilding.gameObject);
                            _flyingBuilding = null;
                            if (towerNew.TwoPath)
                            {
                                _selectPathTower.transform.GetChild(1).GetComponent<Text>().text = towerNew.Name;
                                _selectPathTower.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = towerNew.NamePath1;
                                _selectPathTower.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = towerNew.NamePath2;
                                _setPathTower = towerNew;
                                _selectPathTower.gameObject.SetActive(true);
                                _isPause = true;
                                foreach (var buttonPanel in _buttonPanels)
                                {
                                    buttonPanel.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (_flyingBuilding == null && Input.GetMouseButtonDown(0) && !_isPause && !_isGameOver)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Tower" && hit.collider.GetComponent<Tower>().Team != "" && PhotonNetwork.LocalPlayer.NickName.Contains(hit.collider.GetComponent<Tower>().Team) 
                    && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    if (_upgradeYet != null && _upgradeYet.TowerObj == hit.collider.GetComponent<Tower>())
                        return;
                    else if (_upgradeYet != null)
                    {
                        _upgradeYet.TowerObj.ZoneVisibility.gameObject.SetActive(false);
                        Destroy(_upgradeYet.gameObject);
                        _upgradeYet = null;
                    }
                    EditTower upgrade = Instantiate(_editTower, Vector3.zero, Quaternion.identity);
                    upgrade.TowerObj = hit.collider.gameObject.GetComponent<Tower>();
                    upgrade.Offset = hit.collider.gameObject.GetComponent<Tower>().Offset;
                    upgrade.Money = _money;
                    upgrade.transform.SetParent(_canvas.transform);
                    upgrade.TowerObj.ZoneVisibility.gameObject.SetActive(true);
                    upgrade.BuildingTowers = this;
                    _upgradeYet = upgrade;
                }
                else if (hit.collider.tag != "Tower" && _upgradeYet != null && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    _upgradeYet.TowerObj.ZoneVisibility.gameObject.SetActive(false);
                    Destroy(_upgradeYet.gameObject);
                    _upgradeYet = null;
                }
            }
        }
        else if (_flyingBuilding == null && _lastTower != null && Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift) && _money.Count >= _lastTower.Cost && !_isPause && !_isGameOver)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "TowerPlace")
                {
                    TowerPlaces place = hit.collider.GetComponent<TowerPlaces>();
                    if (!place.Occupied && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && place.IsBig == _lastTower.IsBig)
                    {
                        Tower lastTower = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Towers", _lastTower.name), hit.transform.position, Quaternion.identity).GetComponent<Tower>();
                        PhotonView.RPC("RPC_SetSendWarriors", RpcTarget.Others, hit.transform.position);
                        lastTower.transform.position = hit.transform.position;
                        _money.CoinMinus(lastTower.Cost);
                        place.Occupied = true;
                        place.Tower = lastTower;
                        lastTower.Team = place.Team;
                        lastTower.TowerPlace = place;
                        lastTower.IsPlaced = true;
                        lastTower.RemoveCost = lastTower.Cost / 2;
                        lastTower.SelectedPath = _lastTower.SelectedPath;
                        lastTower.SendWarriors = _sendWarriors;
                        lastTower.SetNormal();
                        lastTower.SelectedPath = _lastTower.SelectedPath;
                        if (_lastTower.TwoPath)
                        {
                            if (_lastTower.SelectedPath == 1)
                                lastTower.NextLevelTower = _lastTower.NextLevelTowerPath1;
                            else
                                lastTower.NextLevelTower = _lastTower.NextLevelTowerPath2;
                        }
                    }
                }
            }
        }
        else if (_flyingBuilding == null && Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftShift) && !_isPause && !_isGameOver)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Tower hitTower;
                if (hit.collider.tag == "Tower")
                    hitTower = hit.collider.GetComponent<Tower>();
                else
                    return;
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()
                    && hitTower.NextLevelTower != null
                    && hitTower.NextLevelTower.Cost <= _money.Count
                    && hitTower.Team != ""
                    && PhotonNetwork.LocalPlayer.NickName.Contains(hitTower.Team))
                {
                    EditTower upgrade = Instantiate(_editTower, Vector3.zero, Quaternion.identity);
                    upgrade.TowerObj = hitTower;
                    upgrade.Offset = hitTower.Offset;
                    upgrade.Money = _money;
                    upgrade.transform.SetParent(_canvas.transform);
                    upgrade.TowerObj.ZoneVisibility.gameObject.SetActive(true);
                    upgrade.BuildingTowers = this;
                    UpgradeBuilding(upgrade);
                }
            }
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Установка новой летающей башни и удаление старой.
    /// </summary>
    public void StartPlacingBuilding(Tower buildingPrefab)
    {
        if (_flyingBuilding != null)
        {
            Destroy(_flyingBuilding.gameObject);
        }
        _flyingBuilding = Instantiate(buildingPrefab);
        if (_lastTower != null)
            _preLastTower = _lastTower;
        _lastTower = buildingPrefab;
        _flyingBuilding.GetComponent<Collider>().isTrigger = true;
        _flyingBuilding.ZoneTrigger.gameObject.SetActive(false);
        _flyingBuilding.ZoneVisibility.gameObject.SetActive(true);
        _flyingBuilding.transform.localScale = new Vector3(_flyingBuilding.transform.localScale.x + 0.0001f, _flyingBuilding.transform.localScale.y + 0.0001f, _flyingBuilding.transform.localScale.z + 0.0001f);
    }

    /// <summary>
    ///  Улучшение башни.
    /// </summary>
    public void UpgradeBuilding(EditTower upgrade)
    {
        Tower newTower = upgrade.TowerObj.NextLevelTower;
        Tower newTowerObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Towers", newTower.name), upgrade.TowerObj.transform.position, Quaternion.identity).GetComponent<Tower>();
        newTowerObject.TowerPlace = upgrade.TowerObj.TowerPlace;
        newTowerObject.Team = upgrade.TowerObj.Team;
        newTowerObject.RemoveCost = (newTowerObject.Cost / 2) + upgrade.TowerObj.RemoveCost;
        newTowerObject.SetNormal();
        newTowerObject.SendWarriors = _sendWarriors;
        newTowerObject.IsPlaced = true;
        newTowerObject.SelectedPath = upgrade.TowerObj.SelectedPath;
        if (upgrade.TowerObj.TwoPath)
        {
            if (upgrade.TowerObj.SelectedPath == 1)
                newTowerObject.NextLevelTower = newTowerObject.NextLevelTowerPath1;
            else
                newTowerObject.NextLevelTower = newTowerObject.NextLevelTowerPath2;
        }
        upgrade.Money.CoinMinus(newTower.Cost);
        PhotonNetwork.Destroy(upgrade.TowerObj.gameObject);
        upgrade.BuildingTowers.PhotonView.RPC("RPC_SetSendWarriors", RpcTarget.Others, newTowerObject.transform.position);
        Destroy(upgrade.gameObject);
    }
    
    /// <summary>
    ///  Удаление башни.
    /// </summary>
    public void RemoveBuilding(EditTower remove)
    {
        remove.Money.CoinPlus(remove.TowerObj.RemoveCost);
        remove.TowerObj.TowerPlace.Occupied = false;
        PhotonNetwork.Destroy(remove.TowerObj.gameObject);
        Destroy(remove.gameObject);
    }

    /// <summary>
    ///  Отмена строительства башни.
    /// </summary>
    public void CancelBuilding()
    {
        if (_flyingBuilding != null)
        {
            Destroy(_flyingBuilding.gameObject);
            _flyingBuilding = null;
            if (_preLastTower == null)
                _lastTower = null;
            else
                _lastTower = _preLastTower;
        }
    }

    /// <summary>
    ///  Удаление меню улучшения.
    /// </summary>
    public void CancelUpgrade()
    {
        if (_upgradeYet != null)
        {
            _upgradeYet.TowerObj.ZoneVisibility.gameObject.SetActive(false);
            Destroy(_upgradeYet.gameObject);
            _upgradeYet = null;
        }
    }

    /// <summary>
    ///  Звершение игры.
    /// </summary>
    public void GameOver()
    {
        _isGameOver = true;
        foreach (var panel in _panels)
        {
            panel.SetActive(false);
        }
        foreach (var buttonPanel in _buttonPanels)
        {
            buttonPanel.SetActive(false);
        }
        _menu.SetActive(false);
        CancelBuilding();
        CancelUpgrade();
    }

    private void StartTimer()
    {
        _timerStart--;
        try
        {
            PhotonView.RPC("RPC_StartTimer", RpcTarget.Others, _timerStart);
        }
        catch
        {
            Debug.LogWarning("Второй игрок пока не загрузился");
        }
        _timerStartText.text = _timerStart.ToString();
        if (_timerStart == 0)
        {
            SetReady();
            StartGame();
            CancelInvoke("StartTimer");
        }
    }

    private void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
            CancelInvoke("StartTimer");
        _isStart = true;
        _money.IsStart = true;
        _sendWarriors.IsStart = true;
        _readyPanel.SetActive(false);
        foreach (var buttonPanel in _buttonPanels)
        {
            buttonPanel.SetActive(true);
        }
    }

    public void SetReady()
    {
        _buttonReady.SetActive(false);
        _countReadyPlayers++;
        PhotonView.RPC("RPC_SetReady", RpcTarget.Others);
    }

    public void SelectPathFirst()
    {
        _setPathTower.NextLevelTower = _setPathTower.NextLevelTowerPath1;
        _setPathTower.SelectedPath = 1;
        HideSelectPath();
    }

    public void SelectPathSecond()
    {
        _setPathTower.NextLevelTower = _setPathTower.NextLevelTowerPath2;
        _setPathTower.SelectedPath = 2;
        HideSelectPath();
    }

    public void HideSelectPath()
    {
        _lastTower.SelectedPath = _setPathTower.SelectedPath;
        _setPathTower = null;
        _selectPathTower.SetActive(false);
        _isPause = false;
        foreach (var buttonPanel in _buttonPanels)
        {
            buttonPanel.SetActive(true);
        }
    }

    #endregion

    #region PUNCALLBACKS

    [PunRPC]
    private void RPC_SetSendWarriors(Vector3 towerPosition)
    {
        Collider[] colliders = Physics.OverlapSphere(towerPosition + new Vector3(0, 0.1f, 0), 0.001f);
        if (colliders.Length != 0)
        {
            colliders[0].gameObject.GetComponent<Tower>().SendWarriors = GetComponent<SendWarriors>();
        }
    }

    [PunRPC]
    private void RPC_SetReady()
    {
        _countReadyPlayers++;
    }

    [PunRPC]
    private void RPC_StartTimer(int timer)
    {
        _timerStart = timer;
        _timerStartText.text = _timerStart.ToString();
        if (_timerStart == 0)
        {
            SetReady();
            StartGame();
            CancelInvoke("StartTimer");
        }
    }

    #endregion
}
