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

namespace EX03
{
    public partial class Form1 : Form
    {
        int numLayers; // 멤버변수
        // 범례용 DataTable
        DataTable uniqueLegend = new DataTable();
        DataTable graduatedLegend = new DataTable();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 맵 설정
            axMap1.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;
            axMap1.TileProvider = tkTileProvider.OpenStreetMap;
            axMap1.KnownExtents = tkKnownExtents.keSouth_Korea;

            // 연결문자열정의
            string connStr = "PG:host=localhost dbname=gis user=postgres password=8270 port=5433";
            // 명령어를할당할문자열변수선언
            string cmd;

            // 색 변경하고자 하는 Utils
            Utils u = new Utils();

            // select 쿼리(PostgreSQL로부터 호출할 테이블)
            cmd = "select * from public_bicycle";
            numLayers = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(numLayers, "따릉이 대여소");
            //// 심볼 변화     
            //// Point
            Shapefile simplePoint = axMap1.get_Shapefile(numLayers);    // Layer -> Shapefile
            simplePoint.DefaultDrawingOptions.PointSize = 8;
            simplePoint.DefaultDrawingOptions.PointShape = tkPointShapeType.ptShapeCircle;
            simplePoint.DefaultDrawingOptions.FillColor = u.ColorByName(tkMapColor.Aqua);
            //// Line
            cmd = "select * from moct_link";
            numLayers = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(numLayers, "도로");
            Shapefile simpleLine = axMap1.get_Shapefile(numLayers);
            simpleLine.DefaultDrawingOptions.LineWidth = 3;
            simpleLine.DefaultDrawingOptions.FillVisible = false; //Line 경계선만 가시화
            simpleLine.DefaultDrawingOptions.LineColor = u.ColorByName(tkMapColor.DarkGreen);
            //// Polygon
            cmd = "select * from adm_sgg";
            numLayers = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(numLayers, "자치구");
            Shapefile simplePolygon = axMap1.get_Shapefile(numLayers);
            simplePolygon.DefaultDrawingOptions.LineWidth = 3;
            simplePolygon.DefaultDrawingOptions.FillVisible = false; //Line 경계선만 가시화
            simplePolygon.DefaultDrawingOptions.LineColor = u.ColorByName(tkMapColor.OrangeRed);

            // 필드 값을 심볼라이징
            // 1. 건물 용도를 이용한 심볼라이징
            cmd = "select gid, a3 as location, a30 as category, ST_Transform(geom, 4326) from building where a3 like \'%강남구%\' and a30 in (\'단독주택\', \'업무시설\', \'제1종근린생활시설\', \'제2종근린생활시설\') ";
            numLayers = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(numLayers, "건물용도");
            // 범주형, 질적
            Shapefile buildingCategory = axMap1.get_Shapefile(numLayers);
            // AddRange(필드인덱스(컬럼 인덱스), 자료형(명목형), 클래스 개수, 최소, 최대)
            buildingCategory.Categories.AddRange(2, tkClassificationType.ctUniqueValues, 4, 0, 3);
            buildingCategory.Categories.get_Item(0).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Yellow);
            buildingCategory.Categories.get_Item(1).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Black);
            buildingCategory.Categories.get_Item(2).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Brown);
            buildingCategory.Categories.get_Item(3).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Aqua);
            buildingCategory.Categories.ApplyExpressions();
            CreateLegend(numLayers); // 건물용도카테고리적용후,레전드생성메소드호출

            // 2. 필지 가격을 이용한 심볼라이징
            cmd = "select gid, a0 as pnu, a2 as location, a9 as value, ST_Transform(geom, 4326) as geom from land_value where a2 like '%강남구%' and right(a6, 1) ='대' ";
            numLayers = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(numLayers, "필지가격");
            // 수치형, 양적
            Shapefile landValue = axMap1.get_Shapefile(numLayers);
            // landValue 테이블의 4번째 필드(value) 최소값을 min에 저장
            double min = Convert.ToDouble(landValue.Table.MinValue[3]);
            double max = Convert.ToDouble(landValue.Table.MaxValue[3]);
            // AddRange(필드인덱스(컬럼 인덱스), 자료형(수치형), 클래스 개수, 최소, 최대)
            landValue.Categories.AddRange(3, tkClassificationType.ctNaturalBreaks, 5, min, max);
            landValue.Categories.get_Item(0).DrawingOptions.FillColor = u.ColorByName(tkMapColor.White);
            landValue.Categories.get_Item(1).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Salmon);
            landValue.Categories.get_Item(2).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Crimson);
            landValue.Categories.get_Item(3).DrawingOptions.FillColor = u.ColorByName(tkMapColor.Red);
            landValue.Categories.get_Item(4).DrawingOptions.FillColor = u.ColorByName(tkMapColor.DarkRed);
            landValue.Categories.ApplyExpressions();
            CreateLegend(numLayers); // 필지가격카테고리적용후,레전드생성메소드호출

            // Layer Visible off
            for (int i = 0; i <= numLayers; i++)
            {
                axMap1.set_LayerVisible(i, false);
            }

            rbBicycle.Checked = true;
            legendPanel.Hide(); // 폼Load 메소드가장마지막에추가,패널숨기기
        }

        // reprojection (map 이벤트 : projectionMismatch)
        private void axMap1_ProjectionMismatch(object sender, _DMapEvents_ProjectionMismatchEvent e)
        {
            e.reproject = tkMwBoolean.blnTrue;
        }
        // RadioButton : CheckedChanged이벤트 : LayerCheck
        private void LayerCheck(object sender, EventArgs e)
        {
            if (numLayers == 0)
                return;

            RadioButton rb = (RadioButton)sender;
            legendPanel.Hide();
            int layerHandle = 0;

            if (rb.Text == "따릉이 대여소")
                layerHandle = 0;
            if (rb.Text == "도로 링크")
                layerHandle = 1;
            if (rb.Text == "자치구")
                layerHandle = 2;
            if (rb.Text == "건물 용도")
            {
                layerHandle = 3;
                ShowLegend(layerHandle);
            }
            if (rb.Text == "필지 가격")
            {
                layerHandle = 4;
                ShowLegend(layerHandle);
            }
            if (rb.Checked == true)
                axMap1.set_LayerVisible(layerHandle, true);
            else
                axMap1.set_LayerVisible(layerHandle, false);
        }
        private void CreateLegend(int layerHandle)
        {
            Shapefile target = axMap1.get_Shapefile(layerHandle);
            if (layerHandle == 3)
            {
                // 범주형레전드
                // 클래스, 색상
                // 컬럼 생성
                DataColumn dc = new DataColumn();
                dc.ColumnName = "구분";
                uniqueLegend.Columns.Add(dc);
                dc = new DataColumn();
                dc.ColumnName = "색상";
                uniqueLegend.Columns.Add(dc);
                // 로우 생성
                for (int i = 0; i < 4; i++)
                {
                    DataRow dr = uniqueLegend.NewRow();
                    dr[0] = target.Categories.Item[i].Name.ToString();
                    dr[1] = "";
                    uniqueLegend.Rows.Add(dr);
                }
            }
            else
            {
                // 수치형레전드
                // 최소, 최대, 색상
                // 컬럼 생성
                DataColumn dc = new DataColumn();
                dc.ColumnName = "최소";
                graduatedLegend.Columns.Add(dc);
                dc = new DataColumn();
                dc.ColumnName = "최대";
                graduatedLegend.Columns.Add(dc);
                dc = new DataColumn();
                dc.ColumnName = "색상";
                graduatedLegend.Columns.Add(dc);
                // 로우 생성
                for (int i = 0; i < 5; i++)
                {
                    DataRow dr = graduatedLegend.NewRow();
                    dr[0] = target.Categories.Item[i].MinValue.ToString();
                    dr[1] = target.Categories.Item[i].MaxValue.ToString();
                    dr[2] = "";
                    graduatedLegend.Rows.Add(dr);
                }
            }
        }
        //// Legend 생성 메소드
        private void ShowLegend(int layerHandle)
        {
            legendPanel.Show();
            dgvLegend.DataSource = null;
            // dgv의데이터소스및배경색(컬럼인덱스,로우인덱스)설정
            if (layerHandle == 3)
            {
                dgvLegend.DataSource = uniqueLegend;
                // dgvLegend[열Index, 행Index]
                dgvLegend[1, 0].Style.BackColor = Color.Aqua;
                dgvLegend[1, 1].Style.BackColor = Color.Yellow;
                dgvLegend[1, 2].Style.BackColor = Color.Green;
                dgvLegend[1, 3].Style.BackColor = Color.Black;
            }
            else
            {
                dgvLegend.DataSource = graduatedLegend;
                dgvLegend[2, 0].Style.BackColor = Color.White;
                dgvLegend[2, 1].Style.BackColor = Color.Salmon;
                dgvLegend[2, 2].Style.BackColor = Color.Crimson;
                dgvLegend[2, 3].Style.BackColor = Color.Red;
                dgvLegend[2, 4].Style.BackColor = Color.DarkRed;
            }
        }
      
    }
}
    
