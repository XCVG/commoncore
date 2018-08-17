using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore;

namespace GameUI
{

    public class MainMenuMessageScript : MonoBehaviour
    {
        public Text ContentText;

        void Start()
        {
            ContentText.text = CCBaseUtil.LoadResource<TextAsset>("User/startmessage").text;
        }

    }
}