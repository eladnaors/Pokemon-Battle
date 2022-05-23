using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // to show classes in inspector
public class Pokemon
{
    [SerializeField] PokemonBase _base;

    public PokemonBase Base
    {
        get
        {
            return _base;
        }
        private set
        {
            _base = value;
        }
    }
    
    public List<Move> Moves { get; set; }

    public void Init()
    {
        HP = MaxHp;
        
        // Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {     
                Moves.Add(new Move(move.Base));       
        }
    }

    public Pokemon(PokemonBase pokemonbase)
    {
        //we gonna know what pokemon inside there
        Base = pokemonbase;
        Init();
    }

    //all this info same as pokemon fire red
    #region Stats
    public int Attack {
        get { return Mathf.FloorToInt((Base.Attack ) / 100f) + 5; } // in the pokemon fire red the base attack is * lvl but here lvls are not needed
    }

    public int Defense {
        get { return Mathf.FloorToInt((Base.Defense ) / 100f) + 5; }
    }

    public int MaxHp {
        get {
            return Mathf.FloorToInt((Base.MaxHp ) / 100f) + 10;
        }
    }



    public int HP { get; set; }
    #endregion
    public void healPokemon()
    {
        HP = MaxHp;
    }

    #region damage
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        //pokemon can have 2 types
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Fainted = false
        };

        //calculations from the game pokemon fire red.. this is how they calculate damage
        //float modifiers  = Random.Range(0.85f, 1f)*type; //type effectiveness
        float modifiers = type;
        float a = (2 * 1 + 10) / 250f; // instead of 1 it wass 2*level - lets say its 1
        float d = a * move.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if (HP <= 0) //pokemon fainted
        {
            HP = 0; // so negative will not be shown in ui
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    public class DamageDetails
    {
        public bool Fainted { get; set; }
        public float TypeEffectiveness { get; set; }
    }

    #endregion

    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }


    public void ResetHP()
    {
        HP = MaxHp;
    }
}
