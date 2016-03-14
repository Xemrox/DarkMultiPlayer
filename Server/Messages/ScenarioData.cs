using System;
using System.IO;
using MessageStream2;
using DarkMultiPlayerCommon;
using System.Collections.Generic;
using System.Globalization;

namespace DarkMultiPlayerServer.Messages
{
    public class ScenarioData
    {
        public static void SendScenarioModules(ClientObject client)
        {
            int numberOfScenarioModules = Directory.GetFiles(Path.Combine(Server.universeDirectory, "Scenarios", client.playerName)).Length;
            int currentScenarioModule = 0;
            string[] scenarioNames = new string[numberOfScenarioModules];
            byte[][] scenarioDataArray = new byte[numberOfScenarioModules][];
            foreach (string file in Directory.GetFiles(Path.Combine(Server.universeDirectory, "Scenarios", client.playerName)))
            {
                //Remove the .txt part for the name
                scenarioNames[currentScenarioModule] = Path.GetFileNameWithoutExtension(file);
                if (scenarioNames[currentScenarioModule] == "ResearchAndDevelopment") {
                    CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");

                    using (MemoryStream ms = new MemoryStream())
                    using (StreamWriter wr = new StreamWriter(ms)) {
                        wr.WriteLine("name = ResearchAndDevelopment");
                        wr.WriteLine("scene = 7, 8, 5, 6");
                        wr.WriteLine(string.Format("sci = {0}", DarkMultiPlayerServer.ResearchLibrary.fetch.ScienceAmount.ToString("0.00",ci)));

                        foreach(KeyValuePair<string,TechTransfer> kv in DarkMultiPlayerServer.ResearchLibrary.fetch.TechNodes) {
                            var node = kv.Value;
                            wr.WriteLine("Tech");
                            wr.WriteLine("{");
                            wr.WriteLine(string.Format("\tid = {0}",node.id));
                            wr.WriteLine(string.Format("\tstate = {0}",node.state.ToString()));
                            wr.WriteLine(string.Format("\tcost = {0}",node.cost.ToString("",ci)));
                            foreach(var p in node.parts) {
                                wr.WriteLine(string.Format("\tpart = {0}", p));
                            }
                            wr.WriteLine("}");
                        }

                        foreach (KeyValuePair<string, ScienceTransfer> kv in DarkMultiPlayerServer.ResearchLibrary.fetch.ScienceNodes) {
                            var node = kv.Value;
                            wr.WriteLine("Science");
                            wr.WriteLine("{");
                            wr.WriteLine(string.Format("\tid = {0}", node.id));
                            wr.WriteLine(string.Format("\ttitle = {0}", node.title));
                            wr.WriteLine(string.Format("\tdsc = {0}", node.dataScale.ToString("0.00",ci)));
                            wr.WriteLine(string.Format("\tscv = {0}", node.scientificValue.ToString("0.00",ci)));
                            wr.WriteLine(string.Format("\tsbv = {0}", node.subjectValue.ToString("0.00",ci)));
                            wr.WriteLine(string.Format("\tsci = {0}", node.science.ToString("0.00",ci)));
                            wr.WriteLine(string.Format("\tcap = {0}", node.cap.ToString("0.00",ci)));
                            wr.WriteLine("}");
                        }
                        wr.WriteLine("");
                        wr.Flush();
                        scenarioDataArray[currentScenarioModule] = ms.ToArray();

                        File.WriteAllBytes("latestRAD.txt", scenarioDataArray[currentScenarioModule]);
                    }

                } else {
                    scenarioDataArray[currentScenarioModule] = File.ReadAllBytes(file);
                }
                currentScenarioModule++;
            }
            ServerMessage newMessage = new ServerMessage();
            newMessage.type = ServerMessageType.SCENARIO_DATA;
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<string[]>(scenarioNames);
                foreach (byte[] scenarioData in scenarioDataArray)
                {
                    if (client.compressionEnabled)
                    {
                        mw.Write<byte[]>(Compression.CompressIfNeeded(scenarioData));
                    }
                    else
                    {
                        mw.Write<byte[]>(Compression.AddCompressionHeader(scenarioData, false));
                    }
                }
                newMessage.data = mw.GetMessageBytes();
            }
            ClientHandler.SendToClient(client, newMessage, true);
        }

        public static void HandleScenarioModuleData(ClientObject client, byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                //Don't care about subspace / send time.
                string[] scenarioName = mr.Read<string[]>();
                DarkLog.Debug("Saving " + scenarioName.Length + " scenario modules from " + client.playerName);

                for (int i = 0; i < scenarioName.Length; i++)
                {
                    byte[] scenarioData = Compression.DecompressIfNeeded(mr.Read<byte[]>());
                    File.WriteAllBytes(Path.Combine(Server.universeDirectory, "Scenarios", client.playerName, scenarioName[i] + ".txt"), scenarioData);
                }
            }
        }
    }
}

