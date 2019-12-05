﻿using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class MenuBar : Widget
    {
        public List<MenuItem> Items { get; private set; } = new List<MenuItem>();
        public MenuItem SelectedItem { get; private set; }
        public int SelectedIndex { get; private set; }

        ContextMenu ActiveMenu;

        public MenuBar(object Parent, string Name = "menuBar")
            : base(Parent, Name)
        {
            Sprites["header"] = new Sprite(this.Viewport);
            Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(60, 228, 254)));
            Sprites["selector"].Visible = false;
            Sprites["selector"].Y = 30;
            WidgetIM.OnMouseMoving += MouseMoving;
            WidgetIM.OnMouseDown += MouseDown;
            WidgetIM.OnMousePress += MousePress;
            WidgetIM.OnMouseUp += MouseUp;

            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.S, Keycode.CTRL), delegate (object sender, EventArgs e) { Editor.SaveProject(); }, true)
            });
        }

        public void SetItems(List<MenuItem> Items)
        {
            this.Items = Items;
            SetSize(192, Items.Count * 23 + 3);
        }

        protected override void Draw()
        {
            if (Sprites["header"].Bitmap != null) Sprites["header"].Bitmap.Dispose();
            int TotalWidth = 0;
            Font f = Font.Get("Fonts/ProductSans-M", 12);
            foreach (MenuItem mi in Items)
            {
                TotalWidth += f.TextSize(mi.Text).Width + 12;
            }
            Sprites["header"].Bitmap = new Bitmap(TotalWidth, 32);
            Sprites["header"].Bitmap.Unlock();
            Sprites["header"].Bitmap.Font = f;
            int x = 12;
            foreach (MenuItem mi in Items)
            {
                Size s = f.TextSize(mi.Text);
                Sprites["header"].Bitmap.DrawText(mi.Text, x, 10, Color.WHITE);
                x += s.Width + 12;
            }
            Sprites["header"].Bitmap.Lock();
            base.Draw();
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            UpdateSelection(e);
        }

        private void UpdateSelection(MouseEventArgs e)
        {
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            Font f = Sprites["header"].Bitmap.Font;
            int x = 12;
            int idx = -1;
            int selwidth = 0;
            int selx = 0;
            if (ry >= 0 && ry < Size.Height)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    int w = f.TextSize(Items[i].Text).Width;
                    if (rx > x && rx < x + w)
                    {
                        idx = i;
                        selwidth = w;
                        selx = x;
                        break;
                    }
                    x += w + 12;
                }
            }
            if (idx == -1)
            {
                Sprites["selector"].Visible = false;
                SelectedIndex = -1;
                SelectedItem = null;
            }
            else
            {
                Sprites["selector"].X = selx;
                (Sprites["selector"].Bitmap as SolidBitmap).SetSize(selwidth, 2);
                Sprites["selector"].Visible = true;
                SelectedIndex = idx;
                SelectedItem = Items[idx];
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (e.LeftButton != e.OldLeftButton && e.LeftButton)
            {
                if (SelectedIndex != -1)
                    ShowItem(SelectedIndex);
            }
        }

        private void ShowItem(int index)
        {
            if (ActiveMenu is ContextMenu && !ActiveMenu.Disposed)
            {
                ActiveMenu.Dispose();
            }
            ActiveMenu = new ContextMenu(Window);
            ActiveMenu.SetInnerColor(10, 23, 37);
            ActiveMenu.SetOuterColor(79, 108, 159);
            ActiveMenu.SetPosition(WidthUntil(index), Size.Height + 1);
            ActiveMenu.SetItems(SelectedItem.Items);
        }

        private int WidthUntil(int index)
        {
            int w = 12;
            Font f = Sprites["header"].Bitmap.Font;
            for (int i = 0; i < index; i++)
            {
                if (i == index) break;
                int size = f.TextSize(Items[i].Text).Width;
                w += size + 12;
            }
            return w;
        }
    }
}
