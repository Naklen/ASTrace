using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ASTrace
{
    class ASTracer
    {
        public bool TracedToEnd { get; private set; }       

        public string[] Trace(string adress)
        {
            if (!IsIPAdress(adress))
                adress = GetIPAdress(adress);
            if (adress == null)
                return null;
            var route = GetRoute(adress);
            return TraceAutonomousSystems(route);
        }

        static string[] GetRoute(string adress)
        {
            Process pr = Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c tracert -d {adress}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            });
            return pr.StandardOutput.ReadToEnd().Split('\r').Skip(3).Reverse().Skip(3).Reverse()
                .Select(s => s.Split(' ').Where(ss => ss != "")).Select(e => String.Join(" ", e)).ToArray();
        }

        string[] TraceAutonomousSystems(string[] route)
        {
            var result = new List<string>();
            foreach (var row in route.Select(s => s.Split(' ')))
            {
                var adress = row[row.Length - 1];
                if (!IsIPAdress(adress))
                {
                    TracedToEnd = false;
                    break;
                }
                var number = row[1];
                var asNumber = GetASNumber(adress);
                result.Add(number + " " + adress + " " + asNumber);
                TracedToEnd = true;
            }
            return result.ToArray();
        }

        static string GetASNumber(string adress)
        {
            var result = "";
            using (var ws = new WebClient())
            {
                var obj = JObject.Parse(ws.DownloadString("https://stat.ripe.net/data/network-info/data.json?resource=" + adress));
                if (obj["data"]["asns"].HasValues)
                    result = obj["data"]["asns"][0].ToString();
                else return "-";
            }
            return result;
        }

        static bool IsIPAdress(string adress)
        {
            var octet = "(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
            return Regex.IsMatch(adress, $"{octet}\x2E{octet}\x2E{octet}\x2E{octet}");
        }

        static string GetIPAdress(string adress)
        {
            try
            {
                return Dns.GetHostAddresses(adress)[0].ToString();
            }
            catch
            {
                return null;
            }
        }

        public static string GetAdressText(string adress)
        {
            if (!IsIPAdress(adress))
            {
                var old = string.Copy(adress);
                adress = GetIPAdress(adress);
                if (adress == null)
                    return null;
                return old + " [" + adress + "]";
            }
            else return adress;
        }
    }
}