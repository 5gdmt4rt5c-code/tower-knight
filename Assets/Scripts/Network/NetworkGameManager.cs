using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// 네트워크 게임 매니저
/// Photon PUN 2를 사용한 멀티플레이 관리
/// </summary>
public class NetworkGameManager : MonoBehaviourPunCallbacks
{
    public static NetworkGameManager Instance { get; private set; }

    [SerializeField]
    private string _gameVersion = "1.0";

    [SerializeField]
    private int _maxPlayers = 4;

    [SerializeField]
    private GameObject _playerPrefab;

    private PhotonView _photonView;
    private int _connectedPlayers = 0;
    private bool _isConnected = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // Photon 초기화
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = _gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Photon 연결 시작...");
        }
    }

    /// <summary>
    /// 마스터 서버 연결 성공
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버에 연결됨");
        _isConnected = true;
    }

    /// <summary>
    /// 로비 진입
    /// </summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 진입");
    }

    /// <summary>
    /// 방 생성 및 진입
    /// </summary>
    public void CreateRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = _maxPlayers;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
        Debug.Log($"방 생성: {roomName}");
    }

    /// <summary>
    /// 기존 방에 진입
    /// </summary>
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        Debug.Log($"방 진입: {roomName}");
    }

    /// <summary>
    /// 무작위 방에 진입
    /// </summary>
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("무작위 방 찾는 중...");
    }

    /// <summary>
    /// 방 진입 성공
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log($"방에 진입함. 플레이어 수: {PhotonNetwork.CurrentRoom.PlayerCount}/{_maxPlayers}");
        _connectedPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

        // 플레이어 스폰
        SpawnPlayer();
    }

    /// <summary>
    /// 플레이어 스폰
    /// </summary>
    private void SpawnPlayer()
    {
        if (_playerPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-5f, 5f), 0f, 0f);
            GameObject playerInstance = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);

            // Photon 인스턴스로 등록
            PhotonNetwork.Instantiate(
                _playerPrefab.name,
                spawnPosition,
                Quaternion.identity
            );

            Debug.Log("플레이어 스폰");
        }
    }

    /// <summary>
    /// 다른 플레이어 진입
    /// </summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"플레이어 진입: {newPlayer.NickName} ({PhotonNetwork.CurrentRoom.PlayerCount}/{_maxPlayers})");
        _connectedPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    /// <summary>
    /// 플레이어 퇴장
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"플레이어 퇴장: {otherPlayer.NickName}");
        _connectedPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    /// <summary>
    /// 방 생성 실패
    /// </summary>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패: {message}");
    }

    /// <summary>
    /// 방 진입 실패
    /// </summary>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 진입 실패: {message}");
    }

    /// <summary>
    /// 게임 시작 신호 (마스터 플레이어만)
    /// </summary>
    [PunRPC]
    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Debug.Log("게임 시작!");
        GameManager.Instance.RestartGame();
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        Debug.Log("방 퇴장");
    }

    // Getter 메서드들
    public bool IsConnected => _isConnected;
    public int ConnectedPlayers => _connectedPlayers;
    public bool IsMasterClient => PhotonNetwork.IsMasterClient;
}
