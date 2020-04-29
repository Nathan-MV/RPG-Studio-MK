﻿using System;
using System.Collections.Generic;
using MKEditor.Game;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class MainEditorWindow : Window
    {
        /// <summary>
        /// The main UI manager object.
        /// </summary>
        public UIManager UI;
        /// <summary>
        /// The active Widget in the window. Used for higher priority popup windows that overlay the old active widget.
        /// </summary>
        public IContainer ActiveWidget;
        /// <summary>
        /// The list of former active widgets. Used to go back to an older active widget when the currently active widget closes.
        /// </summary>
        public List<IContainer> Widgets = new List<IContainer>();

        /// <summary>
        /// The main active mode.
        /// </summary>
        public Widget MainEditorWidget;

        /// <summary>
        /// The MappingWidget object of the mapping mode. Null if not active.
        /// </summary>
        public MappingWidget MapWidget { get { return MainEditorWidget as MappingWidget; } }
        /// <summary>
        /// The EventingWidget object of the eventing mode. Null if not active.
        /// </summary>
        public EventingWidget EventingWidget { get { return MainEditorWidget as EventingWidget; } }
        /// <summary>
        /// The DatabaseWidget object of the database mode. Null if not active.
        /// </summary>
        public DatabaseWidget DatabaseWidget { get { return MainEditorWidget as DatabaseWidget; } }

        /// <summary>
        /// The main grid layout which divides menubar, toolbar, main area and statusbar from one another.
        /// </summary>
        public Grid MainGridLayout;
        /// <summary>
        /// The menubar.
        /// </summary>
        public MenuBar MenuBar;
        /// <summary>
        /// The status bar.
        /// </summary>
        public StatusBar StatusBar;
        /// <summary>
        /// The toolbar.
        /// </summary>
        public ToolBar ToolBar;
        /// <summary>
        /// The home screen, if shown.
        /// </summary>
        public HomeScreen HomeScreen;

        public MainEditorWindow(string ProjectFile)
        {
            this.SetMinimumSize(600, 400);
            this.SetText("RPG Studio MK");
            this.Initialize();
            Editor.LoadGeneralSettings();
            SetPosition(Editor.GeneralSettings.LastX, Editor.GeneralSettings.LastY);
            SetSize(Editor.GeneralSettings.LastWidth, Editor.GeneralSettings.LastHeight);
            if (Editor.GeneralSettings.WasMaximized) SDL2.SDL.SDL_MaximizeWindow(SDL_Window);

            this.OnClosing += delegate (BoolEventArgs e)
            {
                int x, y;
                SDL2.SDL.SDL_GetWindowPosition(this.SDL_Window, out x, out y);
                int w, h;
                SDL2.SDL.SDL_GetWindowSize(this.SDL_Window, out w, out h);
                Editor.GeneralSettings.LastX = x;
                Editor.GeneralSettings.LastY = y;
                Editor.GeneralSettings.LastWidth = w;
                Editor.GeneralSettings.LastHeight = h;
                SDL2.SDL.SDL_WindowFlags flags = (SDL2.SDL.SDL_WindowFlags) SDL2.SDL.SDL_GetWindowFlags(this.SDL_Window);
                Editor.GeneralSettings.WasMaximized = (flags & SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) == SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;
                Editor.DumpGeneralSettings();

                if (Editor.InProject)
                {
                    // Save window when closing with the top-right X button
                    if (Program.ReleaseMode && !Program.ThrownError)
                    {
                        e.Value = true;
                        EnsureSaved(Dispose);
                    }
                }
            };

            this.UI = new UIManager(this);
            this.UI.SetBackgroundColor(10, 23, 37);

            // Widgets may now be created

            Editor.MainWindow = this;
            Utilities.Initialize();

            #region Grid
            MainGridLayout = new Grid(UI);
            MainGridLayout.SetSize(Width, Height);
            /* 0 m m m m m m m m m m m m m
             * 1 t t t t t t t t t t t t t
             * 2 - - - - - - - - - - - - -
             * 3 a a a a a a a a a a a a a
             *   a a a a a a a a a a a a a
             *   a a a a a a a a a a a a a
             *   a a a a a a a a a a a a a
             *   a a a a a a a a a a a a a
             * 4 - - - - - - - - - - - - -
             * 5 s s s s s s s s s s s s s
             * m => menubar
             * t => toolbar
             * a => main editor area (divided in a grid of its own)
             * s => statusbar
             * - => divider*/
            MainGridLayout.SetRows(
                new GridSize(32, Unit.Pixels),
                new GridSize(31, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1),
                new GridSize(1, Unit.Pixels),
                new GridSize(26, Unit.Pixels)
            );

            #endregion
            #region Menubar + Toolbar
            Color DividerColor = new Color(79, 108, 159);

            // Header + Menubar
            MenuBar = new MenuBar(MainGridLayout);
            MenuBar.SetBackgroundColor(10, 23, 37);
            MenuBar.SetGridRow(0);
            MenuBar.SetItems(new List<MenuItem>()
            {
                new MenuItem("File")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("New")
                        {
                            HelpText = "Create a new project.",
                            OnLeftClick = delegate (MouseEventArgs e) { EnsureSaved(Editor.NewProject); }
                        },
                        new MenuItem("Open")
                        {
                            HelpText = "Open an existing project.",
                            Shortcut = "Ctrl+O",
                            OnLeftClick = delegate (MouseEventArgs e) { EnsureSaved(Editor.OpenProject); }
                        },
                        new MenuItem("Save")
                        {
                            HelpText = "Save all changes in the current project.",
                            Shortcut = "Ctrl+S",
                            OnLeftClick = delegate (MouseEventArgs e) { Editor.SaveProject(); },
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; }
                        },
                        new MenuSeparator(),
                        new MenuItem("Close Project")
                        {
                            HelpText = "Close this project and return to the welcome screen.",
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; },
                            OnLeftClick = delegate (MouseEventArgs e) { EnsureSaved(Editor.CloseProject); }
                        },
                        new MenuItem("Reload Project")
                        {
                            HelpText = "Closes and immediately reopens the project. Used for quickly determining if changes are saved properly, or to restore an old version.",
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; },
                            OnLeftClick = delegate (MouseEventArgs e) { EnsureSaved(Editor.ReloadProject); }
                        },
                        new MenuItem("Exit Editor")
                        {
                            HelpText = "Close this project and quit the program.",
                            OnLeftClick = delegate (MouseEventArgs e) { EnsureSaved(Editor.ExitEditor); }
                        }
                    }
                },
                new MenuItem("Edit")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Import Maps")
                        {
                            HelpText = "Import Maps made with RPG Maker XP.",
                            OnLeftClick = delegate (MouseEventArgs e) { Editor.ImportMaps(); },
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; }
                        },
                        new MenuItem("Restore Map")
                        {
                            HelpText = "Restore a map that was deleted during this session.",
                            OnLeftClick = delegate (MouseEventArgs e) { Editor.RestoreMap(); },
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; }
                        },
                        new MenuItem("Clear deleted map cache")
                        {
                            HelpText = "Clears the internal cache of restore-able deleted maps.",
                            OnLeftClick = delegate (MouseEventArgs e) { Editor.ClearMapCache(); },
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; }
                        }
                    }
                },
                new MenuItem("View")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Toggle Animations")
                        {
                            HelpText = "Toggles the animation of autotiles, fogs and panoramas.",
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; },
                            OnLeftClick = delegate (MouseEventArgs e) { Editor.ToggleMapAnimations(); }
                        },
                        new MenuItem("Toggle Grid")
                        {
                            HelpText = "Toggles the visibility of the grid overlay while mapping.",
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; },
                            OnLeftClick = delegate (MouseEventArgs e) { Editor.ToggleGrid(); }
                        }
                    }
                },
                new MenuItem("Game")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Play Game")
                        {
                            Shortcut = "F12",
                            HelpText = "Play the game.",
                            OnLeftClick = delegate (MouseEventArgs e) { Editor.StartGame(); },
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; }
                        },
                        new MenuItem("Open Game Folder")
                        {
                            HelpText = "Opens the file explorer and navigates to the project folder.",
                            OnLeftClick = delegate (MouseEventArgs e) { Editor.OpenGameFolder(); },
                            IsClickable = delegate (BoolEventArgs e ) { e.Value = Editor.InProject; }
                        }
                    }
                },
                new MenuItem("Help")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Help")
                        {
                            Shortcut = "F1",
                            HelpText = "Opens the help window.",
                            OnLeftClick = delegate (MouseEventArgs e) { OpenHelpWindow(); }
                        },
                        new MenuItem("About RPG Studio MK")
                        {
                            HelpText = "Shows information about this program.",
                            OnLeftClick = delegate (MouseEventArgs e) { OpenAboutWindow(); }
                        }
                    }
                }
            });


            // Toolbar (modes, icons, etc)
            ToolBar = new ToolBar(MainGridLayout);
            ToolBar.SetBackgroundColor(28, 50, 73);
            ToolBar.SetGridRow(1);
            #endregion
            #region Dividers
            // Blue 1px separator
            Widget Blue1pxSeparator = new Widget(MainGridLayout);
            Blue1pxSeparator.SetBackgroundColor(DividerColor);
            Blue1pxSeparator.SetGridRow(2);

            // Status bar divider
            Widget StatusBarDivider = new Widget(MainGridLayout);
            StatusBarDivider.SetBackgroundColor(DividerColor);
            StatusBarDivider.SetGridRow(4);
            #endregion
            #region Statusbar
            // Status bar
            StatusBar = new StatusBar(MainGridLayout);
            StatusBar.SetGridRow(5);
            #endregion

            // If an argument was passed, load that project file and skip the home screen
            if (!string.IsNullOrEmpty(ProjectFile))
            {
                Data.SetProjectPath(ProjectFile);
                CreateEditor();
                Editor.MakeRecentProject();
            }
            else
            {
                MainGridLayout.Rows[1] = new GridSize(0, Unit.Pixels);
                MainGridLayout.Rows[4] = new GridSize(0, Unit.Pixels);
                MainGridLayout.Rows[5] = new GridSize(0, Unit.Pixels);
                MainGridLayout.UpdateContainers();
                StatusBar.SetVisible(false);
                ToolBar.SetVisible(false);
                HomeScreen = new HomeScreen(MainGridLayout);
                HomeScreen.SetGridRow(3);
            }

            #region Events
            this.UI.Update();
            this.Start();
            #endregion

            UI.SizeChanged(new BaseEventArgs());
        }

        /// <summary>
        /// Initializes the editor after the home screen has been shown.
        /// </summary>
        public void CreateEditor()
        {
            DateTime start = DateTime.Now;
            if (HomeScreen != null)
                HomeScreen.Dispose();

            MainGridLayout.Rows[1] = new GridSize(31, Unit.Pixels);
            MainGridLayout.Rows[4] = new GridSize(1, Unit.Pixels);
            MainGridLayout.Rows[5] = new GridSize(26, Unit.Pixels);
            MainGridLayout.UpdateContainers();

            Editor.LoadProjectSettings();
            Data.LoadGameData();

            Editor.SetMode(Editor.ProjectSettings.LastMode, true);
            TimeSpan time = DateTime.Now - start;
            StatusBar.QueueMessage($"Project loaded ({time.Milliseconds}ms)", true, 5000);
        }

        /// <summary>
        /// Opens the Help window.
        /// </summary>
        public void OpenHelpWindow()
        {
            new MessageBox("Help",
                "As there is no built-in wiki or documentation yet, please direct any questions to the official Discord server or Twitter account.");
        }
        
        /// <summary>
        /// Open the About window.
        /// </summary>
        public void OpenAboutWindow()
        {
            new MessageBox("About RPG Studio MK",
                "This program is intended to be an editor for games made with the MK Starter Kit.\n" +
                "It was created by Marin, with additional support of various other individuals.\n" +
                "\n" +
                "Please turn to the GitHub page for a full credits list."
            );
        }

        /// <summary>
        /// Prompts the user to save if there are unsaved changes.
        /// </summary>
        /// <param name="Function">The function to call if saved or continued.</param>
        public void EnsureSaved(Action Function)
        {
            if (!Editor.UnsavedChanges)
            {
                Function();
                return;
            }
            MessageBox box = new MessageBox("Warning", "The game contains unsaved changes. Are you sure you would like to proceed? All unsaved changes will be lost.",
                new List<string>() { "Save", "Continue", "Cancel" }, IconType.Warning);
            box.OnButtonPressed += delegate (BaseEventArgs e)
            {
                if (box.Result == 0) // Save
                {
                    Editor.SaveProject();
                    Function();
                }
                else if (box.Result == 1) // Continue
                {
                    Function();
                }
            };
        }

        /// <summary>
        /// Sets the main active widget.
        /// </summary>
        /// <param name="Widget">The widget to set as the main widget.</param>
        public void SetActiveWidget(IContainer Widget)
        {
            this.ActiveWidget = Widget;
            if (!Widgets.Contains(Widget)) Widgets.Add(Widget);
            if (Graphics.LastMouseEvent is MouseEventArgs) Graphics.LastMouseEvent.Handled = true;
        }

        /// <summary>
        /// Sets the opacity of the main window overlay.
        /// </summary>
        public void SetOverlayOpacity(byte Opacity)
        {
            TopSprite.Opacity = Opacity;
        }

        /// <summary>
        /// Sets the Z index of the main window overlay's viewport.
        /// </summary>
        public void SetOverlayZIndex(int Z)
        {
            TopViewport.Z = Z;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            UI.MouseDown(e);
        }

        public override void MousePress(MouseEventArgs e)
        {
            base.MousePress(e);
            UI.MousePress(e);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            UI.MouseUp(e);
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            UI.MouseMoving(e);
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            base.MouseWheel(e);
            UI.MouseWheel(e);
        }

        public override void TextInput(TextEventArgs e)
        {
            base.TextInput(e);
            UI.TextInput(e);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            UI.SizeChanged(e);
        }

        /// <summary>
        /// Updates the UIManager, and subsequently all widgets.
        /// </summary>
        public override void Tick(BaseEventArgs e)
        {
            base.Tick(e);
            this.UI.Update();
        }
    }
}
