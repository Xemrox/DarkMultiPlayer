using DarkMultiPlayerCommon;
using MessageStream2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkMultiPlayerServer.Messages {
    public class ResearchLibrary {

        public static void HandleResearchMessage(ClientObject client, byte[] fullMessageData) {
           
            using (MessageReader mr = new MessageReader(fullMessageData)) {
                ResearchMessageType messageType = (ResearchMessageType) mr.Read<int>();

                DarkLog.Debug("Researchmsg " + client.playerName + " " + messageType.ToString());

                switch (messageType) {
                    case ResearchMessageType.TECHRESEARCHED:
                        HandleTechResearched(client, fullMessageData);
                        break;
                    case ResearchMessageType.SCIENCERECIEVED:
                        HandleScienceRecieved(client, fullMessageData);
                        break;
                    default:
                        DarkLog.Fatal("Invalid Researchmessage!");
                        break;
                }
            }
        }

        #region "TechHandling"
       
        private static void HandleTechResearched(ClientObject client, byte[] messageData) {
            TechTransfer tt = new TechTransfer();
            using(MessageReader mr = new MessageReader(messageData)) {
                mr.Read<int>(); //discard type

                tt.id = mr.Read<string>();
                tt.state = (TechTransfer.State)Enum.Parse(typeof(TechTransfer.State), mr.Read<string>());
                tt.cost = mr.Read<int>();

                int numParts = mr.Read<int>();
                while(numParts > 0) {
                    tt.parts.Add(mr.Read<string>());
                    numParts--;
                }
            }

            ///TODO
            /// Techstate?
            
            if (DarkMultiPlayerServer.ResearchLibrary.fetch.AddTech(tt)) {
                DarkLog.Debug("TechResearched: " + tt.id + " for: " + tt.cost + " state: " + tt.state.ToString());
                ServerMessage newMessage = new ServerMessage();
                newMessage.type = ServerMessageType.RESEARCH_LIBRARY;
                newMessage.data = messageData; //rewrap
                ClientHandler.SendToAll(client, newMessage, true);
            } else {
                DarkLog.Debug("TechResearched-F: " + tt.id + " for: " + tt.cost + " state: " + tt.state.ToString());
            }
        }
        
        
        #endregion
        private static void HandleScienceRecieved(ClientObject client, byte[] messageData) {
            ScienceTransfer st = new ScienceTransfer();
            using (MessageReader mr = new MessageReader(messageData)) {
                mr.Read<int>(); //discard type

                st.dataAmount = mr.Read<float>().clampRound();

                st.id = mr.Read<string>();
                st.title = mr.Read<string>();
                st.dataScale = mr.Read<float>().clampRound();
                st.subjectValue = mr.Read<float>().clampRound();
                st.scientificValue = mr.Read<float>().clampRound();
                st.cap = mr.Read<float>().clampRound();
                st.science = mr.Read<float>().clampRound();
            }

            if(DarkMultiPlayerServer.ResearchLibrary.fetch.AddResearch(st.dataAmount, st)) {
                DarkLog.Debug("ScienceReceived: " + st.id + " amt: " + st.dataAmount.ToString());
                ServerMessage newMessage = new ServerMessage();
                newMessage.type = ServerMessageType.RESEARCH_LIBRARY;

                using(MessageWriter wr = new MessageWriter()) {

                    wr.Write<int>((int)ResearchMessageType.SCIENCERECIEVED);

                    wr.Write<float>(st.dataAmount);
                    wr.Write<string>(st.id);
                    wr.Write<string>(st.title);
                    wr.Write<float>(st.dataScale);
                    wr.Write<float>(st.subjectValue);
                    wr.Write<float>(st.scientificValue);
                    wr.Write<float>(st.cap);
                    wr.Write<float>(st.science);

                    newMessage.data = wr.GetMessageBytes();
                }

                newMessage.data = messageData; 
                ClientHandler.SendToAll(client, newMessage, true);
            } else {
                DarkLog.Debug("ScienceReceived-F: " + st.id + " amt: " + st.dataAmount.ToString());
            }
        }
    }

    public static class Ext {
        public static float clampRound(this float val) {
            return (float) Math.Round(val, 2);
        }
    }
}
