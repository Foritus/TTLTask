using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AddressProcessing.CSV
{
    public sealed class CSVReader : IDisposable
    {
        public char[] SeparatorChars { get; }

        private bool _disposed;
        private readonly StreamReader _reader;
        private readonly string _separator;
        private readonly Stream _source;

        public CSVReader(string separator, Stream source)
        {
            if (separator == null) throw new ArgumentNullException(nameof(separator));
            if (source == null) throw new ArgumentNullException(nameof(source));

            _separator = separator;
            SeparatorChars = _separator.ToCharArray(); // Cache the separator char array once so that we don't allocate arrays all the time in read calls
            _source = source;
            _reader = new StreamReader(source);
        }

        public async Task<IEnumerable<string>> ReadColumnsAsync(uint maxColumnsToRead)
        {
            string line = await _reader.ReadLineAsync();

            return ReadColumnsFromLine(line, maxColumnsToRead);
        }

        /// <summary>
        /// Coroutine for reading column data without having to copy the values to a result array
        /// </summary>
        /// <param name="line">The line to extract columns from</param>
        /// <param name="maxColumnsToRead">The maximum number of columns to extract from the line</param>
        /// <returns>An enumeration of strings which represent each of the columns in the line that was read</returns>
        private IEnumerable<string> ReadColumnsFromLine(string line, uint maxColumnsToRead)
        {
            // Maintain backwards compatibility, null just means EOF
            if (line == null)
            {
                yield break;
            }

            // Note, that if we wanted to further minimise allocations we could do a manual search through the string and yield return substrings as we find them.
            // but that might be getting a bit too far into micro-optimisation.
            string[] columns = line.Split(SeparatorChars);

            uint max = (uint)Math.Min(columns.Length, maxColumnsToRead);
            for (int i = 0; i < max; ++i)
            {
                yield return columns[i];
            }
        }
        
        private void Dispose(bool disposeManaged)
        {
            if (!_disposed)
            {
                if (disposeManaged)
                {
                    _reader.Dispose();
                    _source.Dispose();
                }

                _disposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
