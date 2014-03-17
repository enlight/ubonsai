#region License

/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2014 Vadim Macagon
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#endregion License

using UnityEngine;

namespace UBonsai.Editor
{
    /// <summary>
    /// A GUI.Window that displays all the available undo and redo commands in a given
    /// CommandHistory in a list, and allows the user to jump to any point in the history by
    /// clicking on the corresponding item in the list.
    /// </summary>
    public class CommandHistoryWindow
    {
        /// <summary>
        /// The command history that's displayed in the window.
        /// </summary>
        public CommandHistory CommandHistory { get; set; }

        private int _windowID;
        private Rect _bounds;
        private Vector2 _scrollPosition = Vector2.zero;

        public CommandHistoryWindow(int windowID)
        {
            _windowID = windowID;
            _bounds = new Rect(0, 0, 300, 300);
        }

        public void OnGUI(Event e)
        {
            _bounds = GUI.Window(_windowID, _bounds, ProcessWindowEvent, "History");
        }

        public void ProcessWindowEvent(int windowID)
        {
            if (windowID != _windowID)
            {
                Debug.LogError("This is the not the window you're looking for!");
            }

            var undoCommands = CommandHistory.UndoCommands;
            var redoCommands = CommandHistory.RedoCommands;
            // TODO: Instead of hardcoding the height compute it using GUIStyle.CalcHeight()
            const int lineHeight = 20;
            var outerRect = new Rect(
                5, GUI.skin.window.padding.top, _bounds.width - 10, _bounds.height
            );
            var innerRect = new Rect(
                0, 0,
                _bounds.width - 10, (undoCommands.Length + redoCommands.Length + 1) * lineHeight
            );
            float buttonWidth = _bounds.width - 10;

            _scrollPosition = GUI.BeginScrollView(outerRect, _scrollPosition, innerRect);
            {
                int count = 1; // the initial state item is always drawn first

                var oldColor = GUI.color;
                if (undoCommands.Length == 0)
                {
                    GUI.color = Color.red;
                }
                if (GUI.Button(new Rect(0, 0, buttonWidth, lineHeight), "<Initial State>"))
                {
                    if (undoCommands.Length > 0)
                        CommandHistory.Undo(undoCommands.Length);
                }
                if (undoCommands.Length == 0)
                {
                    GUI.color = oldColor;
                }

                for (int i = undoCommands.Length - 1; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        GUI.color = Color.red;
                    }
                    if (GUI.Button(new Rect(0, count * lineHeight, buttonWidth, lineHeight),
                        undoCommands[i].Name))
                    {
                        if (count < undoCommands.Length)
                        {
                            CommandHistory.Undo(undoCommands.Length - count);
                        }
                    }
                    if (i == 0)
                    {
                        GUI.color = oldColor;
                    }
                    count++;
                }

                for (int i = 0; i < redoCommands.Length; i++)
                {
                    if (GUI.Button(new Rect(0, count * lineHeight, buttonWidth, lineHeight),
                        redoCommands[i].Name))
                    {
                        CommandHistory.Redo(i + 1);
                    }
                    count++;
                }
            }
            GUI.EndScrollView();
            GUI.DragWindow();
        }
    }
}