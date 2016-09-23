using System;
using System.IO;
using System.Threading.Tasks;

namespace AddressProcessing.CSV
{
    public sealed class CSVWriter : IDisposable
    {
        private bool _disposeCalled = false;
        private readonly string _separator;
        private readonly Stream _targetStream;
        private readonly StreamWriter _writer;

        public CSVWriter(string separator, Stream targetStream)
        {
            if (separator == null) throw new ArgumentNullException(nameof(separator));
            if (targetStream == null) throw new ArgumentNullException(nameof(targetStream));

            _separator = separator;
            _targetStream = targetStream;
            _writer = new StreamWriter(_targetStream);
        }
        
        private void Dispose(bool disposeManaged)
        {
            if (!_disposeCalled)
            {
                if (disposeManaged)
                {
                    _writer.Close();
                    _writer.Dispose();
                    _targetStream.Dispose();
                }

                _disposeCalled = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        public async Task WriteAsync(params string[] columns)
        {
            string line = string.Join(_separator, columns);

            await _writer.WriteLineAsync(line);
        }
    }
}
