using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideMapbutton : MonoBehaviour
{
    // Start is called before the first frame update
    public  GameObject Mapbutton;
    void Start()
    {
        Mapbutton.SetActive(false);
    }

    // Update is called once per frame
   public void hidemapbutton()
    {
        Mapbutton.SetActive(false);
    }
}
