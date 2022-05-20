using System;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using CY_ORM;
using System.ComponentModel;

namespace TestApp
{
    public class Server
    {
        //[Key(Assigned = true)]
        public string id
        {
            get;
            set;
        }

        public string name
        {
            get;
            set;
        }

        //[Key(Assigned = true)]
        [ForeignColumn("game", ColumnName = "id", IsKey = true)]
        [ForeignColumn("serverarea", ColumnName = "gameid", IsKey = true)]
        public string gameid
        {
            get;
            set;
        }

        [ForeignColumn("game")]
        public string gamename
        {
            get;
            set;
        }

        [ForeignColumn("serverarea", ColumnName = "id", IsKey = true)]
        public string areaid
        {
            get;
            set;
        }

        [ForeignColumn("serverarea", ColumnName = "areaname")]
        public string area_name
        {
            get;
            set;
        }

        public bool isshow
        {
            get;
            set;
        }

        public int clicknum
        {
            get;
            set;
        }
        public ServerType type
        {
            get;
            set;
        }

        public DateTime adddate
        {
            get;
            set;
        }


        public string sname
        {
            get
            {
                SqlServer.GetCount++; return "";
            }
        }
        public ulong scount
        {
            get
            {
                return SqlServer.GetCount; ;
            }
        }
    }

    public class Game
    {
        [Description("游戏ID")]
        public string gameid { get; set; }
        [Description("游戏名称")]
        public string gamename { get; set; }
        [Description("游戏名称简写")]
        public string gamename_short { get; set; }
        public int sort { get; set; }

        public string info { get; set; }
        public bool ishot { get; set; }
        public string imageurl { get; set; }

    }

    public enum ServerType
    {
        A, B, C
    }

    /*	
    DapperEx
    305-83
    DapperExtensions
    350-88
    Dapper
    280-81
    初次执行和连续执行3次的平均值
    */

    class Program
    {
        //private const string connstr = @"Server=(LocalDB)\MSSQLLocalDB; Integrated Security=true ;AttachDbFileName=F:\我的坚果云\Code\CY_ORM\TestDB.mdf";
        private const string connstr = @"Data Source=.;Initial Catalog=TestDB;uid=sa;pwd=ok;";

        static void Main(string[] args)
        {
            var date = DateTime.Parse("2016-03-30");
            using (var conn = new System.Data.SqlClient.SqlConnection(connstr))
            {
                var list = 
                    conn.Query<string>("select distinct [name] as name from [server]  where adddate > @start and adddate < @end",
                        new { start = date.ToString("yyyy-MM-dd HH:mm:ss"), end = date.AddDays(11).ToString("yyyy-MM-dd HH:mm:ss") }).ToList();
            }

            //ExpandoObjectTest.Test();

            //SqlServer.Test();
            //Sqlite.Test();

            //var type = typeof (SqlConnection);
            //var fun= CY_ORM.DbConnectionContext.CreateFunc(type);

            //CY_ORM.DbConnectionContext.Set<SqlConnection>(connstr, "default");
            //CY_ORM.DbConnectionContext.Set<SqlConnection>(connstr);
            //const int z = 1000;
            //Stopwatch sw1 = new Stopwatch(), sw2 = new Stopwatch(), sw3 = new Stopwatch();
            //var conn1 = new SqlConnection(connstr);
            //var conn2 = CY_ORM.DbConnectionContext.Instance;

            //sw1.Start();
            //for (int i = 0; i < z; i++)
            //{
            //    var conn = new SqlConnection(connstr);
            //}
            //sw1.Stop();

            //sw2.Start();
            //for (int i = 0; i < z; i++)
            //{
            //    var conn = CY_ORM.DbConnectionContext.Instance;
            //}
            //sw2.Stop();
            //sw3.Start();
            //for (int i = 0; i < z; i++)
            //{
            //    var conn = fun();
            //    conn.ConnectionString = connstr;
            //}
            //sw3.Stop();

            //SqlServer.Test();

            Server a = new Server() { adddate = DateTime.UtcNow, id = "aa", clicknum = 1, areaid = "1", gameid = "1", isshow = true, name = "haha", type = ServerType.B };

            //using (var Conn = new SqlConnection(connstr))
            //{
            //    Conn.Delete<Server>(a);
            //    Conn.Open();
            //    using (var tran = Conn.BeginTransaction())
            //    {
            //        var id= tran.Insert(a);
            //        var b = 1;
            //    }
            //}
            using (var Conn = new SqlConnection(connstr))
            {
                var where = Predicate.Generate<Server>(p => p.isshow == true || (p.id==""&& p.name == "")) ;
                //where = where.OrBracket<Server>(p => p.name == "");
                var servers = Conn.Select<Server>().Where(where).ToList();

                Conn.Delete<Server>(a);

                var a1=Conn.Insert(a);
                var gameid1= "us_wow";

                Conn.Update<Server>(p => p.id.InSql("select gameid from server where gameid = '"+ gameid1+"'"), p => p.adddate == DateTime.UtcNow);

                var serverlist = Conn.Select<Server>().Where(null).OrderBy(p => p.id.Order(OrderType.Desc), p => p.clicknum.Order(OrderType.Asc)).ToPageList(1, 100);
                var list = Conn.Select<Server>().ToList();
                var ids = list.Select(p => p.id);

              
            }

            Server servermodel = new Server();
            servermodel.id = "id" + DateTime.Now.Ticks.ToString();
            servermodel.name = "name";
            servermodel.gameid = "gameid";
            servermodel.areaid = "areaid";
            servermodel.isshow = true;
            servermodel.clicknum = 100;
            servermodel.adddate = DateTime.Now;


            const int Times = 1000;


            Stopwatch sw = new Stopwatch();


            using (var Conn = new SqlConnection(connstr))
            {
                var list =
                    Conn.Select<Server>(p => p.name)
                        .Where(p => p.gameid == "wow_us" && p.name.StartsWith("a"))
                        .OrderBy(p => p.name.Order(OrderType.Asc))
                        .GroupBy(p => p.name)
                        .ToPageList(1, 1);

            }

            using (var Conn = new SqlConnection(connstr))
            {
                Conn.Delete<Server>(servermodel);

                var a1 = Conn.Insert<Server>(servermodel);

                Conn.Update<Server>(servermodel);
                Conn.Update<Server>(p => p.name.StartsWith("a"), p => p.name == p.name + "aaaaa", p => p.areaid == "5453453");


                Conn.Select<Server>().Debug().Where(p => p.isshow == false).ToSingle();
            }


            sw.Restart();
            for (int i = 0; i < Times; i++)
            {
                using (var Conn = new SqlConnection(connstr))
                {
                    Conn.Select<Server>()
                        .Where(p => p.gameid == "wow_us" && p.name.StartsWith("a"))
                        .OrderBy(p => p.name.Order(OrderType.Desc), p => p.gameid.Order(OrderType.Asc))
                        .ToList();
                }
            }
            sw.Stop();
            Console.WriteLine("CY_ORM:" + sw.ElapsedMilliseconds.ToString());

            sw.Restart();
            for (int i = 0; i < Times; i++)
            {
                using (var Conn = new SqlConnection(connstr))
                {
                    Conn.Select<Server>().Where(p => p.gameid == "wow_us" && p.name.StartsWith("a")).ToList();
                }
            }
            sw.Stop();
            Console.WriteLine("CY_ORM:" + sw.ElapsedMilliseconds.ToString());
            sw.Restart();
            for (int i = 0; i < Times; i++)
            {
                using (var Conn = new SqlConnection(connstr))
                {
                    Conn.Select<Server>().Where(p => p.gameid == "wow_us" && p.name.StartsWith("a")).ToList();
                }
            }
            sw.Stop();
            Console.WriteLine("CY_ORM:" + sw.ElapsedMilliseconds.ToString());
            sw.Restart();
            for (int i = 0; i < Times; i++)
            {
                using (var Conn = new SqlConnection(connstr))
                {
                    Conn.Select<Server>().ToList();
                }
            }
            sw.Stop();
            Console.WriteLine("CY_ORM:" + sw.ElapsedMilliseconds.ToString());


            sw.Restart();
            for (int i = 0; i < Times; i++)
            {
                using (var Conn = new SqlConnection(connstr))
                {
                    Dapper.SqlMapper.Query<Server>(Conn, "select * from [server] where gameid=@gameid and name like @name", new { gameid = "wow_us", name = "a%" }).ToList();
                }
            }
            sw.Stop();
            Console.WriteLine("Dapper:" + sw.ElapsedMilliseconds.ToString());

            sw.Restart();
            for (int i = 0; i < Times; i++)
            {
                using (var Conn = new SqlConnection(connstr))
                {
                    Dapper.SqlMapper.Query<Server>(Conn, "select * from [server] where gameid=@gameid and name like @name",
                        new { gameid = "wow_us", name = "a%" }).ToList();
                }
            }
            sw.Stop();
            Console.WriteLine("Dapper:" + sw.ElapsedMilliseconds.ToString());
            sw.Restart();
            for (int i = 0; i < Times; i++)
            {
                using (var Conn = new SqlConnection(connstr))
                {
                    Dapper.SqlMapper.Query<Server>(Conn, "select * from [server] where gameid=@gameid and name like @name",
                        new { gameid = "wow_us", name = "a%" }).ToList();
                }
            }
            sw.Stop();
            Console.WriteLine("Dapper:" + sw.ElapsedMilliseconds.ToString());
            sw.Restart();
            for (int i = 0; i < Times; i++)
            {
                using (var Conn = new SqlConnection(connstr))
                {
                    Dapper.SqlMapper.Query<Server>(Conn, "select * from [server] where gameid=@gameid and name like @name",
                        new { gameid = "wow_us", name = "a%" }).ToList();
                }
            }
            sw.Stop();
            Console.WriteLine("Dapper:" + sw.ElapsedMilliseconds.ToString());

            Console.ReadKey();
        }
    }

}
