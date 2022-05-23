using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SC_CharacterManager : MonoBehaviour
{
    #region SerializeField
    [SerializeField] PokemonParty playersParty;
    [SerializeField] PokemonParty AiParty;
    public SC_CharacterDataBase characterDB;
    public Text nameText;
    public Image artworkSprite;
    private int selectedOption=0; // keep track on selected char
    private List<PokemonBase> currentPokemon= new List<PokemonBase>(); //new list pokemon base not null
    private List<PokemonBase> AiPokemonData = new List<PokemonBase>(); 
    const int maxParty = 3;
    [SerializeField]
    List<Image> chosenPokemon;
    [SerializeField] Sprite emptypokemonSprite;
    [SerializeField] SC_GameController gameController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] GameObject screenSelectionCanvas;
    [SerializeField] GameObject Battle;
    [SerializeField] GameObject BattleManager;
    [SerializeField] GameObject MainMenu;
    [SerializeField] Button Next;
    [SerializeField] Button Back;
    [SerializeField] Button SelectBtn;
    [SerializeField] Text Select;

    List<int> selectedPokemonIndex = new List<int>();
    bool moveCompleted = false;
    bool isMyMove = false;
    bool pokemonSent = false;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        Listener.OnMoveCompleted += MoveCompleted;
    }

    void Start()
    {
        if(!PlayerPrefs.HasKey("selectedOption"))
        {
            selectedOption = 0;
        }
        else
        {
            Load();
        }
        openSystem();
    }
    #endregion

    //
    public void MoveCompleted(MoveEvent _Move)
    {
        Debug.Log("OnMoveCompleted " + _Move.getNextTurn() + " " + _Move.getMoveData());
        if (_Move.getSender() != SC_GlobalVariables.userId && _Move.getMoveData() != null)
        {
            Dictionary<string, object> _data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_Move.getMoveData());
            if (_data.ContainsKey("IndexOne"))
            {
                int _index = int.Parse(_data["IndexOne"].ToString());
                AiPokemonData.Add(characterDB.GetCharacter(_index).pokemonbase);
                Debug.Log("Added Pokemon Index " + _index);
            }
            if (_data.ContainsKey("IndexTwo"))
            {
                int _index = int.Parse(_data["IndexTwo"].ToString());
                AiPokemonData.Add(characterDB.GetCharacter(_index).pokemonbase);
                Debug.Log("Added Pokemon Index " + _index);
            }
            if (_data.ContainsKey("IndexThree"))
            {
                int _index = int.Parse(_data["IndexThree"].ToString());
                AiPokemonData.Add(characterDB.GetCharacter(_index).pokemonbase);
                Debug.Log("Added Pokemon Index " + _index);
            }

            if (_data.ContainsKey("IndexOne"))
            {
                moveCompleted = true;
                // Change moveCompleted bool and move to battle scene
            }
        }
        else if (_Move.getSender() == SC_GlobalVariables.userId && _Move.getMoveData() != null)
        {
            Dictionary<string, object> _data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_Move.getMoveData());
            if (_data.ContainsKey("IndexOne"))
            {
                pokemonSent = true;
            }
        }

        if (_Move.getNextTurn() == SC_GlobalVariables.userId)
            isMyMove = true;
        else isMyMove = false;
    }

    #region resetMenu
    public void openSystem()
    {
        UpdateCharacter(selectedOption);
        Next.interactable = true;
        Back.interactable = true;
        Select.text = "I choose You !";
        SelectBtn.interactable = true;
        AiPokemonData.Clear();
        currentPokemon.Clear();
        selectedPokemonIndex.Clear();
        updateParty(); //fill in images

        moveCompleted = false;
        isMyMove = false;
        pokemonSent = false;
    }
    #endregion

    #region buttons
    public void NextOption()
    {
        selectedOption++;
        if (selectedOption>=characterDB.Charactercount)
        {
            selectedOption = 0;
        }

        UpdateCharacter(selectedOption);
        Save();
    }

    public void BackOption()
    {
        selectedOption--;
        if (selectedOption < 0)
        {
            selectedOption = characterDB.Charactercount - 1;
        }
        UpdateCharacter(selectedOption);
        Save();
    }

    public void BackToMainMenu()
    {
        openSystem();
        MainMenu.SetActive(true);

        SC_MenuLogic.Instance.ResetLogic();
        BattleManager.SetActive(false);
    }

    public void ChooseOption()
    {
        if (battleSystem.gameType == BattleSystem.GameType.MultiPlayer)
        {
            //If Multiplayer
            if (currentPokemon.Count < maxParty)
            {
                currentPokemon.Add(characterDB.GetCharacter(selectedOption).pokemonbase);
                updateParty();

                //Saving selected Pokemon index
                selectedPokemonIndex.Add(selectedOption);

                if (currentPokemon.Count == 3)
                {
                    Select.text = "Play!";
                    Next.interactable = false;
                    Back.interactable = false;
                }
            }
            else
            {
                Select.text = "Waiting for other player";
                SelectBtn.interactable = false;

                SendPokemonInformation();
                //Sending pokemon information

                StartCoroutine(MultiPlayButtonClick());
            }
        }
        else 
        {
            //If Singleplayer
            if (currentPokemon.Count < maxParty)
            {
                AiPokemonData.Add(characterDB.GetCharacter(UnityEngine.Random.Range(0, 10)).pokemonbase); //for ai
                currentPokemon.Add(characterDB.GetCharacter(selectedOption).pokemonbase);
                updateParty();

                if (currentPokemon.Count == 3)
                {
                    Select.text = "Play!";
                    Next.interactable = false;
                    Back.interactable = false;
                }
            }
            else
            {
                playersParty.setTeam(currentPokemon);
                //for ai
                AiParty.setTeam(AiPokemonData);

                Debug.Log("SinglePlayer battle Started");
                gameController.StartSinglePlayerBattle();

                screenSelectionCanvas.SetActive(false);
                Battle.gameObject.SetActive(true); //after finish battle... and we back to the select menu so need to activate again
            }
        }
    }

    private void SendPokemonInformation()
    {
        //Multiplayer pokemon selection Synconization
        //Sending pokemon information to other client
        Dictionary<string, object> _toSend = new Dictionary<string, object>();
        _toSend.Add("IndexOne", selectedPokemonIndex[0]);
        _toSend.Add("IndexTwo", selectedPokemonIndex[1]);
        _toSend.Add("IndexThree", selectedPokemonIndex[2]);
        string _toJson = MiniJSON.Json.Serialize(_toSend);
        WarpClient.GetInstance().sendMove(_toJson);

        Debug.Log("Send pokemon One");
    }

    #endregion
    IEnumerator MultiPlayButtonClick()
    {
        while (pokemonSent == false)
        {
            yield return new WaitForSeconds(1f);
            if (pokemonSent == true)
                break;

            //Multiplayer pokemon selection Synconization
            //Sending pokemon information to other client
            Dictionary<string, object> _toSend = new Dictionary<string, object>();
            _toSend.Add("IndexOne", selectedPokemonIndex[0]);
            _toSend.Add("IndexTwo", selectedPokemonIndex[1]);
            _toSend.Add("IndexThree", selectedPokemonIndex[2]);
            string _toJson = MiniJSON.Json.Serialize(_toSend);
            WarpClient.GetInstance().sendMove(_toJson);

            Debug.Log("Send pokemon Two");
            //This is for backup, if somehow pokemon information is not sent the first time it will loop untill it is sent.
        }

        yield return new WaitForSeconds(0.1f);

        //Waiting for other player
        yield return new WaitUntil(() => moveCompleted == true);

        Select.text = "Play!";
        SelectBtn.interactable = true;

        playersParty.setTeam(currentPokemon);
        AiParty.setTeam(AiPokemonData);

        Debug.Log("Multiplayer battle Started");
        gameController.StartMultiPlayerBattle(isMyMove);

        screenSelectionCanvas.SetActive(false);
        Battle.gameObject.SetActive(true); //after finish battle... and we back to the select menu so need to activate again
    }

    #region MenuUpdates
    void updateParty()
    {
        for (int i = 0; i < maxParty; i++)
        {
            if (currentPokemon.Count > i)
            {
                chosenPokemon[i].sprite = currentPokemon[i].FrontSprite;
            }
            else
            {
                chosenPokemon[i].sprite = emptypokemonSprite;
            }
        }
    }

    private void UpdateCharacter(int selectedOption)
    {
        SC_SelectPokemon character = characterDB.GetCharacter(selectedOption);
        artworkSprite.sprite = character.characterSprite;
        nameText.text = character.characterName;
    }

    private void Load()
    {
        selectedOption = PlayerPrefs.GetInt("selectedOption");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("selectedOption", selectedOption);
    }
    #endregion
}
