using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_MenuMusic : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    public void changeVolume()
    {
        AudioListener.volume = volumeSlider.value;

    }

}
