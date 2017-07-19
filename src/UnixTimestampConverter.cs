/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2017 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Externals.Json.
 *
 * Zongsoft.Externals.Json is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Externals.Json is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Externals.Json; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zongsoft.Externals.Json
{
	public class UnixTimestampConverter : DateTimeConverterBase
	{
		private static readonly DateTime OriginalTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
		{
			bool isNullable = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);

			if(reader.TokenType == JsonToken.Null)
			{
				if(isNullable)
					return null;
				else
					return DateTime.MinValue;
			}

			Type type = (isNullable) ? Nullable.GetUnderlyingType(objectType) : objectType;

			if(reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
			{
				var number = System.Convert.ToUInt64(reader.Value);

				if(type == typeof(DateTimeOffset))
					return OriginalTimestamp.AddMilliseconds(number);
				else
					return OriginalTimestamp.AddMilliseconds(number).ToLocalTime();
			}

			var iso = new IsoDateTimeConverter();
			return iso.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
		{
			string text;

			if(value is DateTime)
			{
				text = (((DateTime)value - OriginalTimestamp.ToLocalTime()).TotalMilliseconds).ToString();
			}
            else if (value is DateTimeOffset)
			{
				text = (((DateTimeOffset)value - OriginalTimestamp).TotalMilliseconds).ToString();
			}
			else
			{
				throw new NotSupportedException("Unexpected value when converting date. Expected DateTime or DateTimeOffset.");
			}

			writer.WriteValue(text);
		}
	}
}
