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
// API
using RestSharp;             // C# 개발환경에서 REST API를 손쉽게 호출할 수 있도록 도와주는 패키지
using Newtonsoft.Json.Linq;  // JSON 타입의 API 출력 결과를 파싱할 수 있도록 도와주는 패키지
// Filestream
using System.IO;             // filestream : Input, Output : 파일을 읽고 쓰는 기능 지원

namespace EX04
{
    public partial class Form1 : Form
    {
        DataTable inputTable; // 멤버변수 추가 : 비어있는 테이블 선언
        int getXY = 0;

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
        // btn_Open_Click : openFileDialog와 FileStream 기능을 이용한 텍스트 파일 열기 & DGV에 띄우기
        private void button1_Click(object sender, EventArgs e) 
        {
            inputTable = new DataTable(); //테이블 초기화
            var filePath = string.Empty; // 비어이는 문자열 선언 // var : 입력된 값에 따라 변수타입 변환

            using (OpenFileDialog openFileDialog = new OpenFileDialog()) // OpenFileDialog : 파일 열기 창
            {
                openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"; // csv거나 all 파일 형태
                openFileDialog.FilterIndex = 2;
                // ShowDialog : 파일 창이 열림 : OK버튼 클릭시
                if (openFileDialog.ShowDialog() == DialogResult.OK) 
                {   // 파일경로 및 이름 (FilePath에 입력)
                    filePath = openFileDialog.FileName;
                    // 열게된 파일 (fileStream 변수에 저장)
                    var fileStream = openFileDialog.OpenFile(); 

                    using (StreamReader reader = new StreamReader(fileStream)) // StreamReader : Text파일을 읽을 때
                    {
                        // 한줄씩 읽기 Split(',') : ,기준으로 나뉘어서 배열로 저장
                        // 컬럼명칭 해당 row 추가(Header row 추가)
                        string[] headers = reader.ReadLine().Split(',');                        
                        foreach (string header in headers)    // 컬럼에 하나씩 추가
                            inputTable.Columns.Add(header);

                        while (!reader.EndOfStream) // 파일 끝날때까지 읽음
                        {
                            string[] rows = reader.ReadLine().Split(',');
                            DataRow dr = inputTable.NewRow();
                            for (int i = 0; i < headers.Length; i++)
                                dr[i] = rows[i].Trim();

                            inputTable.Rows.Add(dr); // inputTable에 행별 데이터 넣기
                        }
                    }
                }
            }
            //// DGV에 띄우기
            // DataGridView에 DataTable 추가
            dgvData.DataSource = inputTable;
            // 입력데이터 textbox : 테이블 row의 개수(처리 전 데이터 개수)
            tbInput.Text = inputTable.Rows.Count.ToString(); 
            // ProgressBar
            progressBar1.Minimum = 0;                        // 처리 사항 최소 : 0
            progressBar1.Maximum = inputTable.Rows.Count;    // 처리 사항 최대 : 행 개수
            progressBar1.Value = 0;  // 0부터 시작
            progressBar1.Step = 1;  // 1씩 증가 
        }
        // btn_Geocoding_Click
        // RestSharp, Newtonsoft.JSON 패키지를 이용한 지오코더API
        private void button2_Click(object sender, EventArgs e)
        {
            // InputTable에 컬럼 추가            
            dgvData.DataSource = null;          // datagridview와 연결 해제 
            // 좌표 컬럼 추가
            DataColumn xCol = new DataColumn();
            xCol.ColumnName = "x";
            DataColumn yCol = new DataColumn();
            yCol.ColumnName = "y";
            DataColumn errorCol = new DataColumn();
            errorCol.ColumnName = "error";            
            inputTable.Columns.Add(xCol);
            inputTable.Columns.Add(yCol);
            inputTable.Columns.Add(errorCol);
            // 테이블 ROW 한줄씩 읽어오기
            foreach (DataRow item in inputTable.Rows)
            {
                // 주소 컬럼 읽기 (텍스트 파일 내)
                string address = item["도로명주소"].ToString();
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
                        getXY++; // 처리가능 데이터 수
                        item["x"] = result["response"]["result"]["point"]["x"].ToString();
                        item["y"] = result["response"]["result"]["point"]["y"].ToString();
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
                // ROW 1개당 progressBar이동
                progressBar1.PerformStep();
            }
            // dataGridView에 DataTable 할당
            dgvData.DataSource = inputTable;
            // 처리된 데이터 개수
            tbOutput.Text = getXY.ToString();

            Shapefile sf = CreateMarker();
            // Shapefile 좌표계 설정
            sf.GeoProjection.ImportFromEPSG(4326);
            // Map에 Layer로 추가
            axMap1.AddLayer(sf, true);
        }
        // Marker 형성 메소드 : Shape 형성하기
        private Shapefile CreateMarker()
        {
            // Shapefile 정의
            Shapefile sf = new Shapefile();
            sf.CreateNew("", ShpfileType.SHP_POINT);
            foreach (DataRow item in inputTable.Rows)
            {
                // 오류 행이 아니라면 Point 객체 생성
                if (item["error"].ToString() == "")
                {
                    double x = Convert.ToDouble(item["x"]);
                    double y = Convert.ToDouble(item["y"]);

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

        private void axMap1_ProjectionMismatch(object sender, _DMapEvents_ProjectionMismatchEvent e)
        {
            e.reproject = tkMwBoolean.blnTrue;
        }

        // btn_save_Click
        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "csv (*.csv)|*.csv|txt (*txt)|*.txt|All files (*.*)|*.*";
            
            // 취소 버튼 클릭시
            if (saveDlg.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            // new FileStream(파일명, FileMode.Create(생성), FileAccess.Write(쓰기));
            FileStream fs = new FileStream(saveDlg.FileName, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            string line = string.Join(",", inputTable.Columns.Cast<object>());

            sw.WriteLine(line);

            foreach (DataRow item in inputTable.Rows)
            {
                line = string.Join(",", item.ItemArray.Cast<object>());
                sw.WriteLine(line);
            }            
            sw.Close(); // StreamWriter.Close()
            fs.Close(); // FileStream.Close()
        }
    }    
}
