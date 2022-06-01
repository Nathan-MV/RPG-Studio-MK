﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class MapTilesetsChangeUndoAction : BaseUndoAction
{
    public int MapID;
    public List<int> OldTilesetIDs;
    public List<int> NewTilesetIDs;

    public MapTilesetsChangeUndoAction(int MapID, List<int> OldTilesetIDs, List<int> NewTilesetIDs)
    {
        this.MapID = MapID;
        this.OldTilesetIDs = OldTilesetIDs;
        this.NewTilesetIDs = NewTilesetIDs;
    }

    public static void Create(int MapID, List<int> OldTilesetIDs, List<int> NewTilesetIDs)
    {
        var c = new MapTilesetsChangeUndoAction(MapID, OldTilesetIDs, NewTilesetIDs);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        // Ensure we're in the Mapping mode
        bool Continue = true;
        if (!InMode(EditorMode.Mapping))
        {
            SetMode(EditorMode.Mapping);
            Continue = false;
        }
        // Ensure we're on the map this action was taken on
        if (Editor.MainWindow.MapWidget.Map.ID != MapID)
        {
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps[this.MapID]);
            Continue = false;
        }
        if (!Continue) return false;
        if (IsRedo)
        {
            Data.Maps[this.MapID].TilesetIDs = NewTilesetIDs;
        }
        else
        {
            Data.Maps[this.MapID].TilesetIDs = OldTilesetIDs;
        }
        // Update autotiles
        for (int i = 0; i < 7; i++)
        {
            Data.Maps[this.MapID].AutotileIDs[i] = Data.Maps[this.MapID].TilesetIDs[0] * 7 + i;
        }
        Editor.MainWindow.MapWidget.SetMap(Data.Maps[this.MapID]);
        return true;
    }
}
