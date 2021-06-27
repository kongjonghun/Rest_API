using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapWinGIS;
using AxMapWinGIS;
using RestSharp;
using Newtonsoft.Json.Linq;
// DB의 주소 -> 지오코더 API -> 좌표 변환 -> Shape 생성
namespace practice
{
    public partial class Form1 : Form
    {
        int layerHandle;
        DataTable dt = new DataTable();
        Utils u;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {            
            axMap1.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;         
            axMap1.TileProvider = tkTileProvider.OpenStreetMap;            
            axMap1.KnownExtents = tkKnownExtents.keSouth_Korea;            
            //axMap1.CursorMode = tkCursorMode.cmPan;

            u = new Utils();

            string connStr = "PG:host=localhost dbname=gis user=postgres password=8270 port=5433";            
            string cmd;
            cmd = "select id, name, sgg, address from public_bicycle limit 20";            
            layerHandle = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            Shapefile sf = axMap1.get_Shapefile(layerHandle);
            //sf.DefaultDrawingOptions.PointSize = 10;
            //sf.DefaultDrawingOptions.PointShape = tkPointShapeType.ptShapeCircle;
            //sf.DefaultDrawingOptions.FillColor = u.ColorByName(tkMapColor.LightBlue);

            //// DataTable 컬럼 생성                  
            for (int i = 0; i < sf.NumFields; i++)
            {
                DataColumn dc = new DataColumn();                
                dc.ColumnName = sf.Table.Field[i].Name;
                dt.Columns.Add(dc);  // 테이블 컬럼 추가
            }
            //// DataTable 로우 생성            
            for (int i = 0; i < sf.NumShapes; i++) // 행 개수
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < sf.NumFields; j++) // 컬럼개수
                {
                    if (sf.get_CellValue(j, i) != null) // null 경우 제외
                    {
                        // Shaplefile.get_CellValue(컬럼 인덱스:열, 피쳐 id:행) 활용
                        string val = sf.get_CellValue(j, i).ToString();
                        dr[j] = val;
                    }
                }
                dt.Rows.Add(dr); // 테이블 로우 추가
            }
            dgvData.DataSource = dt;
            /// ProgressBar
            progressBar1.Minimum = 0;                // 처리 사항 최소 : 0
            progressBar1.Maximum = dt.Rows.Count;    // 처리 사항 최대 : DataTable 행 개수
            progressBar1.Value = 0;                  // 0부터 시작
            progressBar1.Step = 1;                   // 1씩 증가 

            // 입력 레이어 개수
            tbInput.Text = dt.Rows.Count.ToString();
        }
        private void btnGeocoding_Click(object sender, EventArgs e)
        {
            // InputTable에 컬럼 추가            
            dgvData.DataSource = null;          // datagridview와 연결 해제 
            int OK = 0;
            // 좌표 컬럼 추가
            DataColumn xCol = new DataColumn();
            xCol.ColumnName = "x_geo";
            DataColumn yCol = new DataColumn();
            yCol.ColumnName = "y_geo";
            DataColumn errorCol = new DataColumn();
            errorCol.ColumnName = "error";
            dt.Columns.Add(xCol);
            dt.Columns.Add(yCol);
            dt.Columns.Add(errorCol);
            // 테이블 ROW 한줄씩 읽어오기
            foreach (DataRow item in dt.Rows)
            {                
                string address = item["address"].ToString();
                // address={0} : 주소 넣기 & type=road/parcel : 도로명/지번
                string url = string.Format("http://api.vworld.kr/req/address?service=address&request=getcoord&version=2.0&crs=epsg:4326&address={0}&refine=true&simple=false&type=road&key=4DC3C0AD-CFCA-3277-8293-B13CADFB6A7A", address);
                try
                {
                    // RestSharp을 이용한 API 요청 및 응답 처리
                    var client = new RestClient(url);   // RestClient객체에 url 전달
                    client.Timeout = -1;  // 요청 시간이 너무 길면 요청 끊어라
                    var request = new RestRequest(Method.GET); // 요청 방식 GET방식 : 데이터 얻기만 할거다
                    IRestResponse response = client.Execute(request); // request을 실행해라

                    var result = JObject.Parse(response.Content); // json 결과 : Newtonsoft.Json.Linq 패키지
                    var status = result["response"]["status"].ToString();

                    if (status.ToUpper() == "OK")
                    {
                        OK++;
                        item["x_geo"] = result["response"]["result"]["point"]["x"].ToString();
                        item["y_geo"] = result["response"]["result"]["point"]["y"].ToString();
                    }
                    else
                    {
                        item["error"] = "좌표를 찾을 수 없음";
                    }
                }
                catch
                {
                    item["error"] = "오류";
                }
                progressBar1.PerformStep();
            }
            // dataGridView에 DataTable 할당
            dgvData.DataSource = dt;
            tbOutput.Text = OK.ToString();

            Shapefile sf2 = CreateMarker(); // 좌표로 Point 생성하기

            // Shapefile 좌표계 설정 (생성된 shape의 경우 좌표계 설정)
            sf2.GeoProjection.ImportFromEPSG(4326);

            // Map에 Layer로 추가(DB에서 추가시 : AddLayerFromDatabase)
            int index = axMap1.AddLayer(sf2, true);
            axMap1.ZoomToLayer(index);
            // Layer 추가 후 심볼라이징
            Shapefile sf = axMap1.get_Shapefile(index);
            sf.DefaultDrawingOptions.PointSize = 15;
            sf.DefaultDrawingOptions.PointShape = tkPointShapeType.ptShapeStar;
            sf.DefaultDrawingOptions.FillColor = u.ColorByName(tkMapColor.Aquamarine);
        }
        // On the fly!! 중요!!
        private void axMap1_ProjectionMismatch(object sender, _DMapEvents_ProjectionMismatchEvent e)
        {
            e.reproject = tkMwBoolean.blnTrue;
        }
        private Shapefile CreateMarker()
        {
            Shapefile sf = new Shapefile();
            sf.CreateNew("", ShpfileType.SHP_POINT);
            foreach (DataRow item in dt.Rows)
            {
                // 오류 행이 아니라면 Point 객체 생성
                if (item["error"].ToString() == "")
                {
                    double x = Convert.ToDouble(item["x_geo"]);
                    double y = Convert.ToDouble(item["y_geo"]);

                    // Shape 정의
                    Shape shp = new Shape();
                    shp.Create(ShpfileType.SHP_POINT);
                    MapWinGIS.Point p = new MapWinGIS.Point();
                    // POINT에 좌표 부여
                    p.x = x;
                    p.y = y;

                    // shp객체의 point 개수 = pointIndex
                    int pointIndex = shp.numPoints;
                    // shp에 Point 객체 입력
                    shp.InsertPoint(p, ref pointIndex);
                    // Shape의 개수 = shpIndex
                    int shpIndex = sf.NumShapes;
                    // Shapefile에 shape 넣기
                    sf.EditInsertShape(shp, ref shpIndex);
                }
            }
            return sf;
        }
    }
}
