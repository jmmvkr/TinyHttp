using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace jvk.lib
{
    public class EchoService : IHttpService
    {
        private st_BufData buf_http_ok = st_BufData.fromString(String.Format(TinyHttp.FMT_HTTP_OK, "text/plain"));

        public object OnDispatch(ref st_RequestHandle st)
        {
            return "";
        }

        public void OnServe(ref st_RequestHandle st, object objKey)
        {
            System.IO.Stream ns = st.ns;
            string s = Encoding.UTF8.GetString(st.bufIn, 0, st.nRead);
            using (StreamWriter wr = new StreamWriter(ns))
            {
                ns.Write(buf_http_ok.buf, 0, buf_http_ok.len);
                ns.Flush();

                wr.WriteLine("## echo Server ##");
                wr.WriteLine();
                wr.WriteLine(s);
                wr.Flush();
            }
        }

    } // end - class EchoService
}
