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
    public abstract class ControlNode : Node
    {
        private List<Node> _children;

        public ControlNode(Vector2 midPoint, BehaviourTreeBlueprint tree)
            : base(midPoint, tree)
        {
        }

        public void AddChild(Node node)
        {
            if (_children == null)
            {
                _children = new List<Node>();
            }
            _children.Add(node);
            Dirty = true;
        }

        public override void OnGUI(Event e)
        {
            base.OnGUI(e);

            if ((_children != null) && (e.type != EventType.Used))
            {
                foreach (var child in _children)
                {
                    if (e.type == EventType.Repaint)
                    {
                        DrawEdge(Bounds, child.Bounds);
                    }
                    child.OnGUI(e);
                    // if one of the nodes has used up the event there's no need to check the rest
                    if (e.type == EventType.Used)
                        break;
                }
            }
        }

        public override void MoveWindow(Vector2 delta)
        {
            base.MoveWindow(delta);

            // drag the entire sub-tree rooted at this node
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.MoveWindow(delta);
                }
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