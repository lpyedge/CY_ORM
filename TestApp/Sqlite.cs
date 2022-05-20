using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using CY_ORM;
using NUnit.Framework;

namespace TestApp
{
    public class Sqlite
    {

        public static ulong GetCount = 0;

        static readonly DbConnection Conn = new SQLiteConnection("Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "data.db;");




        [Test]
        public static void Test()
        {
            string tempid = DateTime.Now.Millisecond.ToString();
            Server servermodel1 = new Server { id = tempid + "1", name = "servername1", gameid = "us_wow", areaid = "1558719695" };
            Server servermodel2 = new Server { id = tempid + "2", name = "servername2", gameid = "us_wow", areaid = "1558719695" };

            List<string> gameidsList = new List<string>();
            gameidsList.Add("us_wow");
            gameidsList.Add("eu_wow");
            var slist = Conn.Select<Server>().Where(p => p.gameid.In(gameidsList.ToArray()) && p.name.IsNotNull()).ToList();
            var aaaa1 = Conn.Select<Server>().Where(p => p.gameid.Contains("wow")).ToList();


            Conn.Insert(servermodel1);
            Conn.Insert<Server>(p => p.id == (tempid + "2"), p => p.name == "servername2", p => p.gameid == servermodel2.gameid, p => p.areaid == servermodel2.areaid);

            servermodel1.name = "servername1" + "1";
            servermodel1.areaid = "1558719695" + servermodel1.areaid + string.Join("#", "32131", "4342");
            servermodel2.name = "servername3";

            Conn.Update<Server>(p => p.id == servermodel1.id, p => p.name == servermodel1.name, p => p.areaid == servermodel1.areaid);
            Conn.Update<Server>(servermodel2);

            servermodel1 =
                Conn.Select<Server>().Where(p => p.id == servermodel1.id && p.name == "servername11").ToSingle();
            servermodel2 =
                Conn.Select<Server>().Where(p => p.id == servermodel2.id | p.name == "servername2").ToSingle();

            Assert.AreEqual(servermodel1.areaid, "1558719695155871969532131#4342");
            Assert.AreEqual(servermodel2.name, servermodel2.name);




            var servermodelTemp =
                Conn.Select<Server>().Where(p => (p.id == servermodel1.id | p.name == "servername1111") && p.name == "us_wow").ToSingle();

            servermodelTemp =
                Conn.Select<Server>(p => p.name).ToSingle();

            var servermodelTemps1 = Conn.Select<Server>(p => p.name).OrderBy(p => p.scount.Order(OrderType.Asc), p => p.name.Order(OrderType.Desc)).ToList();

            var servermodelTemps2 = Conn.Select<Server>().OrderBy(p => p.id.Order(OrderType.Desc)).ToPageList(3, 5);

            Conn.Delete(servermodel1);
            Conn.Delete<Server>(p => p.id == servermodel2.id);

            Assert.AreEqual(Conn.Select<Server>().Where(p => p.id == servermodel1.id).ToSingle(), null);
            Assert.AreEqual(Conn.Select<Server>().Where(p => p.id == servermodel2.id).ToSingle(), null);
        }
    }
}
