using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;

namespace CommonCore.Rpg
{ 
    //TODO split into PlayerModel and CharacterModel
    public class PlayerModel
    {
        public float Health
        {
            get
            {
                return DerivedStats.MaxHealth * HealthFraction;
            }
            set
            {
                HealthFraction = Health / DerivedStats.MaxHealth;
            }
        }

        public float HealthFraction { get; set; }

        public StatsSet BaseStats { get; private set; }
        public StatsSet DerivedStats { get; private set; }

        public List<Condition> Conditions { get; private set; }

        public PlayerModel()
        {
            Conditions = new List<Condition>();

            //create blank stats and derive stats
            BaseStats = new StatsSet(); //TODO load defaults
            UpdateStats();
        }

        public void UpdateStats() //TODO link this into messaging at some point
        {
            //copy base stats
            DerivedStats = new StatsSet(BaseStats);

            //apply conditions
            foreach(Condition c in Conditions)
            {
                c.Apply(BaseStats, DerivedStats);
            }
        }

        public void SetAV<T>(string av, T value)
        {
            SetAV(av, value, null); //null=auto
        }

        public void SetAV<T>(string av, T value, bool? propagate)
        {
            if(av.Contains("."))
            {
                string firstPart = av.Substring(0, av.IndexOf('.'));
                string secondPart = av.Substring(av.IndexOf('.') + 1);
                if(firstPart == "BaseStats")
                {
                    BaseStats.SetStat(secondPart, value);
                    if (!propagate.HasValue)
                        UpdateStats();
                }
                else if(firstPart == "DerivedStats")
                {
                    DerivedStats.SetStat(secondPart, value);
                }
                else if(firstPart == "Conditions")
                {
                    string fqConditionName = GetType().Namespace + "." + value.ToString();
                    Condition c = (Condition)Activator.CreateInstance(Type.GetType(fqConditionName));
                    Conditions.Add(c);

                    if (!propagate.HasValue)
                        UpdateStats();
                }
            }
            else
            {
                //search and set property
                GetType().GetProperty(av).SetValue(this, value, null);
            }

            if(propagate.HasValue && propagate.Value)
                UpdateStats();
        }

        public void ModAV<T>(string av, T value)
        {
            ModAV(av, value, null);
        }

        public void ModAV<T>(string av, T value, bool? propagate)
        {
            if (av.Contains("."))
            {
                string firstPart = av.Substring(0, av.IndexOf('.'));
                string secondPart = av.Substring(av.IndexOf('.') + 1);
                if (firstPart == "BaseStats")
                {
                    BaseStats.ModStat(secondPart, value);
                    if (!propagate.HasValue)
                        UpdateStats();
                }
                else if (firstPart == "DerivedStats")
                {
                    DerivedStats.ModStat(secondPart, value);
                }
                else if (firstPart == "Conditions")
                {
                    //delete if present, add if not

                    string fqConditionName = GetType().Namespace + "." + value.ToString();
                    Condition newCondition = (Condition)Activator.CreateInstance(Type.GetType(fqConditionName));

                    Condition oldCondition = null;
                    foreach (Condition c in Conditions)
                    {
                        if(c.GetType() == newCondition.GetType())
                        {
                            oldCondition = c;
                            break;
                        }
                    }

                    if(oldCondition != null)
                    {
                        Conditions.Remove(oldCondition);
                    }
                    else
                    {
                        Conditions.Add(newCondition);
                    }
                    

                    if (!propagate.HasValue)
                        UpdateStats();
                }
            }
            else
            {
                //search and set property
                GetType().GetProperty(av).SetValue(this, value, null);
            }

            if (propagate.HasValue && propagate.Value)
                UpdateStats();
        }

        public T GetAV<T>(string av)
        {
            if (av.Contains("."))
            {
                string firstPart = av.Substring(0, av.IndexOf('.'));
                string secondPart = av.Substring(av.IndexOf('.') + 1);
                if (firstPart == "BaseStats")
                {
                    return BaseStats.GetStat<T>(secondPart);                    
                }
                else if (firstPart == "DerivedStats")
                {
                    return DerivedStats.GetStat<T>(secondPart);
                }
                else if (firstPart == "Conditions")
                {
                    string fqConditionName = GetType().Namespace + "." + secondPart.ToString();
                    Condition newC = (Condition)Activator.CreateInstance(Type.GetType(fqConditionName));
                    bool found = false;

                    foreach(Condition c in Conditions)
                    {
                        if(c.GetType() == newC.GetType())
                        {
                            found = true;
                            break;
                        }
                    }

                    return (T)(object)found;                    
                }
            }
            else
            {
                //search and get property
                GetType().GetProperty(av).GetValue(this, null);
            }

            //fail
            throw new KeyNotFoundException();
        }

        
    }
}
