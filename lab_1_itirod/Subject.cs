using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace lab_1_itirod
{
    [DataContract]
    public class Subject
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public float Mark { get; set; }
        public Subject(string name, float mark)
        {
            Name = name;
            Mark = mark;
        }
    }
}
