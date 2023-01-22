using System;
using System.Collections.Generic;
using System.Text;

namespace FilumDLWPF
{
    public class Secrets
    {
        public string TwitterBearerToken { get; set; }
        public string SetTwitterBearerToken()
        {
            TwitterBearerToken = "Bearer AAAAAAAAAAAAAAAAAAAAAE62lAEAAAAA2pDCyjRIzEnhAR%2B3iZwNPSJn8OM%3DnDuGSQkQ95qldcjTnriF6AM6btmdv2xnx7JY7D4eFB9DbaYO63";
            return TwitterBearerToken;
        }
        
    }
}
