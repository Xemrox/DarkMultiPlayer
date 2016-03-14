using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkMultiPlayerCommon {
    public class TechTransfer {
        public TechTransfer() {
            this.parts = new List<string>();
            this.state = State.Unavailable;
            this.cost = 0;
        }

        public enum State {
            Unavailable,
            Available
        }

        public string id { get; set; }
        public State state { get; set; }
        public int cost { get; set; }
        public List<string> parts { get; set; }
    }

    
}
