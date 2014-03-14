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

namespace UBonsai.Editor
{
    /// <summary>
    /// Provides undo/redo functionality via the Command pattern.
    /// </summary>
    public class CommandHistory
    {
        private bool _inUndoRedo = false;
        private Stack<ICommand> _undoStack = new Stack<ICommand>();
        private Stack<ICommand> _redoStack = new Stack<ICommand>();

        /// <summary>
        /// Undo the last command that was executed or redone.
        /// </summary>
        public void Undo()
        {
            _inUndoRedo = true;
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            _inUndoRedo = false;
        }

        /// <summary>
        /// Redo the last command that was undone.
        /// </summary>
        public void Redo()
        {
            _inUndoRedo = true;
            var command = _redoStack.Pop();
            command.Redo();
            _undoStack.Push(command);
            _inUndoRedo = false;
        }

        public void Execute(ICommand command)
        {
            if (_inUndoRedo)
            {
                throw new InvalidOperationException("An undo/redo operation is in progress.");
            }
            _redoStack.Clear();
            command.Execute();
            _undoStack.Push(command);
        }
    }
}