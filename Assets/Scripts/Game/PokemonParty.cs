using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    //Pokeman List
    [SerializeField] List<Pokemon> pokemons;

    void Awake()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    public void setTeam(List<PokemonBase> currentTeam)
    {
        pokemons = new List<Pokemon>();
        for (int i = 0; i < currentTeam.Count; i++)
        {
            pokemons.Add(new Pokemon(currentTeam[i]));
        }
    }

    public Pokemon GetHealthyPokemon()
    {      
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    //Getting Pokemon using index
    public Pokemon GetPokemon(int index)
    {
        return pokemons[index];
    }

    public int GetPokemonCount()
    {
        return pokemons.Count;
    }

    public void healParty()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            pokemons[i].healPokemon();
        }
    }
}


