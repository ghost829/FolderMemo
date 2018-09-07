using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace FolderMemo
{
    public partial class CustomTooltip : UserControl
    {
        string m_title = "";
        string m_text = "";

        public CustomTooltip(String title, String text)
        {
            InitializeComponent();

            this.m_title = title;
            this.m_text = text;

            this.label1.Text = title;
            this.label2.Text = text;

            line_transparent.ForeColor = Color.White;
            //line_transparent.Text = "";

            int term = 20;

            Graphics g = this.CreateGraphics();
            StringFormat txtStringFormat = new StringFormat(StringFormat.GenericTypographic);
            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };
            txtStringFormat.SetMeasurableCharacterRanges(ranges);
            Region[] region = g.MeasureCharacterRanges(text, label2.Font,
                new RectangleF(0, 0, this.label2.Width, (DEFINE.CUSTOMTOOLTIP_MAXIMUMHEIGHT - this.label2.Location.Y) - term),
                txtStringFormat);

            if (region.Length > 0)
            {
                RectangleF rectF = region[0].GetBounds(g);
                g.Dispose();

                this.Height = Math.Min(this.label2.Location.Y + Convert.ToInt32(rectF.Height) + term,
                    DEFINE.CUSTOMTOOLTIP_MAXIMUMHEIGHT);
                line_transparent.Location = new Point(0, this.Height - line_transparent.Height);
                this.label2.Height = line_transparent.Location.Y - this.label2.Location.Y;
            }
            else
            {
                Console.WriteLine("Length is Zero");
            }
        }
    }
}
