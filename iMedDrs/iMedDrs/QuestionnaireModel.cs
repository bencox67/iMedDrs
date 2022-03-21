using System;
using System.Collections.Generic;

namespace iMedDrs
{
    public class QuestionnaireModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string DefaultReport { get; set; }
        public string Instructions { get; set; }
        public string Question { get; set; }
        public string QuestionName { get; set; }
        public int Sequence { get; set; }
        public int EndSequence { get; set; }
        public int ResponseId { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public string Language { get; set; }
        public string Function { get; set; }
        public string[] Responses { get; set; }
        public List<SysScript> Script { get; set; }
        public List<string> ActResponses { get; set; }
        public List<string> EngResponses { get; set; }
    }
}
