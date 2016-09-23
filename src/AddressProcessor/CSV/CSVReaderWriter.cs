using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AddressProcessing.CSV
{
    /*
        2) Refactor this class into clean, elegant, rock-solid & well performing code, without over-engineering.
           Assume this code is in production and backwards compatibility must be maintained.
    */

    /// <summary>
    /// I'm assuming we need to maintain source-level back compat for this exercise, so that consumers don't take any compiler errors when updating to this library.
    /// This constrains how tidy I can make the public interface, but I'll move the internals around.
    /// </summary>
    public class CSVReaderWriter : IDisposable
    {
        [Flags]
        public enum Mode
        {
            Read = 1,
            Write = 2
        };

        public string Separator { get; }

        private bool _disposeCalled;
        private CSVReader _reader;
        private CSVWriter _writer;

        /// <summary>
        /// Constructor with default argument to preserve back compat. This won't preserve back compat if someone just copies this DLL into an application that used the previous version,
        /// but such hybrid mish-mash deployments are rarely a good idea anyway.
        /// </summary>
        /// <param name="separator">Separator character used for delineating columns in the data file</param>
        public CSVReaderWriter(string separator = "\t")
        {
            if (string.IsNullOrEmpty(separator)) throw new ArgumentException($"{nameof(separator)} is null or empty");

            Separator = separator;
        }

        /// <summary>
        /// Opens the specified file in the specified mode, multiple calls are required if you wish to read and write to the same file.
        /// Method behaviour is preserved for backcompat purposes.
        /// </summary>
        /// <param name="fileName">The file to open</param>
        /// <param name="mode">The mode to open the file in (e.g. reading or writing)</param>
        public void Open(string fileName, Mode mode)
        {
            // As the "Mode" enum is marked as [Flags], callers could use it in a bitfield fashion. But because this is production code, changing this behaviour to
            // something like if ((mode & Mode.Read) == Mode.Read) { } if ((mode & Mode.Write) == Mode.Write) { } to allow for opening a reader and writer in one call would
            // be a breaking change.
            if (mode == Mode.Read)
            {
                _reader = new CSVReader(Separator, File.OpenRead(fileName));
            }
            else if (mode == Mode.Write)
            {
                _writer = new CSVWriter(Separator, File.OpenWrite(fileName));
            }
            else
            {
                throw new ArgumentException($"Unknown file mode {(int)mode} specified for " + fileName);
            }
        }

        /// <summary>
        /// Reads the next line and returns true if the next line contained more than zero columns
        /// </summary>
        /// <param name="column1">Parameter is not used, maintained for back compat.</param>
        /// <param name="column2">Parameter is not used, maintained for back compat.</param>
        /// <returns><c>true</c> if the next line contained more than zero columns, otherwise false</returns>
        public bool Read(string column1, string column2)
        {
            // As these are not "out" parameters, we can safely pass the references to the overload and not have to worry about breaking compat
            return Read(out column1, out column2);
        }

        /// <summary>
        /// Reads the next line in the file currently open for reading and sets column1 and column2 to their respecive columns in the line.
        /// If the line has zero columns, column1 and column2 are set to null. If the line has 1 column, IndexOutOfRangeException is thrown.
        /// </summary>
        /// <param name="column1">Parameter the first column in the line will be written to</param>
        /// <param name="column2">Parameter the second column in the line will be written to</param>
        /// <returns><c>true</c> if two columns were read from the next line, <c>false</c> otherwise</returns>
        public bool Read(out string column1, out string column2)
        {
            // Configure task to continue on a different execution context, this avoids deadlocks and reduces memory usage a bit.
            string[] result = ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToArray();

            if (result.Length == 0)
            {
                column1 = null;
                column2 = null;
                return false;
            }

            // Need to preserve IndexOutOfRangeException behaviour for back compat.
            column1 = result[0];
            column2 = result[1];

            return true;
        }

        /// <summary>
        /// Reads the next line asynchronously and returns an enumeration which contains all columns found on that line
        /// </summary>
        /// <returns>An enumerator which reads up to maxColumnsToRead columns from the next line</returns>
        public async Task<IEnumerable<string>> ReadAsync(uint maxColumnsToRead = 2)
        {
            return await _reader.ReadColumnsAsync(maxColumnsToRead);
        }

        /// <summary>
        /// Synchronous overload of WriteAsync(), writes the specified columns to the end of the currently Open()'ed file
        /// </summary>
        /// <param name="columns">String values representing the value of one column per string</param>
        public void Write(params string[] columns)
        {
            WriteAsync(columns).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Writes the specified columns to the end of the currently Open()'ed file
        /// </summary>
        /// <param name="columns"></param>
        /// <returns>A task that will complete on completion of the underlying I/O operation</returns>
        public async Task WriteAsync(params string[] columns)
        {
            await _writer.WriteAsync(columns);
        }

        /// <summary>
        /// Closes any streams that are currently opened by this class
        /// </summary>
        public void Close()
        {
            // idea, perhaps this should just call Dispose() instead of duplicating effort?
            _writer?.Dispose();
            _reader?.Dispose();
        }

        /// <summary>
        /// Protected Dispose implementation that child implementations can override, allowing the base type to simply implement the IDisposable interface
        /// and use a virtual method call to resolve the right Dispose logic
        /// </summary>
        /// <param name="disposeManaged">Whether to dispose managed objects</param>
        protected virtual void Dispose(bool disposeManaged)
        {
            if (!_disposeCalled)
            {
                if (disposeManaged)
                {
                    _reader?.Dispose();
                    _writer?.Dispose();

                    _writer = null;
                    _reader = null;
                }

                _disposeCalled = true;
            }
        }

        /// <summary>
        /// Releases all resources allocated to this object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
