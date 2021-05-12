using System;
using NDraw;


namespace NDraw.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Page page = new();

            int which = 0;

            try
            {
                switch(which)
                {
                    case 0:
                        Parser p = new();
                        page = p.ParseFile(@"C:\Dev\repos\NDraw\drawing1.nd");

                        if(p.Errors.Count > 0)
                        {

                        }
                        break;

                    case 1:
                        page = new() { Width = 20.0f, Height = 10.0f, UnitsName = "feet", Grid = 0.5f, Snap = 0.1f };
                        break;

                    case 2:
                        page.Rects.Add(new RectShape() { Id = "R_1", Text = "foo", TL = new(50, 50),   BR = new(100, 100) });
                        page.Rects.Add(new RectShape() { Id = "R_2", Text = "bar", TL = new(160, 170), BR = new(200, 300) });
                        page.Rects.Add(new RectShape() { Id = "R_3", Text = "abc", TL = new(300, 250), BR = new(330, 300) });
                        page.Rects.Add(new RectShape() { Id = "R_4", Text = "def", TL = new(400, 300), BR = new(550, 350) });
                        page.Rects.Add(new RectShape() { Id = "R_5", Text = "ggg", TL = new(450, 250), BR = new(460, 550) });
                        page.Lines.Add(new LineShape() { Id = "L_1", Text = "bar",  Start = new(250, 250), End = new(275, 455) });
                        break;

                    case 3:
                        int RECT_SIZE = 20;
                        int RECT_SPACE = 100;
                        for (int x = -70; x < 1500; x += RECT_SPACE)
                        {
                            for (int y = -50; y < 1000; y += RECT_SPACE)
                            {
                                page.Rects.Add(new RectShape() { Text = $"R_{x}_{y}", TL = new(x, y), BR = new(x + RECT_SIZE, y + RECT_SIZE) });
                            }
                        }
                        break;

                    case 4:
                        page = Page.Load("page.json");
                        break;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Fail: {ex}");
            }
        }
    }
}
