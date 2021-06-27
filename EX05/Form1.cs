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

namespace EX05
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
        // RestSharp, Newtonsoft.Json.Linq 패키지를 이용한 검색API
        // Search 버튼 클릭시, 해당 주소 API, Shape 생성, Map에 Shape Layer 추가
        private void btnSearch_Click(object sender, EventArgs e)
        {
            // 초기화
            lstSearch.Items.Clear();  // listbox 초기화
            axMap1.RemoveAllLayers(); // Map Layer 초기화

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
            var items = (JArray)result["response"]["result"]["items"]; // JSON 배열 , var = JArray

            //// 공간 데이터 추가하기, 생성하기
            // 1. shapefile 생성(파일 객체 생성)
            Shapefile sf = new Shapefile();
            // 2. Create 모드 활성화 : (파일명)sf.CreateNew("", ShpfileType.SHP_POINT): 명칭은 공백, 타입은 point
            sf.CreateNew("", ShpfileType.SHP_POINT);

            //// 리스트박스에 채우기
            // 리스트박스에 아이템 추가하기 : lstSearch.Items.Add("item");           
            foreach (var item in items) // var = JObject
            {
                lstSearch.Items.Add(item["title"]);

                // 3. 공간 피쳐(Shape) 생성 : Shapefile의 각 shpae (feature) 생성
                // 3.1 : shp 초기화
                Shape shp = new Shape();
                // 3.2 : shp 타입 선언, Create 모드 활성화
                shp.Create(ShpfileType.SHP_POINT);
                // 3.3 : shp 구성요소
                MapWinGIS.Point p = new MapWinGIS.Point();
                p.x = Convert.ToDouble(item["point"]["x"]);
                p.y = Convert.ToDouble(item["point"]["y"]);

                // 4. shp에 지오메트리(Point) 추가
                int pointindex = shp.numPoints; // shp에 추가된 point 개수 
                shp.InsertPoint(p, ref pointindex);

                // 5. Shapefile(파일)에 shp 추가
                int shpindex = sf.NumShapes;
                sf.EditInsertShape(shp, ref shpindex);
            }
            // 좌표계 정의
            sf.GeoProjection.ImportFromEPSG(4326);
            axMap1.AddLayer(sf, true);
        }
        // On the fly
        private void axMap1_ProjectionMismatch(object sender, _DMapEvents_ProjectionMismatchEvent e)
        {
            e.reproject = tkMwBoolean.blnTrue;
        }
        // ListBox 더블 클릭 시 Zoom : lstSearch_MouseDoubleClick 이벤트
        private void lstSearch_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            axMap1.ZoomToShape(0, lstSearch.SelectedIndex);
            axMap1.CurrentScale = 2500; // 100 vs 1000 : 1000이 더 넓은 범위, 1m가 실세계 1000m 나타냄
        }
    }
}
