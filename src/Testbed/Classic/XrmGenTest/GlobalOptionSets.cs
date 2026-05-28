namespace XrmGenTest.OptionSets
{
    using System.CodeDom.Compiler;
    using System.Runtime.Serialization;
	[DataContract]
    [GeneratedCode("","")]
	public enum Datastate
	{
		[EnumMember] Default = 0,
		[EnumMember] Retain = 1,
	}
	[DataContract]
	public enum Supportedlanguages
	{
		[EnumMember] Fr = 125410000,
		[EnumMember] Nl = 125410001,
		[EnumMember] En = 125410002,
		[EnumMember] Es = 125410003,
		[EnumMember] De = 125410004,
	}
}