﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Zongsoft.Runtime.Serialization;

namespace Zongsoft.Externals.Json
{
	public class JsonSerializer : Zongsoft.Runtime.Serialization.ISerializer, Zongsoft.Runtime.Serialization.ITextSerializer
	{
		#region 单例字段
		public static readonly JsonSerializer Default = new JsonSerializer();
		#endregion

		#region 成员字段
		private Zongsoft.Runtime.Serialization.TextSerializationSettings _settings;
		#endregion

		#region 构造函数
		public JsonSerializer() : this(null)
		{
		}

		public JsonSerializer(Zongsoft.Runtime.Serialization.TextSerializationSettings settings)
		{
			_settings = settings ?? new Zongsoft.Runtime.Serialization.TextSerializationSettings();
		}
		#endregion

		#region 公共属性
		public Zongsoft.Runtime.Serialization.TextSerializationSettings Settings
		{
			get
			{
				return _settings;
			}
		}
		#endregion

		#region 公共方法
		public string Serialize(object graph, TextSerializationSettings settings = null)
		{
			if(graph == null)
				return null;

			return Newtonsoft.Json.JsonConvert.SerializeObject(graph, this.GetSerializerSettings(settings ?? _settings));
		}

		public void Serialize(Stream serializationStream, object graph, SerializationSettings settings = null)
		{
			if(serializationStream == null)
				throw new ArgumentNullException("serializationStream");

			if(graph == null)
				return;

			using(var writer = new StreamWriter(serializationStream, Encoding.UTF8))
			{
				var serializer = this.GetSerializer(settings);
				serializer.Serialize(writer, graph);
			}
		}

		public void Serialize(TextWriter writer, object graph, TextSerializationSettings settings = null)
		{
			if(writer == null)
				throw new ArgumentNullException("writer");

			if(graph == null)
				return;

			var serializer = this.GetSerializer(settings);
			serializer.Serialize(writer, graph);
		}

		public T Deserialize<T>(Stream serializationStream)
		{
			return (T)this.Deserialize(serializationStream, typeof(T));
		}

		public object Deserialize(Stream serializationStream, Type type)
		{
			if(serializationStream == null)
				throw new ArgumentNullException("serializationStream");

			using(var reader = new StreamReader(serializationStream, Encoding.UTF8))
			{
				var serializer = this.GetSerializer(_settings);
				return serializer.Deserialize(reader, type);
			}
		}

		public T Deserialize<T>(TextReader reader)
		{
			return (T)this.Deserialize(reader, typeof(T));
		}

		public object Deserialize(TextReader reader, Type type)
		{
			if(reader == null)
				throw new ArgumentNullException("reader");

			var serializer = this.GetSerializer(_settings);
			return serializer.Deserialize(reader, type);
		}

		public T Deserialize<T>(string text)
		{
			return (T)this.Deserialize(text, typeof(T));
		}

		public object Deserialize(string text, Type type)
		{
			if(string.IsNullOrWhiteSpace(text))
				return null;

			using(var reader = new StringReader(text))
			{
				var serializer = this.GetSerializer(_settings);
				return serializer.Deserialize(reader, type);
			}
		}
		#endregion

		#region 私有方法
		private Newtonsoft.Json.JsonSerializerSettings GetSerializerSettings(Zongsoft.Runtime.Serialization.TextSerializationSettings settings)
		{
			var result = new Newtonsoft.Json.JsonSerializerSettings()
			{
				Formatting = Formatting.None,
				ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			};

			if(settings == null)
				return result;

			result.Formatting = settings.IsIndented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;

			if(settings.NamingConvention == Zongsoft.Runtime.Serialization.SerializationNamingConvention.Camel)
				result.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
			else
				result.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();

			return result;
		}

		private Newtonsoft.Json.JsonSerializer GetSerializer(Zongsoft.Runtime.Serialization.SerializationSettings settings)
		{
			var jsonSettings = this.GetSerializerSettings(settings as TextSerializationSettings ?? _settings);
			return Newtonsoft.Json.JsonSerializer.Create(jsonSettings);
		}
		#endregion

		#region 显式实现
		Zongsoft.Runtime.Serialization.SerializationSettings Zongsoft.Runtime.Serialization.ISerializer.Settings
		{
			get
			{
				return this.Settings;
			}
		}

		object Zongsoft.Runtime.Serialization.ISerializer.Deserialize(Stream serializationStream)
		{
			throw new NotSupportedException();
		}

		object Zongsoft.Runtime.Serialization.ITextSerializer.Deserialize(TextReader reader)
		{
			throw new NotSupportedException();
		}

		object Zongsoft.Runtime.Serialization.ITextSerializer.Deserialize(string text)
		{
			throw new NotSupportedException();
		}
		#endregion
	}
}
