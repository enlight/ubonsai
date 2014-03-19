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

using UBonsai.Editor.Utility;
using UnityEditor;

namespace UBonsai.Editor
{
    /// <summary>
    /// This command encapsulates a change to a single property.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    public class ChangePropertyCommand<T> : ICommand
    {
        public string Name
        {
            get { return _name; }
        }

        private string _name;
        private BackingField<T> _field;
        private T _initialValue;
        private T _newValue;

        public ChangePropertyCommand(System.Type ownerType, BackingField<T> field, T newValue)
        {
            _name = "Change " + ObjectNames.NicifyVariableName(ownerType.Name)
                + " " + field.PropertyName;
            _field = field;
            _initialValue = field.Value;
            _newValue = newValue;
        }

        public void Execute()
        {
            _field.Value = _newValue;
        }

        public void Undo()
        {
            _field.Value = _initialValue;
        }

        public void Redo()
        {
            _field.Value = _newValue;
        }

        public bool CombineWith(ICommand otherCommand)
        {
            // currently only changes to the same string field are merged together
            if (_newValue is string)
            {
                var other = otherCommand as ChangePropertyCommand<T>;
                if ((other != null) && (other._field == _field))
                {
                    _newValue = other._newValue;
                    return true;
                }
            }
            return false;
        }
    }
}