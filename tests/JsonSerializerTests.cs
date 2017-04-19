using System;
using System.Collections.Generic;

using Xunit;

using Zongsoft.Runtime.Serialization;

namespace Zongsoft.Externals.Json.Tests
{
	public class JsonSerializerTests
	{
		#region 私有变量
		private Zongsoft.Security.Credential _credential;
		#endregion

		#region 构造函数
		public JsonSerializerTests()
		{
			var user = new UserProfile(101, "Popeye")
			{
				Avatar = "/:mono:/",
				Gender = JsonSerializerTests.Gender.Male,
				FullName = "Popeye Zhong",
				Email = "popeye@automao.cn",
				PhoneNumber = "18912345678",
				Birthdate = new DateTime(1979, 5, 15),
				Grade = 0,
				TotalPoints = Zongsoft.Common.RandomGenerator.GenerateInt32(),
				Description = "我是凹凸猫的一号员工！",
				PrincipalId = "1",
				Principal = new Employee()
				{
					EmployeeId = 1,
					EmployeeNo = "A001",
					Hiredate = new DateTime(2015, 4, 20),
					UserId = 101,
				},
			};

			((Employee)user.Principal).User = user;

			_credential = new Security.Credential("20150801", user, "Web", TimeSpan.FromHours(2));

			_credential.ExtendedProperties.Add("QQ", "9555****");
			_credential.ExtendedProperties.Add("WeChatNo", "Automao");
			_credential.ExtendedProperties.Add("NativePlace", "湖南邵阳");
		}
		#endregion

		#region 测试方法
		[Fact]
		public void SerializeTest()
		{
			JsonSerializer.Default.Settings.Indented = true;

			var text = JsonSerializer.Default.Serialize(new
			{
				Avatar = "/:mono:/",
				Gender = JsonSerializerTests.Gender.Male,
				FullName = "Popeye Zhong",
				Email = "popeye@automao.cn",
				PhoneNumber = "18912345678",
				Birthdate = new DateTime(1979, 5, 15),
				Principal = "001",
				Grade = 0,
				TotalPoints = Zongsoft.Common.RandomGenerator.GenerateInt32(),
				Description = "我是凹凸猫的一号员工！",
			});

			Assert.NotNull(text);

			text = JsonSerializer.Default.Serialize(_credential);

			Assert.NotNull(text);

			JsonSerializer.Default.Settings.NamingConvention = SerializationNamingConvention.Camel;
			text = JsonSerializer.Default.Serialize(_credential);

			Assert.NotNull(text);
		}

		[Fact]
		public void DeserializeTest()
		{
			JsonSerializer.Default.Settings.Indented = true;
			JsonSerializer.Default.Settings.NamingConvention = SerializationNamingConvention.Camel;
			var text = JsonSerializer.Default.Serialize(_credential);

			Assert.NotNull(text);

			var certification = JsonSerializer.Default.Deserialize<Zongsoft.Security.Credential>(text);

			Assert.NotNull(certification);
		}

		[Fact]
		public void DeserializeDictionaryTest()
		{
			var text = @"{AssetId:100001, AssetNo:'A001', Projects:[{ProjectId:1},{ProjectId:3}], Creator:{UserId:100, Name:'Popeye'}}";
			var dictionary = JsonSerializer.Default.Deserialize<Dictionary<string, object>>(text);
			Assert.NotNull(dictionary);
			Assert.Equal(4, dictionary.Count);
		}
		#endregion

		#region 测试实体
		public class Employee
		{
			#region 构造函数
			public Employee()
			{
			}
			#endregion

			#region 公共属性
			public int EmployeeId
			{
				get;
				set;
			}

			public string EmployeeNo
			{
				get;
				set;
			}

			public int CorporationId
			{
				get;
				set;
			}

			public byte JobState
			{
				get;
				set;
			}

			public DateTime Hiredate
			{
				get;
				set;
			}

			public DateTime? Leavedate
			{
				get;
				set;
			}

			public int UserId
			{
				get;
				set;
			}

			public UserProfile User
			{
				get;
				set;
			}
			#endregion

			#region 重写方法
			public override bool Equals(object obj)
			{
				if(obj == null || obj.GetType() != this.GetType())
					return false;

				return ((Employee)obj).EmployeeId == this.EmployeeId;
			}

			public override int GetHashCode()
			{
				return this.EmployeeId;
			}
			#endregion
		}

		public class UserProfile : Zongsoft.Security.Membership.User
		{
			#region 构造函数
			public UserProfile(uint userId, string name) : base(userId, name)
			{
			}

			public UserProfile(uint userId, string name, string @namespace) : base(userId, name, @namespace)
			{
			}
			#endregion

			#region 公共属性
			[SerializationMember("Sex")]
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

			[SerializationMember(SerializationMemberBehavior.Required)]
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

			#region 重写方法
			public override bool Equals(object obj)
			{
				if(obj == null || obj.GetType() != this.GetType())
					return false;

				var other = (UserProfile)obj;

				return other.UserId == this.UserId && other.Namespace == this.Namespace;
			}

			public override int GetHashCode()
			{
				return (int)this.UserId;
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
