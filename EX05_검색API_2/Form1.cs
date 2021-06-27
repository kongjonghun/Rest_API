using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// Namespace 추가
using AxMapWinGIS;
using MapWinGIS;
using System.IO; //Input, Output : 파일을 읽고 쓰는 기능 지원
using RestSharp;
using Newtonsoft.Json.Linq;

namespace EX05_검색API_2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 맵좌표계설정
            axMap1.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;
            // 배경맵적용(타일맵)
            axMap1.TileProvider = tkTileProvider.OpenStreetMap;
            // 맵초기공간범위
            axMap1.KnownExtents = tkKnownExtents.keSouth_Korea;
        }
    }
}
