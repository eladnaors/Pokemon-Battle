using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColor;
    
    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    
    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    string currentText = "";
    

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {

        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
    }

    //BackUp
    //public IEnumerator TypeDialog(string dialog)
    //{
    //    for (int i = 0; i < dialog.Length; i++)
    //    {
    //        currentText = dialog.Substring(0, i);
    //        dialogText.text = currentText;
    //        yield return new WaitForSeconds(1f / lettersPerSecond);
    //    }
    //}

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i=0; i<actionTexts.Count; ++i)
        {
            if (i == selectedAction)
                actionTexts[i].color = highlightedColor;
            else
                actionTexts[i].color = Color.black;
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i=0; i<moveTexts.Count; ++i)
        {
            if (i == selectedMove)
                moveTexts[i].color = highlightedColor;
            else
                moveTexts[i].color = Color.black;
        }

    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i=0; i<moveTexts.Count; ++i)
        {
           moveTexts[i].text = moves[i].Base.Name;
         
        }
    }
}
