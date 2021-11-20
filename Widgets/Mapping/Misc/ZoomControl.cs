﻿using System;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class ZoomControl : Widget
    {
        IconButton ZoomOut;
        IconButton ZoomIn;
        NumericSlider Slider;

        public double Factor { get; protected set; } = 1;

        public ZoomControl(IContainer Parent) : base(Parent)
        {
            SetSize(160, 26);

            ZoomOut = new IconButton(this);
            ZoomOut.Selectable = false;
            ZoomOut.SetIcon(7, 0);
            ZoomOut.SetSelectorOffset(-2);
            ZoomOut.OnMouseDown += delegate (MouseEventArgs e)
            {
                if (ZoomOut.WidgetIM.Hovering && e.LeftButton != e.OldLeftButton) DecreaseZoom();
            };
            ZoomIn = new IconButton(this);
            ZoomIn.Selectable = false;
            ZoomIn.SetIcon(8, 0);
            ZoomIn.SetPosition(Size.Width - 23, 0);
            ZoomIn.SetSelectorOffset(-2);
            ZoomIn.OnMouseDown += delegate (MouseEventArgs e)
            {
                if (ZoomIn.WidgetIM.Hovering && e.LeftButton != e.OldLeftButton) IncreaseZoom();
            };

            Slider = new NumericSlider(this);
            Slider.SetPosition(30, 4);
            Slider.SetMinimumValue(0);
            Slider.SetMaximumValue(6);
            Slider.SetValue(3);
            Slider.SetSnapValues(0, 1, 2, 3, 4, 5, 6);
            Slider.OnValueChanged += delegate (BaseEventArgs e)
            {
                if (Slider.Value == 0) SetZoomFactor(0.125);
                else if (Slider.Value == 1) SetZoomFactor(0.25);
                else if (Slider.Value == 2) SetZoomFactor(0.5);
                else if (Slider.Value == 3) SetZoomFactor(1.0);
                else if (Slider.Value == 4) SetZoomFactor(2.0);
                else if (Slider.Value == 5) SetZoomFactor(3.0);
                else if (Slider.Value == 6) SetZoomFactor(4.0);
            };
        }

        public void SetZoomFactor(double Factor, bool FromMapViewer = false)
        {
            if (this.Factor != Factor)
            {
                this.Factor = Factor;
                if (!FromMapViewer)
                {
                    if (Editor.MainWindow.MapWidget != null) Editor.MainWindow.MapWidget.SetZoomFactor(Factor, true);
                }
                if (this.Factor == 0.125) Slider.SetValue(0);
                else if (this.Factor == 0.25) Slider.SetValue(1);
                else if (this.Factor == 0.5) Slider.SetValue(2);
                else if (this.Factor == 1.0) Slider.SetValue(3);
                else if (this.Factor == 2.0) Slider.SetValue(4);
                else if (this.Factor == 3.0) Slider.SetValue(5);
                else if (this.Factor == 4.0) Slider.SetValue(6);
                Redraw();
            }
        }

        public void IncreaseZoom()
        {
            if (this.Factor == 0.125) SetZoomFactor(0.25);
            else if (this.Factor == 0.25) SetZoomFactor(0.5);
            else if (this.Factor == 0.5) SetZoomFactor(1.0);
            else if (this.Factor == 1.0) SetZoomFactor(2.0);
            else if (this.Factor == 2.0) SetZoomFactor(3.0);
            else if (this.Factor == 3.0) SetZoomFactor(4.0);
        }

        public void DecreaseZoom()
        {
            if (this.Factor == 0.25) SetZoomFactor(0.125);
            else if (this.Factor == 0.5) SetZoomFactor(0.25);
            else if (this.Factor == 1.0) SetZoomFactor(0.5);
            else if (this.Factor == 2.0) SetZoomFactor(1.0);
            else if (this.Factor == 3.0) SetZoomFactor(2.0);
            else if (this.Factor == 4.0) SetZoomFactor(3.0);
        }
    }
}
