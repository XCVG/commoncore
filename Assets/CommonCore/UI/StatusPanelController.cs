using CommonCore.Rpg;
using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    public class StatusPanelController : PanelController
    {
        public RawImage CharacterImage;
        public Text HealthText;
        public Text ArmorText;
        public Text AmmoText;

        public override void SignalPaint()
        {
            CharacterModel pModel = GameState.Instance.PlayerRpgState;
            //PlayerControl pControl = PlayerControl.Instance;

            //repaint 
            HealthText.text = string.Format("Health: {0}/{1}", (int) pModel.Health, (int) pModel.DerivedStats.MaxHealth);

            //this is now somewhat broken because there are more choices in the struct
            string rid = pModel.Gender == Sex.Female ? "portrait_f" : "portrait_m";
            CharacterImage.texture = Resources.Load<Texture2D>("UI/Portraits/" + rid);
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}