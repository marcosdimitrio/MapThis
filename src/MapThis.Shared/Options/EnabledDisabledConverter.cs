﻿// MIT License

// Copyright (c) 2016 Andrei Dzimchuk

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.ComponentModel;
using System.Globalization;

namespace MapThis.Vsix.Options
{
    public class EnabledDisabledConverter : BooleanConverter
    {
        private const string enabled = "Enabled";
        private const string disabled = "Disabled";

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                if (stringValue.Equals(enabled, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (stringValue.Equals(disabled, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is bool && destinationType == typeof(string))
            {
                return (bool)value ? enabled : disabled;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
