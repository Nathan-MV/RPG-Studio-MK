﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using odl;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class Tileset : ICloneable
    {
        public int ID;
        public string Name;
        public string GraphicName;
        public List<Passability> Passabilities = new List<Passability>();
        public List<int> Priorities = new List<int>();
        public List<int> Tags = new List<int>();

        // RMXP properties
        public int PanoramaHue;
        public string PanoramaName;
        public string FogName;
        public int FogSX;
        public int FogSY;
        public int FogOpacity;
        public int FogHue;
        public int FogZoom;
        public int FogBlendType;
        public string BattlebackName;
        public List<Autotile> Autotiles = new List<Autotile>();
        /// <summary>
        /// List of tile IDs that have the bush flag set.
        /// </summary>
        public List<int> BushFlags = new List<int>();

        public Bitmap TilesetBitmap;
        public Bitmap TilesetListBitmap;

        public Tileset()
        {

        }

        public Tileset(IntPtr data)
        {
            this.ID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@id"));
            this.Name = Ruby.String.FromPtr(Ruby.GetIVar(data, "@name"));
            this.GraphicName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@tileset_name"));
            IntPtr passability = Ruby.GetIVar(data, "@passages");
            for (int i = 0; i < Compatibility.RMXP.Table.Size(passability); i++)
            {
                int code = (int) Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(passability, i));
                if ((code & 64) != 0)
                {
                    BushFlags.Add(i);
                    code -= 64;
                }
                Passabilities.Add((Passability) 15 - code);
            }
            IntPtr priorities = Ruby.GetIVar(data, "@priorities");
            for (int i = 0; i < Compatibility.RMXP.Table.Size(priorities); i++)
            {
                int code = (int) Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(priorities, i));
                this.Priorities.Add(code);
            }
            IntPtr tags = Ruby.GetIVar(data, "@terrain_tags");
            for (int i = 0; i < Compatibility.RMXP.Table.Size(tags); i++)
            {
                int code = (int) Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(tags, i));
                this.Tags.Add(code);
            }
            this.PanoramaHue = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@panorama_hue"));
            this.PanoramaName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@panorama_name"));
            this.FogName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@fog_name"));
            this.FogSX = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_sx"));
            this.FogSY = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_sy"));
            this.FogOpacity = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_opacity"));
            this.FogHue = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_hue"));
            this.FogZoom = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_zoom"));
            this.FogBlendType = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_blend_type"));
            this.BattlebackName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@battleback_name"));
            IntPtr autotiles = Ruby.GetIVar(data, "@autotile_names");
            for (int i = 0; i < Ruby.Array.Length(autotiles); i++)
            {
                string name = Ruby.String.FromPtr(Ruby.Array.Get(autotiles, i));
                if (string.IsNullOrEmpty(name)) this.Autotiles.Add(null);
                else
                {
                    Autotile autotile = new Autotile();
                    autotile.ID = this.ID * 7 + i;
                    autotile.Name = name;
                    autotile.Passability = this.Passabilities[(i + 1) * 48];
                    autotile.Priority = (int) this.Priorities[(i + 1) * 48];
                    autotile.Tag = (int) this.Tags[(i + 1) * 48];
                    autotile.SetGraphic(name);
                    if (autotile.AutotileBitmap.Height == 32) autotile.Format = AutotileFormat.Single;
                    else if (autotile.AutotileBitmap.Height == 96) autotile.Format = AutotileFormat.RMVX;
                    else autotile.Format = AutotileFormat.RMXP;
                    this.Autotiles.Add(autotile);
                    Data.Autotiles[autotile.ID] = autotile;
                }
            }

            // Make sure the three arrays are just as big; trailing nulls may be left out if the data is edited externally
            int maxcount = Math.Max(Math.Max(Passabilities.Count, Priorities.Count), Tags.Count);
            this.Passabilities.AddRange(new Passability[maxcount - Passabilities.Count]);
            this.Priorities.AddRange(new int[maxcount - Priorities.Count]);
            this.Tags.AddRange(new int[maxcount - Tags.Count]);
            if (!string.IsNullOrEmpty(this.GraphicName)) this.CreateBitmap();
        }

        public IntPtr Save()
        {
            IntPtr obj = Ruby.Funcall(Compatibility.RMXP.Tileset.Class, "new");
            Ruby.Pin(obj);
            Ruby.SetIVar(obj, "@id", Ruby.Integer.ToPtr(this.ID));
            Ruby.SetIVar(obj, "@name", Ruby.String.ToPtr(this.Name));
            Ruby.SetIVar(obj, "@tileset_name", Ruby.String.ToPtr(this.GraphicName));
            Ruby.SetIVar(obj, "@panorama_name", Ruby.String.ToPtr(this.PanoramaName));
            Ruby.SetIVar(obj, "@panorama_hue", Ruby.Integer.ToPtr(this.PanoramaHue));
            Ruby.SetIVar(obj, "@fog_name", Ruby.String.ToPtr(this.FogName));
            Ruby.SetIVar(obj, "@fog_sx", Ruby.Integer.ToPtr(this.FogSX));
            Ruby.SetIVar(obj, "@fog_sy", Ruby.Integer.ToPtr(this.FogSY));
            Ruby.SetIVar(obj, "@fog_hue", Ruby.Integer.ToPtr(this.FogHue));
            Ruby.SetIVar(obj, "@fog_opacity", Ruby.Integer.ToPtr(this.FogOpacity));
            Ruby.SetIVar(obj, "@fog_zoom", Ruby.Integer.ToPtr(this.FogZoom));
            Ruby.SetIVar(obj, "@fog_blend_type", Ruby.Integer.ToPtr(this.FogBlendType));
            Ruby.SetIVar(obj, "@battleback_name", Ruby.String.ToPtr(this.BattlebackName));
            IntPtr autotile_names = Ruby.Array.Create(7, Ruby.String.ToPtr(""));
            Ruby.SetIVar(obj, "@autotile_names", autotile_names);
            foreach (Autotile autotile in this.Autotiles)
            {
                if (autotile == null) continue;
                int idx = autotile.ID - this.ID * 7;
                Ruby.Funcall(autotile_names, "[]=", Ruby.Integer.ToPtr(idx), Ruby.String.ToPtr(autotile.GraphicName));
            }
            IntPtr passages = Ruby.Funcall(Compatibility.RMXP.Table.Class, "new", Ruby.Integer.ToPtr(this.Passabilities.Count));
            Ruby.SetIVar(obj, "@passages", passages);
            for (int i = 0; i < this.Passabilities.Count; i++)
            {
                int code = 15 - (int) this.Passabilities[i];
                if (BushFlags.Contains(i)) code += 64;
                Compatibility.RMXP.Table.Set(passages, i, Ruby.Integer.ToPtr(code));
            }
            IntPtr priorities = Ruby.Funcall(Compatibility.RMXP.Table.Class, "new", Ruby.Integer.ToPtr(this.Priorities.Count));
            Ruby.SetIVar(obj, "@priorities", priorities);
            for (int i = 0; i < this.Priorities.Count; i++)
            {
                Compatibility.RMXP.Table.Set(priorities, i, Ruby.Integer.ToPtr(this.Priorities[i]));
            }
            IntPtr tags = Ruby.Funcall(Compatibility.RMXP.Table.Class, "new", Ruby.Integer.ToPtr(this.Tags.Count));
            Ruby.SetIVar(obj, "@terrain_tags", tags);
            for (int i = 0; i < this.Tags.Count; i++)
            {
                Compatibility.RMXP.Table.Set(tags, i, Ruby.Integer.ToPtr(this.Tags[i]));
            }
            Ruby.Unpin(obj);
            return obj;
        }

        public void SetGraphic(string GraphicName)
        {
            if (this.GraphicName != GraphicName)
            {
                this.GraphicName = GraphicName;
                this.CreateBitmap(true);
                int tileycount = (int) Math.Floor(TilesetBitmap.Height / 32d);
                int size = tileycount * 8;
                this.Passabilities.AddRange(new Passability[size - Passabilities.Count]);
                this.Priorities.AddRange(new int[size - Priorities.Count]);
                this.Tags.AddRange(new int[size - Tags.Count]);
            }
        }

        public void CreateBitmap(bool Redraw = false)
        {
            if (this.TilesetBitmap == null || Redraw)
            {
                this.TilesetBitmap?.Dispose();
                this.TilesetListBitmap?.Dispose();
                
                Bitmap bmp = new Bitmap($"{Data.ProjectPath}/Graphics/Tilesets/{this.GraphicName}.png");
                this.TilesetBitmap = bmp;
                int tileycount = (int) Math.Floor(bmp.Height / 32d);

                this.TilesetListBitmap = new Bitmap(bmp.Width + 7, bmp.Height + tileycount - 1, Graphics.MaxTextureSize);
                this.TilesetListBitmap.Unlock();

                for (int tiley = 0; tiley < tileycount; tiley++)
                {
                    for (int tilex = 0; tilex < 8; tilex++)
                    {
                        this.TilesetListBitmap.Build(tilex * 32 + tilex, tiley * 32 + tiley, bmp, tilex * 32, tiley * 32, 32, 32);
                    }
                }
                this.TilesetListBitmap.Lock();
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public object Clone()
        {
            Tileset t = new Tileset();
            t.ID = this.ID;
            t.Name = this.Name;
            t.GraphicName = this.GraphicName;
            t.Passabilities = new List<Passability>(this.Passabilities);
            t.Priorities = new List<int>(this.Priorities);
            t.Tags = new List<int>(this.Tags);
            t.PanoramaHue = this.PanoramaHue;
            t.PanoramaName = this.PanoramaName;
            t.FogName = this.FogName;
            t.FogSX = this.FogSX;
            t.FogSY = this.FogSY;
            t.FogOpacity = this.FogOpacity;
            t.FogHue = this.FogHue;
            t.FogZoom = this.FogZoom;
            t.FogBlendType = this.FogBlendType;
            t.BattlebackName = this.BattlebackName;
            t.Autotiles = new List<Autotile>();
            this.Autotiles.ForEach(a => t.Autotiles.Add((Autotile) a.Clone()));
            t.BushFlags = new List<int>(this.BushFlags);
            t.TilesetBitmap = this.TilesetBitmap;
            t.TilesetListBitmap = this.TilesetListBitmap;
            return t;
        }
    }
}
