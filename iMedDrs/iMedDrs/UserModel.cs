using System;
using System.Collections.Generic;

namespace iMedDrs
{
    public class UserModel
    {
        public int? Id { get; set; }
        public string Password1 { get; set; }
        public string Password2 { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime Birthdate { get; set; }
        public string Language { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public int[] Age { get; set; }
        public int QuestionnaireId { get; set; }
        public string VisitDate { get; set; }
        public DateTime DateUpdated { get; set; }
        public QuestionnaireModel Questionnaire { get; set; }
        public string Response { get; set; }
        public string AudioFile { get; set; }
        public string Function { get; set; }
        public List<QuestionnaireModel> QuestionnaireList { get; set; }
        public string[] LanguageList { get; set; }
    }
}
