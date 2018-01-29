using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TesteCode
{
    public class Propeties
    {
        public string AsrApiUrl { get; set; }
        public string AudioPath { get; set; }
        public string LMUri { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int PacketSizeInBytes { get; set; }
        public int PacketDelayInMillis { get; set; }
        public int NumSessionStart { get; set; }
        public int NumSessionEnd { get; set; }
        public int NumRecogs { get; set; }
        public int NumExecutions { get; set; }
    }
}
