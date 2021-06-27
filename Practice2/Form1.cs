using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// Namespac 추가
using AxMapWinGIS;
using MapWinGIS;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace Practice2
{
    public partial class Form1 : Form
    {
        JArray items;
        int layerHandle;
        Utils u = new Utils();
        object result;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            axMap1.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;
            axMap1.TileProvider = tkTileProvider.OpenStreetMap;
            axMap1.KnownExtents = tkKnownExtents.keSouth_Korea;
            axMap1.SendSelectBoxFinal = true;
        }

        private void axMap1_ProjectionMismatch(object sender, _DMapEvents_ProjectionMismatchEvent e)
        {
            e.reproject = tkMwBoolean.blnTrue;
        }

        private void lstSearch_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            axMap1.RemoveAllLayers();
            dgvData.DataSource = null;

            var item = items[lstSearch.SelectedIndex];

            Shapefile sf = new Shapefile();
            sf.CreateNew("", ShpfileType.SHP_POINT);

            Shape shp = new Shape();
            shp.Create(ShpfileType.SHP_POINT);
            MapWinGIS.Point p = new MapWinGIS.Point();
            p.x = Convert.ToDouble(item["point"]["x"]);
            p.y = Convert.ToDouble(item["point"]["y"]);

            int pointindex = shp.numPoints;
            shp.InsertPoint(p, ref pointindex);

            int shpindex = sf.NumShapes;
            sf.EditInsertShape(shp, ref shpindex);

            sf.GeoProjection.ImportFromEPSG(4326);

            layerHandle = axMap1.AddLayer(sf, true);
            Shapefile sf2 = axMap1.get_Shapefile(layerHandle);
            sf2.DefaultDrawingOptions.PointSize = 15;
            sf2.DefaultDrawingOptions.PointShape = tkPointShapeType.ptShapeStar;

            /////// DataBase 연결
            //// 2. 해당 포인트의 500m 범위 내에 존재하는 따릉이 대여소들을 맵에 레이어로 가시화 
            string connStr = "PG:host=localhost dbname=gis user=postgres password=8270 port=5433";
            string cmd;
            cmd = string.Format("select * from public_bicycle where ST_Within(ST_Transform(geom, 5186), ST_Buffer(ST_Transform(ST_SetSRID(ST_MakePoint({0}, {1}), 4326), 5186), 500))", p.x, p.y);
            layerHandle = axMap1.AddLayerFromDatabase(connStr, cmd, true);

            axMap1.set_LayerName(layerHandle, "500m_따릉이대여소");
            Shapefile sf3 = axMap1.get_Shapefile(layerHandle);
            sf3.DefaultDrawingOptions.PointSize = 15;
            sf3.DefaultDrawingOptions.PointShape = tkPointShapeType.ptShapeCircle;
            double min = Convert.ToDouble(sf3.Table.MinValue[7]);
            double max = Convert.ToDouble(sf3.Table.MaxValue[7]);
            sf3.Categories.AddRange(7, tkClassificationType.ctNaturalBreaks, 3, min, max);
            sf3.Categories.get_Item(0).DrawingOptions.FillColor = u.ColorByName(tkMapColor.White);
            sf3.Categories.get_Item(1).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Crimson);
            sf3.Categories.get_Item(2).DrawingOptions.FillColor = u.ColorByName(tkMapColor.DarkRed);
            sf3.Categories.ApplyExpressions();

            CreateDataTable();
        }

        private void CreateDataTable()
        {
            DataTable dt = new DataTable();
            Shapefile sf = axMap1.get_Shapefile(layerHandle);
            //// DataTable 컬럼 생성                  
            for (int i = 0; i < sf.NumFields; i++)
            {
                DataColumn dc = new DataColumn();
                dc.ColumnName = sf.Table.Field[i].Name;
                dt.Columns.Add(dc);
            }

            //// DataTable 로우 생성
            // Shaplefile.get_CellValue(컬럼 인덱스(컬럼), 피쳐 id(로우)) 활용
            for (int i = 0; i < sf.NumShapes; i++) // 행 개수
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < sf.NumFields; j++) // 컬럼개수
                {
                    if (sf.get_CellValue(j, i) != null) // null 경우 제외
                    {
                        string val = sf.get_CellValue(j, i).ToString();
                        dr[j] = val;
                    }
                }
                dt.Rows.Add(dr); // 테이블 로우 추가
            }
            // DGV의 데이터소스 할당
            dgvData.DataSource = dt;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            lstSearch.Items.Clear();  // listbox 초기화            

            string target = tbSearch.Text;
            string url = string.Format("http://api.vworld.kr/req/search?service=search&request=search&version=2.0&crs=EPSG:4326&size=1000&page=1&query={0}&type=place&format=json&errorformat=json&key=4DC3C0AD-CFCA-3277-8293-B13CADFB6A7A", target);

            var client = new RestClient(url);           // RestClient : 요청(url)을 서비스제공자(서버)에게 전달하는 객체
            client.Timeout = -1;                       // 요청 시간이 너무 길면 요청 끊어라
            var request = new RestRequest(Method.GET); // 요청 처리 방식 : GET방식 : 데이터 얻기만
            IRestResponse response = client.Execute(request); // Client 일해라

            var result = JObject.Parse(response.Content);
            items = (JArray)result["response"]["result"]["items"]; // JSON 배열 , var = JArray           

            foreach (var item in items) // var = JObject
            {
                lstSearch.Items.Add(item["title"]);
            }
        }

        private void btnPan_Click(object sender, EventArgs e)
        {
            axMap1.CursorMode = tkCursorMode.cmPan;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            axMap1.CursorMode = tkCursorMode.cmSelection;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dgvData.DataSource = null;
            lstSearch.Items.Clear();
            Shapefile sf = axMap1.get_Shapefile(layerHandle);
            sf.SelectNone();
        }

        private void axMap1_SelectBoxFinal(object sender, _DMapEvents_SelectBoxFinalEvent e)
        {
            dgvData.DataSource = null;
            Shapefile sf = axMap1.get_Shapefile(layerHandle);
            result = null;
            int[] shapes = null;

            if (sf != null)
            {
                double left = 0.0;
                double top = 0.0;
                double bottom = 0.0;
                double right = 0.0;

                axMap1.PixelToProj(e.left, e.top, ref left, ref top);
                axMap1.PixelToProj(e.right, e.bottom, ref right, ref bottom);

                Extents ext = new Extents();

                ext.SetBounds(left, bottom, 0.0, right, top, 0.0);

                sf.SelectNone();

                // 마우스의 선택 범위에 따라 레이어에서 피쳐 선택
                // Shapefile.SelectShapes(공간 범위, 톨러런스, 선택모드, 피쳐 id를 리턴할 오브젝트)
                if (sf.SelectShapes(ext, 0.0, SelectMode.INTERSECTION, ref result))
                {
                    // 피쳐 id 집합(result) :정수형 배열(shapes)에 저장
                    shapes = result as int[];
                    if (shapes == null) return;

                    // set_ShapeSelected(피쳐 id, 하이라이트 여부)
                    for (int i = 0; i < shapes.Length; i++)
                    {
                        // 피쳐 하이라이트
                        sf.set_ShapeSelected(shapes[i], true);
                    }
                }

                /*   //// DGV의 DataTable 생성 및 데이터 할당  
                   // 비어 있는 데이터 테이블 생성
                   DataTable dt_SelectedFeatures = new DataTable();

                   //// DataTable 컬럼 생성      
                   // Shaplefile.Table 속성 활용
                   for (int i = 0; i < sf.NumFields; i++)
                   {
                       DataColumn dc = new DataColumn();
                       dc.ColumnName = sf.Table.Field[i].Name;
                       dt_SelectedFeatures.Columns.Add(dc); // 테이블 컬럼 추가
                   }

                   //// DataTable 로우 생성
                   // Shaplefile.get_CellValue(컬럼 인덱스, 피쳐 id) 활용
                   for (int i = 0; i < shapes.Length; i++) // 행
                   {
                       DataRow dr = dt_SelectedFeatures.NewRow();
                       for (int j = 0; j < sf.NumFields; j++) // 열
                       {
                           string val = sf.get_CellValue(j, shapes[i]).ToString();
                           dr[j] = val;
                       }
                       dt_SelectedFeatures.Rows.Add(dr); // 테이블 로우 추가
                   }
                   // DGV의 데이터소스 할당
                   dgvData.DataSource = dt_SelectedFeatures;
               }*/
                axMap1.Redraw(); // 맵 컨트롤 refresh
            }
        }
    }
}
