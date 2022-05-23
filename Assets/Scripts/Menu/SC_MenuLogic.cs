using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SC_MenuLogic : MonoBehaviour
{
    private Stack<GameObject> pathScreens;
    private Dictionary<string, GameObject> unityObjects;
    private Dictionary<string, GameObject> SliderObject;

    private string apiKey = "0cbeedca6c116e367be5c586d00dd4d37f5ae2ce1a0d3d4d21edc336383da1ff";
    private string secretKey = "bbdb2eff9322520890a7aee95082f26c58dc24c41be3991173a457572a8a5740";
    private Listener listner;

    private Dictionary<string, GameObject> MpObjects;
    private Dictionary<string, object> passedParams;
    //private string userId;
    private string roomId;
    private List<string> roomIds;
    private int roomIndex;

    #region Singleton
    private static SC_MenuLogic instance;
    public static SC_MenuLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_MenuLogic").GetComponent<SC_MenuLogic>();
            return instance;
        }
    }
    #endregion

    #region MonoBehaviour

    public void Awake()
    {
        InitMp();
        Init();
    }

    private void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        Listener.OnRoomsInRange += OnRoomsInRange;
        Listener.OnCreateRoom += OnCreateRoom;
        Listener.OnJoinRoom += OnJoinRoom;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        Listener.OnRoomsInRange -= OnRoomsInRange;
        Listener.OnCreateRoom -= OnCreateRoom;
        Listener.OnJoinRoom -= OnJoinRoom;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnGameStarted -= OnGameStarted;
    }


    #endregion

    #region Callbacks
    private void OnConnect(bool _IsSuccess)
    {
        Debug.Log("OnConnect " + _IsSuccess);
        if (_IsSuccess)
        {
            MpObjects["Btn_Play"].GetComponent<Button>().interactable = true;
            UpdateStatus("Connected.");
        }
        else
        {
            MpObjects["Btn_Play"].GetComponent<Button>().interactable = true;
            ResetLogic();
            UpdateStatus("Failed to Connect.");
        }
        //  WarpClient.GetInstance().chat
    }

    private void OnRoomsInRange(bool _IsSuccess, MatchedRoomsEvent eventObj)
    {
        Debug.Log("OnRoomsInRange " + _IsSuccess);
        if (_IsSuccess)
        {
            UpdateStatus("Parsing Rooms.");
            roomIds = new List<string>();
            foreach (var RoomData in eventObj.getRoomsData())
            {
                Debug.Log("Room Id: " + RoomData.getId());
                Debug.Log("Room Owner: " + RoomData.getRoomOwner());
                roomIds.Add(RoomData.getId());
            }

            roomIndex = 0;
            DoRoomsSearchLogic();
        }
        else
        {
            UpdateStatus("Error fetching room data");
            MpObjects["Btn_Play"].GetComponent<Button>().interactable = true;
        }     
    }

    private void OnCreateRoom(bool _IsSuccess, string _RoomId)
    {
        Debug.Log("OnCreateRoom: " + _IsSuccess + ", RoomId: " + _RoomId);
        if (_IsSuccess)
        {
            roomId = _RoomId;
            MpObjects["Txt_RoomId"].GetComponent<Text>().text = "RoomId: " + roomId;
            UpdateStatus("Room have been created, RoomId: " + roomId);
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
        }
    }

    private void OnJoinRoom(bool _IsSuccess, string _RoomId)
    {
        Debug.Log("OnJoinRoom: " + _IsSuccess + ", RoomId: " + _RoomId);
        if (_IsSuccess)
            UpdateStatus("Joined Room: " + _RoomId + ", Waiting for an opponent...");
        else UpdateStatus("Failed to join room: " + _RoomId);
    }

    private void OnGetLiveRoomInfo(LiveRoomInfoEvent eventObj)
    {
        Debug.Log("OnGetLiveRoomInfo " + eventObj.getProperties());
        if (eventObj != null && eventObj.getProperties() != null && eventObj.getProperties().ContainsKey("Password") &&
           eventObj.getProperties()["Password"].ToString() == passedParams["Password"].ToString())
        {
            Debug.Log("Matched Room!");
            roomId = eventObj.getData().getId();
            UpdateStatus("Recived Room info, joining room... (" + roomId + ")");
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
        }
        else
        {
            roomIndex++;
            DoRoomsSearchLogic();
        }
    }

    private void OnUserJoinRoom(RoomData eventObj, string _UserName)
    {
        Debug.Log("User joined room: " + _UserName);
        UpdateStatus("User joined room: " + _UserName);
        if (eventObj.getRoomOwner() == SC_GlobalVariables.userId && SC_GlobalVariables.userId != _UserName)
        {
            UpdateStatus("Starting game...");
            WarpClient.GetInstance().startGame();
        }
        MpObjects["Btn_Play"].GetComponent<Button>().interactable = true;
    }

    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        UpdateStatus("The game have started, nextTurn: " + _NextTurn);
        StartCoroutine(LeaveMenuToBattleSystem("BattleSystem", true));
    }
    #endregion

    #region Logic
    private void Init()
    {
        pathScreens = new Stack<GameObject>();
        unityObjects = new Dictionary<string, GameObject>();
        SliderObject = new Dictionary<string, GameObject>();
        GameObject[] _slide = GameObject.FindGameObjectsWithTag("SliderObject");
        foreach (GameObject g in _slide)
        {
            SliderObject.Add(g.name, g);
        }

        GameObject[] _objs = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _objs)
        {
            unityObjects.Add(g.name, g);
            if (g.name != "Screen_MainMenu")
                unityObjects[g.name].SetActive(false);
            else pathScreens.Push(g);
        }
    }


    //multiplayer
    private void InitMp()
    {
        MpObjects = new Dictionary<string, GameObject>();
        GameObject[] _MpObjects = GameObject.FindGameObjectsWithTag("MpObjects");
        foreach (GameObject g in _MpObjects)
            MpObjects.Add(g.name, g);

        passedParams = new Dictionary<string, object>();
        passedParams.Add("Password", "Shenkar");

        if (listner == null)
            listner = new Listener();

        WarpClient.initialize(apiKey, secretKey);
        WarpClient.GetInstance().AddConnectionRequestListener(listner);
        WarpClient.GetInstance().AddChatRequestListener(listner);
        WarpClient.GetInstance().AddUpdateRequestListener(listner);
        WarpClient.GetInstance().AddLobbyRequestListener(listner);
        WarpClient.GetInstance().AddNotificationListener(listner);
        WarpClient.GetInstance().AddRoomRequestListener(listner);
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(listner);
        WarpClient.GetInstance().AddZoneRequestListener(listner);

        SC_GlobalVariables.userId = System.DateTime.Now.Ticks.ToString();
        MpObjects["Txt_UserId"].GetComponent<Text>().text = "UserId: " + SC_GlobalVariables.userId;

        WarpClient.GetInstance().Connect(SC_GlobalVariables.userId);
        UpdateStatus("Open Connection...");

    }

    public void ResetLogic()
    {
        if (roomId != string.Empty)
            WarpClient.GetInstance().DeleteRoom(roomId);

        UpdateStatus("Conecting...");
        if (roomIds != null && roomIds.Count > 0)
            roomIds.Clear();
        roomIndex = 0;
        roomId = "";
        MpObjects["Btn_Play"].GetComponent<Button>().interactable = true;
    }

    private void UpdateStatus(string _Message)
    {
        MpObjects["Txt_Status"].GetComponent<Text>().text = _Message;
    }

    private void DoRoomsSearchLogic()
    {
        //Check if there are rooms to lookup
        if (roomIndex < roomIds.Count)
        {
            UpdateStatus("Bring room Info (" + roomIds[roomIndex] + ")");
            WarpClient.GetInstance().GetLiveRoomInfo(roomIds[roomIndex]);
        }
        else //No rooms create a new room
        {
            UpdateStatus("Creating Room...");
            WarpClient.GetInstance().CreateTurnRoom("Test", SC_GlobalVariables.userId, 2, passedParams, 30);
        }
    }


    #endregion

    #region Controller
    public void Btn_Screen(string _Screen)
    {
        ChangeScreen(_Screen);
    }

    public void ChangeScreen(string _NewScreen)
    {
        //Pop
        GameObject _tmp = pathScreens.Pop();
        //Push
        pathScreens.Push(_tmp);
        //save the top element in _tmp
        //Push new Screen
        pathScreens.Push(unityObjects[_NewScreen]);
        _tmp.SetActive(false);
        unityObjects[_NewScreen].SetActive(true);
    }

    IEnumerator LeaveMenuToBattleSystem(string _NewScreen, bool multiMode)
    {
        unityObjects[_NewScreen].SetActive(true);
        yield return new WaitForSeconds(0.1f);

        if (multiMode)
        {
            FindObjectOfType<BattleSystem>().gameType = BattleSystem.GameType.MultiPlayer;
        }
        else
        {
            FindObjectOfType<BattleSystem>().gameType = BattleSystem.GameType.SinglePlayer;
        }

        GameObject.Find("Menu").SetActive(false);
    }

    public void Btn_SinglePlayer()
    {
        Debug.Log("click");
        StartCoroutine(LeaveMenuToBattleSystem("BattleSystem", false));
    }

    public void Btn_MultiPlayer()
    {
        Debug.Log("Multi");
        ChangeScreen("Screen_Connection");
    }

    public void Btn_Student()
    {
        Debug.Log("Student");
        ChangeScreen("Screen_StudentInfo");
    }

    public void Btn_Options()
    {
        Debug.Log("Options");
        ChangeScreen("Screen_Options");
    }

    public void Btn_Site()
    {
        Application.OpenURL("http://ynet.co.il");
    }

    public void Btn_BackLogic()
    {
        string _curScreen = string.Empty;
        // pop Screen 
        unityObjects[_curScreen = pathScreens.Pop().name].SetActive(false);
        //pop top element 
        GameObject _tmp = pathScreens.Pop();
        //Push it again to the Stack
        pathScreens.Push(_tmp);
        unityObjects[_tmp.name].SetActive(true);
    }

    public void Slider_MultiPlayer()
    {
         int _value = Mathf.RoundToInt(SliderObject["Slider_MultiPlayer"].GetComponent<Slider>().value);
         SliderObject["Txt_NumberPlayers"].GetComponent<Text>().text = _value.ToString();
        //2 ways but comment one is not efficiency so i changed to dictionary
        //  int _value = Mathf.RoundToInt( GameObject.Find("Slider_MultiPlayer").GetComponent<Slider>().value);
        // GameObject.Find("Txt_NumberPlayers").GetComponent<Text>().text = _value.ToString();
    }

    public void Btn_Play()
    {
        Debug.Log("Btn_Play");
        MpObjects["Btn_Play"].GetComponent<Button>().interactable = false;
        WarpClient.GetInstance().GetRoomsInRange(1, 2);
        UpdateStatus("Searching for an available room...");
    }

    public void Btn_Go_Out()
    {
        WarpClient.GetInstance().DeleteRoom(roomId);
        UpdateStatus("connected.");

        if (roomIds != null && roomIds.Count > 0)
            roomIds.Clear();
        roomIndex = 0;
        roomId = "";
        MpObjects["Btn_Play"].GetComponent<Button>().interactable = true;

        Btn_BackLogic();
    }
    #endregion
}
