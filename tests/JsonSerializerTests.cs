using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Zongsoft.Runtime.Serialization;

namespace Zongsoft.Externals.Json.Tests
{
	public class JsonSerializerTests
	{
		#region 测试数据
		private Zongsoft.Security.Certification _certification;
		#endregion

		#region 构造函数
		public JsonSerializerTests()
		{
			var user = new UserProfile(101, "Popeye")
			{
				Gender = JsonSerializerTests.Gender.Male,
				FullName = "Popeye Zhong",
				Email = "popeye@automao.cn",
				PhoneNumber = "18912345678",
				Birthdate = new DateTime(1979, 5, 15),
				Principal = "001",
				Grade = 10,
				TotalPoints = Zongsoft.Common.RandomGenerator.GenerateInt32(),
				Description = "我是凹凸猫的一号员工！",
			};

			_certification = new Security.Certification("20150801", user, "Web", TimeSpan.FromHours(2));

			_certification.ExtendedProperties.Add("QQ", "9555****");
			_certification.ExtendedProperties.Add("WeChatNo", "Automao");
			_certification.ExtendedProperties.Add("NativePlace", "湖南邵阳");
		}
		#endregion

		#region 测试方法
		[Fact]
		public void SerializeTest()
		{
			JsonSerializer.Default.Settings.Indented = true;
			var text = JsonSerializer.Default.Serialize(_certification);

			Assert.NotNull(text);

			JsonSerializer.Default.Settings.NamingConvention = SerializationNamingConvention.Camel;
			text = JsonSerializer.Default.Serialize(_certification);

			Assert.NotNull(text);
		}

		[Fact]
		public void DeserializeTest()
		{
			JsonSerializer.Default.Settings.Indented = true;
			JsonSerializer.Default.Settings.NamingConvention = SerializationNamingConvention.Camel;
			var text = JsonSerializer.Default.Serialize(_certification);

			Assert.NotNull(text);

			var certification = JsonSerializer.Default.Deserialize<Zongsoft.Security.Certification>(text);

			Assert.NotNull(certification);
		}
		#endregion

		#region 测试实体
		public class UserProfile : Zongsoft.Security.Membership.User
		{
			#region 构造函数
			public UserProfile(int userId, string name) : base(userId, name)
			{
			}

			public UserProfile(int userId, string name, string @namespace) : base(userId, name, @namespace)
			{
			}
			#endregion

			#region 公共属性
			public Gender Gender
			{
				get;
				set;
			}

			public DateTime? Birthdate
			{
				get;
				set;
			}

			public byte Grade
			{
				get;
				set;
			}

			[SerializationMember(SerializationMemberBehavior.Ignored)]
			public int TotalPoints
			{
				get;
				set;
			}
			#endregion
		}

		public enum Gender
		{
			Female = 0,
			Male = 1,
		}
		#endregion
	}
}
