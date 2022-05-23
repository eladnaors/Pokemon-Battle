using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_MenuController : MonoBehaviour
{
    public SC_MenuLogic sc_MenuLogic;
    public void Btn_SinglePlayer()
    {
        SC_MenuLogic.Instance.Btn_SinglePlayer();
    }

    public void Slider_MultiPlayer()
    {
        SC_MenuLogic.Instance.Slider_MultiPlayer();     
    }

    public void Btn_MultiPlayer()
    {
        SC_MenuLogic.Instance.Btn_MultiPlayer();
    }

    public void Btn_BackLogic()
    {
        SC_MenuLogic.Instance.Btn_Go_Out();
    }

    public void Btn_Student()
    {
        SC_MenuLogic.Instance.Btn_Student();
    }

    public void Btn_Options()
    {
        SC_MenuLogic.Instance.Btn_Options();
    }

    public void Btn_Site()
    {
        SC_MenuLogic.Instance.Btn_Site();
    }

    public void Btn_Connection()
    {
        SC_MenuLogic.Instance.Btn_Play();
    }
}

