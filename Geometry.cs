using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NDraw
{
    /// <summary>
    /// Performs all mapping ops between the virtual and the displayed.
    /// </summary>
    public class GeometryMap
    {
        /// <summary>The horizontal offset in pixels.</summary>
        public static int OffsetX = 0;

        /// <summary>The vertical offset in pixels.</summary>
        public static int OffsetY = 0;

        /// <summary>Current zoom.</summary>
        public static float Zoom = 1.0F;

        /// <summary>Available area.</summary>
        public static Rectangle DrawArea = new();


        public static void Reset()
        {
            Zoom = 1.0f;
            OffsetX = 0;
            OffsetY = 0;
        }

        /// <summary>
        /// Map a virtual point to the display.
        /// </summary>
        /// <param name="virt"></param>
        /// <returns></returns>
        public static PointF VirtualToDisplay(PointF virt)
        {
            var dispX = virt.X * Zoom + OffsetX;
            var dispY = virt.Y * Zoom + OffsetY;
            return new(dispX, dispY);
        }

        /// <summary>Obtain the virtual point for a display position.</summary>
        /// <param name="disp">The display point.</param>
        /// <returns>The virtual point.</returns>
        public static PointF DisplayToVirtual(Point disp)
        {
            var virtX = (disp.X - OffsetX) / Zoom;
            var virtY = (disp.Y - OffsetY) / Zoom;
            return new PointF(virtX, virtY);
        }

        /// <summary>
        /// Map a virtual rectangle to the display.
        /// </summary>
        /// <param name="virt"></param>
        /// <returns></returns>
        public static RectangleF VirtualToDisplay(RectangleF virt)
        {
            var tl = VirtualToDisplay(virt.Location);
            var br = VirtualToDisplay(new PointF(virt.Right, virt.Bottom));
            var disp = new RectangleF(tl, new SizeF(br.X - tl.X, br.Y - tl.Y));
            return disp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vtl"></param>
        /// <param name="vbr"></param>
        /// <returns></returns>
        public static RectangleF VirtualToDisplay(PointF vtl, PointF vbr)
        {
            var tl = VirtualToDisplay(vtl);
            var br = VirtualToDisplay(vbr);
            var disp = new RectangleF(tl, new SizeF(br.X - tl.X, br.Y - tl.Y));
            return disp;
        }

        /// <summary>Obtain the virtual rect for a display rect.</summary>
        /// <param name="disp">The display point.</param>
        /// <returns>The virtual point.</returns>
        public static RectangleF DisplayToVirtual(Rectangle disp)
        {
            var tl = DisplayToVirtual(disp.Location);
            var br = DisplayToVirtual(new Point(disp.Right, disp.Bottom));
            var virt = new RectangleF(tl, new SizeF(br.X - tl.X, br.Y - tl.Y));
            return virt;
        }
    }
}
