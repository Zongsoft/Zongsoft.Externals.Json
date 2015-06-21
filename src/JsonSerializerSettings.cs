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

using Newtonsoft.Json;

namespace Zongsoft.Externals.Json
{
	public class JsonSerializerSettings : Zongsoft.Runtime.Serialization.SerializerSettings
	{
		#region 单例字段
		public static readonly JsonSerializerSettings Default = new JsonSerializerSettings();
		#endregion

		#region 成员字段
		private Formatting _formatting;
		private ReferenceLoopHandling _referenceLoopHandling;
		#endregion

		#region 构造函数
		public JsonSerializerSettings()
		{
#if DEBUG
			_formatting = Newtonsoft.Json.Formatting.Indented;
#else
			_formatting = Newtonsoft.Json.Formatting.None;
#endif

			_referenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize;
		}
		#endregion

		#region 公共属性
		public Formatting Formatting
		{
			get
			{
				return _formatting;
			}
			set
			{
				this.SetPropertyValue(() => this.Formatting, ref _formatting, value);
			}
		}

		public ReferenceLoopHandling ReferenceLoopHandling
		{
			get
			{
				return _referenceLoopHandling;
			}
			set
			{
				this.SetPropertyValue(() => this.ReferenceLoopHandling, ref _referenceLoopHandling, value);
			}
		}
		#endregion
	}
}
