using Newtonsoft.Json;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAssembly
{
    public class MyClass
    {
        public OracleConnection Connection { get; set; }
        public static string ToJson(object data) => JsonConvert.SerializeObject(data);
    }
}
