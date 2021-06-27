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
using RestSharp;
using Newtonsoft.Json.Linq;

namespace EX06_열린데이터광장
{
    public partial class Form1 : Form
    {
        JArray items;
        Utils u;
        Shapefile sf;
        int center;
        object result;

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
            // 맵커서모드변경
            axMap1.CursorMode = tkCursorMode.cmPan;
            // SelectBoxFinal 이벤트 사용시 꼭!!
            axMap1.SendSelectBoxFinal = true;
            u = new Utils();

            GetData();
        }
        // API로 데이터 불러오기 (1000개씩 3000개)
        private void GetData()
        {
            items = new JArray(); // 초기화

            int i = 1;
            while (i+900 <= 3000)
            {
                string url = string.Format("http://openapi.seoul.go.kr:8088/646e6266766b7962373649596b6867/json/bikeList/{0}/{1}/",i, i+999);

                // Rest API
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);

                IRestResponse response = client.Execute(request);
                var result = JObject.Parse(response.Content);

                items.Merge((JArray)result["rentBikeStatus"]["row"]);

                i += 1000;
            }
            AddLayer();                                
        }
        private void AddLayer()
        {
            axMap1.RemoveAllLayers(); // Map Layer 초기화 

            sf = new Shapefile();
            sf.CreateNew("", ShpfileType.SHP_POINT);

            //// 필드 생성(컬럼 생성)
            // EditAddField(필드명, 타입, 소수점, 전체길이)
            sf.EditAddField("rackTotCnt", FieldType.INTEGER_FIELD, 0, 10);
            sf.EditAddField("stationName", FieldType.STRING_FIELD, 0, 10);
            sf.EditAddField("parkingBikeTotCnt", FieldType.INTEGER_FIELD, 0, 10);
            sf.EditAddField("shared", FieldType.INTEGER_FIELD, 0, 10);
            sf.EditAddField("stationId", FieldType.STRING_FIELD, 0, 10);

            foreach (var item in items) // var = JObject
            {
                //// 포인트 생성
                Shape shp = new Shape();
                shp.Create(ShpfileType.SHP_POINT);

                MapWinGIS.Point p = new MapWinGIS.Point();
                p.x = Convert.ToDouble(item["stationLongitude"]);
                p.y = Convert.ToDouble(item["stationLatitude"]);
                
                int pointindex = shp.numPoints;
                shp.InsertPoint(p, ref pointindex);

                int shpindex = sf.NumShapes;
                sf.EditInsertShape(shp, ref shpindex);

                //// 필드값 입력
                // EditCellValue(Field_Index, Shape_Index, object)
                sf.EditCellValue(0, shpindex, Convert.ToInt32(item["rackTotCnt"].ToString()));
                sf.EditCellValue(1, shpindex, item["stationName"].ToString());
                sf.EditCellValue(2, shpindex, Convert.ToInt32(item["parkingBikeTotCnt"].ToString()));
                sf.EditCellValue(3, shpindex, Convert.ToInt32(item["shared"].ToString()));
                sf.EditCellValue(4, shpindex, item["stationId"].ToString());
            }

            // 좌표계 정의
            sf.GeoProjection.ImportFromEPSG(4326);
            center = axMap1.AddLayer(sf, true);

            // 심볼라이징
            sf = axMap1.get_Shapefile(center);
            sf.DefaultDrawingOptions.PointSize = 15;
            // 수치형
            double min = Convert.ToDouble(sf.Table.MinValue[2]);
            double max = Convert.ToDouble(sf.Table.MaxValue[2]);
            // AddRange(필드인덱스(기준), 자료형(수치형), 구분_개수, 최소, 최대)
            sf.Categories.AddRange(2, tkClassificationType.ctNaturalBreaks, 5, min, max);
            sf.Categories.get_Item(0).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Red);
            sf.Categories.get_Item(1).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Salmon);
            sf.Categories.get_Item(2).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Yellow);
            sf.Categories.get_Item(3).DrawingOptions.FillColor = u.ColorByName(tkMapColor.GreenYellow);
            sf.Categories.get_Item(4).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Green);
            sf.Categories.ApplyExpressions();                        
        }
        private void btnSelect_Click(object sender, EventArgs e)
        {
            // 커서 모드를 선택 모드로 변경
            axMap1.CursorMode = tkCursorMode.cmSelection;
        }
        private void btnPan_Click(object sender, EventArgs e)
        {
            // 커서 모드를 Pan 모드로 변경
            axMap1.CursorMode = tkCursorMode.cmPan;
        }        
        private void axMap1_ProjectionMismatch(object sender, _DMapEvents_ProjectionMismatchEvent e)
        {
            // On the fly
            e.reproject = tkMwBoolean.blnTrue;
        }
        // Select 커서로 객체 선택 → dgv연결
        private void axMap1_SelectBoxFinal(object sender, _DMapEvents_SelectBoxFinalEvent e)
        {
            dgvData.DataSource = null;
            sf = axMap1.get_Shapefile(center);
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

                // 현재 레이어에서 선택된 피쳐들이 있다면 clear
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

                //// DGV의 DataTable 생성 및 데이터 할당  
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
                        if (sf.get_CellValue(j,i) != null)
                        {
                            string val = sf.get_CellValue(j, shapes[i]).ToString();
                            dr[j] = val;
                        }                        
                    }
                    dt_SelectedFeatures.Rows.Add(dr); // 테이블 로우 추가
                }
                // DGV의 데이터소스 할당
                dgvData.DataSource = dt_SelectedFeatures;
            }
            axMap1.Redraw(); // 맵 컨트롤 refresh
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GetData();
            dgvData.DataSource = null;
        }
        private void dgvData_DoubleClick(object sender, EventArgs e)
        {            
            int[] shapes = result as int[];
            // DGV, ROW 선택시 ZOOM
            axMap1.ZoomToShape(0, shapes[dgvData.CurrentRow.Index]);
            axMap1.CurrentScale = 2500;
            // 선택된 Shap, 하이라이트 off
            sf.SelectNone();
        }
    }
}
