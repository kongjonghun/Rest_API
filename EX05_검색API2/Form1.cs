using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxMapWinGIS;
using MapWinGIS;
using System.IO; //Input, Output : 파일을 읽고 쓰는 기능 지원
using RestSharp;
using Newtonsoft.Json.Linq;

//// ListBox의 아이템 더블 클릭 시,
//// 1. 해당 데이터만 공간 데이터로 생성하고 레이어 추가
//// 2. 해당 포인트의 500m 범위 내에 존재하는 따릉이 대여소들을 맵에 레이어로 가시화
namespace EX05_검색API2
{
    public partial class Form1 : Form
    {
        JArray items;
        int layerHandle;
        Shapefile bufferPoints;     
        
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
        // 검색 버튼 클릭 시 : 리스트박스에 내역들 추가
        private void btnSearch_Click(object sender, EventArgs e)
        {
            // 초기화
            lstSearch.Items.Clear();  // listbox 초기화            

            // 사용자가 텍스트 박스에 입력한 값 : 텍스트 프로퍼티
            string target = tbSearch.Text;
            // API 호출 : RestSharp, Newtonsoft.Json.Linq 패키지 설치
            // 전달하고자 하는 url : query={0} : target
            string url = string.Format("http://api.vworld.kr/req/search?service=search&request=search&version=2.0&crs=EPSG:4326&size=1000&page=1&query={0}&type=place&format=json&errorformat=json&key=4DC3C0AD-CFCA-3277-8293-B13CADFB6A7A", target);

            // RestSharp을 이용한 API 요청 및 응답 처리
            var client = new RestClient(url);           // RestClient : 요청(url)을 서비스제공자(서버)에게 전달하는 객체
            client.Timeout = -1;                       // 요청 시간이 너무 길면 요청 끊어라
            var request = new RestRequest(Method.GET); // 요청 처리 방식 : GET방식 : 데이터 얻기만
            IRestResponse response = client.Execute(request); // Client 일해라

            //// 결과 데이터 파싱 : Json형태로 데이터를 받기 때문에, Parsing하여 변수에 저장
            // JObject : JSON타입의 객체
            var result = JObject.Parse(response.Content);
            // JSON 배열로 결과 저장, JSON 배열 = [JObject1, JObject2, ...JObjectN]  JObject = Json
            items = (JArray)result["response"]["result"]["items"]; // JSON 배열 , var = JArray           

            //// 리스트박스에 채우기
            // 리스트박스에 아이템 추가하기 : lstSearch.Items.Add("item");           
            foreach (var item in items) // var = JObject
            {
                lstSearch.Items.Add(item["title"]);
            }                         
        }
        private void axMap1_ProjectionMismatch(object sender, _DMapEvents_ProjectionMismatchEvent e)
        {
            e.reproject = tkMwBoolean.blnTrue;
        }

        //// ListBox의 아이템 더블 클릭 시,
        //// 1. 해당 데이터만 공간 데이터로 생성하고 레이어 추가 (API)
        //// 2. 해당 포인트의 500m 범위 내에 존재하는 따릉이 대여소들을 맵에 레이어로 가시화 (DB)
        //// 3. 버퍼 지역에 포함된 대여소 중 현재 위치와 가장 가까운 대여소 하이라이트!
        //// 4. 해당 아이템 Zoom
        //// 5. DataTable 생성
        private void lstSearch_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            axMap1.RemoveAllLayers(); // Map Layer 초기화
            dgvData.DataSource = null;

            /////// API이용
            //// 1. 해당 데이터만 공간 데이터로 생성하고 레이어 추가
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

            // 포인트 레이어 추가 및 심볼라이징 : Layer 추가 후 -> 심볼라이징(커스터마이징)
            int center = axMap1.AddLayer(sf, true);
            Shapefile centerPoint = axMap1.get_Shapefile(center);
            centerPoint.DefaultDrawingOptions.PointSize = 15;
            centerPoint.DefaultDrawingOptions.PointShape = tkPointShapeType.ptShapeStar;

            /////// DataBase 연결
            //// 2. 해당 포인트의 500m 범위 내에 존재하는 따릉이 대여소들을 맵에 레이어로 가시화 
            string connStr = "PG:host=localhost dbname=gis user=postgres password=8270 port=5433";
            string cmd;
            cmd = string.Format("select * from public_bicycle where ST_Within(ST_Transform(geom, 5186), ST_Buffer(ST_Transform(ST_SetSRID(ST_MakePoint({0}, {1}), 4326), 5186), 500))", p.x, p.y);
            layerHandle = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(layerHandle, "500m_따릉이대여소");
            
            //// 시립대 → 서울역 dijkstra 알고리즘
            // select ST_Union(ST_Transform(x.geom, 4326)) from moct_link as x join (select *from pgr_dijkstra('select CAST(link_id as bigint) as id, CAST(f_node as bigint) as source, CAST(t_node as bigint) as target, ST_Length(geom) as cost from moct_link', 1050025400, 1020034700)) as y on CAST(x.link_id as bigint) = y.edge

            bufferPoints = axMap1.get_Shapefile(layerHandle);
            bufferPoints.DefaultDrawingOptions.PointSize = 15;
            bufferPoints.DefaultDrawingOptions.PointShape = tkPointShapeType.ptShapeCircle;

            //// 3. 버퍼 지역에 포함된 대여소 중 현재 위치와 가장 가까운 대여소 하이라이트!            
            // select * from public_bicycle where geom = (select ST_ClosestPoint(ST_Union(geom), ST_SetSRID(ST_MakePoint(127.06, 37.58), 4326)) from public_bicycle)
            cmd = string.Format("select * from public_bicycle where geom = (select ST_ClosestPoint(ST_Union(geom), ST_SetSRID(ST_MakePoint({0}, {1}), 4326)) from public_bicycle)", p.x, p.y);
            layerHandle = axMap1.AddLayerFromDatabase(connStr, cmd, false);
            axMap1.set_LayerName(layerHandle, "가장 가까운 대여소");

            Shapefile ColosestPoint = axMap1.get_Shapefile(layerHandle);
            // 레이어(Shapefile)간 속성 비교
            // 주변(500m) 따릉이 대여소 레이어(속성, 도형)와 가장 가까운 따릉이 대여소 레이어(속성, 도형)을 가지고 있다.
            // 두 레이어의 속성 비교(id)를 통해, 가장 가까운 따릉이 대여소를 하이라이트 하자!            
            // get_CellValue(컬럼 인덱스(열), shape 인덱스(행))            
            string filter = ColosestPoint.get_CellValue(0, 0).ToString();

            for (int i = 0; i < bufferPoints.NumShapes; i++) // Row
            {                
                // i번째 피쳐의 ID 컬럼(0번째) 값
                string id = bufferPoints.get_CellValue(0, i).ToString();                
                if (id == filter)
                {
                    // 하이라이트
                    bufferPoints.set_ShapeSelected(i, true); // set_ShapeSelected(shape 인덱스, 하이라이트여부)
                }
            }
            // Listbox에 item 넣은 순서(Index) = Feature 
            // Listbox 순으로 Feature 입력
            //// 4. 해당 아이템 Zoom
            axMap1.ZoomToShape(0, lstSearch.SelectedIndex);
            axMap1.CurrentScale = 5000;

            //// 5. DataTable 생성
            CreateDataTable();
        }
        /// DataTable 생성 후 DGV에 할당
        private void CreateDataTable()
        {
            // DataTable을 정의
            DataTable dt = new DataTable();            

            //// DataTable 컬럼 생성                  
            for (int i = 0; i < bufferPoints.NumFields; i++)
            {
                DataColumn dc = new DataColumn();                
                dc.ColumnName = bufferPoints.Table.Field[i].Name;
                dt.Columns.Add(dc); 
            }

            //// DataTable 로우 생성
            // Shaplefile.get_CellValue(컬럼 인덱스(컬럼), 피쳐 id(로우)) 활용
            for (int i = 0; i < bufferPoints.NumShapes; i++) // 행 개수
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < bufferPoints.NumFields; j++) // 컬럼개수
                {
                    if(bufferPoints.get_CellValue(j, i) != null) // null 경우 제외
                    {
                        string val = bufferPoints.get_CellValue(j, i).ToString();
                        dr[j] = val;
                    }                    
                }
                dt.Rows.Add(dr); // 테이블 로우 추가
            }
            // DGV의 데이터소스 할당
            dgvData.DataSource = dt;
        }
    }
}
