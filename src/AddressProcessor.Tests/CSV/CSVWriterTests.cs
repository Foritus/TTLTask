using AddressProcessing.CSV;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AddressProcessing.Tests.CSV
{
    [TestFixture]
    public sealed class CSVWriterTests
    {
        // Note, I would have added more tests here but running out of time for the exercise.

        [Test]
        public async Task WriteLine_Correctly_Formats_Columns()
        {
            const string sep = "\t";
            const string col1 = "1";
            const string col2 = "2";
            const string col3 = "3";
            const string col4 = "4";

            string expected = string.Join(sep, col1, col2, col3, col4) + Environment.NewLine;

            using (var ms = new MemoryStream())
            {
                using (var writer = new CSVWriter(sep, ms))
                {
                    await writer.WriteAsync(col1, col2, col3, col4);
                }

                Assert.That(Encoding.UTF8.GetString(ms.ToArray()), Is.EqualTo(expected));
            }
        }
    }
}
