using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;

namespace NDraw
{
    public static class Utils
    {
        /// <summary>
        /// Perform a blind deep copy of an object. The class must be marked as [Serializable] in order for this to work.
        /// There are many ways to do this: http://stackoverflow.com/questions/129389/how-do-you-do-a-deep-copy-an-object-in-net-c-specifically/11308879
        /// The binary serialization is apparently slower but safer. Feel free to reimplement with a better way.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }


        // /// <summary>
        // /// Utility to create an svg from lists.
        // /// </summary>
        // /// <param name="fn"></param>
        // /// <param name="scale"></param>
        // public static void ConvertToSVG(string fn, List<LineX> lines, List<PointX> points)
        // {
        //     double scale = 10.0; // or maybe 5.0f
        //     int swidth = 2;

        //     List<string> ls = new List<string>();

        //     // Get extents.
        //     double left = double.MaxValue;
        //     double right = double.MinValue;
        //     double top = double.MinValue;
        //     double bottom = double.MaxValue;

        //     if(lines != null)
        //     {
        //         foreach (LineX l in lines)
        //         {
        //             left = Math.Min(left, l.Start.X);
        //             left = Math.Min(left, l.End.X);
        //             right = Math.Max(right, l.Start.X);
        //             right = Math.Max(right, l.End.X);
        //             top = Math.Max(top, l.Start.Y);
        //             top = Math.Max(top, l.End.Y);
        //             bottom = Math.Min(bottom, l.Start.Y);
        //             bottom = Math.Min(bottom, l.End.Y);
        //         }
        //     }

        //     if (points != null)
        //     {
        //         foreach (PointX p in points)
        //         {
        //             left = Math.Min(left, p.X);
        //             right = Math.Max(right, p.X);
        //             top = Math.Max(top, p.Y);
        //             bottom = Math.Min(bottom, p.Y);
        //         }
        //     }

        //     int hpix = (int)(Math.Ceiling(Math.Abs(right - left)) / scale);
        //     int vpix = (int)(Math.Ceiling(Math.Abs(top - bottom)) / scale);

        //     int voffs = vpix / 20;

        //     ls.Add(string.Format("<svg version=\"1.1\" height=\"{0}\" width=\"{1}\" xmlns=\"http://www.w3.org/2000/svg\">", hpix + hpix / 10, vpix + vpix / 10));

        //     // handy center marker:
        //     PointX zero = new PointX(-left / scale, top / scale); // xlate
        //     ls.Add(string.Format("<polygon fill=\"black\" stroke=\"black\" points=\"{0:f},{1:f} {2:f},{1:f}, {2:f},{3:f}, {0:f},{3:f} \"/>",
        //         zero.X - 100, zero.Y + voffs - 100, zero.X + 100, zero.Y + voffs + 100));

        //     if (lines != null)
        //     {
        //         foreach (LineX l in lines)
        //         {
        //             PointX transStart = new PointX((l.Start.X - left) / scale, (top - l.Start.Y ) / scale); // xlate
        //             PointX transEnd = new PointX((l.End.X - left) / scale, (top - l.End.Y) / scale); // xlate

        //             // <line x1="0" y1="0" x2="200" y2="200" style="stroke:rgb(255,0,0);stroke-width:2" />
        //             ls.Add(string.Format("<line x1=\"{0:f}\" y1=\"{1:f}\" x2=\"{2:f}\" y2=\"{3:f}\" stroke=\"{4}\" stroke-width=\"{5}\" />",
        //                 transStart.X, transStart.Y + voffs, transEnd.X, transEnd.Y + voffs, "black", swidth));
        //         }
        //     }

        //     if (points != null)
        //     {
        //         foreach (PointX p in points)
        //         {
        //             PointX transP = new PointX((p.X - left) / scale, (top - p.Y) / scale); // xlate

        //             ls.Add(string.Format("<polygon fill=\"red\" stroke=\"red\" points=\"{0:f},{1:f} {2:f},{1:f}, {2:f},{3:f}, {0:f},{3:f} \"/>",
        //                 transP.X - 1, transP.Y + voffs - 1, transP.X + 1, transP.Y + voffs + 1));
        //         }
        //     }

        //     // Done.
        //     ls.Add("</svg>");
        //     File.WriteAllText(fn, string.Join(Environment.NewLine, ls));
        // }
    }
}
