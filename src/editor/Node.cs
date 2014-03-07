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

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UBonsai.Editor
{
    public delegate void NodeEventHandler(Node node);

    public class Node
    {
        /// <summary>
        /// Fires whenever the node is selected or deselected.
        /// </summary>
        public event NodeEventHandler NodeSelectionChanged;

        /// <summary>
        /// Fires whenever the value of the Dirty property of the node changes.
        /// </summary>
        public event NodeEventHandler NodeDirtyChanged;

        public Rect Bounds { get; set; }

        /// <summary>
        /// Indicates whether this node is currently selected.
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
        /// Indicates whether this node needs to be repainted.
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

        private Tree _tree;
        private List<Node> _children;
        private bool _selected;
        private bool _dirty;
        //private GUIStyle _selectedStyle;

        public Node(float xMid, float yMid, Tree tree)
        {
            float width = 100;
            float height = 100;
            Bounds = new Rect(xMid - (width * 0.5f), yMid - (height * 0.5f), 100, 100);
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
            menu.AddItem(new GUIContent("Create Node"), false, _tree.CreateNode);
        }

        public void AddChild(Node node)
        {
            if (_children == null)
            {
                _children = new List<Node>();
            }
            _children.Add(node);
        }

        public void OnGUI(Event e)
        {
            switch (e.type)
            {
                case EventType.Repaint:
                    OnRepaint();
                    return;

                case EventType.ContextClick:
                    if (Bounds.Contains(e.mousePosition))
                    {
                        OnContextClick();
                        e.Use();
                    }
                    break;

                case EventType.MouseDown:
                    if (Bounds.Contains(e.mousePosition))
                    {
                        Selected = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (Selected)
                    {
                        OnMouseDrag(e);
                        e.Use();
                    }
                    break;
            }

            if ((_children != null) && (Event.current.type != EventType.Used))
            {
                foreach (var child in _children)
                {
                    child.OnGUI(e);
                    // if one of the nodes has used up the event there's no need to check the rest
                    if (e.type == EventType.Used)
                        break;
                }
            }
        }

        private void OnRepaint()
        {
            var oldColor = GUI.color;
            if (Selected)
                GUI.color = Color.red;
            GUI.Box(Bounds, "Node Name");
            GUI.color = oldColor;

            Dirty = false;

            if (_children != null)
            {
                foreach (var child in _children)
                {
                    DrawEdge(Bounds, child.Bounds);
                    child.OnRepaint();
                }
            }
        }

        protected virtual void OnContextClick()
        {
        }

        protected virtual void OnSelectedChanged()
        {
            if (NodeSelectionChanged != null)
            {
                NodeSelectionChanged(this);
            }
            Dirty = true;
        }

        protected virtual void OnDirtyChanged()
        {
            if (NodeDirtyChanged != null)
                NodeDirtyChanged(this);
        }

        protected virtual void OnMouseDrag(Event e)
        {
            if (e.button == 0)
            {
                var oldBounds = Bounds;
                Bounds = new Rect(
                    oldBounds.x + e.delta.x, oldBounds.y + e.delta.y,
                    oldBounds.width, oldBounds.height
                );
                Dirty = true;
            }
        }

        private static void DrawEdge(Rect parentBounds, Rect childBounds)
        {
            var edgeStart = new Vector3(parentBounds.center.x, parentBounds.yMax, 0f);
            var edgeEnd = new Vector3(childBounds.center.x, childBounds.yMin, 0f);
            Handles.DrawBezier(
                edgeStart, edgeEnd,
                edgeStart + 50f * Vector3.up, edgeEnd + 50f * Vector3.down,
                Color.black, null, 2
            );
        }
    }
}