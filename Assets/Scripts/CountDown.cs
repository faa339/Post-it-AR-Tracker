using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDown : MonoBehaviour
{
    TextMeshProUGUI countdown;
    bool start = false;
    List<float> times = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        countdown = GetComponentInChildren<TextMeshProUGUI>();
        times.Add(0);
        times.Add(0);
        times.Add(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (start) //que the janky timer code lol 
        {
            if (times[2] <= 0) { //seconds
                if (times[1] <= 0) //mins 
                {
                    if (times[0] <= 0) // hours (reset moment)
                    {
                        times[0] = 0;
                        times[1] = 0;
                        times[2] = 0;
                        start = false;
                    }
                    else //rollback hrs by 1, reset mins
                    {
                        times[0] -= 1;
                        times[1] = 59;
                        times[2] = 59;
                    }
                }
                else { //rollback mins by 1, reset secs 
                    times[1] -= 1;
                    times[2] = 59;
                }
            }
            else //standard secs subtract 
            {
                times[2] -= Time.deltaTime;
            }
            countdown.text = string.Format("{0:00.}:{1:00.}:{2:00.}", times[0], times[1], Mathf.Floor(times[2]));
        }
    }

    public void ParseInput(string toParse)
    {
        // parse the text based on whats passed in (passed from the input 
        // controller)
        start = false;
        string[] inputSplit = toParse.Split(':');
        if (inputSplit.Length != 3)
        {
            //countdown.text = "input split len=" + inputSplit.Length;
            return;
        }

        for (int i = 0; i < inputSplit.Length; i++)
        {
            int parseAtmpt;
            if (int.TryParse(inputSplit[i], out parseAtmpt) == false)
            {
                //countdown.text = "parse failed";
                return;
            }
            //very easy way of doing this, handles both minutes and seconds
            //and caps hours at a reasonable amount lol
            times[i] = Mathf.Clamp(parseAtmpt,0, 59);
        }
        start = true;
    }

}
