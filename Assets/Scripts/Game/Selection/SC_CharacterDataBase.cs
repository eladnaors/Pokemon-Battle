using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SC_CharacterDataBase : ScriptableObject
{
    public SC_SelectPokemon[] character; //many pokemons
    public int Charactercount
    {
        get
        {

            return character.Length;
        }
    }

    public SC_SelectPokemon GetCharacter(int index)
    {

        return character[index];
    }
}
