﻿using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class ShiftMapWindow : PopupWindow
{
    public bool Apply = false;

    public Direction Direction;
    public int Value;
    public bool ShiftEvents;

    DropdownBox DirectionBox;
    NumericBox NumberBox;
    CheckBox ShiftEventsBox;

    public ShiftMapWindow(Map Map)
    {
        SetTitle("Shift Map");
        MinimumSize = MaximumSize = new Size(320, 140);
        SetSize(MaximumSize);
        Center();

        DirectionBox = new DropdownBox(this);
        DirectionBox.SetPosition(100, 30);
        DirectionBox.SetSize(80, 27);
        DirectionBox.SetItems(new List<ListItem>()
        {
            new ListItem("Down"), new ListItem("Left"), new ListItem("Right"), new ListItem("Up")
        });
        DirectionBox.OnSelectionChanged += _ =>
        {
            if (DirectionBox.SelectedIndex == 0 || DirectionBox.SelectedIndex == 3) NumberBox.MaxValue = Map.Height - 1;
            else NumberBox.MaxValue = Map.Width - 1;
            if (NumberBox.Value > NumberBox.MaxValue) NumberBox.SetValue(1);
        };

        Label Text1Label = new Label(this);
        Text1Label.SetFont(Fonts.CabinMedium.Use(11));
        Text1Label.SetText("Direction:");
        Text1Label.SetPosition(DirectionBox.Position.X - Text1Label.Size.Width - 8, 34);

        NumberBox = new NumericBox(this);
        NumberBox.SetPosition(230, 30);
        NumberBox.SetSize(64, 27);
        NumberBox.MinValue = 1;
        NumberBox.MaxValue = Map.Height - 1;
        NumberBox.SetValue(1);

        Label Text2Label = new Label(this);
        Text2Label.SetFont(Fonts.CabinMedium.Use(11));
        Text2Label.SetText("Tiles:");
        Text2Label.SetPosition(NumberBox.Position.X - Text2Label.Size.Width - 8, 34);

        ShiftEventsBox = new CheckBox(this);
        ShiftEventsBox.SetPosition(156, 66);
        ShiftEventsBox.SetFont(Fonts.CabinMedium.Use(11));
        ShiftEventsBox.SetMirrored(true);
        ShiftEventsBox.SetText("Also shift events:");
        ShiftEventsBox.SetChecked(true);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });
    }

    private void OK()
    {
        Apply = true;
        this.Direction = (Direction) ((DirectionBox.SelectedIndex + 1) * 2);
        this.Value = NumberBox.Value;
        this.ShiftEvents = ShiftEventsBox.Checked;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
