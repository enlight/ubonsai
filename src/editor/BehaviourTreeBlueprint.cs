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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UBonsai.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace UBonsai.Editor
{
    public class BehaviourTreeBlueprint
    {
        public event GenericEventHandler<BehaviourTreeBlueprint, EventArgs> SelectionChanged;

        public static Type[] ControlNodeTypes;
        public static string[] ControlNodeTypeNames;

        /// <summary>
        /// Indicates whether this tree needs to be repainted.
        /// </summary>
        public bool Dirty { get; set; }

        public ReadOnlyCollection<Node> Selection
        {
            get
            {
                return _selectedNodes.AsReadOnly();
            }
        }

        public CommandHistory CommandHistory { get; set; }

        private Node _rootNode;
        private Vector2 _mousePosition;
        private List<Node> _selectedNodes = new List<Node>();
        private bool _allowMultiSelect = false;
        private bool _dragging = false;
        private Vector2 _dragStartPosition;

        static BehaviourTreeBlueprint()
        {
            ControlNodeTypes = new Type[]
            {
                typeof(ParallelNode),
                typeof(RandomNode),
                typeof(SelectorNode),
                typeof(SequenceNode)
            };

            ControlNodeTypeNames = new string[]
            {
                ParallelNode.TypeName,
                RandomNode.TypeName,
                SelectorNode.TypeName,
                SequenceNode.TypeName
            };
        }

        public void OnGUI(Event e)
        {
            _mousePosition = e.mousePosition;
            _allowMultiSelect = e.shift;

            if (_rootNode != null)
                _rootNode.OnGUI(e);

            switch (e.type)
            {
                case EventType.Repaint:
                    Dirty = false;
                    break;

                case EventType.ContextClick:
                    OnContextClick();
                    e.Use();
                    break;

                case EventType.MouseDown:
                    if (e.button == 0) // left mouse button
                        ClearSelection();
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        if (!_dragging)
                        {
                            _dragging = true;
                            _dragStartPosition = _mousePosition;
                        }
                        DragSelection(e.delta);
                    }
                    e.Use();
                    break;

                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        if (_dragging)
                        {
                            _dragging = false;
                            if (_selectedNodes.Count > 0)
                            {
                                var command = new DragNodesCommand(
                                    _selectedNodes, _mousePosition - _dragStartPosition
                                );
                                CommandHistory.Execute(command);
                            }
                        }
                    }
                    break;
            }
        }

        private void OnContextClick()
        {
            var menu = new GenericMenu();
            if (_rootNode != null)
            {
                if (_selectedNodes.Count == 1)
                {
                    _selectedNodes[0].AddOuterContextMenuEntries(menu);
                    if (menu.GetItemCount() > 0)
                    {
                        menu.AddSeparator("");
                    }
                    for (var i = 0; i < ControlNodeTypes.Length; i++)
                    {
                        // TODO: add all the action nodes
                        // ...
                        AddNodeTypeToContextMenu(
                            menu, "Create Control Node/" + ControlNodeTypeNames[i],
                            ControlNodeTypes[i]
                        );
                        // TODO: add all the constraint nodes
                    }
                }
            }
            else
            {
                for (var i = 0; i < ControlNodeTypes.Length; i++)
                {
                    AddNodeTypeToContextMenu(
                        menu, "Create Control Node/" + ControlNodeTypeNames[i],
                        ControlNodeTypes[i]
                    );
                }
            }
            menu.ShowAsContext();
        }

        private void AddNodeTypeToContextMenu(GenericMenu menu, string itemName, Type nodeType)
        {
            menu.AddItem(
                new GUIContent(itemName), false,
                () =>
                {
                    var cmd = new CreateNodeCommand(nodeType, _mousePosition, this);
                    CommandHistory.Execute(cmd);
                }
            );
        }

        internal void AttachNode(Node node, ControlNode parentNode)
        {
            node.NodeSelectionChanged += NodeSelectionChanged;
            node.NodeDirtyChanged += NodeDirtyChanged;

            if (parentNode != null)
            {
                parentNode.AddChild(node);
            }
            else if (_rootNode == null)
            {
                _rootNode = node;
                Dirty = true;
            }
            else
            {
                throw new InvalidOperationException("Can't attach orphaned node.");
            }
        }

        internal void DetachNode(Node node, ControlNode parentNode)
        {
            node.Selected = false;

            node.NodeSelectionChanged -= NodeSelectionChanged;
            node.NodeDirtyChanged -= NodeDirtyChanged;

            if (parentNode != null)
            {
                parentNode.RemoveChild(node);
            }
            else if (_rootNode == node)
            {
                _rootNode = null;
                Dirty = true;
            }
            else
            {
                throw new InvalidOperationException("Can't detach orphaned node.");
            }
        }

        private void ClearSelection()
        {
            // iterate in reverse order because when the selected status of a node changes
            // it'll fire off an event that will call NodeDeselected() to remove the node
            // from the list (removing from the back of the list will not invalidate the
            // iteration index)
            for (var i = _selectedNodes.Count - 1; i >= 0; i--)
            {
                _selectedNodes[i].Selected = false;
            }
        }

        private void DragSelection(Vector2 delta)
        {
            foreach (var node in _selectedNodes)
            {
                node.MoveWindow(delta);
            }
        }

        private void NodeSelectionChanged(Node node)
        {
            if (node.Selected)
            {
                if (!_allowMultiSelect)
                {
                    ClearSelection();
                }
                _selectedNodes.Add(node);
            }
            else
            {
                _selectedNodes.Remove(node);
            }
            OnSelectionChanged();
        }

        private void NodeDirtyChanged(Node node)
        {
            if (node.Dirty)
                Dirty = true;
        }

        private void OnSelectionChanged()
        {
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }
    }
}