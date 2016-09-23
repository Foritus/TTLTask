using AddressProcessing.CSV;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddressProcessing.Tests.CSV
{
    [TestFixture]
    public sealed class CSVReaderTests
    {
        [Test]
        public void SeparatorChars_Are_Correctly_Populated()
        {
            using (var reader = new CSVReader("\t", Stream.Null))
            {
                Assert.That(reader.SeparatorChars, Is.EquivalentTo(new char[] { '\t' }));
            }
        }

        [Test]
        public void Multi_Character_Separator_Is_Converted_To_Char_Array()
        {
            const string sep = "test";
            using (var reader = new CSVReader(sep, Stream.Null))
            {
                Assert.That(reader.SeparatorChars, Is.EquivalentTo(sep.ToCharArray()));
            }
        }
    }
}
