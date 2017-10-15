using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Robocodo.AI
{

    public class HumanCommunicator
    {

        public static void ShowPopupForAllHumans(string text, Vector3 position, TextPopup.ColorType colorType)
        {
            foreach (PlayerController player in FindHumanPlayerControllers())
                player.ShowPopupForOwner(text, position, colorType);
        }

        public static List<PlayerController> FindHumanPlayerControllers()
        {
            return WorldController.instance.FindPlayerControllers().Where(p => p.GetComponent<AI>() == null).ToList();
        }

    }

}