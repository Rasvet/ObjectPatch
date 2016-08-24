using System;
using System.IO;
using System.IO.Compression;

namespace ObjectPatch
{
	class Program
	{
		public enum Mode
		{
			Diff, Patch, Invalid
		};

		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				PrintUsage();
				return;
			}

			Mode mode = Mode.Invalid;
			switch (args[0])
			{
				case "diff": mode = Mode.Diff; break;
				case "patch": mode = Mode.Patch; break;
			}
			
			if (mode == Mode.Diff)
			{
				if (args.Length != 5)
				{
					PrintUsage();
					return;
				}

				int chunk_size = int.Parse(args[1]);
				string src1 = args[2];
				string src2 = args[3];
				string dst = args[4];

				byte[] ms1 = ReadFile(src1);
				byte[] ms2 = ReadFile(src2);

				ObjectPatch patch = ObjectPatchRuntime.GeneratePatch(ms1, ms2, chunk_size);
				byte[] patch_data = patch.Serialize();

				MemoryStream ms = new MemoryStream();
				GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
				compressedzipStream.Write(patch_data, 0, patch_data.Length);
				compressedzipStream.Close();

				FileStream fs = new FileStream(dst, FileMode.Create);
				byte[] data = ms.ToArray();
				fs.Write(data, 0, data.Length);
				fs.Close();
			}
			else if (mode == Mode.Patch)
			{
				if (args.Length != 4)
				{
					PrintUsage();
					return;
				}

				string src1 = args[1];
				string src2 = args[2];
				string dst = args[3];

				byte[] org = ReadFile(src1);
				byte[] zip_patch = ReadFile(src2);

				MemoryStream ms = new MemoryStream(zip_patch);
				MemoryStream ds = new MemoryStream();
				GZipStream zipStream = new GZipStream(ms, CompressionMode.Decompress);
				zipStream.CopyTo(ds);

				ObjectPatch op = ObjectPatch.Deserialize(ds.ToArray());

				byte[] mod = ObjectPatchRuntime.ApplyPatch(org, op);

				FileStream fs = new FileStream(dst, FileMode.Create);
				fs.Write(mod, 0, mod.Length);
				fs.Close();
			}
			else
			{
				PrintUsage();
			}			
		}

		public static void PrintUsage()
		{
			Console.WriteLine("Usage: {0} diff chunk_size $original $modified patch", System.Diagnostics.Process.GetCurrentProcess().ProcessName);
			Console.WriteLine("Usage: {0} patch $modified $patch $original", System.Diagnostics.Process.GetCurrentProcess().ProcessName);
		}

		public static byte[] ReadFile(string filename)
		{
			byte[] array;
			FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
			using (BinaryReader br = new BinaryReader(fs))
			{
				array = br.ReadBytes((int)fs.Length);
			}
			return array;
		}
	}
}
