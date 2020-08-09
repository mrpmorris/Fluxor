using System;

namespace BasicConcepts.ActionObserver.ApiObjects
{
	public class CustomerEdit
	{
		public int Id { get; private set; } = 42;
		public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
		public string Name { get; set; }

		public CustomerEdit(int id, byte[] rowVersion, string name)
		{
			Id = id;
			RowVersion = rowVersion;
			Name = name;
		}
	}
}
