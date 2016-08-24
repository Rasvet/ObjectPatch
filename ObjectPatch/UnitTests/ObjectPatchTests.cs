using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnitTests
{
	[Serializable]
	class DeltaTest
	{
		public int[] data;

		public DeltaTest(int size)
		{
			data = new int[size];
			for (int i = 0; i < size; ++i)
				data[i] = 0;
		}

		public DeltaTest Randomize(int start, int end)
		{
			Random rng = new Random(42);
			for (int i = start; i < end; ++i)
				data[i] = rng.Next();
			return this;
		}

		public byte[] Serialize()
		{
			MemoryStream stream = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(stream, this);
			return stream.ToArray();
		}
	}

	[TestClass]
	public class ObjectPatchUnitTests
	{
		[TestMethod]
		public void TestIdenticalObjects()
		{
			byte[] b1 = new DeltaTest(100).Serialize();
			byte[] b2 = new DeltaTest(100).Serialize();

			Assert.IsTrue(GeneratePatchAndApply(b1, b2));			
		}

		[TestMethod]
		public void TestDifferentObjects()
		{
			byte[] b1 = new DeltaTest(2048).Serialize();
			byte[] b2 = new DeltaTest(2048).Randomize(1000, 1100).Serialize();

			Assert.IsTrue(GeneratePatchAndApply(b1, b2));
		}

		[TestMethod]
		public void TestDifferentObjects2()
		{
			byte[] b1 = new DeltaTest(4096).Serialize();
			byte[] b2 = new DeltaTest(4096).Randomize(1000, 1100).Randomize(2000, 2100).Serialize();

			Assert.IsTrue(GeneratePatchAndApply(b1, b2));
		}

		[TestMethod]
		public void TestShorterMod()
		{
			byte[] b1 = new DeltaTest(120).Serialize();
			byte[] b2 = new DeltaTest(100).Serialize();

			Assert.IsTrue(GeneratePatchAndApply(b1, b2));
		}

		[TestMethod]
		public void TestShorterModDifferent()
		{
			byte[] b1 = new DeltaTest(120).Serialize();
			byte[] b2 = new DeltaTest(100).Randomize(60, 80).Serialize();

			Assert.IsTrue(GeneratePatchAndApply(b1, b2));
		}

		[TestMethod]
		public void TestLongerMod()
		{
			byte[] b1 = new DeltaTest(100).Serialize();
			byte[] b2 = new DeltaTest(120).Serialize();

			Assert.IsTrue(GeneratePatchAndApply(b1, b2));
		}

		[TestMethod]
		public void TestLongerModDifferent()
		{
			byte[] b1 = new DeltaTest(100).Serialize();
			byte[] b2 = new DeltaTest(120).Randomize(50, 90).Serialize();

			Assert.IsTrue(GeneratePatchAndApply(b1, b2));
		}

		private bool GeneratePatchAndApply(byte[] a, byte[] b)
		{
			int[] chunk_sizes = { 4, 8, 16, 32, 64, 128, 256, 512 };
			for (int i = 0; i < chunk_sizes.Length; ++i)
			{
				ObjectPatch.ObjectPatch op = ObjectPatch.ObjectPatchRuntime.GeneratePatch(a, b, chunk_sizes[i]);

				Assert.IsTrue(op.patchedLength == b.Length);

				byte[] result = ObjectPatch.ObjectPatchRuntime.ApplyPatch(a, op);
				if (!ArraysAreEqual(b, result))
				{
					Console.WriteLine("Failed to correctly patch with chunk size {0}", chunk_sizes[i]);
					return false;
				}
			}
			return true;
		}

		private bool GeneratePatchAndApply(byte[] a, byte[] b, int chunk_size)
		{
			ObjectPatch.ObjectPatch op = ObjectPatch.ObjectPatchRuntime.GeneratePatch(a, b, chunk_size);

			Assert.IsTrue(op.patchedLength == b.Length);

			byte[] result = ObjectPatch.ObjectPatchRuntime.ApplyPatch(a, op);
			return ArraysAreEqual(b, result);
		}

		public static bool ArraysAreEqual(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; ++i)
			{
				if (a[i] != b[i])
					return false;
			}

			return true;
		}
	}
}
