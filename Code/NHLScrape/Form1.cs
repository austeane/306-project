using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Timers;


namespace NHLScrape
{

    public partial class Form1 : System.Windows.Forms.Form
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //This simulates a left mouse click
        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        public Form1()
        {
            InitializeComponent();
            InitBrowser();
        }

        public static ChromiumWebBrowser browser;
        private static int nCurrentGame;
        private static List<string> games;
        private static Point pt;

        public void InitBrowser()
        {
            Cef.Initialize(new CefSettings());
            browser = new ChromiumWebBrowser("www.google.com");
            panel1.Controls.Add(browser);
            //browser.Dock = DockStyle.Fill;
        }

        private void writeToFile(List<List<string>> list, string strFile)
        {
            int i, k;
            string strLine;
            // Write a string list to a file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(strFile))
            {
                for (i = 0; i < list.Count; i++)
                {
                    strLine = "";
                    for (k = 0; k < list[i].Count; k++)
                    {
                        if (k > 0)
                        {
                            strLine += "\t";
                        }
                        strLine += list[i][k];
                    }
                    file.WriteLine(strLine);
                }
            }
        }


        private void appendToFile(List<string> input, string strFile)
        {
            string strLine;
            int k;

            // Write a string list to a file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(strFile, true))
            {
                strLine = "";
                for (k = 0; k < input.Count; k++)
                {
                    if (k > 0)
                    {
                        strLine += "\t";
                    }
                    strLine += input[k];
                }
                file.WriteLine(strLine);

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<List<string>> list = new List<List<string>>();
            List<string> item;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlDocument docPlayer = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode root;
            HtmlAgilityPack.HtmlNode pages;
            int nPage;
            int nPages;
            int i = 0;

            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (c == 'X')
                {
                    continue;
                }

                nPage = 1;
                nPages = 100;

                do
                {
                    string strURL = "http://www.nhl.com/ice/playersearch.htm?letter=" + c.ToString() + "&pg=" + nPage.ToString();
                    try
                    {
                        doc.LoadHtml(new System.Net.WebClient().DownloadString(strURL));
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        continue;
                    }

                    root = doc.DocumentNode;
                    pages = root.Descendants().Where(n => n.GetAttributeValue("class", "").Equals("resultCount")).Single();
                    int nStart = pages.InnerText.IndexOf("of ") + 3;
                    int nStop = pages.InnerText.LastIndexOf(" ");
                    string strPages = pages.InnerText.Substring(nStart, nStop - nStart);
                    nPages = Int32.Parse(strPages);
                    nPages = (nPages / 50) + (nPages % 50 == 0 ? 0 : 1);

                    pages = root.Descendants().Where(n => n.GetAttributeValue("class", "").Equals("data playerSearch")).Single();
                    var trs = pages.Descendants("tr");

                    foreach (HtmlAgilityPack.HtmlNode tr in trs)
                    {
                        item = new List<string>();

                        // grab the tds
                        var tds = tr.Descendants("td");

                        if (tds.Count() == 0)
                        {
                            continue;
                        }

                        List<HtmlAgilityPack.HtmlNode> listTD = tds.ToList();

                        for (i = 0; i < 4 && i < listTD.Count; i++)
                        {
                            item.Add(listTD[i].InnerText.Trim());
                        }

                        HtmlAgilityPack.HtmlNode aPlayer = listTD[0].Descendants("a").Single();
                        strURL = "http://www.nhl.com" + aPlayer.Attributes["href"].Value;

                        bool bLoaded = false;

                        while (!bLoaded)
                        {
                            try
                            {
                                docPlayer.LoadHtml(new System.Net.WebClient().DownloadString(strURL));
                                bLoaded = true;
                            }
                            catch (Exception ex)
                            {
                                ex.ToString();
                                continue;
                            }
                        }

                        List<HtmlAgilityPack.HtmlNode> listPlayerBio = docPlayer.DocumentNode.Descendants().Where(n => n.GetAttributeValue("class", "").Equals("bioInfo")).Single().Descendants("tr").ToList()[0].Descendants("td").ToList();

                        if (listPlayerBio.Count >= 2)
                        {
                            item.Add(listPlayerBio[1].InnerText.Trim());
                        }

                        System.Diagnostics.Debug.WriteLine(item[0].ToString());

                        list.Add(item);
                    }

                    nPage++;

                } while (nPage <= nPages);

                
            }

            writeToFile(list, @"output.txt");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<string> teams = new List<string>();
            teams.Add("anaheim-ducks");
            teams.Add("arizona-coyotes");
            teams.Add("calgary-flames");
            teams.Add("edmonton-oilers");
            teams.Add("los-angeles-kings");
            teams.Add("san-jose-sharks");
            teams.Add("vancouver-canucks");
            teams.Add("chicago-blackhawks");
            teams.Add("colorado-avalanche");
            teams.Add("dallas-stars");
            teams.Add("minnesota-wild");
            teams.Add("nashville-predators");
            teams.Add("st-louis-blues");
            teams.Add("winnipeg-jets");
            teams.Add("boston-bruins");
            teams.Add("buffalo-sabres");
            teams.Add("detroit-red-wings");
            teams.Add("florida-panthers");
            teams.Add("montreal-canadiens");
            teams.Add("ottawa-senators");
            teams.Add("tampa-bay-lightning");
            teams.Add("toronto-maple-leafs");
            teams.Add("carolina-hurricanes");
            teams.Add("columbus-blue-jackets");
            teams.Add("new-jersey-devils");
            teams.Add("new-york-islanders");
            teams.Add("new-york-rangers");
            teams.Add("philadelphia-flyers");
            teams.Add("pittsburgh-penguins");
            teams.Add("washington-capitals");

            int i, k, x;
            bool bLoaded;
            string strURL;

            List<List<string>> output = new List<List<string>>();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            for (i = 0; i < teams.Count; i++)
            {
                strURL = "http://www.spotrac.com/nhl/"+teams[i]+"/yearly/cap/";

                // Just loop forever, who cares about error checking!
                bLoaded = false;
                do
                {
                    try
                    {
                        doc.LoadHtml(new System.Net.WebClient().DownloadString(strURL));
                        bLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        continue;
                    }
                } while (bLoaded == false);

                System.Diagnostics.Debug.WriteLine(teams[i]);

                List<HtmlAgilityPack.HtmlNode> listTables = doc.DocumentNode.Descendants().Where(n => n.GetAttributeValue("class", "").Equals("teams")).Single().Descendants("table").ToList();

                for(x = 0; x < listTables.Count; x++)
                {

                    string strType = "";

                    switch (x)
                    {
                        case 0:
                            strType = "Normal";
                            break;
                        case 1:
                            strType = "Injured Reserve Cap Hits";
                            break;
                        case 2:
                            strType = "Long-Term Injured Reserve Cap Hits";
                            break;
                        case 3:
                            strType = "Buyout Cap Hits";
                            break;
                        case 4:
                            strType = "Active Non-Roster Money";
                            break;
                        default:
                            strType = "Other";
                            break;
                    }

                    List<HtmlAgilityPack.HtmlNode> trs = listTables[x].Descendants("tr").ToList();

                    for (k = 0; k < trs.Count; k++)
                    {
                        List<HtmlAgilityPack.HtmlNode> tds = trs[k].Descendants("td").ToList();
                        List<string> item = new List<string>();

                        if (tds.Count >= 3)
                        {
                            List<HtmlAgilityPack.HtmlNode> listHidden = tds[2].Descendants().Where(n => n.GetAttributeValue("style", "").Equals("display:none")).ToList();

                            foreach (HtmlAgilityPack.HtmlNode nr in listHidden)
                            {
                                nr.Remove();
                            }

                            item.Add(teams[i]);
                            item.Add(tds[0].Descendants("a").Single().InnerText.Trim());
                            item.Add(tds[1].InnerText.Trim());
                            item.Add(tds[2].InnerText.Trim());
                            item.Add(strType);
                            output.Add(item);
                        }
                    }
                }
            }

            writeToFile(output, @"salarycaps.txt");
        }



        //eg: http://www.hockey-reference.com/friv/numbers.cgi?number=90&year=2016
        private void button3_Click(object sender, EventArgs e)
        {
            List<List<string>> list = new List<List<string>>();
            List<string> item;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode root;
            List<HtmlAgilityPack.HtmlNode> tables;
            List<HtmlAgilityPack.HtmlNode> trs;
            List<HtmlAgilityPack.HtmlNode> tds;

            
            int i = 0, x;

            for (i = 1; i <= 99; i++)
            {

                string strURL = "http://www.hockey-reference.com/friv/numbers.cgi?number=" + i.ToString() + "&year=2016";
                bool bLoaded = false;

                System.Diagnostics.Debug.WriteLine(strURL);

                while (!bLoaded)
                {
                    try
                    {
                        doc.LoadHtml(new System.Net.WebClient().DownloadString(strURL));
                        bLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        continue;
                    }
                }

                root = doc.DocumentNode;

                tables = root.Descendants("table").Where(n => n.GetAttributeValue("id", "").Equals("stats")).ToList();

                if (tables.Count > 0)
                {
                    trs = tables[0].Descendants("tr").ToList();

                    for (x = 0; x < trs.Count; x++)
                    {
                        tds = trs[x].Descendants("td").ToList();

                        if (tds.Count >= 3)
                        {
                            item = new List<string>();
                            item.Add(i.ToString());
                            item.Add(tds[1].InnerText.Trim());
                            item.Add(tds[2].InnerText.Trim());
                            list.Add(item);
                        }                        
                    }
                }   
            }

            writeToFile(list, @"jerseys.txt");
        }

        // eg. http://stats.nhlnumbers.com/teams/STL?year=2016
        private void button4_Click(object sender, EventArgs e)
        {
            List<string> teams = new List<string>();
            teams.Add("ANA");
            teams.Add("ARI");
            teams.Add("BOS");
            teams.Add("BUF");
            teams.Add("CGY");
            teams.Add("CAR");
            teams.Add("CHI");
            teams.Add("COL");
            teams.Add("CLB");
            teams.Add("DAL");
            teams.Add("DET");
            teams.Add("EDM");
            teams.Add("FLA");
            teams.Add("LAK");
            teams.Add("MIN");
            teams.Add("MTL");
            teams.Add("NAS");
            teams.Add("NJD");
            teams.Add("NYI");
            teams.Add("NYR");
            teams.Add("OTT");
            teams.Add("PHI");
            teams.Add("PIT");
            teams.Add("SJS");
            teams.Add("STL");
            teams.Add("TBL");
            teams.Add("TOR");
            teams.Add("VAN");
            teams.Add("WAS");
            teams.Add("WPG");

            List<List<string>> list = new List<List<string>>();
            List<string> item;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode root;
            List<HtmlAgilityPack.HtmlNode> tables;
            List<HtmlAgilityPack.HtmlNode> trs;
            List<HtmlAgilityPack.HtmlNode> tds;

            int i;

            foreach(string strTeam in teams)
            {

                string strURL = "http://stats.nhlnumbers.com/teams/" + strTeam + "?year=2016";
                bool bLoaded = false;

                System.Diagnostics.Debug.WriteLine(strURL);

                while (!bLoaded)
                {
                    try
                    {
                        doc.LoadHtml(new System.Net.WebClient().DownloadString(strURL));
                        bLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        continue;
                    }
                }

                root = doc.DocumentNode;

                tables = root.Descendants("table").Where(n => n.GetAttributeValue("class", "").Equals("normal")).ToList();

                if (tables.Count > 0)
                {
                    trs = tables[0].Descendants("tr").ToList();

                    for (i = 3; i < trs.Count; i++)
                    {
                        tds = trs[i].Descendants("td").ToList();

                        if (tds.Count >= 5 && tds[0].InnerText.Trim() != "Player")
                        {
                            item = new List<string>();
                            item.Add(strTeam);
                            item.Add(tds[0].InnerText.Trim());
                            item.Add(tds[4].InnerText.Trim());
                            list.Add(item);
                        }
                    }
                }
            }

            writeToFile(list, @"salaries2.txt");
        }

        // eg. http://www.hockey-reference.com/players/k/
        private void button5_Click(object sender, EventArgs e)
        {
            List<List<string>> list = new List<List<string>>();
            List<string> item;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode root;
            char c;
            int i;

            for (c = 'a'; c <= 'z'; c++)
            {
                if (c == 'x')
                {
                    continue;
                }

                string strURL = "http://www.hockey-reference.com/players/"+c.ToString()+"/";

                bool bLoaded = false;

                while (!bLoaded)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine(strURL);
                        doc.LoadHtml(new System.Net.WebClient().DownloadString(strURL));
                        bLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        continue;
                    }
                }

                root = doc.DocumentNode;
                List<HtmlAgilityPack.HtmlNode> trs = root.Descendants("table").Where(n => n.GetAttributeValue("id", "").Equals("players")).Single().Descendants("tr").ToList();

                for (i = 0; i < trs.Count; i++)
                {
                    List<HtmlAgilityPack.HtmlNode> tds = trs[i].Descendants("td").ToList();

                    if (tds.Count >= 7)
                    {
                        if (tds[0].Descendants("strong").ToList().Count > 0)
                        {
                            item = new List<string>();

                            item.Add(tds[0].InnerText.Trim().Replace("*", ""));

                            string strDate = tds[6].InnerText.Trim();
                            DateTime dt;
                            if (DateTime.TryParse(strDate, out dt))
                            {
                                item.Add(dt.ToString("yyyy-MM-dd"));
                            }
                            else
                            {
                                item.Add("");
                            }

                            item.Add(tds[0].Descendants("a").Single().Attributes["href"].Value.Trim().Replace("/players/", "").Replace(".html", ""));

                            list.Add(item);
                        }
                    }
                }
            }

            writeToFile(list, "PlayerLinks.txt");

            System.Diagnostics.Debug.WriteLine("Done.");
            
        }

        // Hockey Reference - Game Logs by Player in Oct7-Jan15 date range
        // eg. http://www.hockey-reference.com/players/a/abdelju01/gamelog/2016/
        private void button6_Click(object sender, EventArgs e)
        {
            List<string> url = new List<string>();
            url.Add("a/abdelju01"); url.Add("Justin Abdelkader");
            url.Add("a/agozzan01"); url.Add("Andrew Agozzino");
            url.Add("a/allenja01"); url.Add("Jake Allen");
            url.Add("a/alzneka01"); url.Add("Karl Alzner");
            url.Add("a/anderfr01"); url.Add("Frederik Andersen");
            url.Add("a/andercr01"); url.Add("Craig Anderson");
            url.Add("a/anderjo05"); url.Add("Josh Anderson");
            url.Add("a/anderjo03"); url.Add("Joakim Andersson");
            url.Add("a/andrean01"); url.Add("Andy Andreoff");
            url.Add("a/andrisv01"); url.Add("Sven Andrighetto");
            url.Add("a/angelmi01"); url.Add("Mike Angelidis");
            url.Add("a/anisiar01"); url.Add("Artem Anisimov");
            url.Add("a/arcobma01"); url.Add("Mark Arcobello");
            url.Add("a/armiajo01"); url.Add("Joel Armia");
            url.Add("a/arvidvi01"); url.Add("Viktor Arvidsson");
            url.Add("a/athanan01"); url.Add("Andreas Athanasiou");
            url.Add("a/atkinca01"); url.Add("Cam Atkinson");
            url.Add("b/bachmri01"); url.Add("Richard Bachman");
            url.Add("b/backeda01"); url.Add("David Backes");
            url.Add("b/backlmi01"); url.Add("Mikael Backlund");
            url.Add("b/backsni02"); url.Add("Nicklas Backstrom");
            url.Add("b/baertsv01"); url.Add("Sven Baertschi");
            url.Add("b/bailejo01"); url.Add("Josh Bailey");
            url.Add("b/barbema01"); url.Add("Mark Barberio");
            url.Add("b/barkoal01"); url.Add("Aleksander Barkov");
            url.Add("b/barrity01"); url.Add("Tyson Barrie");
            url.Add("b/bartkma01"); url.Add("Matt Bartkowski");
            url.Add("b/bartlvi01"); url.Add("Victor Bartley");
            url.Add("b/bassco01"); url.Add("Cody Bass");
            url.Add("b/baunky01"); url.Add("Kyle Baun");
            url.Add("b/beaglja01"); url.Add("Jay Beagle");
            url.Add("b/beaucfr01"); url.Add("Francois Beauchemin");
            url.Add("b/beaulna01"); url.Add("Nathan Beaulieu");
            url.Add("b/beckta01"); url.Add("Taylor Beck");
            url.Add("b/belesma01"); url.Add("Matt Beleskey");
            url.Add("b/bellepi01"); url.Add("Pierre-Edouard Bellemare");
            url.Add("b/bennja01"); url.Add("Jamie Benn");
            url.Add("b/bennjo01"); url.Add("Jordie Benn");
            url.Add("b/bennebe01"); url.Add("Beau Bennett");
            url.Add("b/bennesa01"); url.Add("Sam Bennett");
            url.Add("b/benoian01"); url.Add("Andre Benoit");
            url.Add("b/bergepa01"); url.Add("Patrice Bergeron");
            url.Add("b/berglpa01"); url.Add("Patrik Berglund");
            url.Add("b/bernijo01"); url.Add("Jonathan Bernier");
            url.Add("b/bernist01"); url.Add("Steve Bernier");
            url.Add("b/berrare01"); url.Add("Reto Berra");
            url.Add("b/bertsch02"); url.Add("Christoph Bertschy");
            url.Add("b/berubje02"); url.Add("Jean-Francois Berube");
            url.Add("b/bickebr01"); url.Add("Bryan Bickell");
            url.Add("b/biegaal01"); url.Add("Alex Biega");
            url.Add("b/biekske01"); url.Add("Kevin Bieksa");
            url.Add("b/bigrach01"); url.Add("Chris Bigras");
            url.Add("b/binnijo01"); url.Add("Jordan Binnington");
            url.Add("b/bishobe01"); url.Add("Ben Bishop");
            url.Add("b/bitetan01"); url.Add("Anthony Bitetto");
            url.Add("b/bjugsni01"); url.Add("Nick Bjugstad");
            url.Add("b/blandjo01"); url.Add("Joseph Blandisi");
            url.Add("b/blundmi01"); url.Add("Mike Blunden");
            url.Add("b/bobrose01"); url.Add("Sergei Bobrovsky");
            url.Add("b/bodnaan01"); url.Add("Andrew Bodnarchuk");
            url.Add("b/boedkmi01"); url.Add("Mikkel Boedker");
            url.Add("b/bogosza01"); url.Add("Zach Bogosian");
            url.Add("b/bollja01"); url.Add("Jared Boll");
            url.Add("b/bollada01"); url.Add("Dave Bolland");
            url.Add("b/bollibr01"); url.Add("Brandon Bollig");
            url.Add("b/boninni01"); url.Add("Nick Bonino");
            url.Add("b/borowma01"); url.Add("Mark Borowiecki");
            url.Add("b/borturo01"); url.Add("Robert Bortuzzo");
            url.Add("b/bouchre01"); url.Add("Reid Boucher");
            url.Add("b/boulter01"); url.Add("Eric Boulton");
            url.Add("b/boumala01"); url.Add("Lance Bouma");
            url.Add("b/bourqga01"); url.Add("Gabriel Bourque");
            url.Add("b/bourqre01"); url.Add("Rene Bourque");
            url.Add("b/bouwmja01"); url.Add("Jay Bouwmeester");
            url.Add("b/boychjo01"); url.Add("Johnny Boychuk");
            url.Add("b/boyesbr01"); url.Add("Brad Boyes");
            url.Add("b/boylebr01"); url.Add("Brian Boyle");
            url.Add("b/boyleda01"); url.Add("Dan Boyle");
            url.Add("b/bozakty01"); url.Add("Tyler Bozak");
            url.Add("b/brassde01"); url.Add("Derick Brassard");
            url.Add("b/braunju01"); url.Add("Justin Braun");
            url.Add("b/brickco01"); url.Add("Connor Brickley");
            url.Add("b/broditj01"); url.Add("T.J. Brodie");
            url.Add("b/brodijo01"); url.Add("Jonas Brodin");
            url.Add("b/brodzky01"); url.Add("Kyle Brodziak");
            url.Add("b/brouwtr01"); url.Add("Troy Brouwer");
            url.Add("b/brownch01"); url.Add("Chris Brown");
            url.Add("b/browndu01"); url.Add("Dustin Brown");
            url.Add("b/brownjt01"); url.Add("J.T. Brown");
            url.Add("b/brownmi02"); url.Add("Mike Brown");
            url.Add("b/bulmebr01"); url.Add("Brett Bulmer");
            url.Add("b/burakan01"); url.Add("Andre Burakovsky");
            url.Add("b/burmial01"); url.Add("Alexander Burmistrov");
            url.Add("b/burnsbr01"); url.Add("Brent Burns");
            url.Add("b/burroal01"); url.Add("Alexandre Burrows");
            url.Add("b/butlech01"); url.Add("Chris Butler");
            url.Add("b/byfugdu01"); url.Add("Dustin Byfuglien");
            url.Add("b/byronpa01"); url.Add("Paul Byron");
            url.Add("c/callary01"); url.Add("Ryan Callahan");
            url.Add("c/calvema01"); url.Add("Matt Calvert");
            url.Add("c/cammami01"); url.Add("Mike Cammalleri");
            url.Add("c/campbbr02"); url.Add("Brian Campbell");
            url.Add("c/campbgr01"); url.Add("Gregory Campbell");
            url.Add("c/careypa01"); url.Add("Paul Carey");
            url.Add("c/carlema01"); url.Add("Matt Carle");
            url.Add("c/carlsjo01"); url.Add("John Carlson");
            url.Add("c/caronjo01"); url.Add("Jordan Caron");
            url.Add("c/carpery01"); url.Add("Ryan Carpenter");
            url.Add("c/carrda01"); url.Add("Daniel Carr");
            url.Add("c/carrico01"); url.Add("Connor Carrick");
            url.Add("c/carteje01"); url.Add("Jeff Carter");
            url.Add("c/cartery01"); url.Add("Ryan Carter");
            url.Add("c/cecico01"); url.Add("Cody Ceci");
            url.Add("c/chapumi01"); url.Add("Michael Chaput");
            url.Add("c/charazd01"); url.Add("Zdeno Chara");
            url.Add("c/chiarbe01"); url.Add("Ben Chiarot");
            url.Add("c/chiasal01"); url.Add("Alex Chiasson");
            url.Add("c/chimeja01"); url.Add("Jason Chimera");
            url.Add("c/chipcky01"); url.Add("Kyle Chipchura");
            url.Add("c/chornta01"); url.Add("Taylor Chorney");
            url.Add("c/cizikca01"); url.Add("Casey Cizikas");
            url.Add("c/claesfr01"); url.Add("Fredrik Claesson");
            url.Add("c/clarkda03"); url.Add("David Clarkson");
            url.Add("c/clendad01"); url.Add("Adam Clendening");
            url.Add("c/cliffky01"); url.Add("Kyle Clifford");
            url.Add("c/cluneri01"); url.Add("Rich Clune");
            url.Add("c/cluttca01"); url.Add("Cal Clutterbuck");
            url.Add("c/coburbr01"); url.Add("Braydon Coburn");
            url.Add("c/coglian01"); url.Add("Andrew Cogliano");
            url.Add("c/colaica01"); url.Add("Carlo Colaiacovo");
            url.Add("c/colbojo01"); url.Add("Joe Colborne");
            url.Add("c/coleia01"); url.Add("Ian Cole");
            url.Add("c/collise02"); url.Add("Sean Collins");
            url.Add("c/comeabl01"); url.Add("Blake Comeau");
            url.Add("c/condomi01"); url.Add("Mike Condon");
            url.Add("c/condrer01"); url.Add("Erik Condra");
            url.Add("c/connake01"); url.Add("Kevin Connauton");
            url.Add("c/connobr01"); url.Add("Brett Connolly");
            url.Add("c/coppan01"); url.Add("Andrew Copp");
            url.Add("c/cormipa01"); url.Add("Patrice Cormier");
            url.Add("c/corrafr01"); url.Add("Frank Corrado");
            url.Add("c/cousini01"); url.Add("Nick Cousins");
            url.Add("c/coutulo01"); url.Add("Logan Couture");
            url.Add("c/coutuse01"); url.Add("Sean Couturier");
            url.Add("c/cowenja01"); url.Add("Jared Cowen");
            url.Add("c/coylech01"); url.Add("Charlie Coyle");
            url.Add("c/crackad01"); url.Add("Adam Cracknell");
            url.Add("c/crawfco01"); url.Add("Corey Crawford");
            url.Add("c/crosbsi01"); url.Add("Sidney Crosby");
            url.Add("c/crossto01"); url.Add("Tommy Cross");
            url.Add("c/cullema01"); url.Add("Matt Cullen");
            url.Add("c/cunnicr01"); url.Add("Craig Cunningham");
            url.Add("d/dahlbkl01"); url.Add("Klas Dahlbeck");
            url.Add("d/daleytr01"); url.Add("Trevor Daley");
            url.Add("d/danauph01"); url.Add("Phillip Danault");
            url.Add("d/danoma01"); url.Add("Marko DaÃ…Ë†o");
            url.Add("d/darlisc01"); url.Add("Scott Darling");
            url.Add("d/datsypa01"); url.Add("Pavel Datsyuk");
            url.Add("d/dauphla01"); url.Add("Laurent Dauphin");
            url.Add("d/davidbr01"); url.Add("Brandon Davidson");
            url.Add("d/dehaaca01"); url.Add("Calvin de Haan");
            url.Add("d/delarja01"); url.Add("Jacob de La Rose");
            url.Add("d/dekeyda01"); url.Add("Danny DeKeyser");
            url.Add("d/delzomi01"); url.Add("Michael Del Zotto");
            url.Add("d/demeldy01"); url.Add("Dylan DeMelo");
            url.Add("d/demerja01"); url.Add("Jason Demers");
            url.Add("d/deshada01"); url.Add("David Desharnais");
            url.Add("d/desjaan01"); url.Add("Andrew Desjardins");
            url.Add("d/deslani01"); url.Add("Nicolas Deslauriers");
            url.Add("d/desprsi01"); url.Add("Simon Despres");
            url.Add("d/digiuph01"); url.Add("Phillip Di Giuseppe");
            url.Add("d/dillobr01"); url.Add("Brenden Dillon");
            url.Add("d/doansh01"); url.Add("Shane Doan");
            url.Add("d/domima01"); url.Add("Max Domi");
            url.Add("d/dominlo01"); url.Add("Louis Domingue");
            url.Add("d/donskjo01"); url.Add("Joonas Donskoi");
            url.Add("d/dorsede01"); url.Add("Derek Dorsett");
            url.Add("d/doughdr01"); url.Add("Drew Doughty");
            url.Add("d/downist01"); url.Add("Steve Downie");
            url.Add("d/draisle01"); url.Add("Leon Draisaitl");
            url.Add("d/driedch01"); url.Add("Chris Driedger");
            url.Add("d/drouijo01"); url.Add("Jonathan Drouin");
            url.Add("d/dubinbr01"); url.Add("Brandon Dubinsky");
            url.Add("d/dubnyde01"); url.Add("Devan Dubnyk");
            url.Add("d/duchema01"); url.Add("Matt Duchene");
            url.Add("d/duclaan01"); url.Add("Anthony Duclair");
            url.Add("d/dumbama01"); url.Add("Mathew Dumba");
            url.Add("d/dumoubr01"); url.Add("Brian Dumoulin");
            url.Add("d/dupuipa01"); url.Add("Pascal Dupuis");
            url.Add("d/dzingry01"); url.Add("Ryan Dzingel");
            url.Add("d/dziurda01"); url.Add("David Dziurzynski");
            url.Add("e/eakinco01"); url.Add("Cody Eakin");
            url.Add("e/eavespa01"); url.Add("Patrick Eaves");
            url.Add("e/eberljo01"); url.Add("Jordan Eberle");
            url.Add("e/edleral01"); url.Add("Alexander Edler");
            url.Add("e/edmunjo01"); url.Add("Joel Edmundson");
            url.Add("e/ehlerni01"); url.Add("Nikolaj Ehlers");
            url.Add("e/ehrhoch01"); url.Add("Christian Ehrhoff");
            url.Add("e/eicheja01"); url.Add("Jack Eichel");
            url.Add("e/ekblaaa01"); url.Add("Aaron Ekblad");
            url.Add("e/ekholma01"); url.Add("Mattias Ekholm");
            url.Add("e/ekmanol01"); url.Add("Oliver Ekman-Larsson");
            url.Add("e/eliaspa01"); url.Add("Patrik Elias");
            url.Add("e/ellerla01"); url.Add("Lars Eller");
            url.Add("e/elliobr01"); url.Add("Brian Elliott");
            url.Add("e/elliost01"); url.Add("Stefan Elliott");
            url.Add("e/ellisry01"); url.Add("Ryan Ellis");
            url.Add("e/emelial01"); url.Add("Alexei Emelin");
            url.Add("e/engelde01"); url.Add("Deryk Engelland");
            url.Add("e/ennisty01"); url.Add("Tyler Ennis");
            url.Add("e/enrotjh01"); url.Add("Jhonas Enroth");
            url.Add("e/enstrto01"); url.Add("Toby Enstrom");
            url.Add("e/ericsjo01"); url.Add("Jonathan Ericsson");
            url.Add("e/erikslo01"); url.Add("Loui Eriksson");
            url.Add("e/etemem01"); url.Add("Emerson Etem");
            url.Add("e/everbde01"); url.Add("Dennis Everberg");
            url.Add("f/fabbrro01"); url.Add("Robby Fabbri");
            url.Add("f/faksara01"); url.Add("Radek Faksa");
            url.Add("f/falkju01"); url.Add("Justin Falk");
            url.Add("f/farnhbo01"); url.Add("Bobby Farnham");
            url.Add("f/fastje01"); url.Add("Jesper Fast");
            url.Add("f/faulkju01"); url.Add("Justin Faulk");
            url.Add("f/faynema01"); url.Add("Mark Fayne");
            url.Add("f/fedunta01"); url.Add("Taylor Fedun");
            url.Add("f/fehrer01"); url.Add("Eric Fehr");
            url.Add("f/ferenan01"); url.Add("Andrew Ference");
            url.Add("f/ferlami01"); url.Add("Micheal Ferland");
            url.Add("f/ferrala01"); url.Add("Landon Ferraro");
            url.Add("f/fialake01"); url.Add("Kevin Fiala");
            url.Add("f/fiddlve01"); url.Add("Vernon Fiddler");
            url.Add("f/filppva01"); url.Add("Valtteri Filppula");
            url.Add("f/fishemi01"); url.Add("Mike Fisher");
            url.Add("f/fleisto01"); url.Add("Tomas Fleischmann");
            url.Add("f/fleurma01"); url.Add("Marc-Andre Fleury");
            url.Add("f/flynnbr01"); url.Add("Brian Flynn");
            url.Add("f/foligma01"); url.Add("Marcus Foligno");
            url.Add("f/foligni01"); url.Add("Nick Foligno");
            url.Add("f/folinch01"); url.Add("Christian Folin");
            url.Add("f/fontaju01"); url.Add("Justin Fontaine");
            url.Add("f/forbode01"); url.Add("Derek Forbort");
            url.Add("f/forsban01"); url.Add("Anton Forsberg");
            url.Add("f/forsbfi01"); url.Add("Filip Forsberg");
            url.Add("f/fowleca01"); url.Add("Cam Fowler");
            url.Add("f/fransco01"); url.Add("Cody Franson");
            url.Add("f/franzjo01"); url.Add("Johan Franzen");
            url.Add("f/fribema01"); url.Add("Max Friberg");
            url.Add("f/froesby01"); url.Add("Byron Froese");
            url.Add("f/frolimi01"); url.Add("Michael Frolik");
            url.Add("g/gaborma01"); url.Add("Marian Gaborik");
            url.Add("g/gabriku01"); url.Add("Kurtis Gabriel");
            url.Add("g/gagnesa01"); url.Add("Sam Gagner");
            url.Add("g/galchal01"); url.Add("Alex Galchenyuk");
            url.Add("g/galiest01"); url.Add("Stanislav Galiev");
            url.Add("g/gallabr01"); url.Add("Brendan Gallagher");
            url.Add("g/garbury01"); url.Add("Ryan Garbutt");
            url.Add("g/gardija01"); url.Add("Jake Gardiner");
            url.Add("g/garrija01"); url.Add("Jason Garrison");
            url.Add("g/gaudety01"); url.Add("Tyler Gaudet");
            url.Add("g/gaudrjo01"); url.Add("John Gaudreau");
            url.Add("g/gauncbr01"); url.Add("Brendan Gaunce");
            url.Add("g/gaustpa01"); url.Add("Paul Gaustad");
            url.Add("g/gazdilu01"); url.Add("Luke Gazdic");
            url.Add("g/geliner01"); url.Add("Eric Gelinas");
            url.Add("g/gerbena01"); url.Add("Nathan Gerbe");
            url.Add("g/getzlry01"); url.Add("Ryan Getzlaf");
            url.Add("g/gibsoch01"); url.Add("Christopher Gibson");
            url.Add("g/gibsojo02"); url.Add("John Gibson");
            url.Add("g/gilbeto01"); url.Add("Tom Gilbert");
            url.Add("g/giontbr01"); url.Add("Brian Gionta");
            url.Add("g/giontst01"); url.Add("Stephen Gionta");
            url.Add("g/giordma01"); url.Add("Mark Giordano");
            url.Add("g/girarda01"); url.Add("Dan Girardi");
            url.Add("g/girgeze01"); url.Add("Zemgus Girgensons");
            url.Add("g/giroucl01"); url.Add("Claude Giroux");
            url.Add("g/glassta01"); url.Add("Tanner Glass");
            url.Add("g/glendlu01"); url.Add("Luke Glendening");
            url.Add("g/goldoni01"); url.Add("Nikolay Goldobin");
            url.Add("g/goligal01"); url.Add("Alex Goligoski");
            url.Add("g/golouco01"); url.Add("Cody Goloubef");
            url.Add("g/gomezsc01"); url.Add("Scott Gomez");
            url.Add("g/goodrba01"); url.Add("Barclay Goodrow");
            url.Add("g/gordobo01"); url.Add("Boyd Gordon");
            url.Add("g/gorgejo01"); url.Add("Josh Gorges");
            url.Add("g/gormlbr01"); url.Add("Brandon Gormley");
            url.Add("g/gostish01"); url.Add("Shayne Gostisbehere");
            url.Add("g/gourdya01"); url.Add("Yanni Gourde");
            url.Add("g/grabnmi01"); url.Add("Michael Grabner");
            url.Add("g/grabomi01"); url.Add("Mikhail Grabovski");
            url.Add("g/gragnma01"); url.Add("Marc-Andre Gragnani");
            url.Add("g/granbpe01"); url.Add("Petter Granberg");
            url.Add("g/granlma01"); url.Add("Markus Granlund");
            url.Add("g/granlmi01"); url.Add("Mikael Granlund");
            url.Add("g/grantde01"); url.Add("Derek Grant");
            url.Add("g/graovty01"); url.Add("Tyler Graovac");
            url.Add("g/greenmi03"); url.Add("Mike Green");
            url.Add("g/greenan01"); url.Add("Andy Greene");
            url.Add("g/greenma01"); url.Add("Matt Greene");
            url.Add("g/greenco01"); url.Add("Colin Greening");
            url.Add("g/greisth01"); url.Add("Thomas Greiss");
            url.Add("g/grenial01"); url.Add("Alexandre Grenier");
            url.Add("g/griffse01"); url.Add("Seth Griffith");
            url.Add("g/grigomi01"); url.Add("Mikhail Grigorenko");
            url.Add("g/grimaro01"); url.Add("Rocco Grimaldi");
            url.Add("g/grossni01"); url.Add("Nicklas Grossmann");
            url.Add("g/grubaph01"); url.Add("Philipp Grubauer");
            url.Add("g/grybaer01"); url.Add("Eric Gryba");
            url.Add("g/gudasra01"); url.Add("Radko Gudas");
            url.Add("g/gudbrer01"); url.Add("Erik Gudbranson");
            url.Add("g/gudlekr01"); url.Add("Kristers Gudlevskis");
            url.Add("g/guenina01"); url.Add("Nate Guenin");
            url.Add("g/gunnaca01"); url.Add("Carl Gunnarsson");
            url.Add("g/gustaer02"); url.Add("Erik Gustafsson");
            url.Add("g/gustajo01"); url.Add("Jonas Gustavsson");
            url.Add("h/hagelca01"); url.Add("Carl Hagelin");
            url.Add("h/hainsro01"); url.Add("Ron Hainsey");
            url.Add("h/halakja01"); url.Add("Jaroslav Halak");
            url.Add("h/haleymi01"); url.Add("Micheal Haley");
            url.Add("h/halisma01"); url.Add("Matt Halischuk");
            url.Add("h/hallta02"); url.Add("Taylor Hall");
            url.Add("h/hamhuda01"); url.Add("Dan Hamhuis");
            url.Add("h/hamildo01"); url.Add("Dougie Hamilton");
            url.Add("h/hammoan01"); url.Add("Andrew Hammond");
            url.Add("h/hamontr01"); url.Add("Travis Hamonic");
            url.Add("h/hanifno01"); url.Add("Noah Hanifin");
            url.Add("h/hannima01"); url.Add("Markus Hannikainen");
            url.Add("h/hanseja01"); url.Add("Jannik Hansen");
            url.Add("h/hanzama01"); url.Add("Martin Hanzal");
            url.Add("h/harrisc01"); url.Add("Scott Harrington");
            url.Add("h/hartmry01"); url.Add("Ryan Hartman");
            url.Add("h/hartnsc01"); url.Add("Scott Hartnell");
            url.Add("h/haulaer01"); url.Add("Erik Haula");
            url.Add("h/havlama01"); url.Add("Martin Havlat");
            url.Add("h/hayesji01"); url.Add("Jimmy Hayes");
            url.Add("h/hayeske01"); url.Add("Kevin Hayes");
            url.Add("h/hedmavi01"); url.Add("Victor Hedman");
            url.Add("h/helgese01"); url.Add("Seth Helgeson");
            url.Add("h/hellbma01"); url.Add("Magnus Hellberg");
            url.Add("h/helleco01"); url.Add("Connor Hellebuyck");
            url.Add("h/helmda01"); url.Add("Darren Helm");
            url.Add("h/hemskal01"); url.Add("Ales Hemsky");
            url.Add("h/hendrma01"); url.Add("Matt Hendricks");
            url.Add("h/henriad01"); url.Add("Adam Henrique");
            url.Add("h/hertlto01"); url.Add("Tomas Hertl");
            url.Add("h/hicketh01"); url.Add("Thomas Hickey");
            url.Add("h/higgich01"); url.Add("Chris Higgins");
            url.Add("h/hillejo01"); url.Add("Jonas Hiller");
            url.Add("h/hinosvi01"); url.Add("Vincent Hinostroza");
            url.Add("h/hjalmni01"); url.Add("Niklas Hjalmarsson");
            url.Add("h/hodgsco01"); url.Add("Cody Hodgson");
            url.Add("h/hoffmmi02"); url.Add("Mike Hoffman");
            url.Add("h/holdeni01"); url.Add("Nick Holden");
            url.Add("h/hollape01"); url.Add("Peter Holland");
            url.Add("h/hollobu01"); url.Add("Bud Holloway");
            url.Add("h/holtbbr01"); url.Add("Braden Holtby");
            url.Add("h/holzeko01"); url.Add("Korbinian Holzer");
            url.Add("h/horcosh01"); url.Add("Shawn Horcoff");
            url.Add("h/hornqpa01"); url.Add("Patric Hornqvist");
            url.Add("h/horvabo01"); url.Add("Bo Horvat");
            url.Add("h/hossama01"); url.Add("Marian Hossa");
            url.Add("h/howarja02"); url.Add("Jimmy Howard");
            url.Add("h/howdequ01"); url.Add("Quinton Howden");
            url.Add("h/huberjo01"); url.Add("Jonathan Huberdeau");
            url.Add("h/hudleji01"); url.Add("Jiri Hudler");
            url.Add("h/hudonch01"); url.Add("Charles Hudon");
            url.Add("h/huntbr01"); url.Add("Brad Hunt");
            url.Add("h/hunwima01"); url.Add("Matt Hunwick");
            url.Add("h/hutchmi01"); url.Add("Michael Hutchinson");
            url.Add("h/huttobe01"); url.Add("Ben Hutton");
            url.Add("h/huttoca01"); url.Add("Carter Hutton");
            url.Add("i/iginlja01"); url.Add("Jarome Iginla");
            url.Add("i/irwinma01"); url.Add("Matt Irwin");
            url.Add("j/jackmba01"); url.Add("Barret Jackman");
            url.Add("j/jackmti01"); url.Add("Tim Jackman");
            url.Add("j/jagrja01"); url.Add("Jaromir Jagr");
            url.Add("j/janmama02"); url.Add("Mattias Janmark");
            url.Add("j/jarnkca01"); url.Add("Calle Jarnkrok");
            url.Add("j/jaskidm01"); url.Add("Dmitrij Jaskin");
            url.Add("j/jeffrdu01"); url.Add("Dustin Jeffrey");
            url.Add("j/jennebo01"); url.Add("Boone Jenner");
            url.Add("j/johanry01"); url.Add("Ryan Johansen");
            url.Add("j/johanma03"); url.Add("Marcus Johansson");
            url.Add("j/johnsch02"); url.Add("Chad Johnson");
            url.Add("j/johnser01"); url.Add("Erik Johnson");
            url.Add("j/johnsja02"); url.Add("Jack Johnson");
            url.Add("j/johnsty01"); url.Add("Tyler Johnson");
            url.Add("j/jokinju01"); url.Add("Jussi Jokinen");
            url.Add("j/jokipjy01"); url.Add("Jyrki Jokipakka");
            url.Add("j/jonesda01"); url.Add("David Jones");
            url.Add("j/jonesma02"); url.Add("Martin Jones");
            url.Add("j/jonesse01"); url.Add("Seth Jones");
            url.Add("j/joorijo01"); url.Add("Josh Jooris");
            url.Add("j/jordami01"); url.Add("Michal Jordan");
            url.Add("j/josefja01"); url.Add("Jacob Josefson");
            url.Add("j/josiro01"); url.Add("Roman Josi");
            url.Add("j/jurcoto01"); url.Add("Tomas Jurco");
            url.Add("k/kadrina01"); url.Add("Nazem Kadri");
            url.Add("k/kalinse01"); url.Add("Sergey Kalinin");
            url.Add("k/kampfst01"); url.Add("Steven Kampfer");
            url.Add("k/kaneev01"); url.Add("Evander Kane");
            url.Add("k/kanepa01"); url.Add("Patrick Kane");
            url.Add("k/karlser01"); url.Add("Erik Karlsson");
            url.Add("k/karlsme01"); url.Add("Melker Karlsson");
            url.Add("k/karlswi01"); url.Add("William Karlsson");
            url.Add("k/kassiza01"); url.Add("Zack Kassian");
            url.Add("k/keithdu01"); url.Add("Duncan Keith");
            url.Add("k/kellych01"); url.Add("Chris Kelly");
            url.Add("k/kemppjo01"); url.Add("Joonas Kemppainen");
            url.Add("k/keninro01"); url.Add("Ronalds Kenins");
            url.Add("k/kennety01"); url.Add("Tyler Kennedy");
            url.Add("k/keranmi01"); url.Add("Michael KerÃƒÂ¤nen");
            url.Add("k/kerota01"); url.Add("Tanner Kero");
            url.Add("k/keslery01"); url.Add("Ryan Kesler");
            url.Add("k/kesseph01"); url.Add("Phil Kessel");
            url.Add("k/khairju01"); url.Add("Jujhar Khaira");
            url.Add("k/khokhal01"); url.Add("Alexander Khokhlachev");
            url.Add("k/khudoan01"); url.Add("Anton Khudobin");
            url.Add("k/killoal01"); url.Add("Alex Killorn");
            url.Add("k/kindlja01"); url.Add("Jakub Kindl");
            url.Add("k/kingdw01"); url.Add("Dwight King");
            url.Add("k/kinkake01"); url.Add("Keith Kinkaid");
            url.Add("k/klefbos01"); url.Add("Oscar Klefbom");
            url.Add("k/kleinke01"); url.Add("Kevin Klein");
            url.Add("k/klingjo01"); url.Add("John Klingberg");
            url.Add("k/klinkro01"); url.Add("Rob Klinkhammer");
            url.Add("k/knighco01"); url.Add("Corban Knight");
            url.Add("k/koekksl01"); url.Add("Slater Koekkoek");
            url.Add("k/koivumi01"); url.Add("Mikko Koivu");
            url.Add("k/komarle01"); url.Add("Leo Komarov");
            url.Add("k/kopitan01"); url.Add("Anze Kopitar");
            url.Add("k/korpila01"); url.Add("Lauri Korpikoski");
            url.Add("k/korpijo01"); url.Add("Joonas Korpisalo");
            url.Add("k/kreidch01"); url.Add("Chris Kreider");
            url.Add("k/krejcda01"); url.Add("David Krejci");
            url.Add("k/kronwni01"); url.Add("Niklas Kronwall");
            url.Add("k/krugto01"); url.Add("Torey Krug");
            url.Add("k/krugema01"); url.Add("Marcus Kruger");
            url.Add("k/kucheni01"); url.Add("Nikita Kucherov");
            url.Add("k/kuempda01"); url.Add("Darcy Kuemper");
            url.Add("k/kuhnhto01"); url.Add("Tom Kuhnhackl");
            url.Add("k/kulakbr01"); url.Add("Brett Kulak");
            url.Add("k/kulemni01"); url.Add("Nikolai Kulemin");
            url.Add("k/kulikdm01"); url.Add("Dmitry Kulikov");
            url.Add("k/kunitch01"); url.Add("Chris Kunitz");
            url.Add("k/kuzneev01"); url.Add("Evgeny Kuznetsov");
            url.Add("l/lacked01"); url.Add("Eddie Lack");
            url.Add("l/laddan01"); url.Add("Andrew Ladd");
            url.Add("l/laichbr01"); url.Add("Brooks Laich");
            url.Add("l/landean01"); url.Add("Anton Lander");
            url.Add("l/landega01"); url.Add("Gabriel Landeskog");
            url.Add("l/larkidy01"); url.Add("Dylan Larkin");
            url.Add("l/larssad01"); url.Add("Adam Larsson");
            url.Add("l/larssjo02"); url.Add("Johan Larsson");
            url.Add("l/lattami01"); url.Add("Michael Latta");
            url.Add("l/laughsc01"); url.Add("Scott Laughton");
            url.Add("l/lazarcu01"); url.Add("Curtis Lazar");
            url.Add("l/lecavvi01"); url.Add("Vincent Lecavalier");
            url.Add("l/leddyni01"); url.Add("Nick Leddy");
            url.Add("l/leean01"); url.Add("Anders Lee");
            url.Add("l/legwada01"); url.Add("David Legwand");
            url.Add("l/lehnero01"); url.Add("Robin Lehner");
            url.Add("l/lehtejo01"); url.Add("Jori Lehtera");
            url.Add("l/lehtoka01"); url.Add("Kari Lehtonen");
            url.Add("l/leierta01"); url.Add("Taylor Leier");
            url.Add("l/leivojo01"); url.Add("Josh Leivo");
            url.Add("l/lergbr01"); url.Add("Bryan Lerg");
            url.Add("l/lessilu01"); url.Add("Lucas Lessio");
            url.Add("l/letankr01"); url.Add("Kris Letang");
            url.Add("l/letesma01"); url.Add("Mark Letestu");
            url.Add("l/lewistr01"); url.Add("Trevor Lewis");
            url.Add("l/lilesjo01"); url.Add("John-Michael Liles");
            url.Add("l/lindban01"); url.Add("Anders Lindback");
            url.Add("l/lindbos02"); url.Add("Oscar Lindberg");
            url.Add("l/lindbpe02"); url.Add("Petteri Lindbohm");
            url.Add("l/lindees01"); url.Add("Esa Lindell");
            url.Add("l/lindhel01"); url.Add("Elias Lindholm");
            url.Add("l/lindhha01"); url.Add("Hampus Lindholm");
            url.Add("l/liponj.01"); url.Add("J.C. Lipon");
            url.Add("l/littlbr01"); url.Add("Bryan Little");
            url.Add("l/lovejbe01"); url.Add("Ben Lovejoy");
            url.Add("l/lowryad01"); url.Add("Adam Lowry");
            url.Add("l/lucicmi01"); url.Add("Milan Lucic");
            url.Add("l/lundqhe01"); url.Add("Henrik Lundqvist");
            url.Add("l/luongro01"); url.Add("Roberto Luongo");
            url.Add("l/lupuljo01"); url.Add("Joffrey Lupul");
            url.Add("m/maattol01"); url.Add("Olli Maatta");
            url.Add("m/macarcl01"); url.Add("Clarke MacArthur");
            url.Add("m/macdoan01"); url.Add("Andrew MacDonald");
            url.Add("m/mackede01"); url.Add("Derek MacKenzie");
            url.Add("m/mackina01"); url.Add("Nathan MacKinnon");
            url.Add("m/malkiev01"); url.Add("Evgeni Malkin");
            url.Add("m/malonbr01"); url.Add("Brad Malone");
            url.Add("m/mannibr01"); url.Add("Brandon Manning");
            url.Add("m/mansojo01"); url.Add("Josh Manson");
            url.Add("m/marchbr03"); url.Add("Brad Marchand");
            url.Add("m/marchal01"); url.Add("Alexey Marchenko");
            url.Add("a/audymjo01"); url.Add("Jon Marchessault");
            url.Add("m/marinma01"); url.Add("Martin Marincin");
            url.Add("m/markoan01"); url.Add("Andrei Markov");
            url.Add("m/marksja02"); url.Add("Jacob Markstrom");
            url.Add("m/marlepa01"); url.Add("Patrick Marleau");
            url.Add("m/maroopa01"); url.Add("Patrick Maroon");
            url.Add("m/martima02"); url.Add("Matt Martin");
            url.Add("m/martipa01"); url.Add("Paul Martin");
            url.Add("m/martial01"); url.Add("Alec Martinez");
            url.Add("m/martijo01"); url.Add("Jordan Martinook");
            url.Add("m/martian01"); url.Add("Andreas Martinsen");
            url.Add("m/mashibr01"); url.Add("Brandon Mashinter");
            url.Add("m/masonst01"); url.Add("Steve Mason");
            url.Add("m/mattest02"); url.Add("Stefan Matteau");
            url.Add("m/matthsh01"); url.Add("Shawn Matthias");
            url.Add("m/mayfisc01"); url.Add("Scott Mayfield");
            url.Add("m/mcbaija02"); url.Add("Jamie McBain");
            url.Add("m/mccabja01"); url.Add("Jake McCabe");
            url.Add("m/mccanja01"); url.Add("Jared McCann");
            url.Add("m/mccarmi01"); url.Add("Michael McCarron");
            url.Add("m/mccarjo01"); url.Add("John McCarthy");
            url.Add("m/mccleja01"); url.Add("Jay McClement");
            url.Add("m/mccorma01"); url.Add("Max McCormick");
            url.Add("m/mcdavco01"); url.Add("Connor McDavid");
            url.Add("m/mcdonry01"); url.Add("Ryan McDonagh");
            url.Add("m/mcdonco01"); url.Add("Colin McDonald");
            url.Add("m/mcelhcu01"); url.Add("Curtis McElhinney");
            url.Add("m/mcginbr01"); url.Add("Brock McGinn");
            url.Add("m/mcginja01"); url.Add("Jamie McGinn");
            url.Add("m/mcginty01"); url.Add("Tye McGinn");
            url.Add("m/mcilrdy01"); url.Add("Dylan McIlrath");
            url.Add("m/mckencu01"); url.Add("Curtis McKenzie");
            url.Add("m/mcleoco01"); url.Add("Cody McLeod");
            url.Add("m/mcnabbr01"); url.Add("Brayden McNabb");
            url.Add("m/mcneima01"); url.Add("Mark McNeill");
            url.Add("m/mcquaad01"); url.Add("Adam McQuaid");
            url.Add("m/medveev01"); url.Add("Evgeny Medvedev");
            url.Add("m/megnaja01"); url.Add("Jayson Megna");
            url.Add("m/merrijo01"); url.Add("Jonathon Merrill");
            url.Add("m/merscmi01"); url.Add("Michael Mersch");
            url.Add("m/methoma01"); url.Add("Marc Methot");
            url.Add("m/michami01"); url.Add("Milan Michalek");
            url.Add("m/michazb01"); url.Add("Zbynek Michalek");
            url.Add("m/millean01"); url.Add("Andrew Miller");
            url.Add("m/milleco02"); url.Add("Colin Miller");
            url.Add("m/milledr01"); url.Add("Drew Miller");
            url.Add("m/millejt01"); url.Add("J.T. Miller");
            url.Add("m/milleke03"); url.Add("Kevan Miller");
            url.Add("m/millery01"); url.Add("Ryan Miller");
            url.Add("m/mitchjo01"); url.Add("John Mitchell");
            url.Add("m/mitchto01"); url.Add("Torrey Mitchell");
            url.Add("m/mitchwi01"); url.Add("Willie Mitchell");
            url.Add("m/moentr01"); url.Add("Travis Moen");
            url.Add("m/monahse01"); url.Add("Sean Monahan");
            url.Add("m/montoal01"); url.Add("Al Montoya");
            url.Add("m/mooredo01"); url.Add("Dominic Moore");
            url.Add("m/moorejo01"); url.Add("John Moore");
            url.Add("m/morrojo02"); url.Add("Joseph Morrow");
            url.Add("m/moulsma01"); url.Add("Matt Moulson");
            url.Add("m/mrazepe01"); url.Add("Petr Mrazek");
            url.Add("m/muellmi01"); url.Add("Mirco Mueller");
            url.Add("m/murphco02"); url.Add("Connor Murphy");
            url.Add("m/murphry01"); url.Add("Ryan Murphy");
            url.Add("m/murrama02"); url.Add("Matt Murray");
            url.Add("m/murrary01"); url.Add("Ryan Murray");
            url.Add("m/muzzija01"); url.Add("Jake Muzzin");
            url.Add("m/myersty01"); url.Add("Tyler Myers");
            url.Add("n/namesvl01"); url.Add("Vladislav Namestnikov");
            url.Add("n/nashri01"); url.Add("Rick Nash");
            url.Add("n/nashri02"); url.Add("Riley Nash");
            url.Add("n/nealja01"); url.Add("James Neal");
            url.Add("n/neilch01"); url.Add("Chris Neil");
            url.Add("n/nelsobr01"); url.Add("Brock Nelson");
            url.Add("n/nemetpa01"); url.Add("Patrik Nemeth");
            url.Add("n/nessaa01"); url.Add("Aaron Ness");
            url.Add("n/nesteni01"); url.Add("Nikita Nesterov");
            url.Add("n/nestran01"); url.Add("Andrej Nestrasil");
            url.Add("n/neuvimi01"); url.Add("Michal Neuvirth");
            url.Add("n/nichuva01"); url.Add("Valeri Nichushkin");
            url.Add("n/niedeni01"); url.Add("Nino Niederreiter");
            url.Add("n/nielsfr01"); url.Add("Frans Nielsen");
            url.Add("n/niemian02"); url.Add("Antti Niemi");
            url.Add("n/nietoma01"); url.Add("Matthew Nieto");
            url.Add("n/nikitni01"); url.Add("Nikita Nikitin");
            url.Add("n/nilssan01"); url.Add("Anders Nilsson");
            url.Add("n/niskama01"); url.Add("Matt Niskanen");
            url.Add("n/nolanjo01"); url.Add("Jordan Nolan");
            url.Add("n/nordsjo02"); url.Add("Joakim Nordstrom");
            url.Add("n/nosekto01"); url.Add("Tomas Nosek");
            url.Add("n/nugenry01"); url.Add("Ryan Nugent-Hopkins");
            url.Add("n/nurseda01"); url.Add("Darnell Nurse");
            url.Add("n/nyquigu01"); url.Add("Gustav Nyquist");
            url.Add("n/nystrer01"); url.Add("Eric Nystrom");
            url.Add("o/obrieji01"); url.Add("Jim O'Brien");
            url.Add("o/oconnma01"); url.Add("Matt O'Connor");
            url.Add("o/oneilbr01"); url.Add("Brian O'Neill");
            url.Add("o/oreilca01"); url.Add("Cal O'Reilly");
            url.Add("o/oreilry01"); url.Add("Ryan O'Reilly");
            url.Add("o/oduyajo01"); url.Add("Johnny Oduya");
            url.Add("o/okposky01"); url.Add("Kyle Okposo");
            url.Add("o/oleksja01"); url.Add("Jamie Oleksiak");
            url.Add("o/olofsgu01"); url.Add("Gustav Olofsson");
            url.Add("o/olsendy01"); url.Add("Dylan Olsen");
            url.Add("o/orlovdm01"); url.Add("Dmitry Orlov");
            url.Add("o/orpikbr01"); url.Add("Brooks Orpik");
            url.Add("o/ortiojo01"); url.Add("Joni Ortio");
            url.Add("o/oshietj01"); url.Add("T.J. Oshie");
            url.Add("o/ottst01"); url.Add("Steve Ott");
            url.Add("o/ovechal01"); url.Add("Alex Ovechkin");
            url.Add("p/paajama01"); url.Add("Magnus Paajarvi");
            url.Add("p/pacioma01"); url.Add("Max Pacioretty");
            url.Add("p/pageaje01"); url.Add("Jean-Gabriel Pageau");
            url.Add("p/paillda01"); url.Add("Daniel Paille");
            url.Add("p/pakarii01"); url.Add("Iiro Pakarinen");
            url.Add("p/palaton01"); url.Add("Ondrej Palat");
            url.Add("p/palmiky01"); url.Add("Kyle Palmieri");
            url.Add("p/panarar01"); url.Add("Artemi Panarin");
            url.Add("p/panikri01"); url.Add("Richard Panik");
            url.Add("p/paquece01"); url.Add("Cedric Paquette");
            url.Add("p/parayco01"); url.Add("Colton Parayko");
            url.Add("p/pardyad01"); url.Add("Adam Pardy");
            url.Add("p/parenpi01"); url.Add("P.A. Parenteau");
            url.Add("p/parisza01"); url.Add("Zach Parise");
            url.Add("p/pastrda01"); url.Add("David Pastrnak");
            url.Add("p/patergr01"); url.Add("Greg Pateryn");
            url.Add("p/pavelon01"); url.Add("Ondrej Pavelec");
            url.Add("p/paveljo01"); url.Add("Joe Pavelski");
            url.Add("p/pearsta01"); url.Add("Tanner Pearson");
            url.Add("p/pedanan01"); url.Add("Andrey Pedan");
            url.Add("p/pelecad01"); url.Add("Adam Pelech");
            url.Add("p/pelusan01"); url.Add("Anthony Peluso");
            url.Add("p/perrema01"); url.Add("Mathieu Perreault");
            url.Add("p/perroda01"); url.Add("David Perron");
            url.Add("p/perryco01"); url.Add("Corey Perry");
            url.Add("p/pescebr01"); url.Add("Brett Pesce");
            url.Add("p/petanni01"); url.Add("Nicolas Petan");
            url.Add("p/petroal01"); url.Add("Alex Petrovic");
            url.Add("p/petryje01"); url.Add("Jeff Petry");
            url.Add("p/phanedi01"); url.Add("Dion Phaneuf");
            url.Add("p/pickaca01"); url.Add("Calvin Pickard");
            url.Add("p/pietral01"); url.Add("Alex Pietrangelo");
            url.Add("p/pirribr01"); url.Add("Brandon Pirri");
            url.Add("p/plekato01"); url.Add("Tomas Plekanec");
            url.Add("p/plotnse01"); url.Add("Sergei Plotnikov");
            url.Add("p/polakro01"); url.Add("Roman Polak");
            url.Add("p/pominja01"); url.Add("Jason Pominville");
            url.Add("p/portech01"); url.Add("Chris Porter");
            url.Add("p/porteke01"); url.Add("Kevin Porter");
            url.Add("p/postmpa01"); url.Add("Paul Postma");
            url.Add("p/poulibe01"); url.Add("Benoit Pouliot");
            url.Add("p/poulide01"); url.Add("Derrick Pouliot");
            url.Add("p/priceca01"); url.Add("Carey Price");
            url.Add("p/princsh01"); url.Add("Shane Prince");
            url.Add("p/prossna01"); url.Add("Nate Prosser");
            url.Add("p/proutda01"); url.Add("Dalton Prout");
            url.Add("p/prustbr01"); url.Add("Brandon Prust");
            url.Add("p/puempma01"); url.Add("Matt Puempel");
            url.Add("p/pulkkte01"); url.Add("Teemu Pulkkinen");
            url.Add("p/purcete01"); url.Add("Teddy Purcell");
            url.Add("p/pysykma01"); url.Add("Mark Pysyk");
            url.Add("q/quickjo01"); url.Add("Jonathan Quick");
            url.Add("q/quincky01"); url.Add("Kyle Quincey");
            url.Add("r/raantan01"); url.Add("Antti Raanta");
            url.Add("r/rafflmi01"); url.Add("Michael Raffl");
            url.Add("r/rakelri01"); url.Add("Rickard Rakell");
            url.Add("r/ramoka01"); url.Add("Karri Ramo");
            url.Add("r/randety01"); url.Add("Tyler Randell");
            url.Add("r/rantami01"); url.Add("Mikko Rantanen");
            url.Add("r/rasktu01"); url.Add("Tuukka Rask");
            url.Add("r/raskvi01"); url.Add("Victor Rask");
            url.Add("r/rasmude01"); url.Add("Dennis Rasmussen");
            url.Add("r/rattity01"); url.Add("Ty Rattie");
            url.Add("r/raymoma01"); url.Add("Mason Raymond");
            url.Add("r/readma01"); url.Add("Matt Read");
            url.Add("r/reavery01"); url.Add("Ryan Reaves");
            url.Add("r/redmoza01"); url.Add("Zach Redmond");
            url.Add("r/reillmi01"); url.Add("Mike Reilly");
            url.Add("r/reimeja01"); url.Add("James Reimer");
            url.Add("r/reinhgr01"); url.Add("Griffin Reinhart");
            url.Add("r/reinhsa01"); url.Add("Sam Reinhart");
            url.Add("r/rendubo01"); url.Add("Borna Rendulic");
            url.Add("r/ribeimi01"); url.Add("Mike Ribeiro");
            url.Add("r/richabr01"); url.Add("Brad Richards");
            url.Add("r/richami02"); url.Add("Mike Richards");
            url.Add("r/richabr02"); url.Add("Brad Richardson");
            url.Add("r/riedeto01"); url.Add("Tobias Rieder");
            url.Add("r/riellmo01"); url.Add("Morgan Rielly");
            url.Add("r/rinalza01"); url.Add("Zac Rinaldo");
            url.Add("r/rinnepe01"); url.Add("Pekka Rinne");
            url.Add("r/ristora01"); url.Add("Rasmus Ristolainen");
            url.Add("r/ritchni01"); url.Add("Nick Ritchie");
            url.Add("r/roussan01"); url.Add("Antoine Roussel");
            url.Add("r/rozsimi01"); url.Add("Michal Rozsival");
            url.Add("r/rundbda01"); url.Add("David Rundblad");
            url.Add("r/russekr01"); url.Add("Kris Russell");
            url.Add("r/rustbr01"); url.Add("Bryan Rust");
            url.Add("r/ruututu01"); url.Add("Tuomo Ruutu");
            url.Add("r/ryanbo01"); url.Add("Bobby Ryan");
            url.Add("r/rycheke01"); url.Add("Kerby Rychel");
            url.Add("s/saadbr01"); url.Add("Brandon Saad");
            url.Add("s/salommi01"); url.Add("Miikka Salomaki");
            url.Add("s/samueph01"); url.Add("Philip Samuelsson");
            url.Add("s/santomi01"); url.Add("Mike Santorelli");
            url.Add("s/sarosju01"); url.Add("Juuse Saros");
            url.Add("s/savarda01"); url.Add("David Savard");
            url.Add("s/sbisalu01"); url.Add("Luca Sbisa");
            url.Add("s/scandma01"); url.Add("Marco Scandella");
            url.Add("s/scevico01"); url.Add("Colton Sceviour");
            url.Add("s/schalti01"); url.Add("Tim Schaller");
            url.Add("s/scheima01"); url.Add("Mark Scheifele");
            url.Add("s/schenbr01"); url.Add("Brayden Schenn");
            url.Add("s/schenlu01"); url.Add("Luke Schenn");
            url.Add("s/schleda01"); url.Add("David Schlemko");
            url.Add("s/schmina01"); url.Add("Nate Schmidt");
            url.Add("s/schneco01"); url.Add("Cory Schneider");
            url.Add("s/schrojo01"); url.Add("Jordan Schroeder");
            url.Add("s/schulje02"); url.Add("Jeff Schultz");
            url.Add("s/schulju01"); url.Add("Justin Schultz");
            url.Add("s/schulni01"); url.Add("Nick Schultz");
            url.Add("s/schwaja01"); url.Add("Jaden Schwartz");
            url.Add("s/scottjo01"); url.Add("John Scott");
            url.Add("s/scrivbe01"); url.Add("Ben Scrivens");
            url.Add("s/scudero01"); url.Add("Rob Scuderi");
            url.Add("s/seabrbr01"); url.Add("Brent Seabrook");
            url.Add("s/sedinda01"); url.Add("Daniel Sedin");
            url.Add("s/sedinhe01"); url.Add("Henrik Sedin");
            url.Add("s/seguity01"); url.Add("Tyler Seguin");
            url.Add("s/seidede01"); url.Add("Dennis Seidenberg");
            url.Add("s/sekacji01"); url.Add("Jiri Sekac");
            url.Add("s/sekeran01"); url.Add("Andrej Sekera");
            url.Add("s/seminal01"); url.Add("Alexander Semin");
            url.Add("s/severda01"); url.Add("Damon Severson");
            url.Add("s/sharppa01"); url.Add("Patrick Sharp");
            url.Add("s/shattke01"); url.Add("Kevin Shattenkirk");
            url.Add("s/shawan01"); url.Add("Andrew Shaw");
            url.Add("s/shawlo01"); url.Add("Logan Shaw");
            url.Add("s/sheahri01"); url.Add("Riley Sheahan");
            url.Add("s/shearco01"); url.Add("Conor Sheary");
            url.Add("s/shinkhu01"); url.Add("Hunter Shinkaruk");
            url.Add("s/shorede01"); url.Add("Devin Shore");
            url.Add("s/shoreni01"); url.Add("Nick Shore");
            url.Add("s/silfvja01"); url.Add("Jakob Silfverberg");
            url.Add("s/sillza01"); url.Add("Zach Sill");
            url.Add("s/simmowa01"); url.Add("Wayne Simmonds");
            url.Add("s/sislomi01"); url.Add("Mike Sislo");
            url.Add("s/sissoco01"); url.Add("Colton Sissons");
            url.Add("s/skillja01"); url.Add("Jack Skille");
            url.Add("s/skinnje01"); url.Add("Jeff Skinner");
            url.Add("s/skjeibr01"); url.Add("Brady Skjei");
            url.Add("s/slavija01"); url.Add("Jaccob Slavin");
            url.Add("s/slepyan01"); url.Add("Anton Slepyshev");
            url.Add("s/smidla01"); url.Add("Ladislav Smid");
            url.Add("s/smithbe01"); url.Add("Ben Smith");
            url.Add("s/smithbr05"); url.Add("Brendan Smith");
            url.Add("s/smithcr01"); url.Add("Craig Smith");
            url.Add("s/smithmi01"); url.Add("Mike Smith");
            url.Add("s/smithre01"); url.Add("Reilly Smith");
            url.Add("s/smithza01"); url.Add("Zack Smith");
            url.Add("s/smithde06"); url.Add("Devante Smith-Pelly");
            url.Add("s/soderca01"); url.Add("Carl Soderberg");
            url.Add("s/spalini01"); url.Add("Nick Spaling");
            url.Add("s/sparkga01"); url.Add("Garret Sparks");
            url.Add("s/spezzja01"); url.Add("Jason Spezza");
            url.Add("s/spoonry01"); url.Add("Ryan Spooner");
            url.Add("s/spronda01"); url.Add("Daniel Sprong");
            url.Add("s/spurgja01"); url.Add("Jared Spurgeon");
            url.Add("s/staaler01"); url.Add("Eric Staal");
            url.Add("s/staaljo01"); url.Add("Jordan Staal");
            url.Add("s/staalma01"); url.Add("Marc Staal");
            url.Add("s/staffdr01"); url.Add("Drew Stafford");
            url.Add("s/stajama01"); url.Add("Matt Stajan");
            url.Add("s/stalbvi01"); url.Add("Viktor Stalberg");
            url.Add("s/staloal01"); url.Add("Alex Stalock");
            url.Add("s/stamkst01"); url.Add("Steven Stamkos");
            url.Add("s/stantry01"); url.Add("Ryan Stanton");
            url.Add("s/stastpa01"); url.Add("Paul Stastny");
            url.Add("s/steenal01"); url.Add("Alex Steen");
            url.Add("s/stemple01"); url.Add("Lee Stempniak");
            url.Add("s/stepade01"); url.Add("Derek Stepan");
            url.Add("s/stephch02"); url.Add("Chandler Stephenson");
            url.Add("s/stewach02"); url.Add("Chris Stewart");
            url.Add("s/stollja01"); url.Add("Jarret Stoll");
            url.Add("s/stonema01"); url.Add("Mark Stone");
            url.Add("s/stonemi01"); url.Add("Michael Stone");
            url.Add("s/stonecl01"); url.Add("Clayton Stoner");
            url.Add("s/stracty01"); url.Add("Tyson Strachan");
            url.Add("s/straibr01"); url.Add("Brian Strait");
            url.Add("s/stralan01"); url.Add("Anton Stralman");
            url.Add("s/streebe01"); url.Add("Ben Street");
            url.Add("s/streima01"); url.Add("Mark Streit");
            url.Add("s/stromry01"); url.Add("Ryan Strome");
            url.Add("s/stuarbr02"); url.Add("Brad Stuart");
            url.Add("s/stuarma01"); url.Add("Mark Stuart");
            url.Add("s/subbapk01"); url.Add("P.K. Subban");
            url.Add("s/summech01"); url.Add("Chris Summers");
            url.Add("s/sustran01"); url.Add("Andrej Sustr");
            url.Add("s/suterry01"); url.Add("Ryan Suter");
            url.Add("s/suttebr03"); url.Add("Brandon Sutter");
            url.Add("s/svedbvi01"); url.Add("Viktor Svedberg");
            url.Add("t/talboca01"); url.Add("Cam Talbot");
            url.Add("t/talboma01"); url.Add("Maxime Talbot");
            url.Add("t/tanevch01"); url.Add("Chris Tanev");
            url.Add("t/tangrer01"); url.Add("Eric Tangradi");
            url.Add("t/tangual01"); url.Add("Alex Tanguay");
            url.Add("t/taormma01"); url.Add("Matt Taormina");
            url.Add("t/tarasvl01"); url.Add("Vladimir Tarasenko");
            url.Add("t/tatarto01"); url.Add("Tomas Tatar");
            url.Add("t/tavarjo01"); url.Add("John Tavares");
            url.Add("t/tennyma01"); url.Add("Matt Tennyson");
            url.Add("t/teravte01"); url.Add("Teuvo Teravainen");
            url.Add("t/terrych01"); url.Add("Chris Terry");
            url.Add("t/theodsh01"); url.Add("Shea Theodore");
            url.Add("t/thomach01"); url.Add("Christian Thomas");
            url.Add("t/thompna01"); url.Add("Nate Thompson");
            url.Add("t/thomppa02"); url.Add("Paul Thompson");
            url.Add("t/thorbch01"); url.Add("Chris Thorburn");
            url.Add("t/thornjo01"); url.Add("Joe Thornton");
            url.Add("t/thornsh01"); url.Add("Shawn Thornton");
            url.Add("t/tiernch01"); url.Add("Chris Tierney");
            url.Add("t/tikhovi01"); url.Add("Viktor Tikhonov");
            url.Add("t/tinorja01"); url.Add("Jarred Tinordi");
            url.Add("t/tlustji01"); url.Add("Jiri Tlusty");
            url.Add("t/toewsjo01"); url.Add("Jonathan Toews");
            url.Add("t/toffoty01"); url.Add("Tyler Toffoli");
            url.Add("t/tokardu01"); url.Add("Dustin Tokarski");
            url.Add("t/tootojo01"); url.Add("Jordin Tootoo");
            url.Add("t/trochvi01"); url.Add("Vincent Trocheck");
            url.Add("t/trotmza01"); url.Add("Zach Trotman");
            url.Add("t/troubja01"); url.Add("Jacob Trouba");
            url.Add("t/turriky01"); url.Add("Kyle Turris");
            url.Add("t/tyutife01"); url.Add("Fedor Tyutin");
            url.Add("u/ullmali01"); url.Add("Linus Ullmark");
            url.Add("u/umberrj01"); url.Add("R.J. Umberger");
            url.Add("u/upshasc01"); url.Add("Scottie Upshall");
            url.Add("v/vanrija01"); url.Add("James van Riemsdyk");
            url.Add("v/vanritr01"); url.Add("Trevor van Riemsdyk");
            url.Add("v/vandech01"); url.Add("Chris VandeVelde");
            url.Add("v/vanekth01"); url.Add("Thomas Vanek");
            url.Add("v/varlasi01"); url.Add("Semyon Varlamov");
            url.Add("v/varonph01"); url.Add("Philip Varone");
            url.Add("v/vasilan02"); url.Add("Andrei Vasilevskiy");
            url.Add("v/vatansa01"); url.Add("Sami Vatanen");
            url.Add("v/vatrafr01"); url.Add("Frank Vatrano");
            url.Add("v/vermean01"); url.Add("Antoine Vermette");
            url.Add("v/vermijo01"); url.Add("Joel Vermin");
            url.Add("v/verstkr01"); url.Add("Kris Versteeg");
            url.Add("v/veyli01"); url.Add("Linden Vey");
            url.Add("v/virtaja01"); url.Add("Jake Virtanen");
            url.Add("v/vitaljo01"); url.Add("Joe Vitale");
            url.Add("v/vlasima01"); url.Add("Marc-Edouard Vlasic");
            url.Add("v/voracja01"); url.Add("Jakub Voracek");
            url.Add("v/vrbatra01"); url.Add("Radim Vrbata");
            url.Add("w/wagnech01"); url.Add("Chris Wagner");
            url.Add("w/wardca01"); url.Add("Cam Ward");
            url.Add("w/wardjo02"); url.Add("Joel Ward");
            url.Add("w/warsoda01"); url.Add("David Warsofsky");
            url.Add("w/watsoau01"); url.Add("Austin Watson");
            url.Add("w/wealjo01"); url.Add("Jordan Weal");
            url.Add("w/webermi01"); url.Add("Mike Weber");
            url.Add("w/webersh01"); url.Add("Shea Weber");
            url.Add("w/weberya01"); url.Add("Yannick Weber");
            url.Add("w/weiseda01"); url.Add("Dale Weise");
            url.Add("w/welshje01"); url.Add("Jeremy Welsh");
            url.Add("w/wennbal01"); url.Add("Alexander Wennberg");
            url.Add("w/wheelbl01"); url.Add("Blake Wheeler");
            url.Add("w/whitery01"); url.Add("Ryan White");
            url.Add("w/widemch01"); url.Add("Chris Wideman");
            url.Add("w/widemde01"); url.Add("Dennis Wideman");
            url.Add("w/wiercpa01"); url.Add("Patrick Wiercioch");
            url.Add("w/willro01"); url.Add("Roman Will");
            url.Add("w/williju01"); url.Add("Justin Williams");
            url.Add("w/wilsoco01"); url.Add("Colin Wilson");
            url.Add("w/wilsoga01"); url.Add("Garrett Wilson");
            url.Add("w/wilsosc01"); url.Add("Scott Wilson");
            url.Add("w/wilsoto01"); url.Add("Tom Wilson");
            url.Add("w/wingeto01"); url.Add("Tommy Wingels");
            url.Add("w/winnida01"); url.Add("Daniel Winnik");
            url.Add("w/wisnija01"); url.Add("James Wisniewski");
            url.Add("w/witkolu01"); url.Add("Luke Witkowski");
            url.Add("y/yakupna01"); url.Add("Nail Yakupov");
            url.Add("y/yandlke01"); url.Add("Keith Yandle");
            url.Add("z/zadorni01"); url.Add("Nikita Zadorov");
            url.Add("z/zajactr01"); url.Add("Travis Zajac");
            url.Add("z/zalewmi01"); url.Add("Michael Zalewski");
            url.Add("z/zatkoje01"); url.Add("Jeff Zatkoff");
            url.Add("z/zettehe01"); url.Add("Henrik Zetterberg");
            url.Add("z/zibanmi01"); url.Add("Mika Zibanejad");
            url.Add("z/zidlima01"); url.Add("Marek Zidlicky");
            url.Add("z/zubruda01"); url.Add("Dainius Zubrus");
            url.Add("z/zuccama01"); url.Add("Mats Zuccarello");
            url.Add("z/zuckeja01"); url.Add("Jason Zucker");

            List<List<string>> list = new List<List<string>>();
            List<string> item;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlNode root;
            int c;
            int i, k;

            DateTime dtStart = DateTime.Now;

            for (c = 0; c < url.Count; c+=2)
            {

                string strURL = "http://www.hockey-reference.com/players/"+url[c].ToString()+"/gamelog/2016/";

                bool bLoaded = false;

                while (!bLoaded)
                {
                    try
                    {
                        TimeSpan diff = DateTime.Now - dtStart;
                        string strRemain = "?";
                        if (c > 0)
                        {
                            strRemain = (diff.TotalMinutes * (double)url.Count / (double)c).ToString(".2") + " minutes";
                        }
                        
                        System.Diagnostics.Debug.WriteLine((c/2+1).ToString() + "/" + (url.Count/2).ToString() + " ("+strRemain+"): " + strURL);
                        doc.LoadHtml(new System.Net.WebClient().DownloadString(strURL));
                        bLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        continue;
                    }
                }

                root = doc.DocumentNode;
                List<HtmlAgilityPack.HtmlNode> table = root.Descendants("table").Where(n => n.GetAttributeValue("id", "").Equals("gamelog")).ToList();
                List<HtmlAgilityPack.HtmlNode> trs;

                if(table.Count == 0)
                {
                    continue;
                }
                else
                {
                    trs = table[0].Descendants("tr").ToList();
                }

                DateTime dtFrom = new DateTime(2015, 10, 7);
                DateTime dtTo = new DateTime(2016, 1, 15);

                for (i = 0; i < trs.Count; i++)
                {
                    List<HtmlAgilityPack.HtmlNode> tds = trs[i].Descendants("td").ToList();

                    if (tds.Count == 29 || tds.Count == 16)
                    {
                        string strDate = tds[3].InnerText.Trim();
                        DateTime dt;
                        if (DateTime.TryParse(strDate, out dt))
                        {
                            if (dt >= dtFrom && dt <= dtTo)
                            {
                                item = new List<string>();

                                // Add the player name
                                item.Add(url[c + 1]);

                                for (k = 0; k < tds.Count; k++)
                                {
                                    item.Add(tds[k].InnerText.Trim());
                                }

                                //list.Add(item);

                                if (tds.Count == 29)
                                {
                                    appendToFile(item, "GameDataSkater.txt");
                                }
                                else
                                {
                                    appendToFile(item, "GameDataGoalie.txt");
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }



                    }
                    else if (tds.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Odd width?");
                    }
                }
            }

            //writeToFile(list, "GameData.txt");

            System.Diagnostics.Debug.WriteLine("Done.");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //http://shiny.war-on-ice.net/gamesummary4/?seasongcode=2015201620770
            //DataTables_Table_1
            List<List<string>> list = new List<List<string>>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            int i;
            //int n, k;
            
            int nGameID;
            string strGameID;

            games = new List<string>();
            nCurrentGame = 0;

            // Grab IDS from the database
            try
            {
                using (SqlConnection con = new SqlConnection(@"Server=localhost;Database=NHL;User Id=sa;Password=white;"))
                {
                    con.Open();

                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT DISTINCT CASE WHEN [Game ID] > 20000 THEN [Game ID] - 20000 ELSE [Game ID] END as [Game ID] FROM [Passing]";

                        using (SqlDataReader rs = cmd.ExecuteReader())
                        {
                            while (rs.Read())
                            {
                                nGameID = rs.GetInt32(rs.GetOrdinal("Game ID"));
                                strGameID = nGameID.ToString();
                                strGameID = strGameID.PadLeft(3, '0');
                                games.Add(strGameID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            textBox1.Text = "Total games to download: " + games.Count;
            textBox1.Text += "\r\nOutput file: " + System.IO.Path.GetFullPath("merged.csv");
            textBox1.Refresh();

            pt = new Point(1080, 155);
            pt = PointToScreen(pt);

            Start(this);

            //return;
            return;      

            //doc.DocumentNode.ToString();
        }

        void MergeWOICSVs()
        {
            games = new List<string>();
            nCurrentGame = 0;

            int i, n, k;
            int nGameID;
            string strGameID;

            // Grab IDS from the database
            try
            {
                using (SqlConnection con = new SqlConnection(@"Server=localhost;Database=NHL;User Id=sa;Password=white;"))
                {
                    con.Open();

                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT DISTINCT CASE WHEN [Game ID] > 20000 THEN [Game ID] - 20000 ELSE [Game ID] END as [Game ID] FROM [Passing]";

                        using (SqlDataReader rs = cmd.ExecuteReader())
                        {
                            while (rs.Read())
                            {
                                nGameID = rs.GetInt32(rs.GetOrdinal("Game ID"));
                                strGameID = nGameID.ToString();
                                strGameID = strGameID.PadLeft(3, '0');
                                games.Add(strGameID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            List<List<string>> output = new List<List<string>>();
            List<string> line;

            for (i = 0; i < games.Count; i++)
            {
                List<List<string>> csv = BlackBox.CSV.parseCSV(games[i] + ".csv");

                if (i == 0)
                {
                    line = new List<string>();
                    for (k = 0; k < csv[0].Count; k++)
                    {
                        line.Add(csv[0][k]);
                    }
                    line.Add("GameID");
                    output.Add(line);
                }

                for (k = 1; k < csv.Count; k++)
                {
                    line = new List<string>();
                    for (n = 0; n < csv[k].Count; n++)
                    {
                        if (csv[k][n].ToUpper() == "NA")
                        {
                            line.Add("");
                        }
                        else
                        {
                            line.Add(csv[k][n]);
                        }
                    }
                    line.Add("20" + games[i]);
                    output.Add(line);
                }
            }

            BlackBox.CSV.serializeCSV("merged.csv", output, false);
        }

        static System.Timers.Timer _timer; // From System.Timers
        static int nState;
        static void Start(Form1 a)
        {
            nState = 0;
            _timer = new System.Timers.Timer(15000);
            _timer.Elapsed += (sender, e) => { HandleTimerElapsed(a); };  
            _timer.Enabled = true; // Enable it
            
        }

        static void HandleTimerElapsed(Form1 a)
        {
            if (nState == 0)
            {
                string strFile = games[nCurrentGame] + ".csv";
                while (System.IO.File.Exists(strFile))
                {
                    nCurrentGame++;
                    strFile = games[nCurrentGame] + ".csv";
                }

                string strURL = "http://shiny.war-on-ice.net/gamesummary4/?seasongcode=2015201620" + games[nCurrentGame];
                browser.Load(strURL);
            }
            else if (nState == 1)
            {
                // Screen position of the dropdown we want to manipulate.                
                LeftMouseClick(pt.X, pt.Y);
                System.Threading.Thread.Sleep(500);
                LeftMouseClick(pt.X, pt.Y + 120);
            }
            else if (nState == 2)
            {
                StringBuilder sb = new StringBuilder();

                sb.Clear();
                sb.AppendLine("function downloadFile() {");
                sb.AppendLine("     return document.getElementById(\"downloadData\").getAttribute(\"href\");");
                sb.AppendLine("}");
                sb.AppendLine("downloadFile();");

                var task = browser.EvaluateScriptAsync(sb.ToString());

                System.Threading.Tasks.Task continuation = task.ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        var response = t.Result;

                        if (response.Success == true)
                        {
                            using (var client = new System.Net.WebClient())
                            {

                                client.DownloadFile("http://shiny.war-on-ice.net/gamesummary4/" + response.Result.ToString(), games[nCurrentGame] + ".csv");
                            }
                        }
                    }
                });

                continuation.Wait();

                nCurrentGame++;
            }

            nState++;
            nState %= 3;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            MergeWOICSVs();
        }
    }
}
