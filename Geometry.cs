using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;



namespace NDraw
{
    /// <summary>
    /// Performs all mapping ops between the virtual and the displayed.
    /// </summary>
    public class GeometryMap
    {
        /// <summary>The horizontal offset in pixels.</summary>
        public static int ShiftX = 0;

        /// <summary>The vertical offset in pixels.</summary>
        public static int ShiftY = 0;

        /// <summary>Current zoom.</summary>
        public static float Zoom = 1.0F;


        /// <summary>Available area.</summary>
        public static Rectangle DrawArea = new();



        public static void Reset()
        {
            Zoom = 1.0f;
            ShiftX = 0;
            ShiftY = 0;
        }

        /// <summary>
        /// Map an absolute point in virtual coordinates to the display.
        /// </summary>
        /// <param name="pvirt"></param>
        /// <returns></returns>
        public static PointF VirtualToDisplay(PointF pvirt)
        {
            var pdispX = pvirt.X * Zoom + ShiftX;
            var pdispY = pvirt.Y * Zoom + ShiftY;
            return new(pdispX, pdispY);
        }

        /// <summary>Obtain the virtual point for a display position.</summary>
        /// <param name="pdisp">The display point.</param>
        /// <returns>The virtual point.</returns>
        public static PointF DisplayToVirtual(PointF pdisp)
        {
            var pvirtX = (pdisp.X - ShiftX) / Zoom;
            var pvirtY = (pdisp.Y - ShiftY) / Zoom;
            return new PointF(pvirtX, pvirtY);
        }

        //public static double Map(double val, double start1, double stop1, double start2, double stop2)
        //{
        //    return start2 + (stop2 - start2) * (val - start1) / (stop1 - start1);
        //}

    }
}
