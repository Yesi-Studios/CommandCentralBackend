using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    public static class TestSettings
    {

        public static string Database { get; } = "test_database";
        public static string Server { get; } = "localhost";
        public static string Username { get; } = "root";
        public static string Password { get; } = "password";
        public static List<string> SMTPHosts { get; } = new List<string> { };
        public static bool RebuildIfExists { get; } = true;

    }
}
