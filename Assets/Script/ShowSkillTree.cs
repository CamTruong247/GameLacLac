using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSkillTree : MonoBehaviour
{
    public GameObject skilltree;
    public GameObject skilltreedescription;
    // Start is called before the first frame update
    void Start()
    {
        /*skilltree.SetActive(true);
        skilltreedescription.SetActive(true);*/
    }

    // Update is called once per frame
    public void showSkillTree()
    {
        skilltree.SetActive(true);  // Ẩn skill tree
        skilltreedescription.SetActive(true); // Ẩn description
    }
}
