using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FX_IncrementalNumber : MonoBehaviour
{

    private TextMeshProUGUI fieldTarget;

    public float duration = 0.85f;
    private int currentValue;

    void Start()
    {
        fieldTarget = this.gameObject.GetComponent<TextMeshProUGUI>();
        if (int.TryParse(fieldTarget.text, out currentValue))
        {
            //current value is now current value.
            float d = Random.Range(0.35f, 0.71f);
            LeanTween.value(gameObject, 0, (float)currentValue, d).setOnUpdate((float val)=>{
            Debug.Log("tweened val:"+val);
            int intval = Mathf.RoundToInt(val);
            fieldTarget.text = intval.ToString();
            } );
        }   
        else
        {

        }
    }

    public void Incrementalize(int amountToChange){

        

    }
}