using DarkMultiPlayerCommon;
using MessageStream2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

namespace DarkMultiPlayer {
    class ResearchLibraryWorker {
        #region "Management"
        private bool registered = false;
        private static ResearchLibraryWorker singleton;
        private HashSet<string> NetworkTechnology = new HashSet<string>();

        public static ResearchLibraryWorker fetch {
            get {
                return singleton;
            }
        }

        public static void Reset() {
            lock (Client.eventLock) {
                if (singleton != null) {
                    Client.updateEvent.Remove(singleton.Update);
                    if (singleton.registered) {
                        singleton.UnregisterGameHooks();
                    }
                }
                singleton = new ResearchLibraryWorker();
                Client.updateEvent.Add(singleton.Update);
                singleton.RegisterGameHooks();
            }
        }

        private void Update() {
           
        }

        private void UnregisterGameHooks() {
            GameEvents.OnScienceChanged.Remove(this.OnScienceChanged);
            GameEvents.OnScienceRecieved.Remove(this.OnScienceRecieved);
            GameEvents.OnTechnologyResearched.Remove(this.OnTechnologyResearched);
            GameEvents.OnExperimentDeployed.Remove(this.OnExperimentDeployed);

            GameEvents.Modifiers.OnCurrencyModified.Remove(this.OnCurrencyModified);
        }

        private void RegisterGameHooks() {
            GameEvents.OnScienceChanged.Add(this.OnScienceChanged);
            GameEvents.OnScienceRecieved.Add(this.OnScienceRecieved);
            GameEvents.OnTechnologyResearched.Add(this.OnTechnologyResearched);
            //not working?
            GameEvents.OnExperimentDeployed.Add(this.OnExperimentDeployed);

            GameEvents.Modifiers.OnCurrencyModified.Add(this.OnCurrencyModified);
        }

        private void OnCurrencyModified(CurrencyModifierQuery data) {
            string msg = string.Format("CM: R:{0} ED:{1} IN:{2}",data.reason.ToString(), data.GetEffectDelta(Currency.Science), data.GetInput(Currency.Science));
            ScreenMessages.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
            DarkLog.Debug(msg);
        }

        private void OnExperimentDeployed(ScienceData data) {
            string msg = string.Format("ED: T:{0} SID:{1} TV:{2} DA:{3} LV:{4} LB:{5}",  data.title, data.subjectID, data.transmitValue ,data.dataAmount, data.labValue, data.labBoost);
            ScreenMessages.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
            DarkLog.Debug(msg);
        }

        private void OnTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data) {
            if(NetworkTechnology.Contains(data.host.techID)) {
                //prevent loop
                NetworkTechnology.Remove(data.host.techID);
                return;
            }
            string msg = string.Format("TR: {0}", data.host.host.name);
            //ScreenMessages.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
            DarkLog.Debug(msg);

            if (data.target == RDTech.OperationResult.Successful) {
                //Broadcast Successful purchase
                ConfigNode techNode = new ConfigNode();
                data.host.Save(techNode);

                TechTransfer tt = new TechTransfer() {
                    id = data.host.techID,
                    cost = data.host.scienceCost,
                    state = (TechTransfer.State) data.host.state, //equal cast
                    parts = data.host.partsPurchased.Select(x => x.name).ToList()
                };

                NetworkWorker.fetch.SendTechnologyResearched(tt);
            }
        }

        private void OnScienceRecieved(float dataAmount, ScienceSubject ss, ProtoVessel vessel, bool reverseEngineered) {
            
            string msg = string.Format("SR: DA:{0} ST:{1} VN:{2} SV:{3} SV:{4} S:{5}", dataAmount, ss.title, vessel.vesselName, ss.scientificValue, ss.subjectValue, ss.science);
            //ScreenMessages.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
            DarkLog.Debug(msg);
            
            ScienceTransfer st = new ScienceTransfer() {
                id = ss.id,
                title = ss.title,
                dataScale = ss.dataScale,
                subjectValue = ss.subjectValue,
                scientificValue = ss.scientificValue,
                cap = ss.scienceCap,
                science = ss.science,
                dataAmount = dataAmount
            };
            NetworkWorker.fetch.SendScienceRecieved(st);

            //save->send->broadcast->load?
            /*ConfigNode researchNode = new ConfigNode();
            ResearchAndDevelopment.Instance.OnSave(researchNode);
            NetworkWorker.fetch.SendScienceRecieved(researchNode);*/

            /*
            id = evaReport@KerbinFlyingLowShores
	        title = EVA Report while flying over Kerbin's Shores
	        dsc = 1
	        scv = 0
	        sbv = 0.7
	        sci = 5.6
	        cap = 5.6
            */


        }

        private void OnScienceChanged(float amt, TransactionReasons tr) {
            string msg = string.Format("SC2: {0} {1}", amt, tr.ToString());
            ScreenMessages.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
            DarkLog.Debug(msg);

            //ignore due to hard resync
        }

        public void HandleResearchLibraryMessage(byte[] messageData) {
            try {
                using (MessageReader mr = new MessageReader(messageData)) {
                    ResearchMessageType messageType = (ResearchMessageType) mr.Read<int>();
                    DarkLog.Debug("RL: " + messageType.ToString());
                    switch (messageType) {
                        case ResearchMessageType.TECHRESEARCHED:

                            HandleTechResearched(messageData);
                            break;
                        case ResearchMessageType.SCIENCERECIEVED:

                            HandleScienceReceived(messageData);
                            break;
                        default:
                            DarkLog.Debug("Unkown Message received...");
                            break;
                    }
                }
            } catch ( Exception ex) {
                DarkLog.Debug(ex.Message + ex.StackTrace);
            }
        }
        #endregion        

        private void HandleTechResearched(byte[] data) {
            RDTech tech = new RDTech();
            
            using (MessageReader mr = new MessageReader(data)) {
                mr.Read<int>();//discard type
                
                tech.techID = mr.Read<string>();
                //ignore state due to unlock
                RDTech.State techState = (RDTech.State) Enum.Parse(typeof(RDTech.State), mr.Read<string>());
                //tech.state = (RDTech.State) Enum.Parse(typeof(RDTech.State), mr.Read<string>());
                
                tech.scienceCost = mr.Read<int>();

                /*int numParts = mr.Read<int>();
                while (numParts > 0) {
                    //tt.parts.Add(mr.Read<string>());

                    AvailablePart partInfoByName = PartLoader.getPartInfoByName(mr.Read<string>());
                    if (partInfoByName != null) {
                        tech.partsPurchased.Add(partInfoByName);
                    }
                    numParts--;
                }*/
            }
            DarkLog.Debug("Unlocking Tech: " + tech.techID);

			//ignore tech event
            NetworkTechnology.Add(tech.techID);

            tech.Start(); //recheck state and link to researchanddevelopment
            tech.state = RDTech.State.Unavailable;
            tech.ResearchTech();
            //ResearchAndDevelopment.Instance.CheatAddScience(-tech.scienceCost);
            //tech.UnlockTech(true);
            
            
        }

        private void HandleScienceReceived(byte[] data) {


            ScienceTransfer st = new ScienceTransfer();
            using (MessageReader mr = new MessageReader(data)) {
                mr.Read<int>(); //discard type

                st.dataAmount = mr.Read<float>();

                st.id = mr.Read<string>();
                st.title = mr.Read<string>();
                st.dataScale = mr.Read<float>();
                st.subjectValue = mr.Read<float>();
                st.scientificValue = mr.Read<float>();
                st.cap = mr.Read<float>();
                st.science = mr.Read<float>();
            }

            ScreenMessages.PostScreenMessage(st.dataAmount +" !SCIENCE RECEIVED!", 30f, ScreenMessageStyle.UPPER_CENTER);

            DarkLog.Debug("RAD-Dictionary-Hack-S");
            FieldInfo fn = typeof(ResearchAndDevelopment).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)[1];
            Dictionary<string, ScienceSubject> sets = (Dictionary<string, ScienceSubject>) fn.GetValue(ResearchAndDevelopment.Instance);
            DarkLog.Debug("RAD-Dictionary-Hack-E");

            ScienceSubject ss;
            if (!sets.TryGetValue(st.id, out ss)) {
                ss = new ScienceSubject(st.id, st.title, st.dataScale, st.subjectValue, st.cap) {
                    scientificValue = st.scientificValue,
                    science = st.science
                };

                sets.Add(st.id, ss);
            } else {
                //patch it
                ss.dataScale = st.dataScale;
                ss.subjectValue = st.subjectValue;
                ss.scientificValue = st.scientificValue;
                ss.scienceCap = st.cap;
                ss.science = st.science;
            }

            //sync science reward
            ResearchAndDevelopment.Instance.CheatAddScience(st.dataAmount);

            /*ConfigNode researchNode = ConfigNodeSerializer.fetch.Deserialize(data);
            
            if (researchNode != null) {
                //kill all events
                ResearchAndDevelopment.Instance.OnDestroy();
                //override with new instance
                ResearchAndDevelopment.Instance.OnAwake();

                //fires no events
                ResearchAndDevelopment.Instance.OnLoad(researchNode);
            }*/
        }
    }
}
