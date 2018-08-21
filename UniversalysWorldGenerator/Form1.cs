using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace UniversalysWorldGenerator
{
    public partial class Form1 : Form
    {

            WorldMap map = new WorldMap();

        public Form1()
        {
            InitializeComponent();

        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            InformationDisplay.Text = "";
            Random dice = new Random();

            map.GenerateRegions();
            map.DrawRegionMap();

            map.GenerateLandmass();
            map.DrawHeightMap();

            map.GenerateTemperature();
            map.DrawTemperatureMap();

            MessageBox.Show("Fini !");
            //Image img = Image.FromFile(@"C:\Users\Stagiaire.TAZ\source\repos\UniversalysWorldGenerator\UniversalysWorldGenerator\map\mapTemperature.png");
            pictureBox1.Load(@"C:\Users\Stagiaire.TAZ\source\repos\UniversalysWorldGenerator\UniversalysWorldGenerator\map\mapTemperature.png");

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var relativePoint = this.PointToClient(Cursor.Position);
            InformationDisplay.Text += "{X = " + relativePoint.X + " : Y = " + relativePoint.Y + "}" + Environment.NewLine;
            InformationDisplay.Text +=map.CursorPosition(relativePoint.X, relativePoint.Y);
            InformationDisplay.Text += Environment.NewLine;
            InformationDisplay.SelectionStart = InformationDisplay.Text.Length;
            InformationDisplay.ScrollToCaret();

        }
    }
}
