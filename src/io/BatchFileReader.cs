﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace geheb.smart_backup.io
{
    sealed class BatchFileReader : IDisposable
    {
        readonly TextReader _reader;
        readonly List<string> _items = new List<string>();

        public BatchFileReader(string file)
        {
            _reader = new StreamReader(FileFactory.OpenAsFileStream(file));
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public async Task<IEnumerable<string>> Take(int count)
        {
            string line;
            _items.Clear();
            while (_items.Count < count && (line = await _reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                _items.Add(line);
            }
            return _items;
        }
    }
}
