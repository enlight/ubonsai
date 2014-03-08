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

        private Tree _tree;
        private Rect _bounds;
        private bool _selected;
        private bool _dirty;
        //private GUIStyle _selectedStyle;

        public Node(Vector2 midPoint, Tree tree)
        {
            float width = 100;
            float height = 100;
            Bounds = new Rect(midPoint.x - (width * 0.5f), midPoint.y - (height * 0.5f), 100, 100);
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
        }

        public virtual void OnRepaint()
        {
            var oldColor = GUI.color;
            if (Selected)
                GUI.color = Color.red;
            GUI.Box(Bounds, "Node Name");
            GUI.color = oldColor;

            Dirty = false;
        }

        public virtual void OnContextClick()
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

        public virtual void OnMouseDrag(Event e)
        {
            if (e.button == 0)
            {
                var oldBounds = Bounds;
                Bounds = new Rect(
                    oldBounds.x + e.delta.x, oldBounds.y + e.delta.y,
                    oldBounds.width, oldBounds.height
                );
            }
        }
    }
}