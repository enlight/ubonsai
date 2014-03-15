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

namespace UBonsai.Editor.Commands
{
    /// <summary>
    /// This command creates a new node and attaches it to the currently selected node,
    /// if no node is currently selected the new node becomes the root node of the tree.
    /// </summary>
    internal class CreateNodeCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "Create " + ObjectNames.NicifyVariableName(_nodeType.Name);
            }
        }

        private Type _nodeType;
        private Vector2 _position;
        private BehaviourTreeBlueprint _tree;
        private Node _node = null;
        private ControlNode _parentNode = null;

        public CreateNodeCommand(Type nodeType, Vector2 position, BehaviourTreeBlueprint tree)
        {
            _nodeType = nodeType;
            _position = position;
            _tree = tree;
        }

        public void Execute()
        {
            _node = (Node)Activator.CreateInstance(_nodeType, _position, _tree);
            if (_tree.Selection.Count == 1)
            {
                _parentNode = _tree.Selection[0] as ControlNode;
            }
            _tree.AttachNode(_node, _parentNode);
        }

        public void Undo()
        {
            _tree.DetachNode(_node, _parentNode);
        }

        public void Redo()
        {
            _tree.AttachNode(_node, _parentNode);
        }
    }
}