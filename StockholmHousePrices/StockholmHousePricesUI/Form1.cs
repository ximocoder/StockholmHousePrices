using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using HtmlAgilityPack;
using SQLite;

namespace StockholmHousePricesUI
{
    public partial class FormPrices : Form
    {
        public FormPrices()
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ButtonScrap_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
            this.richTextBox1.Update();
            this.Scrap();
        }

        private void Scrap()
        {
            var html = @"https://www.hemnet.se/bostader?location_ids%5B%5D=17744&living_area_min=80&fee_max=7000&price_max=2000000";

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(html);

            var pagename = htmlDoc.DocumentNode.SelectSingleNode("//head/title");

            var houses = htmlDoc.DocumentNode.SelectNodes("//div[@data-item-id]");

            List<House> housesList = new List<House>();
            foreach (var house in houses)
            {
                House houseObject = new House();

                var id = int.Parse(house.SelectSingleNode(".//img[@class='property-image']").Attributes.AttributesWithName("onerror").FirstOrDefault().DeEntitizeValue.Split('"')[5].Replace("ID", ""));
                var prize = house.SelectSingleNode(".//li[@class='price item-result-meta-attribute-is-bold']").InnerText.Trim();
                var size = house.SelectSingleNode(".//li[@class='living-area item-result-meta-attribute-is-bold']").InnerText.Trim();
                var city = house.SelectSingleNode(".//li[@class='city item-result-meta-attribute-subtle']").InnerText.Trim();
                var rooms = house.SelectSingleNode(".//li[@class='rooms item-result-meta-attribute-is-bold']").InnerText.Trim();
                var pricem2 = house.SelectSingleNode(".//li[@class='price-per-m2 item-result-meta-attribute-subtle']").InnerText.Trim();
                var area = house.SelectSingleNode(".//li[@class='area item-result-meta-attribute-subtle']").InnerText.Trim();

                houseObject.HouseId = id;
                houseObject.Name = id.ToString();
                houseObject.Prize = int.Parse(prize.Replace("kr", "").Trim().Replace(" ", string.Empty));
                houseObject.Size = size;
                houseObject.Area = area;
                houseObject.Location = city;
                houseObject.MapLocation = "";
                houseObject.DistanceFromCenter = 0;
                houseObject.DaysInTheMarket = 0;
                houseObject.AvGift = 0;
                houseObject.NumberOfRooms = 0;
                houseObject.HasBids = false;
                houseObject.LastBid = 0;
                houseObject.Comments = "";
                houseObject.Created = DateTime.Now;

                housesList.Add(houseObject);
            }

            var prices = houses.Descendants();

            // Get an absolute path to the database file
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HousePrices.db");
            var db = new SQLiteConnection(databasePath);

            Console.WriteLine("Node Name: " + pagename.Name + "\n" + pagename.OuterHtml);

            foreach (var ohouse in housesList)
            {
                string line = "House: " + ohouse.Id.ToString() + " - Price " + ohouse.Prize + " - location: " +
                              ohouse.Location + " / " + ohouse.Area + " - size: " + ohouse.Size +
                              " - link: " + " https://www.hemnet.se/bostad/" + ohouse.Id.ToString();
                Console.WriteLine(line);
                this.richTextBox1.Text += line + Environment.NewLine;
                if (this.chkSaveToDatabase.Checked)
                    this.AddHouse(db, ohouse);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Get an absolute path to the database file
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HousePrices.db");

            var db = new SQLiteConnection(databasePath);
            db.CreateTable<House>();
            //db.CreateTable<Valuation>();
           
            var query = db.Table<House>();
            //var remove = db.Table<House>().Delete(x => x.Id > 0);
        }

        private void AddHouse(SQLiteConnection db, House house)
        {
            var s = db.Insert(house);
        }
    }


    public class House
    {
        [PrimaryKey][AutoIncrement]
        public int Id { get; set; }
        public int HouseId { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }
        public int Prize { get; set; }
        public string Size { get; set; }
        public string Location { get; set; }
        public string MapLocation { get; set; }
        public int DistanceFromCenter { get; set; }
        public int DaysInTheMarket { get; set; }
        public int AvGift { get; set; }
        public int NumberOfRooms { get; set; }
        public bool HasBids { get; set; }
        public int LastBid { get; set; }
        public string Comments { get; set; }
        public DateTime Created { get; set; }
    }
}


