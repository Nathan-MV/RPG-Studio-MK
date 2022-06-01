﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets;

namespace RPGStudioMK.Undo;

public class MapAudioFileChangeUndoAction : BaseUndoAction
{
    public int MapID;
    public AudioFile OldAudioFile;
    public bool OldAutoplay;
    public AudioFile NewAudioFile;
    public bool NewAutoplay;
    public bool IsBGM;

    public MapAudioFileChangeUndoAction(int MapID, AudioFile OldAudioFile, bool OldAutoplay, AudioFile NewAudioFile, bool NewAutoplay, bool IsBGM)
    {
        this.MapID = MapID;
        this.OldAudioFile = OldAudioFile;
        this.OldAutoplay = OldAutoplay;
        this.NewAudioFile = NewAudioFile;
        this.NewAutoplay = NewAutoplay;
        this.IsBGM = IsBGM;
    }

    public static void Create(int MapID, AudioFile OldAudioFile, bool OldAutoplay, AudioFile NewAudioFile, bool NewAutoplay, bool IsBGM)
    {
        var c = new MapAudioFileChangeUndoAction(MapID, OldAudioFile, OldAutoplay, NewAudioFile, NewAutoplay, IsBGM);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        // Ensure we're in the Mapping mode
        bool Continue = true;
        if (!InMode(EditorMode.Mapping))
        {
            SetMappingMode(MapMode.Tiles);
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
            if (IsBGM)
            {
                Data.Maps[this.MapID].BGM = NewAudioFile;
                Data.Maps[this.MapID].AutoplayBGM = NewAutoplay;
            }
            else
            {
                Data.Maps[this.MapID].BGS = NewAudioFile;
                Data.Maps[this.MapID].AutoplayBGS = NewAutoplay;
            }
        }
        else
        {
            if (IsBGM)
            {
                Data.Maps[this.MapID].BGM = OldAudioFile;
                Data.Maps[this.MapID].AutoplayBGM = OldAutoplay;
            }
            else
            {
                Data.Maps[this.MapID].BGS = OldAudioFile;
                Data.Maps[this.MapID].AutoplayBGS = OldAutoplay;
            }
        }
        return true;
    }
}
