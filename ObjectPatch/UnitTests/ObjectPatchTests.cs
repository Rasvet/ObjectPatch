using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

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

			ObjectPatch.ObjectPatch op = ObjectPatch.ObjectPatch.GeneratePatch(b1, b2);

			Assert.IsTrue(op.GetPatchedLength() == b1.Length);

			byte[] res = ObjectPatch.ObjectPatch.ApplyPatch(b1, op);
			Assert.IsTrue(ArraysEqual(b2, res));
		}

		[TestMethod]
		public void TestDifferentObjects()
		{
			byte[] b1 = new DeltaTest(2048).Serialize();
			byte[] b2 = new DeltaTest(2048).Randomize(1000, 1100).Serialize();

			ObjectPatch.ObjectPatch op = ObjectPatch.ObjectPatch.GeneratePatch(b1, b2);

			Assert.IsTrue(op.GetPatchedLength() == b2.Length);

			byte[] res = ObjectPatch.ObjectPatch.ApplyPatch(b1, op);
			Assert.IsTrue(ArraysEqual(b2, res));
		}

		[TestMethod]
		public void TestDifferentObjects2()
		{
			byte[] b1 = new DeltaTest(4096).Serialize();
			byte[] b2 = new DeltaTest(4096).Randomize(1000, 1100).Randomize(2000, 2100).Serialize();

			ObjectPatch.ObjectPatch op = ObjectPatch.ObjectPatch.GeneratePatch(b1, b2);

			Assert.IsTrue(op.GetPatchedLength() == b2.Length);

			byte[] res = ObjectPatch.ObjectPatch.ApplyPatch(b1, op);
			Assert.IsTrue(ArraysEqual(b2, res));
		}

		[TestMethod]
		public void TestShorterMod()
		{
			byte[] b1 = new DeltaTest(80).Serialize();
			byte[] b2 = new DeltaTest(100).Serialize();

			ObjectPatch.ObjectPatch op = ObjectPatch.ObjectPatch.GeneratePatch(b1, b2);
			Assert.IsTrue(op.GetPatchedLength() == b2.Length);

			byte[] res = ObjectPatch.ObjectPatch.ApplyPatch(b1, op);
			Assert.IsTrue(ArraysEqual(b2, res));
		}

		[TestMethod]
		public void TestShorterModDifferent()
		{
			byte[] b1 = new DeltaTest(80).Serialize();
			byte[] b2 = new DeltaTest(100).Randomize(60, 80).Serialize();

			ObjectPatch.ObjectPatch op = ObjectPatch.ObjectPatch.GeneratePatch(b1, b2);
			Assert.IsTrue(op.GetPatchedLength() == b2.Length);

			byte[] res = ObjectPatch.ObjectPatch.ApplyPatch(b1, op);
			Assert.IsTrue(ArraysEqual(b2, res));
		}

		[TestMethod]
		public void TestLongerMod()
		{
			byte[] b1 = new DeltaTest(100).Serialize();
			byte[] b2 = new DeltaTest(120).Serialize();
			
			ObjectPatch.ObjectPatch op = ObjectPatch.ObjectPatch.GeneratePatch(b1, b2);
			Assert.IsTrue(op.GetPatchedLength() == b2.Length);

			byte[] res = ObjectPatch.ObjectPatch.ApplyPatch(b1, op);
			Assert.IsTrue(ArraysEqual(b2, res));
		}

		[TestMethod]
		public void TestLongerModDifferent()
		{
			byte[] b1 = new DeltaTest(100).Serialize();
			byte[] b2 = new DeltaTest(120).Randomize(50, 90).Serialize();

			ObjectPatch.ObjectPatch op = ObjectPatch.ObjectPatch.GeneratePatch(b1, b2);
			Assert.IsTrue(op.GetPatchedLength() == b2.Length);

			byte[] res = ObjectPatch.ObjectPatch.ApplyPatch(b1, op);
			Assert.IsTrue(ArraysEqual(b2, res));
		}

		public static bool ArraysEqual(byte[] a, byte[] b)
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
