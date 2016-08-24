using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPatch
{
	public static class ObjectPatchRuntime
	{
		public static ObjectPatch GeneratePatch(byte[] org, byte[] mod, int chunk_size)
		{
			ObjectPatch op = new ObjectPatch();
			List<MemPatch> lp = new List<MemPatch>();

			int min_length = Math.Min(org.Length, mod.Length);

			for (int i = 0; i < min_length; i += chunk_size)
			{
				int remainder = min_length - i;				
				int chunk = Math.Min(remainder, chunk_size);

				if (!CompareChunk(org, mod, i, chunk))
				{
					MemPatch mp = new MemPatch(chunk, i);
					Buffer.BlockCopy(mod, i, mp.data, 0, chunk);
					lp.Add(mp);
				}				
			}

			op.patchedLength = mod.Length;

			if (op.patchedLength > org.Length)
			{
				int diffLength = op.patchedLength - org.Length;
				MemPatch mp = new MemPatch(diffLength, org.Length);			
				Buffer.BlockCopy(mod, org.Length, mp.data, 0, diffLength);
				lp.Add(mp);
			}

			Console.WriteLine("Found {0} deltas.", lp.Count);

			op.patches = lp.ToArray();
			return op;
		}

		public static byte[] ApplyPatch(byte[] org, ObjectPatch patch)
		{
			byte[] mod = new byte[patch.patchedLength];
			int min_length = Math.Min(patch.patchedLength, org.Length);
			Buffer.BlockCopy(org, 0, mod, 0, min_length);

			for (int i = 0; i < patch.patches.Length; ++i)
			{
				MemPatch mp = patch.patches[i];
				Buffer.BlockCopy(mp.data, 0, mod, (int)mp.offset, (int)mp.length);
			}

			return mod;
		}

		private static bool CompareChunk(byte[] a, byte[] b, int offset, int length)
		{
			for (int i = 0; i < length; ++i)
			{
				if (a[offset + i] != b[offset + i])
					return false;
			}

			return true;
		}
	}
}
