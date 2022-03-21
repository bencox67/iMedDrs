using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace iMedDrs
{
    public partial class SysQuestionnaire
    {
        public int QuestionnaireId { get; set; }
        public string Name { get; set; }
        public string DefaultReport { get; set; }
        public string AlternateReport { get; set; }
        public string AlternateSelection { get; set; }
        public string NumberKey { get; set; }
    }
}
