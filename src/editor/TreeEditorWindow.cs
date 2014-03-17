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

using System;
using UnityEditor;
using UnityEngine;

namespace UBonsai.Editor
{
    /// <summary>
    /// A custom editor window that provides functionality to create behaviour trees.
    /// </summary>
    public class TreeEditorWindow : EditorWindow
    {
        public BehaviourTreeBlueprint CurrentTree
        {
            get { return _currentTree; }
        }

        /// <summary>
        /// Raised when custom inspectors should repaint themselves.
        /// </summary>
        public event GenericEventHandler<TreeEditorWindow, System.EventArgs> InspectedStateChanged;

        public const int InvalidWindowID = -1;

        private BehaviourTreeBlueprint _currentTree;
        private Vector2 _mousePosition;
        private bool _dragging = false;
        private CommandHistory _commandHistory;
        private CommandHistoryWindow _historyWindow;

        private static int _nextWindowID = 0;

        [MenuItem("Window/UBonsai Editor")]
        private static void Init()
        {
            var editorWindow = EditorWindow.GetWindow<TreeEditorWindow>("UBonsai");
        }

        /// <summary>
        /// Generate a unique window ID for a GUI.Window.
        /// The generated ID is only unique for any instances of TreeEditorWindow.
        /// </summary>
        /// <returns>A unique ID that can be used to create a new GUI.Window.</returns>
        public static int GenerateWindowID()
        {
            return _nextWindowID++;
        }

        public void OnEnable()
        {
            Debug.Log("TreeEditorWindow.OnEnable()");
            name = "UBonsai Editor";
            _commandHistory = new CommandHistory(false);
            _historyWindow = new CommandHistoryWindow(GenerateWindowID());
            _historyWindow.CommandHistory = _commandHistory;
            _currentTree = new BehaviourTreeBlueprint();
            _currentTree.CommandHistory = _commandHistory;

            _currentTree.SelectionChanged += SelectionChanged;
        }

        public void OnDisable()
        {
            Debug.Log("TreeEditorWindow.OnDisable()");
            if (_currentTree != null)
            {
                _currentTree.SelectionChanged -= SelectionChanged;
                _currentTree.CommandHistory = null;
                _currentTree = null;
            }

            if (_historyWindow != null)
            {
                _historyWindow = null;
            }

            if (_commandHistory != null)
            {
                _commandHistory.Dispose();
                _commandHistory = null;
            }
        }

        /// <summary>
        /// Handle input and redraw the window if necessary.
        /// </summary>
        public void OnGUI()
        {
            Event e = Event.current;
            _mousePosition = e.mousePosition;

            GUI.changed = false;

            switch (e.type)
            {
                case EventType.MouseDrag:
                    // to achieve smooth dragging wantsMouseMove must be set,
                    // doing so will prompt Unity to call OnGUI() more frequently
                    if (!_dragging)
                    {
                        _dragging = true;
                        wantsMouseMove = true;
                    }
                    // note that the event is not marked as used at this point because
                    // the actual dragging is handled further down by the contents of
                    // the editor window
                    break;

                case EventType.MouseUp:
                    _dragging = false;
                    wantsMouseMove = false;
                    break;
            }

            GUI.skin = GUISkinManager.Skin;
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            // need this in order for the node windows to show up
            BeginWindows();

            if (_historyWindow != null)
                _historyWindow.OnGUI(e);

            if (_currentTree != null)
                _currentTree.OnGUI(e);

            EndWindows();
            GUILayout.EndArea();

            // handle any events that haven't been used up above
            switch (e.type)
            {
                case EventType.ContextClick:
                    OnContextClick();
                    e.Use();
                    break;
            }

            if (GUI.changed || _currentTree.Dirty)
            {
                Repaint();
                if (InspectedStateChanged != null)
                    InspectedStateChanged(this, System.EventArgs.Empty);
            }
        }

        private void OnContextClick()
        {
        }

        public void OnFocus()
        {
            Selection.activeInstanceID = GetInstanceID();
        }

        private void SelectionChanged(BehaviourTreeBlueprint sender, EventArgs e)
        {
            if (sender.Selection.Count == 1)
                name = ObjectNames.NicifyVariableName(sender.Selection[0].GetType().Name);
            else
                name = "UBonsai Editor";
        }
    }
} // namespace UBonsai