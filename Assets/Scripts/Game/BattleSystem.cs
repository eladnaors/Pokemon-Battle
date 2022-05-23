using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    public enum GameType
    {
        SinglePlayer, MultiPlayer
    };

    #region Declares
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] HPBar playerHp;
    [SerializeField] HPBar enemyHp;
    [SerializeField] GameObject screenSelectionCanvas;
    [SerializeField] Text timerTxt;
    [SerializeField] GameObject Menu;

    BattleState state;
    public GameType gameType;
    int currentAction;
    int currentMove;
    PokemonParty playerParty;
    PokemonParty wildPokemon;

    [SerializeField] SC_CharacterManager charMan; //could use here singleton
    public event Action<bool> onBattleOver;// to know if battle is over
    #endregion

    private bool isMatchOver = false;
    private bool isGameStarted = false;

    bool myturn = true;
    int playerPokeIndex = 0;
    int enemyPokeIndex = 0;

    private float startTime = 0;

    private void Awake()
    {
        Listener.OnMoveCompleted += MoveCompleted;
        Listener.OnGameStarted += OnGameStarted;
    }

    private void Update()
    {
        //Update Timer only for multiplayer mode
        if (!isMatchOver && isGameStarted && gameType == GameType.MultiPlayer)
        {
            int _current = SC_GlobalVariables.maxTurnTime - (int)(Time.time - startTime);
            if (_current < 0)
            {
                timerTxt.text = "0";
                if (myturn == true)
                {    
                    int value = 1;
                    Dictionary<string, object> _toSend = new Dictionary<string, object>();
                    _toSend.Add("TimeUp", value);
                    string _toJson = MiniJSON.Json.Serialize(_toSend);
                    WarpClient.GetInstance().sendMove(_toJson);
                    //Sending Time up information to all client
                }
            }
            else
            {
                timerTxt.text = _current.ToString();
            }
        }
        else
        {
            timerTxt.text = "";
        }
    }

    #region Start and setup
    public void StartBattleSinglePlayer(PokemonParty playerParty, PokemonParty wildPokemon)
    {
        isMatchOver = false;

        playerPokeIndex = 0;
        enemyPokeIndex = 0;

        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        this.wildPokemon.healParty();
        StartCoroutine(SetupBattle(true));
    }

    public void StartBattleMultiPlayer(PokemonParty playerParty, PokemonParty wildPokemon, bool turn)
    {
        playerPokeIndex = 0;
        enemyPokeIndex = 0;

        isMatchOver = false;

        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        this.playerParty.healParty();
        this.wildPokemon.healParty();
        StartCoroutine(SetupBattle(turn));
    }

    public IEnumerator SetupBattle(bool turn)
    {
        if (gameType == GameType.SinglePlayer)
        {
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon.GetHealthyPokemon());
        }
        else
        {
            //Multiplayer Battle Setup
            playerUnit.Setup(playerParty.GetPokemon(playerPokeIndex));
            enemyUnit.Setup(wildPokemon.GetPokemon(enemyPokeIndex));
        }

        playerHp.SetData(playerUnit.Pokemon);
        enemyHp.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return new WaitForSeconds(1f);
        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared.");
        yield return new WaitForSeconds(1f);


        if (gameType == GameType.SinglePlayer)
        {
            PlayerAction();
        }
        else
        {
            //Multiplayer Battle Setup
            myturn = turn;
            if (myturn)
                PlayerAction();
            else
                EnemyAction();
        }
    }
    #endregion

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);
        dialogBox.EnableDialogText(true);
        dialogBox.EnableMoveSelector(false);
    }

    void EnemyAction()
    {
        StartCoroutine(dialogBox.TypeDialog("Enemy's turn"));
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        startTime = Time.time;
        isGameStarted = true;

        enemyPokeIndex = 0;
        playerPokeIndex = 0;
    }

    #region Enemy and Player Moves
    public void MoveCompleted(MoveEvent _Move)
    {
        Debug.Log("OnMoveCompleted " + _Move.getNextTurn() + " " + _Move.getMoveData());
        if (_Move.getSender() != SC_GlobalVariables.userId && _Move.getMoveData() != null)
        {
            Dictionary<string, object> _data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_Move.getMoveData());
            if (_data.ContainsKey("Move"))
            {
                int _index = int.Parse(_data["Move"].ToString());

                StartCoroutine(MultiPerformEnemyMove(_index));
                Debug.Log("Moved Action " + _index);
                //Perform move on the client
            }
            else if (_data.ContainsKey("TimeUp"))
            {
                int _index = int.Parse(_data["TimeUp"].ToString());
                if (_index >= 0)
                {
                    StartCoroutine(Co_TimedOut());
                }
                Debug.Log("Timed Out!" + _index);
                //Timed up information received
            }
        }
        else if (_Move.getMoveData() == null)
        {
            StartCoroutine(Co_TimedOut());
            Debug.Log("Timed out No DATA.");
            //Incase timed up information was not received still switch turn
        }

        startTime = Time.time;
        //Resetting time every turn

        if (_Move.getNextTurn() == SC_GlobalVariables.userId)
            myturn = true;
        else myturn = false;

        //Changing turn each move completed
    }

    IEnumerator Co_TimedOut()
    {
        yield return new WaitForSeconds(0.2f);
        if (myturn)
        {
            PlayerAction();
        }
        else
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableActionSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(dialogBox.TypeDialog("Enemy's turn"));
        }
    }

    //Multiplayer move sending information
    void MultiplayerPerformMove()
    {
        Dictionary<string, object> _toSend = new Dictionary<string, object>();
        _toSend.Add("Move", currentMove);
        string _toJson = MiniJSON.Json.Serialize(_toSend);
        WarpClient.GetInstance().sendMove(_toJson);
    }

    //Normal player move
    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used { move.Base.Name}");
        yield return new WaitForSeconds(1f);
        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        enemyHp.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} Fainted");
            //enemyUnit.Disappear();
            enemyPokeIndex++;

            Pokemon nextPokemon = null;

            if (gameType == GameType.SinglePlayer)
            {
                nextPokemon = wildPokemon.GetHealthyPokemon();
            }
            else
            {
                if(enemyPokeIndex < wildPokemon.GetPokemonCount())
                    nextPokemon = wildPokemon.GetPokemon(enemyPokeIndex);
            }

            yield return new WaitForEndOfFrame();

            if (nextPokemon != null)
            {
                enemyUnit.Setup(nextPokemon);
                enemyHp.SetData(nextPokemon);
                //dialogBox.SetMoveNames(nextPokemon.Moves);

                yield return dialogBox.TypeDialog($"Enemy Next pokemon is {nextPokemon.Base.Name} !.");

                if(gameType == GameType.SinglePlayer)
                    PlayerAction(); // after enemy pokemons faints so user can make an action

                //Write enemy's turn in multiplayer mode
                if (gameType == GameType.MultiPlayer)
                    StartCoroutine(dialogBox.TypeDialog("Enemy's turn"));
            }
            else
            {
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Wins!");
                isMatchOver = true;
                yield return new WaitForSeconds(2f);
                onBattleOver(false); //new
                charMan.openSystem();
            }
        }
        else
        {
            //AI play only in singleplayer mode
            if (gameType == GameType.SinglePlayer)
                StartCoroutine(EnemyMove());
            else
                StartCoroutine(dialogBox.TypeDialog("Enemy's turn"));

            //Write enemy's turn in multiplayer mode
        }
    }

    //Multiplayer enemy move
    IEnumerator MultiPerformEnemyMove(int moveIn)
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.Moves[moveIn];
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}");

        yield return new WaitForSeconds(1f);

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        playerHp.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Fainted");
            // playerUnit.Disappear();

            Pokemon nextPokemon = null;
            playerPokeIndex++;

            if (gameType == GameType.SinglePlayer)
            {
                nextPokemon = playerParty.GetHealthyPokemon();
            }
            else
            {
                if(playerPokeIndex < playerParty.GetPokemonCount())
                    nextPokemon = playerParty.GetPokemon(playerPokeIndex);
            }


            yield return new WaitForEndOfFrame();

            if (nextPokemon != null)
            {
                playerUnit.Setup(nextPokemon);
                playerHp.SetData(nextPokemon);
                dialogBox.SetMoveNames(nextPokemon.Moves);

                yield return dialogBox.TypeDialog($"Your Next pokemon is {nextPokemon.Base.Name} !.");

                PlayerAction(); //fight or exit ?
            }
            else
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} Wins!");
                isMatchOver = true;
                yield return new WaitForSeconds(2f);
                onBattleOver(false); //new
                charMan.openSystem();
            }
        }
        else
        {
            PlayerAction();
        }
    }

    //Singleplayer enemy move
    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}");

        yield return new WaitForSeconds(1f);

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        playerHp.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Fainted");
            // playerUnit.Disappear();
            var nextPokemon = playerParty.GetHealthyPokemon();

            yield return new WaitForEndOfFrame();

            if (nextPokemon != null)
            {
                playerUnit.Setup(nextPokemon);
                playerHp.SetData(nextPokemon);
                dialogBox.SetMoveNames(nextPokemon.Moves);

                yield return dialogBox.TypeDialog($"Your Next pokemon is {nextPokemon.Base.Name} !.");

                PlayerAction(); //fight or exit ?
            }
            else
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} Wins!");
                yield return new WaitForSeconds(2f);
                onBattleOver(false); //new
                charMan.openSystem();
            }
        }
        else
        {
            PlayerAction();
        }
    }

    #endregion

    IEnumerator ShowDamageDetails(Pokemon.DamageDetails damageDetails)
    {
        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective!");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //Go to menu for multiplayer mode
                if (Menu != null && gameType == GameType.MultiPlayer)
                {
                    Menu.SetActive(true);
                    SC_MenuLogic.Instance.ResetLogic();
                    transform.gameObject.SetActive(false);
                }
                screenSelectionCanvas.SetActive(true);
                charMan.openSystem();
            }
        }
    }

    #region MoveSelection
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (myturn)
            {
                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(PerformPlayerMove());

                if (gameType == GameType.MultiPlayer)
                    MultiplayerPerformMove();
            }
        }
    }
        #endregion

}
