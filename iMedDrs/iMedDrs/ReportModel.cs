using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iMedDrs
{
    public class ReportModel
    {
        public int RepId { get; set; }
        public int MaxId { get; set; }
        public string QuestionnaireName { get; set; }
        public string Location { get; set; }
        public string Language { get; set; }
        public List<Report> Reports { get; set; }
    }

    public class Report
    {
        public string Name { get; set; }
        public string Text { get; set; }
    }
}
