using System;

namespace BasicConcepts.ActionObserver.ApiObjects
{
	public class CustomerEdit
	{
		public Guid Id { get; private set; } = Guid.NewGuid();
		public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
		public string Name { get; set; }

		public CustomerEdit(Guid id, byte[] rowVersion, string name)
		{
			Id = id;
			RowVersion = rowVersion;
			Name = name;
		}
	}
}
