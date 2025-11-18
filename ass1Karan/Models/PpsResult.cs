using System;
using System.ComponentModel.DataAnnotations;

namespace ass1Karan.Models
{
    public class PpsResult
    {
        public int Id { get; set; }
        public string Diagnosis { get; set; }
        public int Ambulation { get; set; }
        public int Activity { get; set; }
        public int Evidence { get; set; }
        public int SelfCare { get; set; }
        public int Intake { get; set; }
        public int Consciousness { get; set; }
        public int FinalScore { get; set; }
        public DateTime DateSaved { get; set; }
        public string UserEmail { get; set; }
    }
}
