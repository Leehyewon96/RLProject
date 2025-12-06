using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject loading = null;
    [SerializeField] private TextMeshProUGUI stateText = null;
    [SerializeField] private TMP_InputField idInputField = null;
    [SerializeField] private Button btnLogin = null;
    [SerializeField] private Button btnStart = null;
    [SerializeField] private GameObject lobbyLoadingUI = null;
    [SerializeField] private TextMeshProUGUI lobbyLoadingText = null;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        loading.SetActive(true);
        stateText.SetText("연결 중...");
        btnLogin.onClick.AddListener(RequestJoinLobby);
        btnLogin.interactable = false;
        idInputField.interactable = false;

        btnStart.gameObject.SetActive(false);
        lobbyLoadingUI.SetActive(false);
        btnStart.onClick.AddListener(OnClickBtnStart);
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("클라이언트가 마스터에 연결됨");
        StartCoroutine(CoConnectedToMaster());
    }

    private IEnumerator CoConnectedToMaster()
    {
        stateText.SetText("연결 성공!");
        loading.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        stateText.SetText("아이디를 입력하세요.");
        btnLogin.interactable = true;
        idInputField.interactable = true;
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        //SceneManager.LoadScene(SceneLocalize.lobbyScene);
        stateText.SetText($"{idInputField.text}님! 반가워요!");
        idInputField.gameObject.SetActive(false);
        btnLogin.gameObject.SetActive(false);
        btnStart.gameObject.SetActive(true);
        Debug.Log("로비 입장 완료");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogWarningFormat("서버와의 연결이 끊어짐. 사유 : {0}", cause);
    }

    private void RequestJoinLobby()
    {
        StartCoroutine(CoRequestJoinLobby());
    }
    
    private IEnumerator CoRequestJoinLobby()
    {
        btnLogin.interactable = false;
        idInputField.interactable = false;
        stateText.SetText("로그인 중...");
        loading.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        stateText.SetText("로그인 성공!");
        loading.SetActive(false);
        GameManager.Instance.userName = idInputField.text;
        yield return new WaitForSeconds(1.5f);
        PhotonNetwork.JoinLobby();
    }

    #region InLobby
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 1;

        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("방생성 완료");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log($"방생성 실패, {returnCode}, {message}");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("방 입장 완료");
        //SceneManager.LoadScene(SceneLocalize.gameScene);
        //GameManager.Instance.isConnect = true;
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            lobbyLoadingText.SetText("게임 발견!");
            //테스트
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(CoStartGame());
            }
        }
        //lobbyLoadingUI.SetActive(true);

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            lobbyLoadingText.SetText("게임 발견!");
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(CoStartGame());
            }
        }
    }

    private IEnumerator CoStartGame()
    {
        yield return new WaitForSecondsRealtime(2f);
        PhotonNetwork.LoadLevel(SceneLocalize.gameScene);
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("방입장 실패. 새로운 방 생성");
        CreateRoom();
    }

    public void OnClickBtnStart()
    {
        btnStart.interactable = false;
        lobbyLoadingText.SetText("로딩중...");
        lobbyLoadingUI.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }
    #endregion
}
