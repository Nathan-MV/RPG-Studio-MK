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
        public bool Blocked = false;

        public WidgetWindow()
        {
            Utilities.Initialize();

            GameData.Initialize("D:\\Desktop\\MK\\MK\\data");

            this.SetSize(1080, 720);
            this.SetMinimumSize(600, 400);
            this.Initialize();
            using (Bitmap b = new Bitmap(9, 14)) // Set cursor
            {
                Color gray = new Color(55, 51, 55);
                Color white = Color.WHITE;
                b.Unlock();
                b.DrawLine(0, 0, 0, 12, gray);
                b.DrawLine(1, 0, 8, 7, gray);
                b.SetPixel(8, 8, gray);
                b.SetPixel(8, 9, gray);
                b.SetPixel(7, 9, gray);
                b.SetPixel(6, 9, gray);
                b.SetPixel(6, 10, gray);
                b.SetPixel(7, 11, gray);
                b.SetPixel(7, 12, gray);
                b.SetPixel(7, 13, gray);
                b.SetPixel(6, 13, gray);
                b.SetPixel(5, 13, gray);
                b.SetPixel(4, 12, gray);
                b.SetPixel(4, 11, gray);
                b.SetPixel(3, 10, gray);
                b.SetPixel(2, 11, gray);
                b.SetPixel(1, 12, gray);
                b.DrawLine(1, 1, 1, 11, white);
                b.DrawLine(2, 2, 2, 10, white);
                b.DrawLine(3, 3, 3, 9, white);
                b.DrawLine(4, 4, 4, 10, white);
                b.DrawLine(5, 5, 5, 12, white);
                b.DrawLine(6, 6, 6, 8, white);
                b.SetPixel(7, 7, white);
                b.SetPixel(7, 8, white);
                b.SetPixel(6, 11, white);
                b.SetPixel(6, 12, white);
                b.Lock();
                Graphics.SetCursor(b);
            }
            this.UI = new UIManager(this);

            Grid layout = new Grid(this);
            layout.SetRows(
                new GridSize(32, Unit.Pixels),
                new GridSize(31, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1),
                new GridSize(1, Unit.Pixels),
                new GridSize(26, Unit.Pixels)
            );
            layout.SetColumns(
                new GridSize(222, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1),
                new GridSize(1, Unit.Pixels),
                new GridSize(283, Unit.Pixels)
            );

            Color DividerColor = new Color(79, 108, 159);

            // Header + Menubar
            MenuBar menu = new MenuBar(layout)
                .SetBackgroundColor(28, 50, 73)
                .SetGrid(0, 0, 0, 4) as MenuBar;
            menu.SetItems(new List<MenuItem>()
            {
                new MenuItem("File")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("New"),
                        new MenuItem("Open") { Shortcut = "Ctrl+O" },
                        new MenuItem("Save") { Shortcut = "Ctrl+S" },
                        new MenuItem("Close Project"),
                        new MenuItem("Exit Editor")
                    }
                },
                new MenuItem("Edit")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Cut"),
                        new MenuItem("Copy") { Shortcut = "Ctrl+C" },
                        new MenuItem("Paste") { Shortcut = "Ctrl+V" },
                        new MenuSeparator(),
                        new MenuItem("Undo") { Shortcut = "Ctrl+Z" },
                        new MenuItem("Redo") { Shortcut = "Ctrl+Y" },
                        new MenuSeparator(),
                        new MenuItem("Delete") { Shortcut = "Del" }
                    }
                },
                new MenuItem("View")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Focus Selected Layer"),
                        new MenuItem("Show Grid"),
                        new MenuItem("Zoom 1:1"),
                        new MenuItem("Zoom 1:2"),
                        new MenuItem("Zoom 1:4")
                    }
                },
                new MenuItem("Game")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Play Game") { Shortcut = "F12" },
                        new MenuItem("Open Game Folder")
                    }
                },
                new MenuItem("Help")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Help") { Shortcut = "F1" },
                        new MenuItem("About MK Editor")
                    }
                }
            });


            // Toolbar (modes, icons, etc)
            ToolBar toolbar = new ToolBar(layout);
            toolbar.SetGrid(1, 1, 0, 4);


            // Blue 1px separator
            new Widget(layout)
                .SetBackgroundColor(DividerColor)
                .SetGrid(2, 2, 0, 4);


            // Left sidebar
            MapSelectTab mst = new MapSelectTab(layout);
            mst.SetGrid(3, 0);

            // Left sidebar divider
            new Widget(layout)
                .SetBackgroundColor(79, 108, 159)
                .SetGrid(3, 3, 1, 1);

            // Right sidebar divider
            new Widget(layout)
                .SetBackgroundColor(DividerColor)
                .SetGrid(3, 3, 3, 3);

            // Right sidebar
            Grid rightcontainer = new Grid(layout).SetGrid(3, 4) as Grid;
            rightcontainer.SetRows(new GridSize(5), new GridSize(2));
            rightcontainer.SetColumns(new GridSize(1));
            rightcontainer.SetBackgroundColor(40, 44, 52);


            // Tileset part of right sidebar
            TilesetTab tt = new TilesetTab(rightcontainer);

            // Layers part of right sidebar
            LayersTab lt = new LayersTab(rightcontainer);
            lt.SetGrid(1, 0);


            // Center map viewer
            MapViewer mv = new MapViewer(layout);
            mv.SetGrid(3, 2);

            // Status bar divider
            new Widget(layout)
                .SetBackgroundColor(DividerColor)
                .SetGrid(4, 4, 0, 4);

            // Status bar
            StatusBar status = new StatusBar(layout);
            status.SetGrid(5, 5, 0, 4);



            // Link the UI pieces together
            mv.LayersTab = lt;
            mv.TilesetTab = tt;
            mv.ToolBar = toolbar;
            mv.StatusBar = status;

            lt.TilesetTab = tt;
            lt.MapViewer = mv;

            tt.LayersTab = lt;
            tt.MapViewer = mv;
            tt.ToolBar = toolbar;

            mst.MapViewer = mv;

            toolbar.MapViewer = mv;
            toolbar.TilesetTab = tt;
            toolbar.StatusBar = status;

            mst.StatusBar = status;

            status.MapViewer = mv;

            // Set initial map
            Map map = null;
            foreach (Map m in GameData.Maps.Values) { map = m; break; }
            mst.SetMap(map);

            // TEMP: Create map properties window
            //new MapPropertiesWindow(this);

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

        public void SetOverlayOpacity(byte Opacity)
        {
            TopSprite.Opacity = Opacity;
        }

        private void Tick(object sender, EventArgs e)
        {
            this.UI.Update();
        }
    }
}
