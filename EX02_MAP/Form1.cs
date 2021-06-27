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

namespace EX02_MAP
{
    public partial class Form1 : Form
    {
        // 맵에추가되는레이어인덱스
        int layerHandle;
        // 선택된 피쳐들을 참조할 Shapefile (MapWindow 라이브러리가 사용하는 데이터 구조)
        Shapefile sf;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //// 1. 맵좌표계설정
            axMap1.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;
            // 배경맵적용(타일맵)
            axMap1.TileProvider = tkTileProvider.OpenStreetMap;
            // 맵초기공간범위
            axMap1.KnownExtents = tkKnownExtents.keSouth_Korea;
            // 맵커서모드변경
            axMap1.CursorMode = tkCursorMode.cmPan;
            // 맵 컨트롤의 선택 모드 활성화
            axMap1.SendSelectBoxFinal = true;

            //// 2. Database 연결
            string connStr = "PG:host=localhost dbname=gis user=postgres password=8270 port=5433";
            // 명령어를할당할문자열변수선언
            string cmd;
            // select 쿼리(PostgreSQL로부터호출할테이블)
            cmd = "select * from adm_sgg";
            // 레이어추가
            // 맵컨트롤.AddLayerFromDatabase(연결문자열, 명렁어, 가시화여부)
            layerHandle = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            // 레이어이름설정
            // 맵컨트롤.set_LayerName(레이어인덱스, 레이어명칭)
            axMap1.set_LayerName(layerHandle, "gu");

            cmd = "select * from adm_umd";
            layerHandle = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(layerHandle, "adm_umd");

            cmd = "select * from bus_stop";
            layerHandle = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(layerHandle, "bus_stop");

            cmd = "select * from moct_link";
            layerHandle = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(layerHandle, "moct_link");

            // PCS1 -> GCS1 -> GCS2 (GCS2 -> PCS2 : ProjectionMismatch)
            cmd = "select ST_Transform(geom, 4326) from building where a3 like \'%강남구%\'";
            layerHandle = axMap1.AddLayerFromDatabase(connStr, cmd, true);
            axMap1.set_LayerName(layerHandle, "gang_building");

            //// 3. CheckedListBox 설정
            // 도로 링크부터 체크드 리스트 박스에 추가(레이어 역으로 호출)
            for (int i = layerHandle; i >= 0; i--)
            {
                // 체크드리스트박스에 아이템 추가
                // 레이어 명칭 얻어오기 : axMap1.get_LayerName(인덱스)
                chkLayers.Items.Add(axMap1.get_LayerName(i));

                // 콤보박스에 아이템 추가
                cbSelectLayer.Items.Add(axMap1.get_LayerName(i));
            }
            // 모든 아이템은 체크가 된 상태로 시작
            for (int i = 0; i < chkLayers.Items.Count; i++)
            {
                chkLayers.SetItemChecked(i, true);
            }            
        }

        //// 4. ProjectionMismatch(reprojection)
        private void axMap1_ProjectionMismatch(object sender, _DMapEvents_ProjectionMismatchEvent e)
        {
            //맵에레이어가추가될때마다reprojection을실시함(메인좌표계로의통일)            
            e.reproject = tkMwBoolean.blnTrue;
        }

        //// 5. ItemCheck 이벤트
        private void chkLayers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // 레이어 인덱스와 CheckedListBox에 추가된 아이템 인덱스는 반대
            int position = layerHandle - e.Index;

            // 레이어 가시화 순서 제어 이전 (기존 코드 주석 처리)
            // if (e.NewValue == CheckState.Checked)
            // {
            // // 레이어의 가시화 설정
            // // 맵 컨트롤.set_LayerVisible(레이어 인덱스, 가시화 여부(T or F)
            // axMap1.set_LayerVisible(position, true);
            // }
            // else
            // {
            // axMap1.set_LayerVisible(position, false);
            // }

            // 레이어 가시화 순서 제어에 따른 변경
            // 가시화 순서에 따른 인덱스 획득
            // 레이어 인덱스 = 맵 컨트롤.get_LayerHandle(가시화 순서)  
            int layerIndex = axMap1.get_LayerHandle(position);

            if (e.NewValue == CheckState.Unchecked)
                axMap1.set_LayerVisible(layerIndex, false);
            else
                axMap1.set_LayerVisible(layerIndex, true);
        }
        //// 6. UP버튼
        private void btn_up_Click(object sender, EventArgs e)
        {
            if (chkLayers.SelectedItem == -1)
            {
                MessageBox.Show("레이어를 선택하시오.");
                return;
            }     
                            
            // CheckedListBox의 아이템 인덱스와 가시화 순서는 반대
            int position = layerHandle - chkLayers.SelectedIndex;
            // 현재 포지션(가시화 순서)에 위치한 레이어를 한 칸 올림
            // 레이어 인덱스가 아닌 포지션임에 주의
            axMap1.MoveLayerUp(position);
            // 체크박스에서 한 칸 올림 
            MoveItemAtChkLayers(-1);
        }
        //// 7. DOWN버튼
        private void btn_down_Click(object sender, EventArgs e)
        {
            int position = layerHandle - chkLayers.SelectedIndex;
            // 현재 포지션(가시화 순서)에 위치한 레이어를 한 칸 내림
            axMap1.MoveLayerDown(position);
            MoveItemAtChkLayers(+1);
        }
        //// 8. CheckBoxList 순서 변경
        private void MoveItemAtChkLayers(int itemInsert)
        {
            // chkLayers에서의 순서 변경
            // itemInsert: 새롭게 할당할 인덱스 증감분(한칸 UP or 한칸 DOWN)
            int source = chkLayers.SelectedIndex;
            // 선택된 아이템의 값(레이어 명칭)
            string sourceValue = chkLayers.SelectedItem.ToString();

            // 선택된 아이템의 체크 상태
            CheckState sourceChkState = chkLayers.GetItemCheckState(source);
            // 선택된 아이템을 우선 삭제
            chkLayers.Items.RemoveAt(source);
            // 인덱스 증감분을 적용하여 새로운 위치에 선택했던 아이템 추가
            chkLayers.Items.Insert(source + itemInsert, sourceValue);
            // 선택 상태 유지
            chkLayers.SetSelected(source + itemInsert, true);
            // 체크 상태 유지
            chkLayers.SetItemCheckState(source + itemInsert, sourceChkState);
        }
        //// 9. 선택 버튼 클릭
        private void btnSelect_Click(object sender, EventArgs e)
        {
            // 커서 모드를 선택 모드로 변경
            axMap1.CursorMode = tkCursorMode.cmSelection;
        }
        //// 10. 영역 선택(콤보박스 선택 아이템) : map의 selectBOxFinal 이벤트
        private void axMap1_SelectBoxFinal(object sender, _DMapEvents_SelectBoxFinalEvent e)
        {
            // 콤보박스에서 현재 선택된 아이템을 CheckedListBox에서 검색            
            // (콤보박스) 현재 선택된 아이템의 CheckedListBox 인덱스를 이용하여 layerPosition 계산
            int layerPosition = 0;
            int[] shapes = null;
            // 콤보박스에서 아이템을 선택하지 않았다면 메소드 종료
            if (cbSelectLayer.SelectedIndex == -1)
            {
                MessageBox.Show("선택 모드를 적용할 레이어를 콤보박스에서 선택하시오.");
                return;
            }
            // 콤보박스 선택시 해당 레이어 Postion = layerPosition
            for (int i = 0; i < chkLayers.Items.Count; i++)
            {                
                if (chkLayers.Items[i].ToString() == cbSelectLayer.SelectedItem.ToString())
                    layerPosition = layerHandle - i;
            }
            // 피쳐를 선택할 레이어의 layerHandle 획득
            int layerIndex = axMap1.get_LayerHandle(layerPosition);
            // 피쳐를 선택할 레이어가 가시화되고 있지 않다면 알림 메시지를 출력하고, 메소드 종료
            if (axMap1.get_LayerVisible(layerIndex) == false)
            {
                MessageBox.Show("활성화된 레이어를 선택하시오.");
                return;
            }
            // layerHandle를 이용하여 레이어의 피쳐들에 접근
            // MapWindow에서는 레이어의 피쳐 테이블에 접근할 때, Shapefile이라는 데이터 구조 활용
            // sf: 레이어의 전체 피쳐들
            sf = axMap1.get_Shapefile(layerIndex);

            // 선택된 피쳐들의 id를 저장할 object 타입 변수 선언
            object result = null;

            if (sf != null)
            {
                // 마우스 선택 범위를 저장할 변수들
                double left = 0.0;
                double top = 0.0;
                double bottom = 0.0;
                double right = 0.0;
                // SelectBoxFinal 메소드의 이벤트 파라미터(e)는 마우스로 선택한 픽셀 범위를 저장함
                // 픽셀 범위를 좌표계 범위로 변환
                // 메소드 호출 단계(픽셀 left 좌표, 픽셀 top 좌표, left(초기값), top(초기값)) 
                // 메소드 종료 시 -> left, top: 좌표 할당 (ref 키워드의 역할!)
                axMap1.PixelToProj(e.left, e.top, ref left, ref top); // 좌상단
                axMap1.PixelToProj(e.right, e.bottom, ref right, ref bottom); // 우하단

                // 공간 범위를 설정하는 객체
                Extents ext = new Extents();

                // SetBounds(...) 3차원 공간 범위 설정 메소드, 파라미터에 min z, max z 요구
                ext.SetBounds(left, bottom, 0.0, right, top, 0.0);

                // 현재 레이어에서 선택된 피쳐들이 있다면 clear
                sf.SelectNone();

                // 마우스의 선택 범위에 따라 레이어에서 피쳐 선택
                // Shapefile.SelectShapes(공간 범위, 톨러런스, 선택모드, 피쳐 id를 리턴할 오브젝트)
                if (sf.SelectShapes(ext, 0.0, SelectMode.INTERSECTION, ref result))
                {
                    // 피쳐 id 집합을 정수형 배열에 저장
                    shapes = result as int[];

                    if (shapes == null) return;
                    // set_ShapeSelected(피쳐 id, 하이라이트 여부)
                    for (int i = 0; i < shapes.Length; i++)
                    {
                        // 하이라이트
                        sf.set_ShapeSelected(shapes[i], true);
                    }
                }
                axMap1.Redraw(); // 맵 컨트롤 refresh
            }
            
            //// DGV에 선택된 피쳐들의 속성 테이블 표현하기                        
            // 비어 있는 데이터 테이블 생성
            DataTable dtSelectedFeatures = new DataTable();

            //// DataTable 컬럼 생성      
            // Shaplefile.Table 속성 활용
            for (int i = 0; i < sf.NumFields; i++) // 컬럼
            {
                DataColumn dc = new DataColumn();
                // Shapefile의 필드(컬럼)네임을 DataTable 필드(컬럼)에 추가
                dc.ColumnName = sf.Table.Field[i].Name;
                dtSelectedFeatures.Columns.Add(dc); // 테이블 컬럼 추가
            }
            //// DataTable 로우 생성
            // Shaplefile.get_CellValue(컬럼 인덱스, 피쳐 id) 활용
            for (int i = 0; i < shapes.Length; i++) // 행
            {
                DataRow dr = dtSelectedFeatures.NewRow();
                for (int j = 0; j < sf.NumFields; j++) // 열
                {
                    string val = sf.get_CellValue(j, shapes[i]).ToString();
                    dr[j] = val;
                }
                dtSelectedFeatures.Rows.Add(dr); // 테이블 로우 추가
            }
            // DGV의 데이터소스 할당
            dgvSelectedFeatures.DataSource = dtSelectedFeatures;        
        }
        // Clear 버튼클릭  
        private void btn_Clear_Click(object sender, EventArgs e)
        {
            SelectedRefresh();
        }     
        // 콤보박스 SelectedIndexChanged 이벤트
        private void cbSelectLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedRefresh();
        }
        private void SelectedRefresh()
        {
            axMap1.CursorMode = tkCursorMode.cmPan;
            dgvSelectedFeatures.DataSource = null; // DGV 비우기
            if (sf != null)
            {
                sf.SelectNone(); // Shape 선택 지우기
                axMap1.Redraw(); // map Refresh
            }
        }
    }
}

