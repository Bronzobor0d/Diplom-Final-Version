using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField] private Vector3 _spawnBlue;
    [SerializeField] private Vector3 _spawnRed;
    [SerializeField] private float xBorderPlusBlue;
    [SerializeField] private float xBorderMinusBlue;
    [SerializeField] private float zBorderPlusBlue;
    [SerializeField] private float zBorderMinusBlue;
    [SerializeField] private float xBorderPlusRed;
    [SerializeField] private float xBorderMinusRed;
    [SerializeField] private float zBorderPlusRed;
    [SerializeField] private float zBorderMinusRed;

    public GameObject PlayerBlue;
    public GameObject PlayerRed;

    private GameObject _playerControl;

    private void Awake()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.PlayerList[0].IsMasterClient)
            {
                PhotonNetwork.PlayerList[0].NickName = "BluePlayer";
                PhotonNetwork.PlayerList[1].NickName = "RedPlayer";
            }
            else
            {
                PhotonNetwork.PlayerList[1].NickName = "BluePlayer";
                PhotonNetwork.PlayerList[0].NickName = "RedPlayer";
            }
            if (PhotonNetwork.LocalPlayer.NickName == "BluePlayer")
            {
                _playerControl = Instantiate(PlayerBlue, _spawnBlue, PlayerBlue.transform.rotation);
                PlayerControl playerControl = _playerControl.GetComponent<PlayerControl>();
                playerControl.XCamPosPlus = xBorderPlusBlue;
                playerControl.XCamPosMinus = xBorderMinusBlue;
                playerControl.ZCamPosPlus = zBorderPlusBlue;
                playerControl.ZCamPosMinus = zBorderMinusBlue;
            }
            else if (PhotonNetwork.LocalPlayer.NickName == "RedPlayer")
            {
                _playerControl = Instantiate(PlayerRed, _spawnRed, PlayerRed.transform.rotation);
                PlayerControl playerControl = _playerControl.GetComponent<PlayerControl>();
                playerControl.XCamPosPlus = xBorderPlusRed;
                playerControl.XCamPosMinus = xBorderMinusRed;
                playerControl.ZCamPosPlus = zBorderPlusRed;
                playerControl.ZCamPosMinus = zBorderMinusRed;
            }
        }
    }
}
