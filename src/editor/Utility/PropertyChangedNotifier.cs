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
using System.ComponentModel;
using System.Linq.Expressions;

namespace UBonsai.Editor.Utility
{
    public abstract class PropertyChangedNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetBackingField<T>(
            BackingField<T> field, T value, CommandHistory history, bool combineCommands
        )
        {
            if ((field != null) && !EqualityComparer<T>.Default.Equals(field, value))
            {
                var command = new ChangePropertyCommand<T>(GetType(), field, value);
                history.Execute(command, combineCommands);
                OnPropertyChanged(field.PropertyName);
                return true;
            }
            return false;
        }

        protected bool SetBackingField<T>(string propertyName, ref T field, T value)
        {
            if ((field != null) && !EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        protected static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }
    }
}