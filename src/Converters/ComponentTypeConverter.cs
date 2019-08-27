/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2019 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.ComponentModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zongsoft.Externals.Json.Converters
{
	public class ComponentTypeConverter : JsonConverter
	{
		#region 成员字段
		private TypeConverter _converter;
		private Type _serializationType;
		#endregion

		#region 构造函数
		public ComponentTypeConverter(TypeConverter converter)
		{
			_converter = converter ?? throw new ArgumentNullException(nameof(converter));

			if(converter is Zongsoft.Runtime.Serialization.ISerializationConverter serializationConverter)
				_serializationType = serializationConverter.SerializationType;
		}
		#endregion

		#region 重写方法
		public override bool CanWrite
		{
			get => _serializationType != null;
		}

		public override bool CanConvert(Type objectType)
		{
			return true;
		}

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

			Type type = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

			if(_converter.CanConvertFrom(reader.ValueType))
				return _converter.ConvertFrom(reader.Value);

			throw new System.Runtime.Serialization.SerializationException($"The {_converter.GetType()} type converter does not support converting {reader.Value} to {type} type.");
		}

		public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
		{
			if(_serializationType != null)
				writer.WriteValue(_converter.ConvertTo(value, _serializationType));
			else
				writer.WriteValue(value);
		}
		#endregion
	}
}
