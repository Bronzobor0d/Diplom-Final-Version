using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitPlayers : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text _loadText;
    [SerializeField] private GameObject _connectText;
    [SerializeField] private GameObject _backButton;
    [SerializeField] private List<string> _maps = new List<string>();

    private PhotonView PhotonView;

    private bool _isWait = true;
    private bool _isAnimate;
    private bool _isFound;
    private string _selectedMap;

    #region MONO

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
    }

    #endregion

    #region BODY

    void Update()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && _isWait)
            {
                _isFound = true;
                _loadText.enabled = false;
                _connectText.SetActive(true);
                _backButton.SetActive(false);
                if (PhotonNetwork.IsMasterClient)
                {
                    _isWait = false;
                    string map = _maps[Random.Range(0, _maps.Count)];
                    PhotonNetwork.LoadLevel(map);
                    PhotonView.RPC("SetMapSecondPlayer", RpcTarget.OthersBuffered, map);
                }
                else if (_selectedMap != null && _selectedMap != "")
                {
                    _isWait = false;
                    PhotonNetwork.LoadLevel(_selectedMap);
                }
            }
            else if (!_isAnimate && !_isFound)
            {
                StartCoroutine(FirstAnim());
            }
        }
    }

    #endregion

    #region CALLBACKS

    IEnumerator FirstAnim()
    {
        _isAnimate = true;
        _loadText.text += ".";
        if (_loadText.text.Length == 9)
        {
            _loadText.text = "Поиск";
        }
        yield return new WaitForSeconds(1f);
        _isAnimate = false;
    }

    public void BackToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MenuScene");
    }

    #endregion

    #region PUNCALLBACKS

    [PunRPC]
    private void SetMapSecondPlayer(string map)
    {
        _selectedMap = map;
    }

    #endregion
}
