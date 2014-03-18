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
    /// <summary>
    /// This command represents a completed node dragging operation (either a single node or
    /// multiple). The command itself doesn't execute the drag (which deviates from the command
    /// pattern but whatever), it's only responsible for undoing and redoing the dragging.
    /// </summary>
    public class DragNodesCommand : ICommand
    {
        public string Name
        {
            get
            {
                if (_nodes.Length == 1)
                {
                    return "Move " + ObjectNames.NicifyVariableName(_nodes[0].GetType().Name);
                }
                else
                {
                    return "Move " + _nodes.Length + " Nodes";
                }
            }
        }

        private Node[] _nodes;
        private Vector2 _delta;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="nodes">The nodes that were dragged.</param>
        /// <param name="delta">An offset from the initial position of the node(s) to the final
        /// position (post-drag).</param>
        public DragNodesCommand(ICollection<Node> nodes, Vector2 delta)
        {
            _nodes = new Node[nodes.Count];
            nodes.CopyTo(_nodes, 0);
            _delta = delta;
        }

        public void Execute()
        {
            // the nodes have already been dragged to their final position so there is nothing
            // to be done here
        }

        public void Undo()
        {
            foreach (var node in _nodes)
            {
                node.MoveWindow(-_delta);
            }
        }

        public void Redo()
        {
            foreach (var node in _nodes)
            {
                node.MoveWindow(_delta);
            }
        }
    }
}