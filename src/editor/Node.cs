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

using UnityEditor;
using UnityEngine;

namespace UBonsai.Editor
{
    public delegate void NodeEventHandler(Node node);

    public abstract class Node
    {
        /// <summary>
        /// Fires whenever the node is selected or deselected.
        /// </summary>
        public event NodeEventHandler NodeSelectionChanged;

        /// <summary>
        /// Fires whenever the value of the Dirty property of the node changes.
        /// </summary>
        public event NodeEventHandler NodeDirtyChanged;

        /// <summary>
        /// The position and size of the node.
        /// </summary>
        public Rect Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                if (_bounds != value)
                {
                    _bounds = value;
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// Indicates whether the node is currently selected.
        /// </summary>
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnSelectedChanged();
                }
            }
        }

        /// <summary>
        /// Indicates whether the node needs to be repainted.
        /// </summary>
        public bool Dirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                if (_dirty != value)
                {
                    _dirty = value;
                    OnDirtyChanged();
                }
            }
        }

        public virtual Texture2D Icon
        {
            get
            {
                return GUISkinManager.GetNodeIcon(GetType());
            }
        }

        /// <summary>
        /// Short user defined text that will displayed next to the node icon.
        /// </summary>
        public string Label { get; set; }

        public int WindowID
        {
            get
            {
                if (_windowID == TreeEditorWindow.InvalidWindowID)
                {
                    _windowID = TreeEditorWindow.GenerateWindowID();
                }
                return _windowID;
            }
        }

        private BehaviourTreeBlueprint _tree;
        private Rect _bounds;
        private bool _selected;
        private bool _dirty;
        private int _windowID = TreeEditorWindow.InvalidWindowID;
        //private GUIStyle _selectedStyle;

        public Node(Vector2 midPoint, BehaviourTreeBlueprint tree)
        {
            float width = 32;
            float height = 32;
            Bounds = new Rect(midPoint.x - (width * 0.5f), midPoint.y - (height * 0.5f), width, height);
            _tree = tree;
        }

        /// <summary>
        /// Add entries to the given context menu.
        /// This method will only be called on a node instance if that node is the one and only
        /// selected node, and the mouse position at which the user decided to invoke the context
        /// menu is outside the bounds of that node.
        /// </summary>
        /// <param name="menu">A valid reference to a context menu.</param>
        public virtual void AddOuterContextMenuEntries(GenericMenu menu)
        {
        }

        public virtual void OnGUI(Event e)
        {
            var style = Selected ?
                GUISkinManager.NodeWindowSelectedStyle : GUISkinManager.NodeWindowNormalStyle;

            _bounds = GUILayout.Window(WindowID, _bounds, ProcessWindowEvent, "", style);

            if (e.type == EventType.Repaint)
                Dirty = false;
        }

        public virtual void ProcessWindowEvent(int windowID)
        {
            if (windowID != WindowID)
            {
                Debug.LogError("This is the not the window you're looking for!");
            }
            if ((Label != null) && (Label != string.Empty))
            {
                GUILayout.BeginHorizontal();
                {
                    if (Icon != null)
                    {
                        GUILayout.Label(Icon, GUILayout.Width(32), GUILayout.Height(32));
                    }
                    GUILayout.Label(Label, GUISkinManager.NodeLabelStyle);
                }
                GUILayout.EndHorizontal();
            }
            else if (Icon != null)
            {
                GUILayout.Label(Icon, GUILayout.Width(32), GUILayout.Height(32));
            }

            // the window only gets mouse events if the cursor is over the window,
            // and the mouse position is relative to the top left corner of the window
            var e = Event.current;
            switch (e.type)
            {
                case EventType.ContextClick:
                    ShowContextMenu();
                    e.Use();
                    break;

                case EventType.MouseDown:
                    Selected = true;
                    e.Use();
                    break;
            }

            // This is normally how one handles window dragging,
            // but in this case it's handled by MoveWindow()
            //GUI.DragWindow();
        }

        public virtual void ShowContextMenu()
        {
        }

        public virtual void OnSelectedChanged()
        {
            if (NodeSelectionChanged != null)
            {
                NodeSelectionChanged(this);
            }
            Dirty = true;
        }

        public virtual void OnDirtyChanged()
        {
            if (NodeDirtyChanged != null)
                NodeDirtyChanged(this);
        }

        public virtual void MoveWindow(Vector2 delta)
        {
            var oldBounds = Bounds;
            Bounds = new Rect(
                oldBounds.x + delta.x, oldBounds.y + delta.y,
                oldBounds.width, oldBounds.height
            );
        }
    }
}