using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    public Pokemon Pokemon { get; set; }
    Image image;
    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Setup(Pokemon pokemon ) // i will pass the pokemon as a parameter
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
            image.sprite = Pokemon.Base.BackSprite;
        else
            image.sprite = Pokemon.Base.FrontSprite;
    }

    public void Disappear()
    {
        image.enabled = false;
    }
}
