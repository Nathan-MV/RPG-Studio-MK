﻿using System;
using System.Collections.Generic;
using System.IO;
using MKEditor.Data;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class WidgetWindow : Window
    {
        public UIManager UI;

        public WidgetWindow()
        {
            GameData.Initialize("D:\\Desktop\\MK\\MK\\data");

            this.SetSize(1080, 720);

            this.Initialize();

            this.SetBackgroundColor(240, 240, 240);

            this.UI = new UIManager(this);

            Grid layout = new Grid(this);
            layout.SetRows(
                new GridSize(37, Unit.Pixels),
                new GridSize(33, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1)
            );
            layout.SetColumns(
                new GridSize(234, Unit.Pixels),
                new GridSize(1),
                new GridSize(333, Unit.Pixels)
            );
            // Header
            new Widget(layout)
                .SetBackgroundColor(40, 44, 52)
                .SetGrid(0, 0, 0, 2);

            // Toolbar
            Widget w = new Widget(layout)
                .SetBackgroundColor(27, 28, 32)
                .SetGrid(1, 1, 0, 2);

            // Orange separator
            new Widget(layout)
                .SetBackgroundColor(255, 191, 31)
                .SetGrid(2, 2, 0, 2);

            // Left sidebar
            MapSelectTab mst = new MapSelectTab(layout);
            mst.SetGrid(3, 0);
            mst.SetBackgroundColor(27, 28, 32);

            // Right sidebar
            Grid rightcontainer = new Grid(layout).SetGrid(3, 2) as Grid;
            rightcontainer.SetRows(new GridSize(2), new GridSize(4, Unit.Pixels), new GridSize(1));
            rightcontainer.SetColumns(new GridSize(1));
            rightcontainer.SetBackgroundColor(40, 44, 52);

            // Tileset part of right sidebar
            TilesetTab tt = new TilesetTab(rightcontainer);

            // Fixed empty space in right sidebar
            new Widget(rightcontainer)
                .SetBackgroundColor(40, 44, 52)
                .SetGrid(1, 0);

            // Layers part of right sidebar
            LayersTab lt = new LayersTab(rightcontainer);
            lt.SetGrid(2, 0);
            lt.SetBackgroundColor(27, 28, 32);

            // Center map viewer
            Container mapcontainer = new Container(layout);
            mapcontainer.SetGrid(3, 1);
            mapcontainer.AutoScroll = true;
            mapcontainer.SetBackgroundColor(40, 44, 52);

            MapViewer mv = new MapViewer(mapcontainer);

            mv.LayersTab = lt;
            mv.TilesetTab = tt;
            lt.TilesetTab = tt;
            lt.MapViewer = mv;
            tt.LayersTab = lt;
            tt.MapViewer = mv;
            mst.MapViewer = mv;

            Map map = null;
            foreach (Map m in GameData.Maps.Values) { map = m; break; }
            mv.SetMap(map);

            this.OnMouseDown += UI.MouseDown;
            this.OnMousePress += UI.MousePress;
            this.OnMouseUp += UI.MouseUp;
            this.OnMouseMoving += UI.MouseMoving;
            this.OnMouseWheel += UI.MouseWheel;
            this.OnTextInput += UI.TextInput;
            this.OnWindowResized += UI.WindowResized;

            this.OnTick += Tick;
            
            this.UI.Update();

            this.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            this.UI.Update();
        }
    }
}
