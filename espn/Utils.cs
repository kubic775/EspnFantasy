﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NBAFantasy
{
    public static class Utils
    {
        public static int ToInt(this string str)
        {
            int.TryParse(str, out int num);
            return num;
        }
        
        public static double ToDouble(this string str)
        {
            double.TryParse(str, out double num);
            return num;
        }

        public static string ToPascalCase(this string str)
        {
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            return info.ToTitleCase(str.ToLower());
        }

        public static IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls).Where(c => c.GetType() == type);
        }

        public static double[] Smooth(double[] array, int windowLength)
        {
            double[] res = new double[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                int start = Math.Max(0, i - windowLength);
                int end = Math.Min(array.Length, i + windowLength);
                res[i] = array.Skip(start).Take(end - start + 1).Average();
            }
            return res;
        }

        //public static double StandardDeviation(this IEnumerable<double> values)
        //{
        //    double avg = values.Average();
        //    return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        //}

        public static Task<string> MakeAsyncRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.Timeout = 20000;

            Task<WebResponse> task = Task.Factory.FromAsync(
                request.BeginGetResponse,
                asyncResult => request.EndGetResponse(asyncResult), null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                //Need to return this response 
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }

        public static string DownloadStringFromUrl(string url)
        {
            using (WebClient wc = new WebClient())
            {
                //wc.Encoding = System.Text.Encoding.UTF8;
                string res = wc.DownloadString(url);
                return res;
            }
        }

        public static bool Ping(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Timeout = 5000;
                request.AllowAutoRedirect = false; // find out if this site is up and don't follow a redirector
                request.Method = "HEAD";

                using (var response = request.GetResponse())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static int GetCurrentYear()
        {
            return DateTime.Now.Month >= 10 ? DateTime.Now.Year : DateTime.Now.Year - 1;
        }

    }
}
