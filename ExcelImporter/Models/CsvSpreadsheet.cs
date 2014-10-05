using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ExcelImporter.Extensions;
using Kent.Boogaart.KBCsv;
using System.Threading.Tasks;
using System.Globalization;

namespace ExcelImporter.Models
{
    public class CsvSpreadsheet : ISpreadsheet
    {
        string _fn;
        FileStream _fs;
        CsvReader _reader;
        IEnumerable<string> _current;

        public CsvSpreadsheet(string fn)
        {
            _fn = fn;
            _fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true);
            _reader = new CsvReader(_fs);
        }

        public async Task<IEnumerable<string>> GetHeaderRow()
        {
            var record = await _reader.ReadHeaderRecordAsync();

            if (record != null)
            {
                if (record.Count == 1)
                { // may be semicolon is used as separator
                    _reader.ValueSeparator = ';';
                    return record[0].Split(';');
                }
                else
                {
                    return record.Select(it => it);
                }
            }

            return null;
        }

        public async Task<IEnumerable<string>> GetNextRow()
        {
            var record = await _reader.ReadDataRecordAsync();

            if (record != null)
            {
                if (record.Count == 1)
                { // may be semicolon is used as separator
                    _reader.ValueSeparator = ';';
                    _current = record[0].Split(';');
                }
                else
                {
                    _current = record.Select(it => it);
                }

                return _current;
            }
            return null;
        }

        public string ConvertCell(int col, Type type = null)
        {
            if (_current == null)
                return null;

            if (type == typeof(DateTime))
            {
                var dt = default(DateTime);
                if (!DateTime.TryParse(_current.ElementAt(col), out dt))
                    DateTime.TryParse(_current.ElementAt(col), CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                return dt.ToString();
            }

            return _current.ElementAt(col);
        }

        public string FileName
        {
            get
            {
                return _fn;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader.Dispose();
                _fs.Dispose();
            }
        }
    }
}