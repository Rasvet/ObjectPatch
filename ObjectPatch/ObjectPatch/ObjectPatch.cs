using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ObjectPatch
{
	[Serializable]
	public class ObjectPatch
	{
		[Serializable]
		struct MemPatch
		{
			public int offset;
			public int length;
			public byte[] data;
		}

		MemPatch[] patches;
		int patchedLength;

		public int GetPatchCount()
		{
			return patches.Length;
		}

		public int GetPatchedLength()
		{
			return patchedLength;
		}

		public byte[] Serialize()
		{
			MemoryStream stream = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(stream, this);
			return stream.ToArray();
		}

		public static ObjectPatch GeneratePatch(byte[] org, byte[] mod)
		{
			ObjectPatch op = new ObjectPatch();
			List<MemPatch> lp = new List<MemPatch>();

			int minlength = Math.Min(org.Length, mod.Length);

			for (int i = 0; i < minlength; )
			{
				if (org[i] != mod[i])
				{
					int length = 1;
					for (int j = i + 1; j < minlength; ++j)
					{
						if (org[j] == mod[j])
						{
							length = j - i;
							break;
						}
					}

					MemPatch mp;
					mp.length = length;
					mp.offset = i;
					mp.data = new byte[length];
					Buffer.BlockCopy(mod, i, mp.data, 0, length);
					lp.Add(mp);

					i += length;
				}
				else
					++i;
			}

			op.patchedLength = mod.Length;

			if (op.patchedLength > org.Length)
			{
				MemPatch mp = new MemPatch();
				int diffLength = op.patchedLength - org.Length;
				mp.data = new byte[diffLength];
				Buffer.BlockCopy(mod, org.Length, mp.data, 0, diffLength);
				mp.length = diffLength;
				mp.offset = org.Length;
				lp.Add(mp);
			}

			Console.WriteLine("Found {0} deltas.", lp.Count);

			op.patches = lp.ToArray();
			return op;
		}

		public static byte[] ApplyPatch(byte[] org, ObjectPatch patch)
		{
			byte[] mod = new byte[patch.patchedLength];
			Buffer.BlockCopy(org, 0, mod, 0, org.Length);

			for (int i = 0; i < patch.patches.Length; ++i)
			{
				MemPatch mp = patch.patches[i];
				Buffer.BlockCopy(mp.data, 0, mod, mp.offset, mp.length);
			}
			
			return mod;
		}
	}
}
