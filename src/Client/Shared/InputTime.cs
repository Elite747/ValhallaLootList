// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace ValhallaLootList.Client.Shared
{
    public class InputTime<T> : InputBase<T>
    {
        private const string _timeFormat = "t";

        /// <summary>
        /// Gets or sets the error message used when displaying an a parsing error.
        /// </summary>
        [Parameter] public string ParsingErrorMessage { get; set; } = "The {0} field must be a time.";

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "type", "time");
            builder.AddAttribute(3, "class", CssClass);
            builder.AddAttribute(4, "value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute(5, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.CloseElement();
        }

        protected override string? FormatValueAsString(T? value)
        {
            DateTime? dt = null;

            if (value is TimeSpan time)
            {
                dt = new DateTime(time.Ticks);
            }
            else if (value is DateTime dateTime)
            {
                dt = dateTime;
            }

            if (dt.HasValue)
            {
                return BindConverter.FormatValue(dt.Value, _timeFormat, CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }

        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out T result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            // Unwrap nullable types. We don't have to deal with receiving empty values for nullable
            // types here, because the underlying InputBase already covers that.
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            bool success;
            if (targetType == typeof(DateTime))
            {
                success = TryParseDateTime(value, out result);
            }
            else if (targetType == typeof(TimeSpan))
            {
                success = TryParseTimeSpan(value, out result);
            }
            else
            {
                throw new InvalidOperationException($"The type '{targetType}' is not a supported time type.");
            }

            if (success)
            {
                Debug.Assert(result != null);
                validationErrorMessage = null;
                return true;
            }
            else
            {
                validationErrorMessage = string.Format(CultureInfo.InvariantCulture, ParsingErrorMessage, DisplayName ?? FieldIdentifier.FieldName);
                return false;
            }
        }

        private static bool TryParseDateTime(string? value, [MaybeNullWhen(false)] out T result)
        {
            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var time))
            {
                result = (T)(object)new DateTime(time.Ticks);
                return true;
            }

            if (BindConverter.TryConvertToDateTime(value, CultureInfo.InvariantCulture, _timeFormat, out var date))
            {
                result = (T)(object)date;
                return true;
            }

            result = default;
            return false;
        }

        private static bool TryParseTimeSpan(string? value, [MaybeNullWhen(false)] out T result)
        {
            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var time))
            {
                result = (T)(object)time;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}