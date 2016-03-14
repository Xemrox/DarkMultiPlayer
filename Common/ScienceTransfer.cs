using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkMultiPlayerCommon {
    public class ScienceTransfer {
        /// <summary>
        /// id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// title
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// dsc
        /// </summary>
        public float dataScale { get; set; }
        /// <summary>
        /// scv
        /// </summary>
        public float scientificValue { get; set; }
        /// <summary>
        /// sbv
        /// </summary>
        public float subjectValue { get; set; }
        /// <summary>
        /// sci
        /// </summary>
        public float science { get; set; }
        /// <summary>
        /// cap
        /// </summary>
        public float cap { get; set; }

        /// <summary>
        /// Awarded Science
        /// </summary>
        public float dataAmount { get; set; }

    }
}
