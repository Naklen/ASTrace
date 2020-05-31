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

        public string[][] Trace(string adress)
        {
            if (!IsIPAdress(adress))
                adress = GetIPAdress(adress);
            if (adress == null)
                return null;
            var route = GetRoute(adress);
            return TraceAutonomousSystems(route, adress);
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

        string[][] TraceAutonomousSystems(string[] route, string destAdress)
        {
            var result = new List<string[]>();
            foreach (var row in route.Select(s => s.Split(' ')))
            {
                var data = new string[5];
                var adress = row[row.Length - 1];
                if (!IsIPAdress(adress))
                {
                    continue;
                }
                data[0] = row[1];
                data[1] = adress;
                data[2] = GetASNumber(adress);
                data[3] = GetCountry(adress);
                data[4] = GetISP(adress);
                result.Add(data);
            }
            TracedToEnd = result.Last()[1] == destAdress;
            return result.ToArray();
        }

        static string GetASNumber(string adress)
        {
            var result = "";
            using (var wc = new WebClient())
            {
                var obj = JObject.Parse(wc.DownloadString("https://stat.ripe.net/data/network-info/data.json?resource=" + adress));
                if (obj["data"]["asns"].HasValues)
                    result = obj["data"]["asns"][0].ToString();
                else result = "-";
            }
            return result;
        }

        static string GetCountry(string adress)
        {
            var result = "";
            using (var wc = new WebClient())
            {
                var obj = JObject.Parse(wc.DownloadString("http://ipinfo.io/" + adress + "/json"));
                if (obj.ContainsKey("country"))
                    result = obj["country"].ToString();
                else
                    result = "-";
            }
            return result;
        }

        static string GetISP(string adress)
        {
            var result = "";
            using (var wc = new WebClient())
            {
                var obj = JObject.Parse(wc.DownloadString("http://ipinfo.io/" + adress + "/json"));
                if (obj.ContainsKey("org"))
                    result = obj["org"].ToString().Split(new char[] { ' ' }, 2)[1];
                else
                    result = "-";
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