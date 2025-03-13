using System;
using System.Collections.Generic;
using System.IO;

namespace SharpSQL
{
    class Config
    {
        public static string instance = "";
        public static string ip = "";
        public static string db = "master";
        public static string linkedinstance = "";
        public static string user = "";
        public static string password = "";
        public static string impersonate = "";
        public static string command = "whoami";
        public static string query = "";
        public static bool verbose = false;
    }
}
