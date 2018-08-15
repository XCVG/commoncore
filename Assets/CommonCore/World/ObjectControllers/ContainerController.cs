using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;
using CommonCore.State;
using CommonCore.Rpg;
using CommonCore.ObjectActions;

namespace CommonCore.World
{

    public class ContainerController : ThingController
    {
        //TODO easier setting of tooltip, with default

        public SerializableContainerModel LocalContainer;
        public string SharedContainer;

        public bool UseLocalContainer = true;
        public bool UsePersistentContainer = true;
        public bool IsStore = false; //TODO handling of store limitations, trading values, etc

        private ContainerModel MyContainer;

        public override void Start()
        {
            base.Start();

            if (!UseLocalContainer)
                MyContainer = GameState.Instance.ContainerState[SharedContainer];
            else
                MyContainer = SerializableContainerModel.MakeContainerModel(LocalContainer);
        }

        public void InvokeContainer(ActionInvokerData data) //for invoking with an action special etc
        {
            if (data.Activator == null || !(data.Activator is PlayerController))
                return;

            Debug.Log("Opened container");
            //TODO actually open container dialog
            UI.ContainerModal.PushModal(GameState.Instance.PlayerRpgState.Inventory, MyContainer, IsStore, null);
        }
        

        //persistence
        public override void SetExtraData(Dictionary<string, object> data)
        {
            if (UseLocalContainer && UsePersistentContainer)
                MyContainer = SerializableContainerModel.MakeContainerModel((SerializableContainerModel)data["Container"]);
        }

        public override Dictionary<string, object> GetExtraData()
        {
            var data = new Dictionary<string, object>();

            if (UseLocalContainer && UsePersistentContainer)
                data.Add("Container", SerializableContainerModel.MakeSerializableContainerModel(MyContainer));

            return data;
        }

    }
}