using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        SetHP((float)pokemon.HP / pokemon.MaxHp);
    }


    public void UpdateHP()
    {
          SetHP((float)_pokemon.HP/_pokemon.MaxHp);
    }


}
