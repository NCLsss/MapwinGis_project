﻿

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MWLite.Symbology.Classes;
using MWLite.Symbology.Controls;
using MapWinGIS;
using MWLite.Symbology.LegendControl;

namespace MWLite.Symbology.Forms.Symbology
{
    public partial class PointsForm : Form
    {
        private LegendControl.Legend m_legend;

        private Layer m_layer;
        
        // 绘制选项
        private ShapeDrawingOptions _options = null;
        
        // tab索引
        private static int _tabIndex = 0;

        // 指示事件能否触发
        private bool _noEvents = false;

        // 状态
        string _initState = "";

        #region Initialization
     
        public PointsForm(LegendControl.Legend legend, Layer layer, ShapeDrawingOptions options, bool applyDisabled)
        {
            InitializeComponent();
            if (options == null || legend == null)
            {
                throw new Exception("PointsForm: Unexpected null parameter");
            }

            m_legend = legend;

            // setting values to the controls
            _options = options;
            m_layer = layer;
            _initState = _options.Serialize();
            _noEvents = true;

            btnApply.Visible = !applyDisabled;

            clpFillColor.SelectedColorChanged += clpFillColor_SelectedColorChanged;
            cboIconCollection.SelectedIndexChanged += CboIconCollectionSelectedIndexChanged;
            cboFillType.SelectedIndexChanged += cboFillType_SelectedIndexChanged;

            icbPointShape.ComboStyle = ImageComboStyle.PointShape;
            icbLineType.ComboStyle = ImageComboStyle.LineStyle;
            icbLineWidth.ComboStyle = ImageComboStyle.LineWidth;
            icbHatchStyle.ComboStyle = ImageComboStyle.HatchStyle;

            pnlFillPicture.Parent = groupBox3;   
            pnlFillPicture.Top = pnlFillHatch.Top;
            pnlFillPicture.Left = pnlFillHatch.Left;

            pnlFillGradient.Parent = groupBox3;    
            pnlFillGradient.Top = pnlFillHatch.Top;
            pnlFillGradient.Left = pnlFillHatch.Left;

            cboFillType.Items.Clear();
            cboFillType.Items.Add("Solid");
            cboFillType.Items.Add("Hatch");
            cboFillType.Items.Add("Gradient");

            cboGradientType.Items.Clear();
            cboGradientType.Items.Add("Linear");
            cboGradientType.Items.Add("Retangular");
            cboGradientType.Items.Add("Circle");

            
            cboFontName.SelectedIndexChanged += cboFontName_SelectedIndexChanged;
            RefreshFontList(null, null);
            characterControl1.SelectedCharacterCode = (byte)_options.PointCharacter;

            
            RefreshIconCombo();

            chkScaleIcons.Checked = _options.PictureScaleX != 1.0 || _options.PictureScaleY != 1.0;

            //if (layer != null)
            //{
                //SymbologySettings settings = Globals.get_LayerSettings(m_layer.Handle);
                //if (settings != null)
                //{
                //    iconControl1.SelectedIndex = settings.IconIndex;
                //    chkScaleIcons.Checked = settings.ScaleIcons;
                //    string name = settings.IconCollection.ToLower();
                //    for (int i = 0; i < cboIconCollection.Items.Count; i++)
                //    {
                //        if (cboIconCollection.Items[i].ToString().ToLower() == name)
                //        {
                //            cboIconCollection.SelectedIndex = i;
                //            break;
                //        }
                //    }
                //}
            //}

            Options2Gui();
            _noEvents = false;

            // -----------------------------------------------------
            // 订阅事件
            // -----------------------------------------------------
            udRotation.ValueChanged += Gui2Options;
            udPointNumSides.ValueChanged += Gui2Options;
            udSideRatio.ValueChanged += Gui2Options;
            udSize.ValueChanged += Gui2Options;
            chkShowAllFonts.CheckedChanged += RefreshFontList;

            // 线
            chkOutlineVisible.CheckedChanged += Gui2Options;
            icbLineType.SelectedIndexChanged += Gui2Options;
            icbLineWidth.SelectedIndexChanged += Gui2Options;
            clpOutline.SelectedColorChanged += clpOutline_SelectedColorChanged;
            
            chkFillVisible.CheckedChanged += Gui2Options;
            
            iconControl1.SelectionChanged += IconControl1SelectionChanged;
            chkScaleIcons.CheckedChanged += Gui2Options;

            // 字符
            characterControl1.SelectionChanged += characterControl1_SelectionChanged;
            symbolControl1.SelectionChanged += SymbolControl1SelectionChanged;

            // hatch填充
            icbHatchStyle.SelectedIndexChanged += Gui2Options;
            chkFillBgTransparent.CheckedChanged += Gui2Options;
            clpHatchBack.SelectedColorChanged += Gui2Options;

            // gradient填充
            clpGradient2.SelectedColorChanged += Gui2Options;
            udGradientRotation.ValueChanged += Gui2Options;
            cboGradientType.SelectedIndexChanged += Gui2Options;

            DrawPreview();

            tabControl1.SelectedIndex = _tabIndex;
        }
        #endregion

        #region ChangingFill
        /// <summary>
        /// 填充样式选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cboFillType_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlFillGradient.Visible = false;
            pnlFillHatch.Visible = false;
            pnlFillPicture.Visible = false;
            lblNoOptions.Visible = false;

            if (cboFillType.SelectedIndex == (int)tkFillType.ftHatch)
            {
                pnlFillHatch.Visible = true;
                _options.FillType = tkFillType.ftHatch;
            }
            else if (cboFillType.SelectedIndex == (int)tkFillType.ftGradient)
            {
                pnlFillGradient.Visible = true;
                _options.FillType = tkFillType.ftGradient;
            }
            else if (cboFillType.SelectedIndex == (int)tkFillType.ftPicture)
            {
                pnlFillPicture.Visible = true;
                _options.FillType = tkFillType.ftPicture;
            }
            else
            {
                lblNoOptions.Visible = true;
                _options.FillType = tkFillType.ftStandard;
            }

            if (!_noEvents)
                btnApply.Enabled = true;
            DrawPreview();
        }
        #endregion

        #region Icons
        /// <summary>
        ///绘制cobo的图标
        /// </summary>
        private void RefreshIconCombo()
        {
            cboIconCollection.Items.Clear();
            
            string path = Globals.GetIconsPath();
            if (!Directory.Exists(path))
            {
                cboIconCollection.Enabled = false;
                chkScaleIcons.Enabled = false;
                return;
            }

            string[] directories = Directory.GetDirectories(path);
            if (directories.Length <= 0)
            {
                // TODO: report error 
            }
            else
            {
                for (int i = 0; i < directories.Length; i++)
                {
                    string[] files = Directory.GetFiles(directories[i]);
                    
                    foreach(var file in files)
                    {
                        string ext = Path.GetExtension(file).ToLower();
                        if (ext == ".png")          //ext == ".bmp" || 
                        {
                            string name = directories[i].Substring(path.Length);
                            cboIconCollection.Items.Add(name);
                            break;
                        }
                    }
                    
                }
                if (cboIconCollection.Items.Count <= 0)
                {
                    cboIconCollection.Enabled = false;
                }
                else
                {
                    foreach(var item in cboIconCollection.Items)
                    {
                        if (item.ToString().ToLower() == "standard")
                        {
                            cboIconCollection.SelectedItem = item;
                            break;
                        }
                    }
                    if (cboIconCollection.SelectedItem == null)
                        cboIconCollection.SelectedIndex = 0;
                }
            }           
        }

        /// <summary>
        ///当选择了icon，更新预览
        /// </summary>
        private void IconControl1SelectionChanged()
        {
            string filename = iconControl1.SelectedName;
            if (string.IsNullOrWhiteSpace(filename))
                return;
            
            var bmp = new Bitmap(filename);
            Color clrTransparent = Color.White;

            MapWinGIS.Image img = new MapWinGIS.Image();
            if (img.Open(filename, ImageType.USE_FILE_EXTENSION, true, null))
            {
                img.LoadBuffer(50);

                img.TransparencyColor = Colors.ColorToUInteger(clrTransparent);
                img.TransparencyColor2 = Colors.ColorToUInteger(clrTransparent);
                img.UseTransparencyColor = true;

                _options.PointType = tkPointSymbolType.ptSymbolPicture;
                _options.Picture = img;

                UpdatePrictureScale();

                DrawPreview();
            }
            else
            {
                string errString = string.Empty;
                if (img.LastErrorCode != 0)
                {
                    errString = ": " + img.ErrorMsg[img.LastErrorCode];
                }
                MessageBox.Show("Failed to open image: " + errString);
            }
            
            if (!_noEvents)
                btnApply.Enabled = true;
        }

        private void UpdatePrictureScale()
        {
            if (chkScaleIcons.Checked && _options.Picture != null)
            {
                var img = _options.Picture;
                int size = Math.Max(img.Width, img.Height);
                if (img.Width > img.Height)
                {
                    _options.PictureScaleX = (double)udSize.Value / size;
                    _options.PictureScaleY = _options.PictureScaleX;
                }
                else
                {
                    _options.PictureScaleY = (double)udSize.Value / size;
                    _options.PictureScaleX = _options.PictureScaleY;
                }
            }
            else
            {
                _options.PictureScaleX = _options.PictureScaleY = 1.0;
            }
        }


        private void CboIconCollectionSelectedIndexChanged(object sender, EventArgs e)
        {
            string path = Globals.GetIconsPath();
            path += cboIconCollection.Text;

            if (Directory.Exists(path))
            {
                iconControl1.CellWidth = 32;
                iconControl1.CellHeight = 32;
                
                try
                {
                    string[] files = Directory.GetFiles(path);
                    foreach (string name in files)
                    {
                        string ext = Path.GetExtension(name);
                        if (ext == ".png")          //ext == ".bmp" || 
                        {
                            Bitmap bmp = new Bitmap(name);
                            if (bmp.Width <= 16 || bmp.Height <= 16)
                            {
                                // do nothing - use 32
                            }
                            else if (bmp.Width < 48 && bmp.Height < 48)
                            {
                                iconControl1.CellWidth = bmp.Height < bmp.Width ? bmp.Height + 16 : bmp.Width + 16;
                                iconControl1.CellHeight = iconControl1.CellWidth;
                            }
                            else
                            {
                                iconControl1.CellWidth = 48 + 16;
                                iconControl1.CellHeight = iconControl1.CellWidth;
                            }
                            break;
                        }
                    }
                }
                catch {}
            }
            
            
            iconControl1.FilePath = path;
            //lblCopyright.Text = "";

            //string filename = path + @"\copyright.txt";
            //if (File.Exists(filename))
            //{
            //    StreamReader reader = null;
            //    try
            //    {
            //        reader = new StreamReader(filename);
            //        lblCopyright.Text = reader.ReadLine();
            //    }
            //    finally
            //    {
            //        if (reader != null)
            //            reader.Close();
            //    }
            //}
        }
        #endregion

        #region FontCharacters
        /// <summary>
        /// 字符列表更新
        /// </summary>
        private void RefreshFontList(object sender, EventArgs e)
        {
            cboFontName.Items.Clear();
            if (!chkShowAllFonts.Checked)
            {
                foreach (FontFamily family in FontFamily.Families)
                {
                    string name = family.Name.ToLower();

                    if (name == "webdings" ||
                        name == "wingdings" ||
                        name == "wingdings 2" ||
                        name == "wingdings 3" ||
                        name == "times new roman")
                    {
                        cboFontName.Items.Add(family.Name);
                    }
                }
            }
            else
            {
                foreach (FontFamily family in FontFamily.Families)
                {
                    cboFontName.Items.Add(family.Name);
                }
            }

            string fontName = _options.FontName.ToLower();
            for (int i = 0; i < cboFontName.Items.Count; i++)
            {
                if (cboFontName.Items[i].ToString().ToLower() == fontName)
                {
                    cboFontName.SelectedIndex = i;
                    break;
                }
            }
            if (cboFontName.SelectedIndex == -1)
            {
                cboFontName.SelectedItem = "Arial";
            }
        }

        /// <summary>
        ///改变字体
        /// </summary>
        private void cboFontName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_noEvents)
                btnApply.Enabled = true;
            characterControl1.SetFontName(cboFontName.Text);
            _options.FontName = cboFontName.Text;
            DrawPreview();
        }


        void characterControl1_SelectionChanged()
        {
            if (!_noEvents)
                btnApply.Enabled = true;
            _options.PointType = tkPointSymbolType.ptSymbolFontCharacter;
            _options.PointCharacter = Convert.ToInt16(characterControl1.SelectedCharacterCode);
            DrawPreview();
        }
        #endregion

        #region Drawing
        /// <summary>
        /// 绘制预览图
        /// </summary>
        private void DrawPreview()
        {
            if (_noEvents)
            {
                return;
            }

            if (pctPreview.Image != null)
            {
                pctPreview.Image.Dispose();
            }

            Rectangle rect = pctPreview.ClientRectangle;
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            IntPtr ptr = g.GetHdc();

            // creating shape to draw
            _options.DrawPoint(ptr, 0.0f, 0.0f, rect.Width, rect.Height, Colors.ColorToUInteger(this.BackColor));

            g.ReleaseHdc();
            pctPreview.Image = bmp;
        }
        #endregion

        private void SymbolControl1SelectionChanged()
        {
            tkDefaultPointSymbol symbol = (tkDefaultPointSymbol)symbolControl1.SelectedIndex;
            _options.SetDefaultPointSymbol(symbol);
            if (!_noEvents)
                btnApply.Enabled = true;
            
            Options2Gui();
            DrawPreview();
        }

        #region Properties
        /// <summary>
        /// 绘制
        /// </summary>
        private void Gui2Options(object sender, EventArgs e)
        {
            if (_noEvents)
            {
                return;
            }
            
            _options.PointSize = (float)udSize.Value;

            UpdatePrictureScale();

            _options.PointRotation = (double)udRotation.Value;
            _options.FillColor = Colors.ColorToUInteger(clpFillColor.Color);
            
            
            _options.PointShape = (tkPointShapeType)icbPointShape.SelectedIndex;
            _options.PointSidesCount = (int)udPointNumSides.Value;
            _options.PointSidesRatio = (float)udSideRatio.Value / 10;
        
            _options.LineStipple = (tkDashStyle)icbLineType.SelectedIndex;
            _options.LineWidth = (float)icbLineWidth.SelectedIndex + 1;
            _options.LineVisible = chkOutlineVisible.Checked;
            _options.FillVisible = chkFillVisible.Checked;
            _options.FillType = (tkFillType)cboFillType.SelectedIndex;

            // hatch
            _options.FillHatchStyle = (tkGDIPlusHatchStyle)icbHatchStyle.SelectedIndex;
            _options.FillBgTransparent = chkFillBgTransparent.Checked;
            _options.FillBgColor = Colors.ColorToUInteger(clpHatchBack.Color);

            // gradient
            _options.FillGradientType = (tkGradientType)cboGradientType.SelectedIndex;
            _options.FillColor2 = Colors.ColorToUInteger(clpGradient2.Color);
            _options.FillRotation = (double)udGradientRotation.Value;

            _options.FillTransparency = (float)transparencyControl1.Value;
            _options.LineTransparency = (float)transparencyControl1.Value;

            if (!_noEvents)
                btnApply.Enabled = true;

            DrawPreview();
        }

        /// <summary>
        /// 加载图例
        /// </summary>
        private void Options2Gui()
        {
            _noEvents = true;
            udSize.SetValue(_options.PointSize);
            udRotation.SetValue(_options.PointRotation);
            clpFillColor.Color = Colors.UintToColor(_options.FillColor);

            // point
            icbPointShape.SelectedIndex = (int)_options.PointShape;
            udPointNumSides.SetValue(_options.PointSidesCount);
            udSideRatio.SetValue(_options.PointSidesRatio * 10.0);
            
            // options
            icbLineType.SelectedIndex = (int)_options.LineStipple;
            icbLineWidth.SelectedIndex = (int)_options.LineWidth - 1;
            cboFillType.SelectedIndex = (int)_options.FillType;
            chkOutlineVisible.Checked = _options.LineVisible;
            clpOutline.Color = Colors.UintToColor(_options.LineColor);
            chkFillVisible.Checked = _options.FillVisible;
            
            // hatch
            icbHatchStyle.SelectedIndex = (int)_options.FillHatchStyle;
            chkFillBgTransparent.Checked = _options.FillBgTransparent;
            clpHatchBack.Color = Colors.UintToColor(_options.FillBgColor);

            // gradient
            cboGradientType.SelectedIndex = (int)_options.FillGradientType;
            clpGradient2.Color = Colors.UintToColor(_options.FillColor2);
            udGradientRotation.Value = (decimal)_options.FillRotation;

            transparencyControl1.Value = (byte)_options.FillTransparency;

            _noEvents = false;
        }
        #endregion

        #region Colors
        /// <summary>
        /// Updates all the controls with the selected fill color
        /// </summary>
        private void clpFillColor_SelectedColorChanged(object sender, EventArgs e)
        {
            _options.FillColor = Colors.ColorToUInteger(clpFillColor.Color);
            symbolControl1.ForeColor = clpFillColor.Color;
            characterControl1.ForeColor = clpFillColor.Color;
            icbPointShape.Color1 = clpFillColor.Color;
            if (!_noEvents)
                btnApply.Enabled = true;
            DrawPreview();
        }

        /// <summary>
        ///  Updates all the control with the selected outline color
        /// </summary>
        void clpOutline_SelectedColorChanged(object sender, EventArgs e)
        {
            _options.LineColor = Colors.ColorToUInteger(clpOutline.Color);
            
            // TODO: implement
            //symbolControl1.ForeColor = clpFillColor.Color;
            //characterControl1.ForeColor = clpFillColor.Color;
            //icbPointShape.Color1 = clpFillColor.Color;

            if (!_noEvents)
                btnApply.Enabled = true;
            DrawPreview();
        }
        #endregion


        private void btnOk_Click(object sender, EventArgs e)
        {
            _tabIndex = tabControl1.SelectedIndex;

            SymbologySettings settings = null; //Globals.get_LayerSettings(_layer.Handle, _mapWin);

            if (settings != null)
            {
                settings.IconCollection = cboIconCollection.Text;
                settings.ScaleIcons = chkScaleIcons.Checked;
                settings.IconIndex = iconControl1.SelectedIndex;
                //Globals.SaveLayerSettings(_layer.Handle, settings, _mapWin);
            }

            if (_options.Serialize() != _initState)
            {
                m_legend.FireLayerPropertiesChanged(m_layer.Handle);
                m_legend.RedrawLegendAndMap();
            }
        }


        private void transparencyControl1_ValueChanged(object sender, byte value)
        {
            Gui2Options(null, null);
        }


        private void btnApply_Click(object sender, EventArgs e)
        {
            m_legend.FireLayerPropertiesChanged(m_layer.Handle);
            m_legend.RedrawLegendAndMap();
            btnApply.Enabled = false;
            _initState = _options.Serialize();
        }

        private void frmPoints_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.Cancel)
            {
                _tabIndex = tabControl1.SelectedIndex;
                _options.Deserialize(_initState);
            }
        }
    }
}
