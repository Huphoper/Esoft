using System;

namespace Esoft
{
    
    class Task
    {
        public int id { get; set; }
        public int Performer { get; set; }
        public string PerformerFullName { get; set; } // Для отображения ФИО в колонках
        public string ManagerFullName { get; set; } // Для отображения ФИО в колонках
        public string Name { get; set; }
        public string Definition { get; set; }
        public int Complexity { get; set; }
        public string Status { get; set; }
        public int LeadTime { get; set; }
        public DateTime DateCompletion { get; set; }
        public DateTime PerformanceDate { get; set; }
        public DateTime DateCreate { get; set; }
        public string TypeWork { get; set; }
    }
}
