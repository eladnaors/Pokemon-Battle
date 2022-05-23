using AssemblyCSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//public enum GameState { Menu, Battle }

public class SC_GameController : MonoBehaviour
{
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] GameObject playerTeam1;
    [SerializeField] GameObject playerTeam2;
    [SerializeField] GameObject battle;
    [SerializeField] GameObject MenuSelection;
    [SerializeField] GameObject Loading;

    // GameState state;
    // Start is called before the first frame update
    private void Start()
    {
        battleSystem.onBattleOver += EndBattle;
    }

    void EndBattle(bool win)
    {
        //state = GameState.Menu;
        battle.gameObject.SetActive(false);
        MenuSelection.gameObject.SetActive(true);
    }
  
    public void StartSinglePlayerBattle()
    {
        //state = GameState.Battle;
        StartCoroutine(LoadingScreen(Loading));
        battleSystem.gameObject.SetActive(true);
        var playerParty = playerTeam1.GetComponent<PokemonParty>();
        var wildPokemon = playerTeam2.GetComponent<PokemonParty>();
        battleSystem.StartBattleSinglePlayer(playerParty, wildPokemon);
    }

    public void StartMultiPlayerBattle(bool turn)
    {
        //state = GameState.Battle;
        StartCoroutine(LoadingScreen(Loading));
        battleSystem.gameObject.SetActive(true);
        var playerParty = playerTeam1.GetComponent<PokemonParty>();
        var wildPokemon = playerTeam2.GetComponent<PokemonParty>();
        battleSystem.StartBattleMultiPlayer(playerParty, wildPokemon, turn);
    }

    public void Update()
    {
        battleSystem.HandleUpdate();
    }

    IEnumerator LoadingScreen(GameObject loading)
    {
        battle.gameObject.SetActive(false);
        loading.SetActive(true);
        yield return new WaitForSeconds(1f);
        loading.SetActive(false);
    }
}
