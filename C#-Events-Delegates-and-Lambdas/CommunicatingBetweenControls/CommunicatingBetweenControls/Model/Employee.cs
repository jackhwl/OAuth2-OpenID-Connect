using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicatingBetweenControls.Model
{
    public class Employee
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<Job> Jobs { get; set; }
    }
}
