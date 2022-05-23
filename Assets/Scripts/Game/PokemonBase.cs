using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name {
        get { return name; }
    }


    public Sprite FrontSprite {
        get { return frontSprite; }
    }

    public Sprite BackSprite {
        get { return backSprite; }
    }

    public PokemonType Type1 {
        get { return type1; }
    }

    public PokemonType Type2 {
        get { return type2; }
    }

    public int MaxHp {
        get { return maxHp; }
    }

    public int Attack {
        get { return attack; }
    }


    public int Defense {
        get { return defense; }
    }



    public List<LearnableMove> LearnableMoves {
        get { return learnableMoves; }
    }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;

    public MoveBase Base {
        get { return moveBase; }
    }

}

public enum PokemonType
{
    
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon


}
public class TypeChart
{
    static float[][] chart =
    {
        //                  NOR   FIR   WAT   ELE  GRA  ICE  FIG  POI               
        /*NOR*/ new float[] { 1f,  1f,   1f,  1f,  1f,  1f,  1f,  1f },
        /*FIR*/ new float[] { 1f, 0.5f, 0.5f, 1f,  2f,  2f,  1f,  1f },
        /*WAT*/ new float[] { 1f,  2f,  0.5f, 2f, 0.5f, 1f,  1f,  1f },
        /*ELE*/ new float[] { 1f,  1f,  2f,  0.5f,0.5f, 2f,  1f,  1f },
        /*GRS*/ new float[] { 1f, 0.5f, 2f,   2f, 0.5f, 1f,  1f, 0.5f },
        /*POI*/ new float[] { 1f,  1f,   1f,  1f,  2f,  1f,  1f,  1f }
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}