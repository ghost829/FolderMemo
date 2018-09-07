using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomControlLibrary
{
    public partial class ProfessionalMenuStrip : MenuStrip
    {
        #region ## VARIABLES
        private ColorTable thisColorTable; // 현재 MenuStrip이 가지고있는 Renderer ColorTable
        #endregion

        #region ## PROPERTIES
        [Browsable(true), Category("CustomControl"), Description("메뉴 클릭 후 DropDown목록의 Border를 지정합니다")]
        public Color MenuBorer
        {
            get
            {
                return thisColorTable.m_menuBorder;
            }
            set
            {
                thisColorTable.m_menuBorder = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("메뉴에 마우스오버시 메뉴의 Border를 지정합니다")]
        public Color MenuItemBorder
        {
            get
            {
                return thisColorTable.m_menuItemBorder;
            }
            set
            {
                thisColorTable.m_menuItemBorder = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("메뉴의 GradientBegin 색상을 지정합니다\r\n(Gradient가 단색이 아닐시 폼 Resize 후 Redraw할때 색상깨짐)")]
        public Color MenuStripGradientBegin
        {
            get
            {
                return thisColorTable.m_menuStripGradientBegin;
            }
            set
            {
                thisColorTable.m_menuStripGradientBegin = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("메뉴의 GradientEnd 색상을 지정합니다\r\n(Gradient가 단색이 아닐시 폼 Resize 후 Redraw할때 색상깨짐)")]
        public Color MenuStripGradientEnd
        {
            get
            {
                return thisColorTable.m_menuStripGradientEnd;
            }
            set
            {
                thisColorTable.m_menuStripGradientEnd = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("메뉴들의 전경색 일괄지정")]
        public Color MenuTextColor
        {
            get
            {
                return this.ForeColor;
            }
            set
            {
                this.ForeColor = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("선택된 메뉴의 배경색 지정[BEGIN COLOR]")]
        public Color MenuItemPressedGradientBegin
        {
            get
            {
                return thisColorTable.m_menuItemPressedGradientBegin;
            }
            set
            {
                thisColorTable.m_menuItemPressedGradientBegin = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("선택된 메뉴의 배경색 지정[END COLOR]")]
        public Color MenuItemPressedGradientEnd
        {
            get
            {
                return thisColorTable.m_menuItemPressedGradientEnd;
            }
            set
            {
                thisColorTable.m_menuItemPressedGradientEnd = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("선택된 메뉴의 Dropdown 목록 배경색 지정")]
        public Color ToolStripDropDownBackground
        {
            get
            {
                return thisColorTable.m_toolStripDropDownBackground;
            }
            set
            {
                thisColorTable.m_toolStripDropDownBackground = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("선택된 메뉴의 Dropdown 목록에서 Image표시부 여백 색상 지정[BEGIN COLOR]")]
        public Color ImageMarginGradientBegin
        {
            get
            {
                return thisColorTable.m_imageMarginGradientBegin;
            }
            set
            {
                thisColorTable.m_imageMarginGradientBegin = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("선택된 메뉴의 Dropdown 목록에서 Image표시부 여백 색상 지정[MIDDLE COLOR]")]
        public Color ImageMarginGradientMiddle
        {
            get
            {
                return thisColorTable.m_imageMarginGradientMiddle;
            }
            set
            {
                thisColorTable.m_imageMarginGradientMiddle = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("선택된 메뉴의 Dropdown 목록에서 Image표시부 여백 색상 지정[END COLOR]")]
        public Color ImageMarginGradientEnd
        {
            get
            {
                return thisColorTable.m_imageMarginGradientEnd;
            }
            set
            {
                thisColorTable.m_imageMarginGradientEnd = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("선택된 메뉴의 Dropdown 목록에 MouseHover시 표시되는 배경색")]
        public Color MenuItemSelected
        {
            get
            {
                return thisColorTable.m_menuItemSelected;
            }
            set
            {
                thisColorTable.m_menuItemSelected = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("최상위 메뉴에 MouseHover시 표시되는 배경색[BEGIN COLOR]")]
        public Color MenuItemSelectedGradientBegin
        {
            get
            {
                return thisColorTable.m_menuItemSelectedGradientBegin;
            }
            set
            {
                thisColorTable.m_menuItemSelectedGradientBegin = value;
                Invalidate();
            }
        }

        [Browsable(true), Category("CustomControl"), Description("최상위 메뉴에 MouseHover시 표시되는 배경색[END COLOR]")]
        public Color MenuItemSelectedGradientEnd
        {
            get
            {
                return thisColorTable.m_menuItemSelectedGradientEnd;
            }
            set
            {
                thisColorTable.m_menuItemSelectedGradientEnd = value;
                Invalidate();
            }
        }

        #endregion

        #region ## CONSTRUCTOR
        public ProfessionalMenuStrip()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            thisColorTable = new ColorTable();
            this.Renderer = new ToolStripProfessionalRenderer(thisColorTable);
            this.Dock = DockStyle.None;

            InitializeComponent();

        }
        #endregion

        #region ## INTERNAL CLASS
        internal class ColorTable : ProfessionalColorTable
        {
            public Color m_menuBorder = Color.Green;
            public Color m_menuItemBorder = Color.Blue;
            public Color m_menuStripGradientBegin = Color.DodgerBlue;
            public Color m_menuStripGradientEnd = Color.White;

            public Color m_menuItemPressedGradientBegin = Color.Blue;
            public Color m_menuItemPressedGradientEnd = Color.Blue;
            public Color m_toolStripDropDownBackground = Color.DodgerBlue;
            public Color m_imageMarginGradientBegin = Color.DodgerBlue;
            public Color m_imageMarginGradientMiddle = Color.DodgerBlue;
            public Color m_imageMarginGradientEnd = Color.DodgerBlue;
            public Color m_menuItemSelected = Color.LightBlue;
            public Color m_menuItemSelectedGradientBegin = Color.LightBlue;
            public Color m_menuItemSelectedGradientEnd = Color.LightBlue;


            //선택된 최상위메뉴와 DropDown목록의 Border 지정
            public override Color MenuBorder
            {
                get
                {
                    return m_menuBorder;
                }
            }

            //선택된 최상위메뉴의 DropDown목록중 마우스 오버되어있는 항목의 Border 지정
            public override Color MenuItemBorder
            {
                get
                {
                    return m_menuItemBorder;
                }
            }

            //최상위 메뉴 색
            public override Color MenuStripGradientBegin
            {
                get
                {
                    return m_menuStripGradientBegin;
                }
            }

            public override Color MenuStripGradientEnd
            {
                get
                {
                    return m_menuStripGradientEnd;
                }
            }

            //선택된 최상위 메뉴 색
            public override Color MenuItemPressedGradientBegin
            {
                get
                {
                    return m_menuItemPressedGradientBegin;
                }
            }

            public override Color MenuItemPressedGradientEnd
            {
                get
                {
                    return m_menuItemPressedGradientEnd;
                }
            }

            //선택된 최상위메뉴에 DropDown목록들의 뒷배경색 지정
            public override Color ToolStripDropDownBackground
            {
                get
                {
                    return m_toolStripDropDownBackground;
                }
            }

            //선택된 최상위메뉴에 DropDown목록들이 가지고 있는 Image 여백의 색상 지정
            public override Color ImageMarginGradientBegin
            {
                get
                {
                    return m_imageMarginGradientBegin;
                }
            }

            public override Color ImageMarginGradientMiddle
            {
                get
                {
                    return m_imageMarginGradientMiddle;
                }
            }

            public override Color ImageMarginGradientEnd
            {
                get
                {
                    return m_imageMarginGradientEnd;
                }
            }

            // DropDown목록에 MouseHover시 BackColor
            public override Color MenuItemSelected
            {
                get
                {
                    return m_menuItemSelected;
                }
            }

            // 최상위 메뉴 MouseHover시 메뉴 BackColor
            public override Color MenuItemSelectedGradientBegin
            {
                get
                {
                    return m_menuItemSelectedGradientBegin;
                }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get
                {
                    return m_menuItemSelectedGradientEnd;
                }
            }
        }
        #endregion

    }


}
