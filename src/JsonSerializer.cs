/*
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

			using(var writer = new StreamWriter(serializationStream, System.Text.Encoding.UTF8, 1024, true))
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

			using(var reader = new StreamReader(serializationStream, System.Text.Encoding.UTF8, false, 1024, true))
			{
				var serializer = this.GetSerializer(_settings);
				serializer.TypeNameHandling = TypeNameHandling.Objects;
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
			serializer.TypeNameHandling = TypeNameHandling.Objects;
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
				serializer.TypeNameHandling = TypeNameHandling.Objects;
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
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				TypeNameHandling = (settings != null && settings.Typed) ? Newtonsoft.Json.TypeNameHandling.Objects : TypeNameHandling.None,
				ContractResolver = new MyJsonContractResolver((settings == null ? SerializationNamingConvention.None : settings.NamingConvention)),
			};

			if(settings != null)
			{
				result.Formatting = settings.Indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;

				if(settings.MaximumDepth > 0)
					result.MaxDepth = settings.MaximumDepth;

				if((settings.SerializationBehavior & SerializationBehavior.IgnoreNullValue) == SerializationBehavior.IgnoreNullValue)
					result.NullValueHandling = NullValueHandling.Ignore;

				if((settings.SerializationBehavior & SerializationBehavior.IgnoreDefaultValue) == SerializationBehavior.IgnoreDefaultValue)
				{
					result.NullValueHandling = NullValueHandling.Ignore;
					result.DefaultValueHandling = DefaultValueHandling.Ignore;
				}
			}

			//添加增强版的日期时间转换器
			result.Converters.Add(new Converters.DateTimeConverter(settings?.DateTimeFormat));

			return result;
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private Newtonsoft.Json.JsonSerializer GetSerializer(Zongsoft.Runtime.Serialization.SerializationSettings settings)
		{
			var serializer = Newtonsoft.Json.JsonSerializer.Create(this.GetSerializerSettings(settings as TextSerializationSettings ?? _settings));
			return serializer;
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
			if(serializationStream == null)
				return null;

			using(var reader = new StreamReader(serializationStream))
			{
				return JsonConvert.DeserializeObject(reader.ReadToEnd());
			}
		}

		object Zongsoft.Runtime.Serialization.ITextSerializer.Deserialize(TextReader reader)
		{
			if(reader == null)
				return null;

			return JsonConvert.DeserializeObject(reader.ReadToEnd());
		}

		object Zongsoft.Runtime.Serialization.ITextSerializer.Deserialize(string text)
		{
			return JsonConvert.DeserializeObject(text);
		}
		#endregion

		#region 嵌套子类
		private class MyJsonContractResolver : DefaultContractResolver
		{
			#region 私有变量
			private SerializationNamingConvention _namingConvention;
			#endregion

			#region 构造函数
			public MyJsonContractResolver(SerializationNamingConvention namingConvention)
			{
				_namingConvention = namingConvention;
				this.IgnoreSerializableAttribute = true;
			}
			#endregion

			#region 重写方法
			protected override JsonObjectContract CreateObjectContract(Type type)
			{
				if(type.Assembly.IsDynamic)
					type = GetEntityInterface(type);

				var contract = base.CreateObjectContract(type);
				this.SetObjectCreator(contract);
				return contract;
			}

			protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
			{
				var properties = base.CreateProperties(type, memberSerialization);

				foreach(var property in properties)
				{
					var attributes = property.AttributeProvider.GetAttributes(typeof(SerializationMemberAttribute), true);

					if(attributes != null && attributes.Count > 0)
					{
						var attribute = (SerializationMemberAttribute)attributes[0];

						if(attribute.Name != null && attribute.Name.Length > 0)
							property.PropertyName = attribute.Name;

						property.Ignored = (attribute.Behavior == SerializationMemberBehavior.Ignored);
						property.Required = (attribute.Behavior == SerializationMemberBehavior.Required) ? Required.AllowNull : Required.Default;
					}

					attributes = property.AttributeProvider.GetAttributes(typeof(SerializationBinderAttribute), true);

					if(attributes != null && attributes.Count > 0)
					{
						//获取当前成员指定的反序列化绑定器
						var binder = this.GetBinder(((SerializationBinderAttribute)attributes[0]).BinderType);

						if(binder != null)
						{
							//绑定器属性可能会依赖其他属性值，所以需要将该属性排在最后
							property.Order = int.MaxValue;

							//如果成员绑定器支持值绑定转换
							if(binder.GetMemberValueSupported)
								property.ValueProvider = new ValueProvider(property.PropertyName, (container, value) => binder.GetMemberValue(property.PropertyName, container, value));

							property.ShouldDeserialize = container =>
							{
								var propertyType = binder.GetMemberType(property.PropertyName, container);

								if(propertyType != null)
									property.PropertyType = propertyType;

								return true;
							};
						}
					}
				}

				return properties.OrderBy(p => p.Order.HasValue ? p.Order.Value : 0).ToArray();
			}

			public override JsonContract ResolveContract(Type type)
			{
				var contract = base.ResolveContract(type);
				return contract;
			}

			protected override JsonConverter ResolveContractConverter(Type objectType)
			{
				//if(objectType == typeof(object))
				//	return new Converters.ObjectConverter();

				return base.ResolveContractConverter(objectType);
			}

			protected override string ResolvePropertyName(string propertyName)
			{
				switch(_namingConvention)
				{
					case SerializationNamingConvention.Camel:
						return ToNamingCase(propertyName, true);
					case SerializationNamingConvention.Pascal:
						return ToNamingCase(propertyName, false);
					default:
						return base.ResolvePropertyName(propertyName);
				}
			}
			#endregion

			#region 私有方法
			private Zongsoft.Runtime.Serialization.ISerializationBinder GetBinder(Type type)
			{
				if(type != null && typeof(Zongsoft.Runtime.Serialization.ISerializationBinder).IsAssignableFrom(type))
					return (Zongsoft.Runtime.Serialization.ISerializationBinder)System.Activator.CreateInstance(type);

				return null;
			}

			private Type GetEntityInterface(Type type)
			{
				var contracts = type.GetInterfaces();

				foreach(var contract in contracts)
				{
					if(contract.Name == "I" + type.Name)
						return contract;
				}

				throw new System.Runtime.Serialization.SerializationException($"The entity interface of the '{type}' dynamic class implementation was not found and serialization was not supported.");
			}

			private void SetObjectCreator(JsonObjectContract contract)
			{
				if(contract.CreatedType == typeof(object))
					contract.DefaultCreator = () => new Dictionary<string, object>();
				else if(contract.CreatedType.IsInterface)
					contract.DefaultCreator = () => Zongsoft.Data.Entity.Build(contract.CreatedType);
				else
				{
					if(contract.CreatorParameters.Count > 0)
						return;

					//获取以构造函数参数的数量多少为排序依据的构造函数信息集合
					var constructors = contract.CreatedType.GetConstructors().OrderByDescending(info => info.GetParameters().Length).ToArray();

					for(int i = 0; i < constructors.Length; i++)
					{
						var constructor = constructors[i];
						var parameters = constructor.GetParameters();

						foreach(var parameter in parameters)
						{
							var property = contract.Properties.GetProperty(parameter.Name, StringComparison.OrdinalIgnoreCase);

							if(property == null || property.Writable)
								break;

							contract.CreatorParameters.AddProperty(property);
						}

						if(parameters.Length != contract.CreatorParameters.Count)
						{
							contract.CreatorParameters.Clear();
						}
						else
						{
							contract.OverrideCreator = new ObjectCreator(constructor).CreateObject;
							break;
						}
					}
				}
			}

			private string ToNamingCase(string name, bool toLower)
			{
				if(string.IsNullOrEmpty(name))
					return name;

				if(toLower)
				{
					if(!char.IsUpper(name[0]))
						return name;
				}
				else
				{
					if(!char.IsLower(name[0]))
						return name;
				}

				char[] chars = name.ToCharArray();

				for(int i = 0; i < chars.Length; i++)
				{
					bool hasNext = (i + 1 < chars.Length);

					if(i > 0 && hasNext)
					{
						if(toLower)
						{
							if(!char.IsUpper(chars[i + 1]))
								break;
						}
						else
						{
							if(!char.IsLower(chars[i + 1]))
								break;
						}
					}

					if(toLower)
						chars[i] = char.ToLowerInvariant(chars[i]);
					else
						chars[i] = char.ToUpperInvariant(chars[i]);
				}

				return new string(chars);
			}
			#endregion

			#region 嵌套子类
			private class ValueProvider : IValueProvider
			{
				private string _name;
				private Func<object, object, object> _bind;

				public ValueProvider(string name, Func<object, object, object> bind)
				{
					_name = name;
					_bind = bind;
				}

				public object GetValue(object target)
				{
					return null;
				}

				public void SetValue(object target, object value)
				{
					if(target == null)
						return;

					//如果待转换的成员值是JObject这样的动态对象，则将其转换成字典
					if(value is JObject)
						value = this.ToDictionary((JObject)value);

					//执行绑定操作，将待设置的成员值转换成成员的真实值
					var boundValue = _bind(target, value);

					if(target is IDictionary)
					{
						((IDictionary)target)[_name] = boundValue;
					}
					else if(target is IDictionary<string, object>)
					{
						((IDictionary<string, object>)target)[_name] = boundValue;
					}
					else if(target is IDictionary<string, string>)
					{
						((IDictionary<string, string>)target)[_name] = Zongsoft.Common.Convert.ConvertValue<string>(boundValue);
					}
					else
					{
						var members = target.GetType().GetMember(_name, MemberTypes.Property | MemberTypes.Field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

						if(members != null && members.Length == 1)
						{
							if(members[0].MemberType == MemberTypes.Field)
								((FieldInfo)members[0]).SetValue(target, boundValue);
							else if(members[0].MemberType == MemberTypes.Property)
								((PropertyInfo)members[0]).SetValue(target, boundValue);
						}
					}
				}

				private IDictionary<string, object> ToDictionary(JObject target)
				{
					if(target == null)
						return null;

					var dictionary = new Dictionary<string, object>(target.Count);

					foreach(var property in target.Properties())
					{
						dictionary[property.Name] = property.Value;
					}

					return dictionary;
				}
			}

			private class ObjectCreator
			{
				private ConstructorInfo _constructor;

				public ObjectCreator(ConstructorInfo constructor)
				{
					_constructor = constructor;
				}

				public object CreateObject(object[] parameters)
				{
					return _constructor.Invoke(parameters);
				}
			}
			#endregion
		}
		#endregion
	}
}
