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
using UnityEngine;

namespace UBonsai.Editor
{
    public delegate void NodeSelectionEventHandler(Node node);

    public class Node
    {
        /// <summary>
        /// Fires whenever the node is selected or deselected.
        /// </summary>
        public event NodeSelectionEventHandler NodeSelectionChanged;

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

        private List<Node> _children;
        private bool _selected;
        //private GUIStyle _selectedStyle;

        public Node(float xMid, float yMid)
        {
            float width = 100;
            float height = 100;
            Bounds = new Rect(xMid - (width * 0.5f), yMid - (height * 0.5f), 100, 100);
        }

        public void OnGUI(Event e)
        {
            switch (e.type)
            {
                case EventType.Repaint:
                    OnRepaint();
                    break;

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
            /*
            if (_selectedStyle == null)
            {
                _selectedStyle = new GUIStyle(GUI.skin.box);
                _selectedStyle.border = new RectOffset(5, 5, 5, 5);
            }
            */
            var oldColor = GUI.color;
            if (Selected)
                GUI.color = Color.red;
            //GUIStyle currentStyle = Selected ? _selectedStyle : GUI.skin.box;
            GUI.Box(Bounds, "Node Name"/*, currentStyle*/);
            GUI.color = oldColor;
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
        }

        protected virtual void OnMouseDrag(Event e)
        {
            var oldBounds = Bounds;
            Bounds = new Rect(
                oldBounds.x + e.delta.x, oldBounds.y + e.delta.y,
                oldBounds.width, oldBounds.height
            );
        }
    }
}