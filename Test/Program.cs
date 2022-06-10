using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using NBagOfTricks;
using NBagOfTricks.PNUT;
using NDraw;


namespace NDraw.Test
{
    class Program
    {
        static void Main()
        {
            TestRunner runner = new(OutputFormat.Readable);
            var cases = new[] { "PARSER_1" };
            runner.RunSuites(cases);
            File.WriteAllLines("test_out.txt", runner.Context.OutputLines);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PARSER_1 : TestSuite
    {
        public override void RunSuite()
        {
            UT_INFO("Tests parser.");

            try
            {
                Parser p = new();

                p.ParseLine("v1=23.5");
                p.ParseLine("v2=10");
                p.ParseLine("v3=15.001");
                p.ParseLine("v4=v2 - 12.33");
                p.ParseLine("v5=45");
                p.ParseLine("v6=v3 - 5.11");
                p.ParseLine("v7=v6 + v2 - 2.2 + 33.00003 - v5 + v1+v3");

                UT_CLOSE(p.UserVals["v1"], 23.5f, 0.00001);
                UT_CLOSE(p.UserVals["v2"], 10.0f, 0.00001);
                UT_CLOSE(p.UserVals["v3"], 15.001f, 0.00001);
                UT_CLOSE(p.UserVals["v4"], -2.33f, 0.00001);
                UT_CLOSE(p.UserVals["v5"], 45.0f, 0.00001);
                UT_CLOSE(p.UserVals["v6"], 9.891, 0.00001);
                UT_CLOSE(p.UserVals["v7"], 44.19203, 0.00001);

                if (p.Errors.Count > 0)
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Epic Fail: {ex}");
            }
        }
    }    

    /// <summary>
    /// 
    /// </summary>
    public class PARSER_2 : TestSuite
    {
        public override void RunSuite()
        {
            UT_INFO("Tests parser.");

            Page page = new();

            int which = 0;
            int RECT_SIZE = 20;
            int RECT_SPACE = 100;

            try
            {
                switch(which)
                {
                    case 0:
                        Parser p = new();
                        p.ParseFile(@"..\..\test1.nd");

                        if (p.Errors.Count > 0)
                        {
                        }
                        break;

                    case 1:
                        page = new() { UnitsName = "feet", Grid = 0.5f };
                        //page = new() { Width = 20.0f, Height = 10.0f, UnitsName = "feet", Grid = 0.5f, Snap = 0.1f };
                        break;

                    case 2:
                        page.Rects.Add(new RectShape() { Id = "R_1", Text = "foo", Location = new(50, 50),   Width = RECT_SIZE, Height = RECT_SIZE });
                        page.Rects.Add(new RectShape() { Id = "R_2", Text = "bar", Location = new(160, 170), Width = RECT_SIZE, Height = RECT_SIZE });
                        page.Rects.Add(new RectShape() { Id = "R_3", Text = "abc", Location = new(300, 250), Width = RECT_SIZE, Height = RECT_SIZE });
                        page.Rects.Add(new RectShape() { Id = "R_4", Text = "def", Location = new(400, 300), Width = RECT_SIZE, Height = RECT_SIZE });
                        page.Rects.Add(new RectShape() { Id = "R_5", Text = "ggg", Location = new(450, 250), Width = RECT_SIZE, Height = RECT_SIZE });
                        page.Lines.Add(new LineShape() { Id = "L_1", Text = "bar", Start = new(250, 250), End = new(275, 455) });
                        break;

                    case 3:
                        for (int x = -70; x < 1500; x += RECT_SPACE)
                        {
                            for (int y = -50; y < 1000; y += RECT_SPACE)
                            {
                                page.Rects.Add(new RectShape() { Text = $"R_{x}_{y}", Location = new(x, y), Width = RECT_SIZE, Height = RECT_SIZE });
                            }
                        }
                        break;

                    case 4:
                        page = Page.Load("page.json")!;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fail: {ex}");
            }
        }
    }    
}
