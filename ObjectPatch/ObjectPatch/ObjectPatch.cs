using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ObjectPatch
{
	[Serializable]
	public struct MemPatch
	{
		public UInt32 offset;
		public UInt32 length;
		public byte[] data;

		public MemPatch(int size, int off)
		{
			length = (UInt32)size;
			offset = (UInt32)off;
			data = new byte[size];
		}
	}

	[Serializable]
	public class ObjectPatch
	{
		public MemPatch[] patches;
		public int patchedLength;

		public byte[] Serialize()
		{
			MemoryStream stream = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(stream, this);
			return stream.ToArray();
		}	
		
		public static ObjectPatch Deserialize(byte[] buffer)
		{
			MemoryStream stream = new MemoryStream(buffer);
			BinaryFormatter bf = new BinaryFormatter();
			return bf.Deserialize(stream) as ObjectPatch;
		}	
	}
}
