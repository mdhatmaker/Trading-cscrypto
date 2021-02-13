using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DemoCrypto
{
    public class Csv
    {
        private string filepath;
        private string headers;
        private string[] headerArr;
        private Dictionary<string, int> columnMap = new Dictionary<string, int>();
        private List<string> lines;

        public string Headers => headers;
        public string[] HeaderArr => headerArr;
        public int HeaderCount => headerArr.Length;
        public List<string> Lines => lines;
        public int LineCount => lines.Count;

        // If you want to reverse the order of the data (i.e. file has oldest data
        // first but you want newest data first), set reverseOrder flag to true.
        
        // CTOR: from CSV file
        public Csv(Uri uri, bool reverseOrder = false)
        {
            this.filepath = uri.LocalPath;
            var csvStr = File.ReadAllText(this.filepath);
            initialize(csvStr, reverseOrder);
        }

        // CTOR: from CSV string
        public Csv(string csvStr, bool reverseOrder = false)
        {
            initialize(csvStr, reverseOrder);
        }

        // Initialize the csv headers and lines of csv data
        private void initialize(string csvStr, bool reverseOrder)
        {
            var lines = csvStr.Split('\n');
            this.filepath = null;

            var headerStr = lines[0];
            parseHeaders(headerStr);

            if (reverseOrder)
                this.lines = lines.Skip(1).Reverse().ToList();
            else
                this.lines = lines.Skip(1).ToList();
        }

        // Given a csv string containing the column header names, initialize headers
        private void parseHeaders(string headerStr)
        {
            this.headers = headerStr;
            if (headerStr != null)
            {
                this.headerArr = headerStr.Split(',');
                for (int i = 0; i < headerArr.Length; ++i)
                {
                    columnMap[headerArr[i]] = i;
                }
            }
            else
            {
                this.headerArr = new string[0];
            }
        }

        /*public void ReadAll()
        {
            if (reverseOrder)
                this.lines = File.ReadAllLines(filepath).Skip(1).Reverse().ToList();
            else
                this.lines = File.ReadAllLines(filepath).Skip(1).ToList();
        }*/

        public string this[int i] => lines[i];

        public List<string> this[string headerName]
        {
            get
            {
                List<string> li = new List<string>();
                int ci = columnMap[headerName];
                for (int i = 0; i < LineCount; ++i)
                {
                    li.Add(lines[i].Split(',')[ci]);
                }
                return li;
            }
        }

        public List<T> ColumnData<T>(string headerName, bool reverseOrder = false) where T:IConvertible
        {
            var strData = this[headerName];
            List<T> rv = new List<T>();
            foreach (var sd in strData)
            {
                var d = (T)Convert.ChangeType(sd, typeof(T));
                rv.Add(d);
            }
            if (reverseOrder)
                rv.Reverse();
            return rv;
        }

        // where filepath like @"C:\Users\mhatm\Downloads\data\mydata.csv"
        public async Task SaveFile(string filepath)
        {
            using (var f = new StreamWriter(filepath))
            {
                await f.WriteAsync(this.Headers + "\n");
                for (int i = 0; i < this.Lines.Count; ++i)
                {
                    await f.WriteAsync(this.Lines[i]);
                    if (i < this.Lines.Count - 1)
                        await f.WriteAsync("\n");
                }
                f.Flush();
            }
        }



    } // class
} // namespace
