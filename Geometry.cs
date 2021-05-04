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
    public class Geometry
    {
        #region Properties
        /// <summary>The horizontal offset in pixels.</summary>
        public static int OffsetX = 0;

        /// <summary>The vertical offset in pixels.</summary>
        public static int OffsetY = 0;

        /// <summary>Current zoom level.</summary>
        public static float Zoom = 1.0F;

        ///// <summary>Available area.</summary>
        //public static Rectangle DrawArea = new();
        #endregion

        #region Functions for display to/from virtual
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
        /// Obtain the virtual rect for a display rect.
        /// </summary>
        /// <param name="disp">The display point.</param>
        /// <returns>The virtual point.</returns>
        public static RectangleF DisplayToVirtual(Rectangle disp)
        {
            var tl = DisplayToVirtual(disp.Location);
            var br = DisplayToVirtual(new Point(disp.Right, disp.Bottom));
            var virt = new RectangleF(tl, new SizeF(br.X - tl.X, br.Y - tl.Y));
            return virt;
        }
        #endregion

        #region Misc functions
        /// <summary>
        /// Defaults.
        /// </summary>
        public static void Reset()
        {
            Zoom = 1.0f;
            OffsetX = 0;
            OffsetY = 0;
        }
        #endregion

        /// <summary>
        /// Make a rectangle from the line start/end.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static RectangleF Expand(PointF start, PointF end, float range)
        {
            float width = end.X - start.X;
            float height = end.Y - start.Y;

            // Normalize. TODO1 generic rect function?
            if (width < 0)
            {
                float f = end.X;
                end.X = start.X;
                start.X = f;
                width = -width;
            }

            if (height < 0)
            {
                float f = end.Y;
                end.Y = start.Y;
                start.Y = f;
                height = -height;
            }

            RectangleF r = new(start.X - range, start.Y - range, width + range * 2, height + range * 2);
            return r;
        }

        /// <summary>
        /// Gets list of lines defining rect edges. Clockwise from top left.
        /// </summary>
        /// <returns></returns>
        public static List<(PointF start, PointF end)> GetEdges(RectangleF rect) // TODO1 ext method?
        {
            List<(PointF start, PointF end)> lines = new();

            lines.Add((new(rect.Left, rect.Top), new(rect.Right, rect.Top)));
            lines.Add((new(rect.Right, rect.Top), new(rect.Right, rect.Bottom)));
            lines.Add((new(rect.Right, rect.Bottom), new(rect.Left, rect.Bottom)));
            lines.Add((new(rect.Left, rect.Bottom), new(rect.Left, rect.Top)));

            return lines;
        }
    }
}
