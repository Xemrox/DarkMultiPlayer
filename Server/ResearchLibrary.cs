using DarkMultiPlayerCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DarkMultiPlayerServer {
    public class ResearchLibrary {

        private const string TechFile = "GlobalTech.json";
        private const string ScienceFile = "GlobalScience.json";

        private static ResearchLibrary instance = new ResearchLibrary();
        public static ResearchLibrary fetch {
            get {
                return instance;
            }
        }


        public float ScienceAmount = 0.0f;

        public Dictionary<string, TechTransfer> TechNodes = new Dictionary<string, TechTransfer>();
        public Dictionary<string, ScienceTransfer> ScienceNodes = new Dictionary<string, ScienceTransfer>();

        public void Load() {
            if (File.Exists(TechFile)) {
                string jsonTech = "";
                using (StreamReader sr = new StreamReader(TechFile)) {
                    jsonTech = sr.ReadToEnd();
                }
                List<TechTransfer> TechTransfers = JsonConvert.DeserializeObject<List<TechTransfer>>(jsonTech);
                foreach (TechTransfer tt in TechTransfers) {
                    TechNodes.Add(tt.id, tt);
                }
            } else {
                TechNodes.Add("start",new TechTransfer() {
                    id = "start",
                    state = TechTransfer.State.Available,
                    cost = 0,
                    parts = new List<string>() {
                        "basicFin",
                        "mk1pod",
                        "solidBooster.sm",
                        "GooExperiment",
                        "trussPiece1x",
                        "parachuteSingle"
                    }
                });
                SaveTech();
            }

            if(File.Exists(ScienceFile)) {
                string jsonResearch = "";
                using (StreamReader sr = new StreamReader(ScienceFile)) {
                    jsonResearch = sr.ReadToEnd();
                }
                ScienceSave save = JsonConvert.DeserializeObject<ScienceSave>(jsonResearch);
                ScienceAmount = save.ScienceAmount;
                List<ScienceTransfer> ScienceTransfers = save.ScienceData;
                foreach (ScienceTransfer st in ScienceTransfers) {
                    ScienceNodes.Add(st.id, st);
                }
            }
        }

        private void SaveTech() {
            string jsonTech = JsonConvert.SerializeObject(TechNodes.Select(x => x.Value).ToList(), Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(TechFile, false)) {
                sw.Write(jsonTech);
                sw.Flush();
            }
        }

        private class ScienceSave {
            public float ScienceAmount;
            public List<ScienceTransfer> ScienceData;
        }

        private void SaveScience() {
            string jsonScience = JsonConvert.SerializeObject( new ScienceSave() { ScienceAmount = this.ScienceAmount, ScienceData = ScienceNodes.Select(x => x.Value).ToList() }, Formatting.Indented);
            
            using (StreamWriter sw = new StreamWriter(ScienceFile, false)) {
                sw.Write(jsonScience);
                sw.Flush();
            }
        }

        public void Save() {
            this.SaveTech();
            this.SaveScience();      
        }

        public bool AddTech(TechTransfer tt) {
            ///TODO
            /// techstate?
            if (!TechNodes.ContainsKey(tt.id) && ScienceAmount - tt.cost >= 0) {
                TechNodes.Add(tt.id, tt);
                ScienceAmount -= tt.cost;
                Save();
                return true;
            }
            //allready purchased or unable to purchase
            return false;
        }

        public bool AddResearch(float dataAmount, ScienceTransfer st) {
            ScienceTransfer _st;
            if(ScienceNodes.TryGetValue(st.id, out _st)) {
                //security checks :(
                if (st.science > _st.science) {
                    ScienceNodes.Remove(_st.id);
                    ScienceNodes.Add(st.id, st);
                } else {
                    return false;
                }
            } else {
                ScienceNodes.Add(st.id, st); 
            }

            ScienceAmount += dataAmount;

            SaveScience();
            return true;
        }

        public static float GetReferenceDataValue(float dataAmount, ScienceTransfer subject) {
            return dataAmount / subject.dataScale * subject.subjectValue;
        }


        public static float GetScienceValue(float dataAmount, ScienceTransfer subject, float xmitScalar = 1f) {
            float num = Math.Min(GetReferenceDataValue(dataAmount, subject) * subject.scientificValue * xmitScalar, subject.cap);
            float b = Lerp(GetReferenceDataValue(dataAmount, subject), subject.cap, xmitScalar) * xmitScalar;
            float num2 = Math.Min(subject.science + num, b);
            return Math.Max(num2 - subject.science, 0f);
        }

        public static float Clamp(float value) {
            if (value < 0f) {
                return 0f;
            }
            if (value > 1f) {
                return 1f;
            }
            return value;
        }

        public static float Lerp(float from, float to, float t) {
            return from + ( to - from ) * Clamp(t);
        }

    }
}
