﻿namespace RPGStudioMK.Widgets;

public class DatabaseWidget : Widget
{
    public DatabaseMode Mode { get; protected set; }

    Grid MainGrid;
    DataTypeList DataTypeList;

    public Widget ActiveDatabaseWidget;

    public DatabaseWidget(IContainer Parent) : base(Parent)
    {
        MainGrid = new Grid(this);
        MainGrid.SetColumns(
            new GridSize(183, Unit.Pixels),
            new GridSize(1)
        );

        DataTypeList = new DataTypeList(MainGrid);
    }

    public void SetMode(DatabaseMode Mode, bool Force = false)
    {
        if (this.Mode == Mode && !Force) return;
        this.Mode = Mode;
        ActiveDatabaseWidget?.Dispose();
        ActiveDatabaseWidget = null;
        Editor.ProjectSettings.LastDatabaseSubmode = Mode;
        DataTypeList.SetSelected(Mode);
        ActiveDatabaseWidget = Mode switch
        {
            DatabaseMode.Species => new DataTypeSpecies(MainGrid),
            DatabaseMode.Moves => new DataTypeMoves(MainGrid),
            DatabaseMode.Tilesets => new DataTypeTilesets(MainGrid),
            _ => null
        };
		ActiveDatabaseWidget?.SetGridColumn(1);
	}
}

