using System;
using System.Collections.Generic;

using Xunit;

using Zongsoft.Data;
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
				Gender = JsonSerializerTests.Gender.Male,
				FullName = "Popeye Zhong",
				Email = "popeye@automao.cn",
				PhoneNumber = "18912345678",
				Birthdate = new DateTime(1979, 5, 15),
				Grade = 0,
				TotalPoints = Zongsoft.Common.RandomGenerator.GenerateInt32(),
				Description = "我是凹凸猫的一号员工！",
			};

			_credential = new Security.Credential("20150801", user, "Web", TimeSpan.FromHours(2));

			_credential.Parameters.Add("QQ", "9555****");
			_credential.Parameters.Add("WeChatNo", "Automao");
			_credential.Parameters.Add("NativePlace", "湖南邵阳");
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
				Birthdate = DateTime.Now,
				Principal = "001",
				Grade = 0,
				TotalPoints = Zongsoft.Common.RandomGenerator.GenerateInt32(),
				Description = "我是凹凸猫的一号员工！",
			}, new TextSerializationSettings() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });

			Assert.NotNull(text);

			text = JsonSerializer.Default.Serialize(_credential);

			Assert.NotNull(text);

			JsonSerializer.Default.Settings.NamingConvention = SerializationNamingConvention.Camel;
			text = JsonSerializer.Default.Serialize(_credential);

			Assert.NotNull(text);

			var department = new Department
			{
				DepartmentId = 1,
				Name = "Development",
				PrincipalKind = 99,
				Principal = new Employee
				{
					UserId = 100,
					EmployeeId = 101,
					EmployeeNo = "A101",
					Hiredate = DateTime.Today,
				}
			};

			var json = JsonSerializer.Default.Serialize(department, new TextSerializationSettings() { Indented = true, Typed = true, SerializationBehavior = SerializationBehavior.IgnoreDefaultValue});
			Assert.NotNull(json);

			department = JsonSerializer.Default.Deserialize<Department>(json);
			Assert.NotNull(department);
			Assert.NotNull(department.Principal);
			Assert.IsType<Employee>(department.Principal);
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

			var conditional = Zongsoft.Data.Entity.Build<IEmployeeConditional>();
			conditional.EmployeeNo = "A001";
			conditional.Hiredate = new ConditionalRange<DateTime>(new DateTime(2010, 1, 1), DateTime.Today);
			conditional.Leavedate = new ConditionalRange<DateTime>(new DateTime(2017, 1, 1), null);

			text = JsonSerializer.Default.Serialize(conditional);
			Assert.NotNull(text);

			var result = JsonSerializer.Default.Deserialize<IEmployeeConditional>(text);
			Assert.NotNull(result);
			Assert.Equal("A001", result.EmployeeNo);
			Assert.NotNull(result.Hiredate);
			Assert.NotNull(result.Leavedate);
			//Assert.Equal(DateTime.Parse("2010-1-1"), result.Hiredate.From);
			//Assert.Equal(DateTime.Today, result.Hiredate.To);
		}

		[Fact]
		public void DeserializeDepartmentTest()
		{
			var department = new Department
			{
				DepartmentId = 101,
				PrincipalKind = 1,
				Principal = new Employee
				{
					EmployeeId = 1,
					EmployeeNo = "A001",
					Hiredate = DateTime.Parse("2017-3-4"),
					JobState = 2,
				}
			};

			var json = @"
{
  ""DepartmentId"": 101,
  ""Name"": ""Software Development"",
  ""PrincipalKind"": 1,
  ""Principal"": {
    ""EmployeeId"": 1,
    ""EmployeeNo"": ""A001"",
    ""CorporationId"": 0,
    ""JobState"": 2,
    ""Hiredate"": 974202655000,
    ""Leavedate"": null,
    ""UserId"": 0,
    ""User"": null
  }
}";
			var result = JsonSerializer.Default.Deserialize<Department>(json);

			Assert.NotNull(result);
			Assert.NotNull(result.Principal);
			Assert.IsType<Employee>(result.Principal);
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
		public interface IEmployeeConditional : Zongsoft.Data.IConditional
		{
			int EmployeeId
			{
				get; set;
			}

			string EmployeeNo
			{
				get; set;
			}

			ConditionalRange<DateTime> Hiredate
			{
				get; set;
			}

			ConditionalRange<DateTime> Leavedate
			{
				get; set;
			}
		}

		public class Department
		{
			public int DepartmentId
			{
				get;
				set;
			}

			public string Name
			{
				get;
				set;
			}

			public byte PrincipalKind
			{
				get;
				set;
			}

			[SerializationBinder(typeof(PrincipalBinder))]
			public object Principal
			{
				get;
				set;
			}

			#region 自定义成员反序列绑定器
			private class PrincipalBinder : SerializationBinderBase<Department>
			{
				protected override Type GetMemberType(string name, Department container)
				{
					switch(container.PrincipalKind)
					{
						case 0:
							return typeof(UserProfile);
						case 1:
							return typeof(Employee);
						default:
							return null;
					}
				}
			}
			#endregion
		}

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

		public class UserProfile : Zongsoft.Security.Membership.IUserIdentity
		{
			#region 构造函数
			public UserProfile(uint userId, string name)
			{
				this.UserId = userId;
				this.Name = name;
			}

			public UserProfile(uint userId, string name, string @namespace)
			{
				this.UserId = userId;
				this.Name = name;
				this.Namespace = @namespace;
			}
			#endregion

			#region 公共属性
			public uint UserId
			{
				get; set;
			}

			public string Name
			{
				get; set;
			}

			public string FullName
			{
				get; set;
			}

			public string Namespace
			{
				get; set;
			}

			public string Email
			{
				get; set;
			}

			public string PhoneNumber
			{
				get; set;
			}

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

			public string Description
			{
				get; set;
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
