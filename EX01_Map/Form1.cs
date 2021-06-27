using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// 네임스페이스 추가(라이브러리)
using AxMapWinGIS;
using MapWinGIS;

namespace EX01_Map
{
    public partial class Form1 : Form
    {
        // 맵에추가되는레이어인덱스
        int layerHandle;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //// 1. 좌표계 설정
            // 맵좌표계설정
            axMap1.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;
            // 배경맵적용(타일맵)
            axMap1.TileProvider = tkTileProvider.OpenStreetMap;
            // 맵초기공간범위
            axMap1.KnownExtents = tkKnownExtents.keSouth_Korea;
            // 맵커서모드변경
            axMap1.CursorMode = tkCursorMode.cmPan;

            //// 2. Database 연결 & Layer 추가
            // 연결문자열정의
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

            //// 3. CheckedListBox 데이터 추가 및 체크 설정
            // 도로 링크부터 체크드 리스트 박스에 추가(레이어 역으로 호출)
            for (int i = layerHandle; i >= 0; i--)
            {
                // 레이어 명칭으로 체크드리스트박스에 아이템 추가
                // 레이어 명칭 얻어오기 : axMap1.get_LayerName(인덱스)
                chkLayers.Items.Add(axMap1.get_LayerName(i));
            }
            // 모든 아이템은 체크가 된 상태로 시작
            for (int i = 0; i < chkLayers.Items.Count; i++)
            {
                chkLayers.SetItemChecked(i, true);
            }
        }
        //// ProjectionMismatch(Reprojection)
        private void axMap1_ProjectionMismatch(object sender, _DMapEvents_ProjectionMismatchEvent e)
        {
            // 맵에레이어가추가될때마다 reprojection을실시함(메인좌표계로의통일)            
            e.reproject = tkMwBoolean.blnTrue;
        }

        // checkListBox의 ItemCheck 이벤트
        private void chkLayers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int position = layerHandle - e.Index;
            // Check시 Visible
            // e.NewValue : 이벤트 이후 체크 여부
            if (e.NewValue == CheckState.Checked)
            {
                // 레이어의 가시화 여부
                axMap1.set_LayerVisible(position, true);
            }
            else
            {
                axMap1.set_LayerVisible(position, false);
            }
        }
    }    
}

