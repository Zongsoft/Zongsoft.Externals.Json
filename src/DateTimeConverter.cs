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
	public class DateTimeConverter : IsoDateTimeConverter
	{
		private static readonly DateTime OriginTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		#region 构造函数
		public DateTimeConverter()
		{
			this.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
		}
		#endregion

		#region 公共属性
		public bool UnixTimestampRequired
		{
			get;
			set;
		}
		#endregion

		#region 重写方法
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
					return OriginTimestamp.AddMilliseconds(number);
				else
					return OriginTimestamp.AddMilliseconds(number).ToLocalTime();
			}

			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
		{
			if(this.UnixTimestampRequired)
			{
				if(value is DateTime)
				{
					var text = (((DateTime)value - OriginTimestamp.ToLocalTime()).TotalMilliseconds).ToString();
					writer.WriteValue(text);
					return;
				}
				else if(value is DateTimeOffset)
				{
					var text = (((DateTimeOffset)value - OriginTimestamp).TotalMilliseconds).ToString();
					writer.WriteValue(text);
					return;
				}
			}

			base.WriteJson(writer, value, serializer);
		}
		#endregion
	}
}
