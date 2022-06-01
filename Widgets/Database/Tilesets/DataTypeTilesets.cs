﻿using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class DataTypeTilesets : Widget
{
    public DataTypeSubList TilesetList;
    public SubmodeView Tabs;
    public TilesetDisplayContainer TilesetContainer;
    public Tileset Tileset { get { return TilesetList.SelectedIndex >= 0 ? (Tileset) TilesetList.SelectedItem.Object : null; } }
    public TextBox NameBox;
    public PickerBox GraphicBox;
    public List<PickerBox> AutotileBoxes = new List<PickerBox>();
    public PickerBox FogBox;
    public PickerBox PanoramaBox;

    Grid Grid;
    Container MainBox;
    VignetteFade Fade;

    Label NameLabel;
    Label GraphicLabel;
    Label FogLabel;
    Label PanoramaLabel;
    List<Label> AutotileLabels = new List<Label>();
    Container ScrollContainer;
    Container AdjustedScrollContainer;
    MultilineLabel TagDetailLabel;

    bool ChangingSelection = false;

    public int SelectedIndex { get { return TilesetList.SelectedIndex; } }
    public Tileset SelectedItem { get { return (Tileset) TilesetList.SelectedItem?.Object; } }

    public DataTypeTilesets(IContainer Parent) : base(Parent)
    {
        Grid = new Grid(this);
        Grid.SetColumns(
            new GridSize(181, Unit.Pixels),
            new GridSize(1),
            new GridSize(0, Unit.Pixels)
        );
        Grid.SetRows(
            new GridSize(29, Unit.Pixels),
            new GridSize(1)
        );
        
        TilesetList = new DataTypeSubList("Tilesets", Editor.ProjectSettings.TilesetCapacity, Grid);
        TilesetList.SetBackgroundColor(28, 50, 73);
        TilesetList.SetGridRow(0, 1);
        TilesetList.OnMaximumChanged += delegate (ObjectEventArgs e)
        {
            int NewCapacity = (int)e.Object;
            if (NewCapacity == Editor.ProjectSettings.TilesetCapacity) return;
            List<Tileset> OldTilesets = Data.Tilesets.ConvertAll(t => (Tileset) t?.Clone());
            int OldCapacity = Editor.ProjectSettings.TilesetCapacity;
            if (NewCapacity > Editor.ProjectSettings.TilesetCapacity)
            {
                // Add blank tilesets
                for (int i = Editor.ProjectSettings.TilesetCapacity + 1; i <= NewCapacity; i++)
                {
                    Tileset t = new Tileset();
                    Data.Tilesets.Add(t);
                }
            }
            else if (NewCapacity < Editor.ProjectSettings.TilesetCapacity)
            {
                // Remove tilesets
                for (int i = NewCapacity; i > Editor.ProjectSettings.TilesetCapacity; i--)
                {
                    Data.Tilesets.RemoveAt(i);
                }
            }
            Editor.ProjectSettings.TilesetCapacity = NewCapacity;
            Undo.TilesetCapacityChangeUndoAction.Create(OldTilesets, Data.Tilesets, OldCapacity, NewCapacity);
            RedrawList();
        };

        RedrawList();

        Tabs = new SubmodeView(Grid);
        Tabs.SetBackgroundColor(23, 40, 56);
        Tabs.SetTextY(4);
        Tabs.SetHeaderHeight(29);
        Tabs.SetHeaderSelHeight(0);
        Tabs.SetHeaderWidth(140);
        Tabs.SetGrid(0, 1, 1, 2);
        Tabs.SetFont(Fonts.UbuntuBold.Use(13));
        Tabs.SetCentered(true);
        Tabs.SetHeaderBackgroundColor(10, 23, 37);
        Tabs.OnSelectionChanged += _ => SelectedTabChanged();

        MainBox = new Container(Grid);
        MainBox.SetGrid(1, 1);

        Fade = new VignetteFade(Grid);
        Fade.SetGrid(1, 1);

        Tabs.CreateTab("Passage");
        Tabs.CreateTab("4-Dir");
        Tabs.CreateTab("Priority");
        Tabs.CreateTab("Bush Flag");
        Tabs.CreateTab("Counter Flag");
        Tabs.CreateTab("Terrain Tag");

        Font Font = Fonts.ProductSansMedium.Use(13);

        NameLabel = new Label(MainBox);
        NameLabel.SetText("Name");
        NameLabel.SetFont(Font);
        NameBox = new TextBox(MainBox);
        NameBox.SetSize(180, 25);
        NameBox.SetFont(Font);
        NameBox.SetTextY(-2);
        NameBox.OnTextChanged += delegate (TextEventArgs e)
        {
            if (Editor.Undoing || Editor.Redoing || ChangingSelection) return;
            Tileset.Name = e.Text;
            RedrawList();
        };
        string OldText = null;
        NameBox.TextArea.OnWidgetSelected += _ =>
        {
            OldText = NameBox.Text;
        };
        NameBox.TextArea.OnWidgetDeselected += _ =>
        {
            Undo.TilesetNameChangeUndoAction.Create(Tileset.ID, OldText, NameBox.Text);
        };

        GraphicLabel = new Label(MainBox);
        GraphicLabel.SetText("Tileset Graphic");
        GraphicLabel.SetFont(Font);
        GraphicBox = new PickerBox(MainBox);
        GraphicBox.SetSize(180, 25);
        GraphicBox.SetFont(Font);
        GraphicBox.SetTextY(-2);
        GraphicBox.OnDropDownClicked += _ =>
        {
            TilesetFilePicker picker = new TilesetFilePicker(Tileset.GraphicName);
            picker.OnClosed += _ =>
            {
                if (picker.PressedOK)
                {
                    string OldGraphicName = Tileset.GraphicName;
                    List<Passability> OldPassabilities = new List<Passability>(Tileset.Passabilities);
                    List<int> OldPriorities = new List<int>(Tileset.Priorities);
                    List<int> OldTags = new List<int>(Tileset.Tags);
                    List<bool> OldBushFlags = new List<bool>(Tileset.BushFlags);
                    List<bool> OldCounterFlags = new List<bool>(Tileset.CounterFlags);
                    Tileset.GraphicName = picker.ChosenFilename;
                    GraphicBox.SetText(Tileset.GraphicName);
                    Tileset.CreateBitmap(true);
                    Tileset.UpdateDataLists();
                    SetTileset(Tileset, true);
                    Undo.TilesetGraphicChangeUndoAction.Create(Tileset.ID, OldGraphicName, Tileset.GraphicName,
                        OldPassabilities, Tileset.Passabilities, OldPriorities, Tileset.Priorities,
                        OldTags, Tileset.Tags, OldCounterFlags, Tileset.CounterFlags,
                        OldBushFlags, Tileset.BushFlags);
                }
            };
        };

        for (int i = 0; i < 7; i++)
        {
            Label AutotileLabel = new Label(MainBox);
            AutotileLabel.SetText($"Autotile {i + 1}");
            AutotileLabel.SetFont(Font);
            AutotileLabels.Add(AutotileLabel);
            PickerBox AutotileBox = new PickerBox(MainBox);
            AutotileBox.SetSize(162, 25);
            AutotileBox.SetFont(Font);
            AutotileBox.SetTextY(-2);
            int id = i;
            AutotileBox.OnDropDownClicked += _ =>
            {
                AutotileFilePicker picker = new AutotileFilePicker(Tileset.Autotiles[id]?.GraphicName);
                picker.OnClosed += _ =>
                {
                    if (picker.PressedOK)
                    {
                        string OldGraphicName = Tileset.Autotiles[id]?.GraphicName;
                        int autotileid = Tileset.ID * 7 + id;
                        if (Data.Autotiles[autotileid] == null)
                        {
                            Autotile a = new Autotile();
                            a.ID = autotileid;
                            a.Name = picker.ChosenFilename ?? "";
                            a.Passability = Tileset.Passabilities[(id + 1) * 48];
                            a.Priority = Tileset.Priorities[(id + 1) * 48];
                            a.Tag = Tileset.Tags[(id + 1) * 48];
                            Data.Autotiles[autotileid] = a;
                            Tileset.Autotiles[id] = Data.Autotiles[autotileid];
                        }
                        Autotile autotile = Data.Autotiles[autotileid];
                        autotile.SetGraphic(picker.ChosenFilename ?? "");
                        AutotileBox.SetText(autotile.GraphicName);
                        TilesetContainer.SetTileset(Tileset, true);
                        Undo.TilesetAutotileChangeUndoAction.Create(Tileset.ID, id, OldGraphicName, autotile.GraphicName);
                    }
                };
            };
            AutotileBoxes.Add(AutotileBox);
        }

        FogLabel = new Label(MainBox);
        FogLabel.SetText("Fog Graphic");
        FogLabel.SetFont(Font);
        FogBox = new PickerBox(MainBox);
        FogBox.SetSize(180, 25);
        FogBox.SetFont(Font);
        FogBox.SetTextY(-2);
        FogBox.OnDropDownClicked += _ =>
        {
            FogFilePicker picker = new FogFilePicker(Tileset.FogName, Tileset.FogHue,
                Tileset.FogOpacity, Tileset.FogBlendType, Tileset.FogZoom, Tileset.FogSX, Tileset.FogSY);
            picker.OnClosed += _ =>
            {
                if (picker.PressedOK)
                {
                    string OldFogName = Tileset.FogName;
                    int OldFogHue = Tileset.FogHue;
                    byte OldFogOpacity = Tileset.FogOpacity;
                    int OldFogBlendType = Tileset.FogBlendType;
                    int OldFogZoom = Tileset.FogZoom;
                    int OldFogSX = Tileset.FogSX;
                    int OldFogSY = Tileset.FogSY;
                    Tileset.FogName = picker.ChosenFogName;
                    Tileset.FogHue = picker.ChosenFogHue;
                    Tileset.FogOpacity = picker.ChosenFogOpacity;
                    Tileset.FogBlendType = picker.ChosenFogBlendType;
                    Tileset.FogZoom = picker.ChosenFogZoom;
                    Tileset.FogSX = picker.ChosenFogSX;
                    Tileset.FogSY = picker.ChosenFogSY;
                    FogBox.SetText(Tileset.FogName);
                    Undo.TilesetFogChangeUndoAction.Create(Tileset.ID, OldFogName, Tileset.FogName, OldFogHue, Tileset.FogHue,
                                                      OldFogOpacity, Tileset.FogOpacity, OldFogBlendType, Tileset.FogBlendType,
                                                      OldFogZoom, Tileset.FogZoom, OldFogSX, Tileset.FogSX, OldFogSY, Tileset.FogSY);
                }
            };
        };

        PanoramaLabel = new Label(MainBox);
        PanoramaLabel.SetText("Panorama Graphic");
        PanoramaLabel.SetFont(Font);
        PanoramaBox = new PickerBox(MainBox);
        PanoramaBox.SetSize(180, 25);
        PanoramaBox.SetFont(Font);
        PanoramaBox.SetTextY(-2);
        PanoramaBox.OnDropDownClicked = _ =>
        {
            PanoramaFilePicker picker = new PanoramaFilePicker(Tileset.PanoramaName, Tileset.PanoramaHue);
            picker.OnClosed = _ =>
            {
                if (picker.PressedOK)
                {
                    string OldPanoramaName = Tileset.PanoramaName;
                    int OldPanoramaHue = Tileset.PanoramaHue;
                    Tileset.PanoramaName = picker.ChosenPanoramaName;
                    Tileset.PanoramaHue = picker.ChosenPanoramaHue;
                    PanoramaBox.SetText(Tileset.PanoramaName);
                    Undo.TilesetPanoramaChangeUndoAction.Create(Tileset.ID, OldPanoramaName, Tileset.PanoramaName, OldPanoramaHue, Tileset.PanoramaHue);
                }
            };
        };

        TagDetailLabel = new MultilineLabel(MainBox);
        TagDetailLabel.SetFont(Font);
        TagDetailLabel.SetText(
@"0 = None
1 = Ledge
2 = Tall grass
3 = Sand
4 = Rocky
5 = Deep water
6 = Still water
7 = Water
8 = Waterfall
9 = Waterfall crest
10 = Very tall grass
11 = Underwater grass
12 = Ice
13 = Neutral
14 = Sooty grass
15 = Bridge
16 = Puddle
17 = No effect"
        );
        TagDetailLabel.SetPosition(680, 18);
        TagDetailLabel.SetWidth(400);
        TagDetailLabel.SetVisible(false);

        ScrollContainer = new Container(MainBox);
        ScrollContainer.SetPosition(386, 18);
        ScrollContainer.SetSize(263, 600);
        VScrollBar vs1 = new VScrollBar(MainBox);
        ScrollContainer.SetVScrollBar(vs1);
        ScrollContainer.VAutoScroll = true;
        vs1.SetPosition(ScrollContainer.Position.X + 265, 19);
        TilesetContainer = new TilesetDisplayContainer(ScrollContainer);
        TilesetContainer.SetWidth(263);

        MainBox.Sprites["box"] = new Sprite(MainBox.Viewport);
        MainBox.Sprites["box"].X = ScrollContainer.Position.X - 1;
        MainBox.Sprites["box"].Y = ScrollContainer.Position.Y - 1;

        AdjustedScrollContainer = new Container(Grid);
        AdjustedScrollContainer.SetGrid(1, 2);
        VScrollBar vs2 = new VScrollBar(AdjustedScrollContainer);
        vs2.SetPosition(1, 1);
        MainBox.SetVScrollBar(vs2);

        TilesetList.OnSelectionChanged += UpdateSelection;
        Tabs.SelectTab(0);

        TilesetList.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("Copy")
            {
                OnClicked = CopyTileset
            },
            new MenuItem("Paste")
            {
                OnClicked = PasteTileset,
                IsClickable = e => e.Value = Utilities.IsClipboardValidBinary(BinaryData.TILESET)
            },
            new MenuSeparator(),
            new MenuItem("Delete")
            {
                OnClicked = DeleteTileset
            }
        });
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Fade.SetSize(MainBox.Size);
        TagDetailLabel.SetVisible(TilesetContainer.Mode == TilesetDisplayMode.TerrainTag && Window.Width >= 1210);
        bool AutoscrollingMainBox = MainBox.VAutoScroll;
        if (Window.Width < 1036)
        {
            if (Window.Width < 720)
            {
                NameLabel.SetPosition(102 - NameLabel.Size.Width, 20);
                NameBox.SetPosition(110, 18);
                GraphicLabel.SetText("Graphic");
                GraphicLabel.SetPosition(102 - GraphicLabel.Size.Width, 53);
                GraphicBox.SetPosition(110, 51);
                for (int i = 0; i < 7; i++)
                {
                    AutotileLabels[i].SetPosition(119 - AutotileLabels[i].Size.Width, 122 + 33 * i);
                    AutotileBoxes[i].SetPosition(127, 120 + 33 * i);
                }
                FogLabel.SetPosition(102 - FogLabel.Size.Width, 426);
                FogBox.SetPosition(110, 424);
                PanoramaLabel.SetText("Panorama");
                PanoramaLabel.SetPosition(102 - PanoramaLabel.Size.Width, 459);
                PanoramaBox.SetPosition(110, 457);
            }
            else
            {
                NameLabel.SetPosition(148 - NameLabel.Size.Width, 20);
                NameBox.SetPosition(156, 18);
                GraphicLabel.SetText("Tileset Graphic");
                GraphicLabel.SetPosition(148 - GraphicLabel.Size.Width, 53);
                GraphicBox.SetPosition(156, 51);
                for (int i = 0; i < 7; i++)
                {
                    AutotileLabels[i].SetPosition(165 - AutotileLabels[i].Size.Width, 122 + 33 * i);
                    AutotileBoxes[i].SetPosition(173, 120 + 33 * i);
                }
                FogLabel.SetPosition(148 - FogLabel.Size.Width, 426);
                FogBox.SetPosition(156, 424);
                PanoramaLabel.SetText("Panorama Name");
                PanoramaLabel.SetPosition(148 - PanoramaLabel.Size.Width, 459);
                PanoramaBox.SetPosition(156, 457);
            }
            ScrollContainer.SetPosition(24, 494);
            ScrollContainer.SetSize(TilesetContainer.Size);
            ScrollContainer.VAutoScroll = false;
            MainBox.VAutoScroll = true;
            MainBox.VScrollBar.SetHeight(AdjustedScrollContainer.Size.Height - 2);
            if (Grid.Columns[2].Value == 0)
            {
                Grid.Columns[2] = new GridSize(10, Unit.Pixels);
                Grid.UpdateContainers();
                Grid.UpdateLayout();
            }
            MainBox.Sprites["box"].Visible = false;
        }
        else
        {
            NameLabel.SetPosition(148 - NameLabel.Size.Width, 20);
            NameBox.SetPosition(156, 18);
            GraphicLabel.SetText("Tileset Graphic");
            GraphicLabel.SetPosition(148 - GraphicLabel.Size.Width, 53);
            GraphicBox.SetPosition(156, 51);
            for (int i = 0; i < 7; i++)
            {
                AutotileLabels[i].SetPosition(165 - AutotileLabels[i].Size.Width, 122 + 33 * i);
                AutotileBoxes[i].SetPosition(173, 120 + 33 * i);
            }
            FogLabel.SetPosition(148 - FogLabel.Size.Width, 426);
            FogBox.SetPosition(156, 424);
            PanoramaLabel.SetText("Panorama Name");
            PanoramaLabel.SetPosition(148 - PanoramaLabel.Size.Width, 459);
            PanoramaBox.SetPosition(156, 457);

            ScrollContainer.SetPosition(386, 18);
            ScrollContainer.SetHeight(MainBox.Size.Height - ScrollContainer.Position.Y - 8);
            if (TilesetContainer.Size.Height < ScrollContainer.Size.Height) ScrollContainer.SetHeight(TilesetContainer.Size.Height);
            ScrollContainer.VScrollBar.SetHeight(ScrollContainer.Size.Height - 2);
            ScrollContainer.VAutoScroll = true;
            MainBox.VAutoScroll = false;
            if (Grid.Columns[2].Value == 10)
            {
                Grid.Columns[2] = new GridSize(0, Unit.Pixels);
                Grid.UpdateContainers();
                Grid.UpdateLayout();
            }
            MainBox.Sprites["box"].Bitmap?.Dispose();
            MainBox.Sprites["box"].Bitmap = new Bitmap(276, ScrollContainer.Size.Height + 2, Graphics.MaxTextureSize);
            MainBox.Sprites["box"].Bitmap.Unlock();
            MainBox.Sprites["box"].Bitmap.DrawRect(276, ScrollContainer.Size.Height + 2, new Color(121, 121, 122));
            MainBox.Sprites["box"].Bitmap.DrawLine(264, 1, 264, ScrollContainer.Size.Height, new Color(121, 121, 122));
            MainBox.Sprites["box"].Bitmap.Lock();
            MainBox.Sprites["box"].X = ScrollContainer.Position.X - 1;
            MainBox.Sprites["box"].Y = ScrollContainer.Position.Y - 1;
            MainBox.Sprites["box"].Visible = true;
        }
        if (AutoscrollingMainBox != MainBox.VAutoScroll)
        {
            MainBox.VScrollBar.SetValue(0);
            ScrollContainer.VScrollBar.SetValue(0);
        }
    }

    public void RedrawList()
    {
        List<ListItem> TilesetItems = new List<ListItem>();
        for (int i = 1; i <= Editor.ProjectSettings.TilesetCapacity; i++)
        {
            string name = $"{Utilities.Digits(i, 3)}: {Data.Tilesets[i]?.Name}";
            ListItem item = new ListItem(name, Data.Tilesets[i]);
            TilesetItems.Add(item);
        }
        TilesetList.SetItems(TilesetItems);
        if (TilesetList.SelectedIndex == -1) TilesetList.SetSelectedIndex(0);
    }

    public void SetSelectedIndex(int SelectedIndex)
    {
        if (SelectedIndex >= TilesetList.Items.Count) SelectedIndex = TilesetList.Items.Count - 1;
        TilesetList.SetSelectedIndex(SelectedIndex);
    }

    public void SetTileset(Tileset Tileset, bool ForceUpdate = false)
    {
        ListItem Item = TilesetList.Items.Find(i => i.Object == Tileset);
        if (Item == null) throw new Exception("Could not find Tileset list item.");
        TilesetList.SetSelectedIndex(TilesetList.Items.IndexOf(Item));
        TilesetContainer.SetTileset(Tileset, ForceUpdate);
        ScrollContainer.SetHeight(MainBox.Size.Height - ScrollContainer.Position.Y - 8);
        if (TilesetContainer.Size.Height < ScrollContainer.Size.Height) ScrollContainer.SetHeight(TilesetContainer.Size.Height);
        UpdateSelection(new BaseEventArgs());
    }

    public void UpdateSelection(BaseEventArgs e)
    {
        Tileset t = this.Tileset;
        ChangingSelection = true;
        NameBox.SetText(t.Name);
        GraphicBox.SetText(t.GraphicName);
        for (int i = 0; i < 7; i++)
        {
            AutotileBoxes[i].SetText(t.Autotiles[i] == null ? "" : t.Autotiles[i].Name);
        }
        FogBox.SetText(t.FogName);
        PanoramaBox.SetText(t.PanoramaName);
        TilesetContainer.SetTileset(t);
        ScrollContainer.SetHeight(MainBox.Size.Height - ScrollContainer.Position.Y - 8);
        if (TilesetContainer.Size.Height < ScrollContainer.Size.Height) ScrollContainer.SetHeight(TilesetContainer.Size.Height);
        ChangingSelection = false;
    }

    void SelectedTabChanged()
    {
        if (Tabs.SelectedIndex == 0) TilesetContainer.SetMode(TilesetDisplayMode.Passage);
        else if (Tabs.SelectedIndex == 1) TilesetContainer.SetMode(TilesetDisplayMode.Directions);
        else if (Tabs.SelectedIndex == 2) TilesetContainer.SetMode(TilesetDisplayMode.Priority);
        else if (Tabs.SelectedIndex == 3) TilesetContainer.SetMode(TilesetDisplayMode.BushFlag);
        else if (Tabs.SelectedIndex == 4) TilesetContainer.SetMode(TilesetDisplayMode.CounterFlag);
        else if (Tabs.SelectedIndex == 5) TilesetContainer.SetMode(TilesetDisplayMode.TerrainTag);
        TagDetailLabel.SetVisible(TilesetContainer.Mode == TilesetDisplayMode.TerrainTag && Window.Width >= 1210);
    }

    void CopyTileset(BaseEventArgs e)
    {
        Tileset Tileset = (Tileset) TilesetList.HoveringItem.Object;
        Utilities.SetClipboard(Tileset, BinaryData.TILESET);
    }

    void PasteTileset(BaseEventArgs e)
    {
        if (!Utilities.IsClipboardValidBinary(BinaryData.TILESET)) return;
        Tileset OldTileset = (Tileset) TilesetList.HoveringItem.Object;
        Tileset NewTileset = Utilities.GetClipboard<Tileset>();
        Data.Tilesets[OldTileset.ID] = NewTileset;
        NewTileset.ID = OldTileset.ID;
        OldTileset.TilesetBitmap?.Dispose();
        OldTileset.TilesetBitmap = null;
        OldTileset.TilesetListBitmap?.Dispose();
        OldTileset.TilesetListBitmap = null;
        for (int i = 0; i < 7; i++)
        {
            Data.Autotiles[NewTileset.ID * 7 + i] = NewTileset.Autotiles[i];
        }
        RedrawList();
        SetTileset(NewTileset, true);
        Undo.TilesetChangeUndoAction.Create(OldTileset, NewTileset);
    }

    void DeleteTileset(BaseEventArgs e)
    {
        Tileset OldTileset = (Tileset) TilesetList.HoveringItem.Object;
        Tileset NewTileset = (Tileset) OldTileset.Clone();
        NewTileset.MakeEmpty();
        // Since making the tileset empty will dispose the bitmap,
        // we should also set the other references to null
        // or they will point to a disposed bitmap.
        OldTileset.TilesetBitmap = null;
        OldTileset.TilesetListBitmap = null;
        Data.Tilesets[NewTileset.ID] = NewTileset;
        RedrawList();
        SetTileset(NewTileset, true);
        Undo.TilesetChangeUndoAction.Create(OldTileset, NewTileset);
    }
}
