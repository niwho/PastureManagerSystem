// Copyright 2010 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at &lt;your ArcGIS install location&gt;/DeveloperKit10.0/userestrictions.txt.
// 

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;

namespace EditingUsingCustomForm
{
    public sealed partial class MainForm : Form
    {
        #region private members
        private IMapControl3 m_mapControl = null;

        //临时位置
        private Point temp_point;
        #endregion

        const int CS_DropSHADOW = 0x20000;
        const int GCL_STYLE = (-26);
        //声明Win32 API
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassLong(IntPtr hwnd, int nIndex);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);
        [DllImport("User32.dll", EntryPoint = "ReleaseCapture")]
        private static extern int ReleaseCapture();


        #region class constructor
        public MainForm()
        {
            InitializeComponent();
            //this.MyFormMouseDown += new MouseEventHandler(panel_title_bar_MouseDown);
            SetClassLong(this.Handle, GCL_STYLE, GetClassLong(this.Handle, GCL_STYLE) | CS_DropSHADOW); //API函数加载，实现窗体边框阴影效果
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {        
            m_mapControl = (IMapControl3) axMapControl1.Object;

            //relative file path to the sample data from EXE location
            string filePath = @".\data\USAMajorHighways";
            filePath = @".\data\软件用图-白三叶";
 
            //Add Lakes layer
            IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace workspace = (IFeatureWorkspace)workspaceFactory.OpenFromFile(filePath, axMapControl1.hWnd);
            IFeatureLayer featureLayer = new FeatureLayerClass();
            featureLayer.Name = "适宜区省";
            featureLayer.Visible = true;
            featureLayer.FeatureClass = workspace.OpenFeatureClass("适宜区省");

            /*#region create a SimplerRenderer
            IRgbColor color = new RgbColorClass();
            color.Red = 190;
            color.Green = 232;
            color.Blue = 255;

            ISimpleFillSymbol sym = new SimpleFillSymbolClass();
            sym.Color = color;

            ISimpleRenderer renderer = new SimpleRendererClass();
            renderer.Symbol = sym as ISymbol;
            #endregion

            ((IGeoFeatureLayer)featureLayer).Renderer = renderer as IFeatureRenderer;*/
            axMapControl1.Map.AddLayer((ILayer)featureLayer);

            //Add Highways layer
            featureLayer = new FeatureLayerClass();
            featureLayer.Name = "适宜区市";
            featureLayer.Visible = true;
            featureLayer.FeatureClass = workspace.OpenFeatureClass("适宜区市");//usa_major_highways
            axMapControl1.Map.AddLayer((ILayer)featureLayer);

            //Add Highways layer
            featureLayer = new FeatureLayerClass();
            featureLayer.Name = "适宜区县";
            featureLayer.Visible = true;
            featureLayer.FeatureClass = workspace.OpenFeatureClass("适宜区县");//usa_major_highways
            axMapControl1.Map.AddLayer((ILayer)featureLayer);

            //Add Highways layer
            featureLayer = new FeatureLayerClass();
            featureLayer.Name = "次适宜省";
            featureLayer.Visible = true;
            featureLayer.FeatureClass = workspace.OpenFeatureClass("次适宜省");//usa_major_highways

          #region create a SimplerRenderer
          IRgbColor color = new RgbColorClass();
          color.Red = 255;
          color.Green = 0;
          color.Blue = 0;

          ISimpleFillSymbol sym = new SimpleFillSymbolClass();
          sym.Color = color;

          ISimpleRenderer renderer = new SimpleRendererClass();
          renderer.Symbol = sym as ISymbol;
          #endregion

          ((IGeoFeatureLayer)featureLayer).Renderer = renderer as IFeatureRenderer;

            axMapControl1.Map.AddLayer((ILayer)featureLayer);

            //Add Highways layer
            featureLayer = new FeatureLayerClass();
            featureLayer.Name = "次适宜市";
            featureLayer.Visible = true;
            featureLayer.FeatureClass = workspace.OpenFeatureClass("次适宜市");//usa_major_highways
            axMapControl1.Map.AddLayer((ILayer)featureLayer);

            //Add Highways layer
            featureLayer = new FeatureLayerClass();
            featureLayer.Name = "次适宜县";
            featureLayer.Visible = true;
            featureLayer.FeatureClass = workspace.OpenFeatureClass("次适宜县");//usa_major_highways
            axMapControl1.Map.AddLayer((ILayer)featureLayer);

            //******** Important *************
            //store a reference to this form (Mainform) using the EditHelper class
            EditHelper.TheMainForm = this;
            EditHelper.IsEditorFormOpen = false;

            //add the EditCmd command to the toolbar
            axEditorToolbar.AddItem("esriControls.ControlsOpenDocCommand", 0, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
            axEditorToolbar.AddItem("esriControls.ControlsSaveAsDocCommand", 0, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
            axEditorToolbar.AddItem("esriControls.ControlsAddDataCommand", 0, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
            //axEditorToolbar.AddItem(new EditCmd(), 0, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
             
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            //Warn users if the ArcGIS Engine samples used by this application have not been compiled
            ArrayList checkList = new ArrayList();
            checkList.Add("ReshapePolylineEditTask_CS.ReshapePolylineEditTask");
            checkList.Add("VertexCommands_CS.UsingOutOfBoxVertexCommands");

            Type t = null;
            bool success = true;

            foreach (string item in checkList)
            {
                t = Type.GetTypeFromProgID(item);

                if (t == null)
                {
                    success = false;
                    break;
                }
            }

            /*if (!success)
                MessageBox.Show("Editing will not function correctly until the C# ReshapePolylineEditTask and VertexCommands samples have been compiled. More information can be found in the 'How to use' section for this sample.",
                    "Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);*/
            
        }


        #region public properties

        // Returns the MapControl
        public IMapControl3 MapControl
        {
            get { return m_mapControl; }
        }

        #endregion
        //close
        private void close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void min_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void close_MouseEnter(object sender, EventArgs e)
        {
            this.close.Image = Image.FromFile(@".\images\close_hover.png");
        }

        private void close_MouseLeave(object sender, EventArgs e)
        {
            this.close.Image = Image.FromFile(@".\images\close.png");
        }

        private void max_MouseEnter(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.max.Image = Image.FromFile(@".\images\max_hover.png");
            }
            else
            {
                this.max.Image = Image.FromFile(@".\images\yuan_hover.png");
            }
        }

        private void max_MouseLeave(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.max.Image = Image.FromFile(@".\images\max.png");
            }
            else
            {
                this.max.Image = Image.FromFile(@".\images\yuan.png ");
            }
        }

        private void max_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                this.WindowState = FormWindowState.Maximized;
            }else {
                this.WindowState = FormWindowState.Normal;
            }
            
        }

        private void min_MouseEnter(object sender, EventArgs e)
        {
            this.min.Image = Image.FromFile(@".\images\min_hover.png");
        }

        private void min_MouseLeave(object sender, EventArgs e)
        {
            this.min.Image = Image.FromFile(@".\images\min.png");
        }

        //窗体改变大小时
        private void MainForm_Resize(object sender, EventArgs e)
        {
            Point min_point = new Point(this.Width - 100,0);
            this.min.Location = min_point;
            Point max_point = new Point(this.Width - 70, 0);
            this.max.Location = max_point;
            Point close_point = new Point(this.Width - 40, 0);
            this.close.Location = close_point;


            //刷新
            this.Refresh();
        }

        private void panel_title_bar_MouseDown(object sender, MouseEventArgs e)
        {
            temp_point = new Point(e.X, e.Y);
        }

        private void panel_title_bar_DoubleClick(object sender, EventArgs e)
        {
           
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    //设置还原图片
                    this.max.Image = Image.FromFile(@".\images\yuan.png");
                    this.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    this.max.Image = Image.FromFile(@".\images\max.png");
                    this.WindowState = FormWindowState.Normal;
                }
        
        }
        //边框的绘制
        private void panel_container_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics,
                                this.panel_container.ClientRectangle,
                                Color.LightSeaGreen,         //left
                                1,
                                ButtonBorderStyle.None,
                                Color.LightSeaGreen,         //top
                                0,
                                ButtonBorderStyle.Solid,
                                Color.LightSeaGreen,        //right
                                1,
                                ButtonBorderStyle.Solid,
                                Color.LightSeaGreen,        //bottom
                                1,
                                ButtonBorderStyle.Solid);

        }

        private void panel_title_bar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (e.X - temp_point.X != 0 || e.Y - temp_point.Y != 0))
            {
                ReleaseCapture();
                SendMessage(this.Handle.ToInt32(), 0x0112, 0xF012, 0);
            }
        }

       
        




    }

}