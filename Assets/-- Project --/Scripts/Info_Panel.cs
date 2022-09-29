using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Info_Panel : MonoBehaviour
{
    [SerializeField]
    GameObject info_panel;

    //this function is called to enable and disable by just clicking on the nft
    public void Info_Panel_Visible()
    {
        if (info_panel.activeInHierarchy == true)
        {
            info_panel.SetActive(false);
        }
        else
        {
            info_panel.SetActive(true);
        }
    }
}
