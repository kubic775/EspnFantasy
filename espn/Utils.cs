using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace espn
{
    public static class Utils
    {
        public static IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls).Where(c => c.GetType() == type);
        }

        public static double[] Smooth(this double[] array, int windowLength)
        {
            double[] res = new double[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                res[i] = array.Take(i + 1).Skip(Math.Max(0, i + 1 - windowLength)).Average();
            }

            return res;
        }

        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
    }
}
