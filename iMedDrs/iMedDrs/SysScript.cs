using System;
using System.Collections.Generic;

namespace iMedDrs
{
    public partial class SysScript
    {
        public int ScriptId { get; set; }
        public int QuestionnaireId { get; set; }
        public int? QuestionId { get; set; }
        public int? ResponseId { get; set; }
        public int Sequence { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public string Answer { get; set; }
        public int? Branch { get; set; }
    }
}
