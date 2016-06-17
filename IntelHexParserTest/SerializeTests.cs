using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelHexParserTest
{
    [TestClass]
    public class SerializeTests
    {
        [TestMethod]
        public void TestDeserialize()
        {
            // from http://www.keil.com/support/docs/1584/
            // current deserialize ignores starting address of first block
            var serializer = new Coshx.IntelHexParser.Serializer();
            byte[] output = serializer.Deserialize(":10246200464C5549442050524F46494C4500464C33");
            byte[] expected = new byte[] { 0x46, 0x4c, 0x55, 0x49, 0x44, 0x20, 0x50, 0x52, 0x4F, 0x46, 0x49, 0x4C, 0x45, 0x00, 0x46, 0x4C };
            CollectionAssert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestSerialize()
        {
            var serializer = new Coshx.IntelHexParser.Serializer();
            String expected = ":10000000464C5549442050524F46494C4500464CB9\n:00000001FF";
            String output = serializer.Serialize(new byte[] { 0x46, 0x4c, 0x55, 0x49, 0x44, 0x20, 0x50, 0x52, 0x4F, 0x46, 0x49, 0x4C, 0x45, 0x00, 0x46, 0x4C });
            StringAssert.Equals(expected, output);
        }

        [TestMethod]
        public void TestChecksum()
        {
            var record = new Coshx.IntelHexParser.Record();
            record.Data = new byte[] { 0x46, 0x4c, 0x55, 0x49, 0x44, 0x20, 0x50, 0x52, 0x4F, 0x46, 0x49, 0x4C, 0x45, 0x00, 0x46, 0x4C };
            record.DataLength = record.Data.Length;
            if (BitConverter.IsLittleEndian)
                record.Address = 0x6224; // littleEndian
            else
                record.Address = 0x2463; // bigEndian
            record.Type = 00;
            var output = record.Checksum;
            var expected = 0x33;
            Assert.AreEqual(expected, output);
        }
    }
}
