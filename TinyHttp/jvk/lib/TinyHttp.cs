using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace jvk.lib
{
    public class TinyHttp
    {
        public const string FMT_HTTP_OK = "HTTP/1.1 200 OK\r\nContent-Type: {0}\r\nConnection: close\r\n\r\n";

        private bool bStop = false;
        private st_RequestHandle[] arrReq;
        private TcpListener m_svr;
        private IHttpService m_service;
        private int m_port = 8888;

        public event EventHandler OnServerStart;
        public event EventHandler OnServerStop;

        public IHttpService Service { set { m_service = value; } }
        public int Port { get { return m_port; } }

        public void stop(ILog lg)
        {
            try
            {
                TcpListener svr = m_svr;
                bStop = true;
                if (null != svr)
                {
                    svr.Stop();
                }
            }
            catch (Exception ex)
            {
                lg.log("TinyHttp.Stop", ex);
            }
        }

        public bool runServerProc(ILog lg)
        {
            TcpListener srv = null;
            try
            {
                srv = new TcpListener(IPAddress.Any, m_port);
                m_svr = srv;
                srv.Start();

                runServerLoop(srv, lg);

                OnServerStop?.Invoke(this, null);
                return true;
            }
            catch (Exception ex)
            {
                lg.log("TinyHttp.Init", ex);
            }
            finally
            {
                try
                {
                    if (null != srv)
                    {
                        srv.Stop();
                    }
                }
                catch (Exception ex)
                {
                    lg.log("TinyHttp.Fin", ex);
                }
            }
            return false;
        }

        private void runServerLoop(TcpListener srv, ILog lg)
        {
            IHttpService serv = m_service;
            arrReq = new st_RequestHandle[1];
            arrReq[0].bufIn = new byte[st_RequestHandle.SZ_BUF_IN];
            arrReq[0].bufOut = new byte[st_RequestHandle.SZ_BUF_OUT];

            System.Threading.Thread.Sleep(300);

            OnServerStart?.Invoke(this, null);
            while (!bStop)
            {
                try
                {
                    TcpClient tcpConn = srv.AcceptTcpClient();
                    handleRequest(tcpConn, ref arrReq[0], lg, serv);
                }
                catch (Exception ex)
                {
                    lg.log("TinyHttp.Accept", ex);
                }
            }
        }

        private void handleRequest(TcpClient tc, ref st_RequestHandle req, ILog lg, IHttpService service)
        {
            NetworkStream ns = tc.GetStream();
            req.ns = ns;
            req.conn = tc;
            try
            {
                int nRead = 0;
                byte[] buf = req.bufIn;
                req.nRead = 0;
                try
                {
                    nRead = ns.Read(buf, 0, st_RequestHandle.SZ_BUF_IN);
                    req.nRead = nRead;
                }
                catch (Exception ex)
                {
                    lg.log("TinyHttp.Read", ex);
                    ns.Close();
                    tc.Close();
                    return;
                }

                object k = null;
                try
                {
                    k = service.OnDispatch(ref req);
                    ns.Flush();
                }
                catch (Exception ex)
                {
                    lg.log("TinyHttp.OnDispatch", ex);
                }

                try
                {
                    service.OnServe(ref req, k);
                    ns.Flush();
                }
                catch (Exception ex)
                {
                    lg.log("TinyHttp.OnServe", ex);
                }
            }
            finally
            {
                ns.Close();
                tc.Close();
            }
        }

    } // end - class TinyHttp

    public struct st_RequestHandle
    {
        public TcpClient conn;
        public NetworkStream ns;
        public int nRead;
        public byte[] bufIn;
        public byte[] bufOut;

        public const int SZ_BUF_IN = 4000;
        public const int SZ_BUF_OUT = 4000;
    } // end - struct st_Request

    public struct st_BufData
    {
        public int len;
        public byte[] buf;

        public static st_BufData fromString(string v)
        {
            byte[] b = Encoding.UTF8.GetBytes(v);
            st_BufData st;
            st.buf = b;
            st.len = b.Length;
            return st;
        }
    } // end - struct st_BufData

    public interface IHttpService
    {
        object OnDispatch(ref st_RequestHandle st);
        void OnServe(ref st_RequestHandle st, object objKey);
    }

}
