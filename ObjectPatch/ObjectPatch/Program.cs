using System;
using System.IO;

namespace ObjectPatch
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 4)
			{
				Console.WriteLine("Usage: {0} diff $original $modified patch", System.Diagnostics.Process.GetCurrentProcess().ProcessName);
				Console.WriteLine("Usage: {0} patch $modified $patch original", System.Diagnostics.Process.GetCurrentProcess().ProcessName);
				return;
			}

			string src1 = args[1];
			string src2 = args[2];
			string dst = args[3];

			byte[] ms1 = ReadFile(src1);
			byte[] ms2 = ReadFile(src2);

			if (args[0] == "diff")
			{
				ObjectPatch patch = ObjectPatch.GeneratePatch(ms1, ms2);

				FileStream fs = new FileStream(dst, FileMode.Create);
				byte[] data = patch.Serialize();
				fs.Write(data, 0, data.Length);
			}
		}

		public static byte[] ReadFile(string filename)
		{
			byte[] array;
			FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
			using (BinaryReader br = new BinaryReader(fs))
			{
				array = br.ReadBytes(s.Length);
			}
			return array;
		}
	}
}
