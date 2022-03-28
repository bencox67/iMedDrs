using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iMedDrs
{
    public class ScriptModel
    {
        public int ScriptId { get; set; }
        public int Sequence { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
    }
}
