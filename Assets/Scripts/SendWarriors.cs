using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SendWarriors : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Dictionary<string, Warrior> _activeWarriors = new Dictionary<string, Warrior>();
    [SerializeField] private List<Transform> _spawnPointses;
    [SerializeField] private List<Transform> _waypointses;
    [SerializeField] private CastleHP _enemyCastleHp;
    [SerializeField] private CastleHP _castleHP;
    [SerializeField] private List<CastleHPScript> _castleHPScripts;
    [SerializeField] private WarriorHPScript _hp;
    [SerializeField] private Money _money;
    [SerializeField] private Experience _exp;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private Text _costArmyText;
    [SerializeField] private Color _allyWarriorColor;
    [SerializeField] private Text _timeToSendAgainText;
    [SerializeField] private Button _sendButton;

    public List<Button> Enemies;
    public Dictionary<string, Warrior> Warriors = new Dictionary<string, Warrior>();
    public List<Warrior> AlreadySendWarrior = new List<Warrior>();
    public List<Warrior> AlreadyTakeWarrior = new List<Warrior>();

    private int autoincrement = 0;
    private Warrior _warrior;
    private List<Transform> _enemyWaypointsWay1 = new List<Transform>();
    private List<Transform> _enemyWaypointsWay2 = new List<Transform>();
    private List<Transform> _allyWaypointsWay1 = new List<Transform>();
    private List<Transform> _allyWaypointsWay2 = new List<Transform>();
    private Transform _spawnPoints;
    private Transform _enemyWaypoints1;
    private Transform _enemyWaypoints2;
    private Transform _allyWaypoints1;
    private Transform _allyWaypoints2;
    private PhotonView _photonView;


    [Header("Parametrs")]
    [SerializeField] private float _warriorInterval;
    [SerializeField] private float _startTime;
    [SerializeField] private int _sendDelay;


    public int CountAllWarrior;
    public int CostArmy;
    public bool IsStart;

    private int _fullSendDelay;
    private int _lastRandom = 0;
    private int _lastlastRandom = 0;
    private bool _checkMismatch;
    private bool _isFirstWay = true;

    #region MONO

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
        if (PhotonNetwork.LocalPlayer.NickName.Contains("Blue"))
        {
            _castleHPScripts[0].CastleHP = _castleHP;
            _castleHPScripts[0].ColorSlider.color = _allyWarriorColor;
            _castleHPScripts[1].CastleHP = _enemyCastleHp;
            _spawnPoints = _spawnPointses[1];
            _enemyWaypoints1 = _waypointses[1];
            _allyWaypoints1 = _waypointses[0];
            if (_waypointses.Count > 3)
            {
                _enemyWaypoints2 = _waypointses[3];
                _allyWaypoints2 = _waypointses[2];
            }
        }
        else if (PhotonNetwork.LocalPlayer.NickName.Contains("Red"))
        {
            _castleHPScripts[1].CastleHP = _castleHP;
            _castleHPScripts[1].ColorSlider.color = _allyWarriorColor;
            _castleHPScripts[0].CastleHP = _enemyCastleHp;
            _spawnPoints = _spawnPointses[0];
            _enemyWaypoints1 = _waypointses[0];
            _allyWaypoints1 = _waypointses[1];
            if (_waypointses.Count > 3)
            {
                _enemyWaypoints2 = _waypointses[2];
                _allyWaypoints2 = _waypointses[3];
            }
        }
        for (int i = 0; i < _enemyWaypoints1.childCount; i++)
        {
            _enemyWaypointsWay1.Add(_enemyWaypoints1.GetChild(i).transform);
        }
        for (int i = 0; i < _allyWaypoints1.childCount; i++) 
        {
            _allyWaypointsWay1.Add(_allyWaypoints1.GetChild(i).transform);
        }
        if (_enemyWaypoints2 != null && _allyWaypoints2 != null)
        {
            for (int i = 0; i < _enemyWaypoints2.childCount; i++)
            {
                _enemyWaypointsWay2.Add(_enemyWaypoints2.GetChild(i).transform);
            }
            for (int i = 0; i < _allyWaypoints2.childCount; i++)
            {
                _allyWaypointsWay2.Add(_allyWaypoints2.GetChild(i).transform);
            }
        }
        _fullSendDelay = _sendDelay;
        _timeToSendAgainText.text = _sendDelay.ToString();
        InvokeRepeating("SendTimer", 0, 1f);
    }

    #endregion

    #region BODY

    void Update()
    {
        if (_money.Count < CostArmy || _sendDelay != _fullSendDelay)
        {
            _sendButton.interactable = false;
        }
        else
        {
            _sendButton.interactable = true;
        }
        _costArmyText.text = CostArmy.ToString();
        if (_activeWarriors.Count == 0)
        {
            CancelInvoke("SpawnWarrior");
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Отправка мобой на сторону противника, указанных в панели.
    /// </summary>
    public void SendTroops()
    {
        foreach (KeyValuePair<string, Warrior> kvp in Warriors)
        {
            _activeWarriors.Add(kvp.Key, kvp.Value);
        }
        Warriors.Clear();
        InvokeRepeating("SpawnWarrior", _startTime, _warriorInterval);
        InvokeRepeating("SendTimer", 0, 1f);
        if (Enemies.Count > 0)
        {
            _money.Count -= CostArmy;
            foreach (var Warrior in Enemies)
            {
                Warrior.GetComponent<HiringWarriors>().CountWarrior = 0;
            }
        }
        CountAllWarrior = 0;
        CostArmy = 0;
    }

    /// <summary>
    ///  Цикличный спавн мобов на стороне противника.
    /// </summary>
    private void SpawnWarrior()
    {
        if (_activeWarriors.Count == 0)
        {
            return;
        }
        string warriorRemoveKey = null;
        foreach (KeyValuePair<string, Warrior> kvp in _activeWarriors)
        {
            warriorRemoveKey = kvp.Key;
            _warrior = kvp.Value;
            break;
        }
        var name = warriorRemoveKey.Split();
        _activeWarriors.Remove(warriorRemoveKey);
        Warrior warrior = null;
        int rndCount = 0;
        bool _checkCycle = false;
        while (!_checkCycle)
        {
            rndCount = Random.Range(1, 4);
            if (rndCount != _lastRandom && rndCount != _lastlastRandom && !_checkMismatch)
            {
                _lastRandom = rndCount;
                _lastlastRandom = 0;
                _checkCycle = true;
                _checkMismatch = true;
            }
            else if (rndCount != _lastRandom && rndCount != _lastlastRandom && _checkMismatch)
            {
                _lastlastRandom = _lastRandom;
                _lastRandom = rndCount;
                _checkMismatch = false;
                _checkCycle = true;
            }
        }
        bool oldFirstWay = _isFirstWay;
        if (_isFirstWay && _enemyWaypointsWay2.Count != 0)
        {
            _isFirstWay = false;
            warrior = SetWarriorWay(_enemyWaypointsWay1, rndCount);
        }
        else if (!_isFirstWay)
        {
            _isFirstWay = true;
            warrior = SetWarriorWay(_enemyWaypointsWay2, rndCount);
        }
        else
            warrior = SetWarriorWay(_enemyWaypointsWay1, rndCount);
        rndCount--;
        _photonView.RPC("RPC_FindWarriorServer", RpcTarget.Others, warrior.transform.position, autoincrement, oldFirstWay, rndCount);
        CreateHPScript(warrior, true, autoincrement);
        autoincrement++;
    }

    private Warrior SetWarriorWay(List<Transform> enemyWaypointsWay, int rndCount)
    {
        Warrior warrior = null;
        List<Transform> waypoints = new List<Transform>();
        switch (rndCount)
        {
            case 1:
                warrior = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Warriors", _warrior.name), _spawnPoints.GetChild(0).transform.position + _warrior.GetComponent<MoveToWayPoints>().Offset, Quaternion.identity).GetComponent<Warrior>();
                foreach (var waypoint in enemyWaypointsWay)
                {
                    waypoints.Add(waypoint.GetChild(0).transform);
                }
                warrior.GetComponent<MoveToWayPoints>().Waypoints = waypoints;
                break;
            case 2:
                warrior = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Warriors", _warrior.name), _spawnPoints.GetChild(1).transform.position + _warrior.GetComponent<MoveToWayPoints>().Offset, Quaternion.identity).GetComponent<Warrior>();
                foreach (var waypoint in enemyWaypointsWay)
                {
                    waypoints.Add(waypoint.GetChild(1).transform);
                }
                warrior.GetComponent<MoveToWayPoints>().Waypoints = waypoints;
                break;
            case 3:
                warrior = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Warriors", _warrior.name), _spawnPoints.GetChild(2).transform.position + _warrior.GetComponent<MoveToWayPoints>().Offset, Quaternion.identity).GetComponent<Warrior>();
                foreach (var waypoint in enemyWaypointsWay)
                {
                    waypoints.Add(waypoint.GetChild(2).transform);
                }
                warrior.GetComponent<MoveToWayPoints>().Waypoints = waypoints;
                break;
        }
        return warrior;
    }

    private void CreateHPScript(Warrior warrior, bool isSender, int autoincrement, bool isFirstWay = true, int rndCount = -1)
    {
        WarriorHPScript hp = Instantiate(_hp, Vector3.zero, Quaternion.identity);
        hp.Warrior = warrior;
        hp.Offset = warrior.HpOffset;
        warrior.Money = _money;
        warrior.EXP = _exp;
        warrior.Hp = hp;
        if (isSender)
        {
            hp.ColorSlider.color = _allyWarriorColor;
            warrior.IsEnemy = false;
            warrior.EnemyCastleHP = _enemyCastleHp;
            warrior.Index = autoincrement;
            AlreadySendWarrior.Add(warrior);
        }
        else
        {
            List<Transform> waypoints = new List<Transform>();
            if (isFirstWay)
            {
                foreach (var waypoint in _allyWaypointsWay1)
                {
                    waypoints.Add(waypoint.GetChild(rndCount).transform);
                }
            }
            else
            {
                foreach (var waypoint in _allyWaypointsWay2)
                {
                    waypoints.Add(waypoint.GetChild(rndCount).transform);
                }
            }
            warrior.GetComponent<MoveToWayPoints>().Waypoints = waypoints;
            warrior.EnemyCastleHP = _castleHP;
            warrior.IsEnemy = true;
            warrior.Index = autoincrement;
            AlreadyTakeWarrior.Add(warrior);
        }
        hp.transform.SetParent(_canvas.transform);
        hp.transform.SetAsFirstSibling();
        
    }

    /// <summary>
    ///  Отсчёт интервала между отправками мобов.
    /// </summary>
    private void SendTimer()
    {
        if (IsStart)
            _sendDelay--;
        _timeToSendAgainText.text = _sendDelay.ToString();
        if (_sendDelay == 0)
        {
            _sendDelay = _fullSendDelay;
            CancelInvoke("SendTimer");
        }
    }

    #endregion

    #region PUNCALLBACKS

    [PunRPC]
    private void RPC_FindWarriorServer(Vector3 positionWarrior, int autoincrement, bool isFirstWay, int rndCount)
    {
        Collider[] colliders = Physics.OverlapSphere(positionWarrior, 0.001f);
        if (colliders.Length != 0)
        {
            foreach (var collider in colliders)
            {
                Warrior warrior = collider.gameObject.GetComponent<Warrior>();
                if (warrior == null)
                    Debug.LogError("При получении воина он оказался null или объект не являлся воином");
                if (warrior.Index == -1)
                    CreateHPScript(warrior, false, autoincrement, isFirstWay, rndCount);
            }
        }
    }

    #endregion
}
