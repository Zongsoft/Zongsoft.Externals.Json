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
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Zongsoft.Externals.Json.Converters
{
	public class ObjectConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(object);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
		{
			switch(reader.TokenType)
			{
				case JsonToken.StartArray:
					var array = (JArray)JArray.ReadFrom(reader);

					if(array.Count > 0 && array.FirstOrDefault().Type == JTokenType.Object)
					{
						var result = new Dictionary<string, object>[array.Count];

						for(int i = 0; i < result.Length; i++)
							result[i] = array[i].ToObject<Dictionary<string, object>>();

						return result;
					}

					return reader.Value;
				case JsonToken.StartObject:
					var obj = JObject.ReadFrom(reader);

					if(obj.First != null && obj.First.Type == JTokenType.Property)
					{
						if(((JProperty)obj.First).Name == "$type")
						{
							var typeName = ((JValue)((JProperty)obj.First).Value).Value.ToString();
							var type = Type.GetType(typeName, false);

							if(type != null)
								return obj.ToObject(type);
						}
					}

					return obj.ToObject<Dictionary<string, object>>();

				//Tip:以下代码或许可以递归激发该类型转换的解析
				//return serializer.Deserialize<Dictionary<string, object>>(reader);
				case JsonToken.Null:
				case JsonToken.Undefined:
					return null;
				default:
					return reader.Value;
			}
		}

		public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
		{
			throw new NotSupportedException();
		}
	}
}
