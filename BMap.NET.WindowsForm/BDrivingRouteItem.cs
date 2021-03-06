﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using BMap.NET.WindowsForm.BMapElements;

namespace BMap.NET.WindowsForm
{
    /// <summary>
    /// 驾车路线控件
    /// </summary>
    partial class BDrivingRouteItem : UserControl
    {
        /// <summary>
        /// 步骤选择时激发该事件
        /// </summary>
        public event StepSelectedEventHandler StepSelected;
        /// <summary>
        /// 路线选择时激发该事件
        /// </summary>
        public event RouteSelectedEventHandler RouteSelected;
        /// <summary>
        /// 路线起点终点选中时激发该事件
        /// </summary>
        public event StepEndPointSelectedEventHandler StepEndPointSelected;
        private BRoute _dataSource;
        /// <summary>
        /// 路线数据源
        /// </summary>
        public BRoute DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                _dataSource = value;
                if (_dataSource != null) //解析 具体json格式参见api文档
                {
                    _distance = double.Parse((string)_dataSource.DataSource["distance"]);
                    _duration = double.Parse((string)_dataSource.DataSource["duration"]);
                    _toll = double.Parse((string)_dataSource.DataSource["toll"]);
                    if (Origin != null)  //起点
                    {
                        flpSteps.Controls.Add(Origin);
                        Origin.StepEndPointSelected+=new StepEndPointSelectedEventHandler(Origin_StepEndPointSelected);
                    }
                    foreach (JObject step in _dataSource.DataSource["steps"])
                    {
                        BDrivingStepItem item = new BDrivingStepItem();
                        item.DataSource = step;
                        item.Width = flpSteps.Width - 17;
                        flpSteps.Controls.Add(item);
                        item.Margin = new Padding(0);
                        item.StepSelected+=new StepSelectedEventHandler(item_StepSelected);
                        if (item.Step_POIs != null)
                            _pois_near += item.Step_POIs + ",";
                    }
                    if (Destination != null) //终点
                    {
                        flpSteps.Controls.Add(Destination);
                        Destination.StepEndPointSelected+=new StepEndPointSelectedEventHandler(Destination_StepEndPointSelected);
                    }
                    if (_pois_near != null)
                    {
                        _pois_near = _pois_near.TrimEnd(new char[] { ',' });
                    }
                    foreach (Control c in flpSteps.Controls)
                    {
                        _steps_height += c.Height;
                    }
                    Selected = false;
                }
            }
        }

        private double _distance;
        private double _duration;
        private string _pois_near;
        private double _toll;

        private int _steps_height;

        private bool _mouse_hover;

        private bool _selected;
        /// <summary>
        /// 是否被选中
        /// </summary>
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                if (_selected)
                {
                    Height = 70 + _steps_height;
                }
                else
                {
                    Height = 70;
                }
            }
        }
        /// <summary>
        /// 起点
        /// </summary>
        public BStepStartAndEndItem Origin
        {
            get;
            set;
        }
        /// <summary>
        /// 终点
        /// </summary>
        public BStepStartAndEndItem Destination
        {
            get;
            set;
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        public BDrivingRouteItem()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        #region 事件处理
        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BDrivingRouteItem_MouseEnter(object sender, EventArgs e)
        {
            _mouse_hover = true;
            Invalidate();
        }
        /// <summary>
        /// 鼠标移出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BDrivingRouteItem_MouseLeave(object sender, EventArgs e)
        {
            _mouse_hover = false;
            Invalidate();
        }
        /// <summary>
        /// 鼠标点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BDrivingRouteItem_Click(object sender, EventArgs e)
        {
            Selected = true;
            if (_selected)
            {
                if (RouteSelected != null)
                {
                    RouteSelected(_dataSource);
                }
            }
        }
        /// <summary>
        /// 重绘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BDrivingRouteItem_Paint(object sender, PaintEventArgs e)
        {
            if (Selected || _mouse_hover)
            {
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(235, 241, 251)))
                {
                    e.Graphics.FillRectangle(sb, new Rectangle(0, 0, Width - 1, 70 - 1));
                }
            }
            e.Graphics.DrawRectangle(Pens.LightGray, new Rectangle(0, 0, Width - 1, 70 - 1));
            using (Font f = new Font("微软雅黑", 9))
            {
                e.Graphics.DrawString(Math.Round(_duration / 60, 0) + "分钟 | " + Math.Round(_distance / 1000, 1) + "公里 | 过路费" + Math.Round(_toll,1) + "元", f, Brushes.Gray, new PointF(20, 10));
                e.Graphics.DrawString("途径：" + _pois_near, f, Brushes.DarkGray, new PointF(20, 35));
            }           
        }
        /// <summary>
        /// 路线步骤选中
        /// </summary>
        /// <param name="stepPath"></param>
        /// <param name="enlarge"></param>
        void item_StepSelected(string stepPath, bool enlarge)
        {
            if (StepSelected != null)
            {
                StepSelected(stepPath, enlarge);
            }
        }
        /// <summary>
        /// 路线终点选中
        /// </summary>
        /// <param name="bPoint"></param>
        void Destination_StepEndPointSelected(BPoint bPoint)
        {
            if (StepEndPointSelected != null)
            {
                StepEndPointSelected(bPoint);
            }
        }
        /// <summary>
        /// 路线起点选中
        /// </summary>
        /// <param name="bPoint"></param>
        void Origin_StepEndPointSelected(BPoint bPoint)
        {
            if (StepEndPointSelected != null)
            {
                StepEndPointSelected(bPoint);
            }
        }
        #endregion
    }
}
