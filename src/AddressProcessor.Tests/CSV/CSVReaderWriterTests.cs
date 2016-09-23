using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using AddressProcessing.CSV;
using System.Threading.Tasks;

namespace Csv.Tests
{
    [TestFixture]
    public sealed class CSVReaderWriterTests
    {
        // Note: I prefer to keep my unit tests as minimal as possible, so as to make diagnosing failures easier. This is just a personal style thing though.
        // So we have a lot of very small unit tests which look quite similar. I imagine with a little extra work I could reduce these to be data driven tests using 
        // delegates and a map of inputs to expected outputs, but that might be verging a bit too far into over-engineering territory.

        private const string FileRoot = @"test_data/";
        private const string ContactsFile = FileRoot + "contacts.csv";
        private const string EmptyFile = FileRoot + "empty.csv";
        private const string OneColumnFile = FileRoot + "one-column.csv";

        private void Harness(Action<CSVReaderWriter> callback)
        {
            using (var csv = new CSVReaderWriter())
            {
                callback(csv);
            }
        }

        private void HarnessFile(string fileName, CSVReaderWriter.Mode mode, Action<CSVReaderWriter> callback)
        {
            Harness(csv =>
            {
                csv.Open(fileName, mode);

                callback(csv);
            });
        }

        private void HarnessEmpty(Action<CSVReaderWriter> callback)
        {
            HarnessFile(EmptyFile, CSVReaderWriter.Mode.Read, csv =>
            {
                callback(csv);
            });
        }

        private void HarnessRead(Action<CSVReaderWriter> callback)
        {
            HarnessFile(ContactsFile, CSVReaderWriter.Mode.Read, csv =>
            {
                callback(csv);
            });
        }

        private void HarnessSingle(Action<CSVReaderWriter> callback)
        {
            HarnessFile(OneColumnFile, CSVReaderWriter.Mode.Read, csv =>
            {
                callback(csv);
            });
        }

        [Test]
        public void Constructor_Defaults_To_Tab_Seperator()
        {
            Harness(csv =>
            {
                // Ensure class defaults to tab separators so as to preserve compatibility
                Assert.That(csv.Separator, Is.EqualTo("\t"));
            });
        }

        [Test]
        public void Constructor_Throws_On_Null_Separator()
        {
            Assert.Throws<ArgumentException>(() => new CSVReaderWriter(null));
        }

        [Test]
        public void Constructor_Throws_On_Empty_Separator()
        {
            Assert.Throws<ArgumentException>(() => new CSVReaderWriter(string.Empty));
        }

        [Test]
        public void Open_Throws_On_Invalid_Mode()
        {
            Harness(csv =>
            {
                Assert.Throws<ArgumentException>(() => csv.Open("foo.csv", (CSVReaderWriter.Mode)0));
            });
        }

        [Test]
        public void Open_Throws_On_Flagged_Mode()
        {
            Harness(csv =>
            {
                Assert.Throws<ArgumentException>(() => csv.Open("foo.csv", CSVReaderWriter.Mode.Read | CSVReaderWriter.Mode.Write));
            });
        }

        [Test]
        public void Read_No_Out_Params_Return_True()
        {
            HarnessRead(csv =>
            {
                string col1 = null;
                string col2 = null;

                Assert.That(csv.Read(col1, col2), Is.True);
            });
        }

        [Test]
        public void Read_Empty_No_Out_Params_Returns_False()
        {
            HarnessEmpty(csv =>
            {
                string col1 = null;
                string col2 = null;

                Assert.That(csv.Read(col1, col2), Is.False);
            });
        }

        [Test]
        public void Read_One_Column_No_Out_Params_Throws()
        {
            HarnessSingle(csv =>
            {
                string col1 = null;
                string col2 = null;

                Assert.Throws<IndexOutOfRangeException>(() => csv.Read(col1, col2));
            });
        }

        [Test]
        public void Read_Out_Params_Populate_Params()
        {
            HarnessRead(csv =>
            {
                string col1, col2;

                Assert.That(csv.Read(out col1, out col2), Is.True);

                Assert.That(col1, Is.EqualTo("Shelby Macias"));
                Assert.That(col2, Is.EqualTo("3027 Lorem St.|Kokomo|Hertfordshire|L9T 3D5|England"));
            });
        }

        [Test]
        public void Read_Empty_Out_Params_Returns_False()
        {
            HarnessEmpty(csv =>
            {
                string col1 = null;
                string col2 = null;

                Assert.That(csv.Read(out col1, out col2), Is.False);
            });
        }

        [Test]
        public void Read_One_Column_Out_Params_Throws()
        {
            HarnessSingle(csv =>
            {
                string col1 = null;
                string col2 = null;

                Assert.Throws<IndexOutOfRangeException>(() => csv.Read(out col1, out col2));
            });
        }

        // Note, didn't add async overloads to the harness() functions as it was more code than just doing it manually for now.

        [Test]
        public async Task ReadAsync_Returns_Expected_Column_Count()
        {
            using (var csv = new CSVReaderWriter())
            {
                csv.Open(ContactsFile, CSVReaderWriter.Mode.Read);

                string[] result = (await csv.ReadAsync()).ToArray();

                Assert.That(result[0], Is.EqualTo("Shelby Macias"));
                Assert.That(result[1], Is.EqualTo("3027 Lorem St.|Kokomo|Hertfordshire|L9T 3D5|England"));
            }
        }

        [Test]
        public async Task ReadAsync_Empty_Returns_Empty_Enumeration()
        {
            using (var csv = new CSVReaderWriter())
            {
                csv.Open(EmptyFile, CSVReaderWriter.Mode.Read);

                Assert.That(await csv.ReadAsync(), Is.Empty);
            }
        }

        [Test]
        public async Task ReadAsync_One_Column_Returns_One_Column()
        {
            using (var csv = new CSVReaderWriter())
            {
                csv.Open(OneColumnFile, CSVReaderWriter.Mode.Read);

                var result = await csv.ReadAsync();

                Assert.That(result, Is.Not.Empty);
                Assert.That(result.First(), Is.EqualTo("hello"));
            }
        }
    }
}
