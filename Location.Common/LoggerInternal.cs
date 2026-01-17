using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Common
{
    public class LoggerInternal
    {
        public string? TypeLog { get; set; }
        public string? Package { get; set; }
        public string? ClassName { get; set; }
        public string? Method { get; set; }
        public string? Parameters { get; set; }
        public string? Message { get; set; }
        public string? Username { get; set; }
        public DateTime DateTime => DateTime.Now;
    }
}
