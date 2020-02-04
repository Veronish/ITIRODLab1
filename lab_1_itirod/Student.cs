using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace lab_1_itirod
{
    [DataContract]
    public class Student
    {
        [DataMember]
        public string Surname { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Patronymic { get; set; }
        [DataMember]
        public List<Subject> Marks { get; set; }
        [DataMember]
        float Avg;
        public Student(string surname, string name, string patronymic)
        {
            Surname = surname;
            Name = name;
            Patronymic = patronymic;
            Marks = new List<Subject>();

        }
        /// <summary>
        /// вычисление средней оценки
        /// </summary>
        public void CalcAvg()
        {
            Avg = 0;
            for (int i = 0; i < Marks.Count; i++)
                Avg += Marks[i].Mark;
            Avg /= Marks.Count;
            Avg = (float)Math.Round(Avg, 2);


        }
    }
}

