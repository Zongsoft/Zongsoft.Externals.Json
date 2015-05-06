/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.CoreLibrary.
 *
 * Zongsoft.CoreLibrary is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.CoreLibrary is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.CoreLibrary; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Zongsoft.Externals.Json
{
	public class JsonSerializer : Zongsoft.Runtime.Serialization.ISerializer, Zongsoft.Runtime.Serialization.ITextSerializer
	{
		#region 单例字段
		public static readonly JsonSerializer Default = new JsonSerializer();
		#endregion

		#region 成员字段
		private Newtonsoft.Json.JsonSerializer _serializer;
		#endregion

		#region 构造函数
		public JsonSerializer()
		{
			_serializer = Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings()
			{
#if DEBUG
				Formatting = Newtonsoft.Json.Formatting.Indented,
#else
				Formatting = Newtonsoft.Json.Formatting.None,
#endif
				ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
			});
		}
		#endregion

		#region 公共方法
		public string Serialize(object graph)
		{
			if(graph == null)
				return null;

			var text = new StringBuilder();

			using(var writer = new StringWriter(text))
			{
				_serializer.Serialize(writer, graph);
				return text.ToString();
			}
		}

		public void Serialize(Stream serializationStream, object graph)
		{
			if(serializationStream == null)
				throw new ArgumentNullException("serializationStream");

			if(graph == null)
				return;

			using(var writer = new StreamWriter(serializationStream, Encoding.UTF8))
			{
				_serializer.Serialize(writer, graph);
			}
		}

		public void Serialize(TextWriter writer, object graph)
		{
			if(writer == null)
				throw new ArgumentNullException("writer");

			if(graph == null)
				return;

			_serializer.Serialize(writer, graph);
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
				return _serializer.Deserialize(reader, type);
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

			return _serializer.Deserialize(reader, type);
		}
		#endregion

		#region 显式实现
		object Zongsoft.Runtime.Serialization.ISerializer.Deserialize(Stream serializationStream)
		{
			throw new NotSupportedException();
		}

		object Zongsoft.Runtime.Serialization.ITextSerializer.Deserialize(TextReader reader)
		{
			throw new NotSupportedException();
		}
		#endregion
	}
}
