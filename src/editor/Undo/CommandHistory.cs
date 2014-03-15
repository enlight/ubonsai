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
using UnityEngine;

namespace UBonsai.Editor
{
    [Serializable]
    public class Dummy : ScriptableObject
    {
        public int index;

        public void OnEnable()
        {
            Debug.Log("Dummy Enabled");
            hideFlags = HideFlags.HideAndDontSave;
            name = "CommandHistoryDummy";
            index = 0;
        }

        public void OnDisable()
        {
            Debug.Log("Dummy Disabled");
        }

        public void OnDestroy()
        {
            Debug.Log("Dummy Destroyed");
        }
    }

    /// <summary>
    /// Provides undo/redo functionality via the Command pattern.
    /// </summary>
    public class CommandHistory : IDisposable
    {
        private bool _inUndoRedo = false;
        private Stack<ICommand> _undoStack = new Stack<ICommand>();
        private Stack<ICommand> _redoStack = new Stack<ICommand>();
        private UnityEditor.Undo.UndoRedoCallback _oldUndoRedoCallback = null;
        private Dummy _dummy = null;

        public CommandHistory(bool hookIntoUnityEditorUndoSystem)
        {
            if (hookIntoUnityEditorUndoSystem)
                HookIntoUnityEditorUndoSystem();
        }

        /// <summary>
        /// Undo the last command that was executed or redone.
        /// </summary>
        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                _inUndoRedo = true;
                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
                _inUndoRedo = false;
            }
        }

        /// <summary>
        /// Redo the last command that was undone.
        /// </summary>
        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                _inUndoRedo = true;
                var command = _redoStack.Pop();
                command.Redo();
                _undoStack.Push(command);
                _inUndoRedo = false;
            }
        }

        public void Execute(ICommand command)
        {
            if (_inUndoRedo)
            {
                throw new InvalidOperationException("An undo/redo operation is in progress.");
            }

            if (_dummy != null)
            {
                // when Unity calls UnityEditor.Undo.PerformUndo() the dummy index will automatically
                // revert back to the value set here
                _dummy.index = _undoStack.Count;
                UnityEditor.Undo.RecordObject(_dummy, command.Name);
            }

            _redoStack.Clear();
            command.Execute();
            _undoStack.Push(command);

            if (_dummy != null)
            {
                // when Unity calls UnityEditor.Undo.PerformRedo() the dummy index will automatically
                // revert back to the value set here
                _dummy.index = _undoStack.Count;
            }

            Debug.Log("RecordObject Group ID: " + UnityEditor.Undo.GetCurrentGroup());
        }

        private void HookIntoUnityEditorUndoSystem()
        {
            Debug.Log("Hooking into Undo System");
            _dummy = ScriptableObject.CreateInstance<Dummy>();
            if (_dummy == null)
            {
                throw new NullReferenceException("CreateInstance<Dummy>() failed!");
            }
            _oldUndoRedoCallback = UnityEditor.Undo.undoRedoPerformed;
            UnityEditor.Undo.undoRedoPerformed = UndoRedoPerformed;
        }

        private void UnhookFromUnityEditorUndoSystem()
        {
            Debug.Log("Unhooking from Undo System");
            if (_dummy != null)
            {
                UnityEditor.Undo.undoRedoPerformed = _oldUndoRedoCallback;
                _oldUndoRedoCallback = null;
                UnityEditor.Undo.ClearUndo(_dummy);
                ScriptableObject.DestroyImmediate(_dummy);
                _dummy = null;
            }
            else
            {
                throw new InvalidOperationException("Not hooked into UnityEditor.Undo.");
            }
        }

        private void UndoRedoPerformed()
        {
            Debug.Log(
                "UndoRedoPerformed"
                + " Group ID: " + UnityEditor.Undo.GetCurrentGroup()
            );
            if (_oldUndoRedoCallback != null)
            {
                Debug.Log("Calling previous delegate.");
                _oldUndoRedoCallback();
            }

            // if dummy changed then the redo/undo relates to the command history
            if (_dummy.index < _undoStack.Count)
            {
                Debug.Log("Undo Last Command: index = " + _dummy.index + " stack size = " + _undoStack.Count);
                Undo();
            }
            else if (_dummy.index > _undoStack.Count)
            {
                Debug.Log("Redo Last Command: index = " + _dummy.index + " stack size = " + _undoStack.Count);
                Redo();
            }
        }

        public void Dispose()
        {
            if (_dummy != null)
                UnhookFromUnityEditorUndoSystem();
        }
    }
}