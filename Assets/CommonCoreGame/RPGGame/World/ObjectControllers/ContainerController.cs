using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;
using CommonCore.State;
using CommonCore.ObjectActions;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.UI;
using CommonCore.World;

namespace CommonCore.RpgGame.World
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

        protected override bool DeferComponentInitToSubclass => true;

        public override void Start()
        {
            base.Start();

            if (!UseLocalContainer)
                MyContainer = GameState.Instance.ContainerState[SharedContainer];
            else if (MyContainer == null) //should fix...
                MyContainer = SerializableContainerModel.MakeContainerModel(LocalContainer); //called after and overriding save?

            TryExecuteOnComponents(component => component.Init(this));
            Initialized = true;
        }

        public void InvokeContainer(ActionInvokerData data) //for invoking with an action special etc
        {
            if (data.Activator == null || !(data.Activator is PlayerController))
                return;

            //Debug.Log("Opened container");

            ContainerModal.PushModal(GameState.Instance.PlayerRpgState.Inventory, MyContainer, IsStore, null);
        }
        

        //persistence
        //should be fixed now
        public override void RestoreEntityData(Dictionary<string, object> data) //this either isn't called or doesn't work
        {
            base.RestoreEntityData(data);

            if (UseLocalContainer && UsePersistentContainer)
                MyContainer = SerializableContainerModel.MakeContainerModel((SerializableContainerModel)data["Container"]);
        }

        public override Dictionary<string, object> CommitEntityData() //this works
        {
            var data = base.CommitEntityData();

            if (UseLocalContainer && UsePersistentContainer)
                data.Add("Container", SerializableContainerModel.MakeSerializableContainerModel(MyContainer));

            return data;
        }

    }
}