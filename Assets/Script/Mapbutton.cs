using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapbutton : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Buttonpanel;
    public void showbuttonpanel()
    {
        Buttonpanel.SetActive(true);
    }
}
