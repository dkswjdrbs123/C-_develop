using System;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.Windows.Forms;
using SharpDX.Direct2D1;

using SharpDX.Mathematics.Interop;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

using SharpDX.DirectWrite;

using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuadTree;



///////////////////////////


/// //////////////////////

namespace SharpDxTest
{
    public partial class MainForm : Form
    {

        /// <summary>
        /// 버퍼 렌더 타겟
        /// </summary>
        SharpDX.Direct2D1.BitmapRenderTarget bufferRenderTarget = null;

        /// <summary>
        /// 읽기 전용 버퍼 렌더 타겟
        /// </summary>
        public SharpDX.Direct2D1.BitmapRenderTarget BufferRenderTarget
        {
            get
            {
                return bufferRenderTarget;
            }

        }

        /// <summary>
        /// 이미지 저장 컬렉션. 키는 이미지 로드시 사용한 경로명이다.
        /// </summary>
        private Dictionary<string, SharpDX.Direct2D1.Bitmap[]> images;

        /// <summary>
        /// 읽기 전용 이미지 저장 컬렉션
        /// </summary>
        public IReadOnlyDictionary<string, SharpDX.Direct2D1.Bitmap[]> Images
        {
            get
            {
                return images;
            }
        }

        /// <summary>
        /// 이미지 객체의 참조. 실제 보유는 ImageManager가 함.
        /// </summary>
        //public SharpDX.Direct2D1.Bitmap[] Image;
        public SharpDX.Direct2D1.Bitmap[] Image
        {
            get
            {
                return Images[imageFileName];
            }
        }

        /// <summary>
        /// 이미지 파일명
        /// </summary>
        private string imageFileName;

        /// <summary>
        /// 읽기 전용 이미지 파일명
        /// </summary>
        public string ImageFileName
        {
            get
            {
                return imageFileName;
            }
            set
            {
                imageFileName = value;
                ImageLoaded = false;
            }
        }

        /// <summary>
        /// 아무거나 가장 첫 번째 윈도우 렌더타겟을 반환한다. ImageManager에서 씀.
        /// </summary>
        public static SharpDX.Direct2D1.RenderTarget FirstRenderTarget
        {
            get
            {
                return RenderTarget2D;
            }
        }

        /// <summary>
        /// 이미지 가로 픽셀 크기
        /// </summary>
        public int ImageWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// 이미지 세로 픽셀 크기
        /// </summary>
        public int ImageHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// 이미지 로드됐는지 여부
        /// </summary>
        public bool ImageLoaded = false;

        /// <summary>
        /// WIC 이미지 팩토리 단일 객체
        /// </summary>
        public static SharpDX.WIC.ImagingFactory ImagingFactory = null;

        /// <summary>
        /// Direct2D 팩토리 단일 객체
        /// </summary>
        public static SharpDX.Direct2D1.Factory D2DFactory = null;

        /// <summary>
        /// 렌더링 배경색
        /// </summary>f
        SharpDX.Mathematics.Interop.RawColor4 clearColor;

        /// <summary>
        /// 줌 배율. 1.0 = 100%
        /// </summary>
        public float ZoomScale
        {
            get;
            private set;
        }

        /// <summary>
        /// 패닝 오프셋. 0,0이면 도면은 좌상단을 시작으로 그리기된다.
        /// </summary>
        private Point panOffset;

        /// <summary>
        /// 패닝 오프셋 접근 프로퍼티
        /// </summary>
        public Point PanOffset
        {
            get
            {

                return panOffset;
            }
        }

        /// <summary>
        /// 각도를 호도로 바꾸기 위한 상수
        /// </summary>
        static public float ToRad = (float)Math.PI / 180.0f;

        /// <summary>
        /// 마커의 회전 방향(각도)
        /// </summary>
        public float Rotation;

        ///////////////////////////////////////////
        /// <summary>
        /// winform구조로 picture사용해 그리기
        /// </summary>
        public SharpDX.Direct2D1.Factory Factory2D { get; private set; }
        public SharpDX.DirectWrite.Factory FactoryDWrite { get; private set; }
        public static WindowRenderTarget RenderTarget2D { get; private set; }
        public SolidColorBrush SceneColorBrush { get; private set; }
        public SolidColorBrush text_SceneColorBrush { get; private set; }
        /// <summary>
        /// 선택툴 드래그 박스 표시용 브러쉬
        /// </summary>
        public SharpDX.Direct2D1.Brush SelectionBoxBrush;

        /// <summary>
        /// 선택툴 드래그 선 표시용 브러쉬
        /// </summary>
        public SharpDX.Direct2D1.Brush SelectionLineBrush;
        /// <summary>
        /// 마커의 선택 상태 표시용 브러쉬
        /// </summary>
        private SharpDX.Direct2D1.Brush selectedMarkerBrush;

        private SortedDictionary<int, SharpDX.Direct2D1.Brush> brushes = null;

        /// <summary>
        /// 초기화되었는지 여부
        /// </summary>
        public bool Initialized
        {
            get;
            private set;
        }
        public TextFormat TextFormat { get; private set; }
        public TextFormat TextFormat_xy { get; private set; }

        /// <summary>
        /// 마커의 위치와 회전 값을 워크스페이스의 이동 및 줌 값과 계산하여 얻은 트랜스폼.
        /// 렌더타겟에 마커를 그릴 때 사용.
        /// </summary>
        public SharpDX.Mathematics.Interop.RawMatrix3x2 Transform
        {
            get;
            protected set;
        }

        public TextLayout textLayout1 { get; private set; }
        public RawRectangleF ClientRectangle { get; private set; }
        public RawRectangleF ClientRectangle_1 { get; private set; }
        public RawRectangleF ClientRectangle_2 { get; private set; }
        public RawRectangleF ClientRectangle_3 { get; private set; }
        public RawRectangleF ClientRectangle_4 { get; private set; }
        public RawRectangleF ClientRectangle_5 { get; private set; }
        public RawRectangleF ClientRectangle_6 { get; private set; }

        public RawRectangleF rc { get; private set; }


        /// <summary>
        /// 선택된 마커 컬렉션
        /// </summary>
        List<RawRectangleF> selectedMarkers = new List<RawRectangleF>();

        /// <summary>
        /// 선택된 마커
        /// </summary>
        List<RawRectangleF> selectedMarker = new List<RawRectangleF>();

        //마커 컬렉션
        List<RawRectangleF> markers = new List<RawRectangleF>();
        List<RawRectangleF> markers_text = new List<RawRectangleF>();
    
        List<string> markers_num = new List<string>();
        //선택된 객체 정의
        private Rectangle selection;
        List<RawRectangleF> selection_markers = new List<RawRectangleF>();

        /// <summary>
        /// 고속 컬링을 위해 마커를 저장하는 쿼드트리
        /// </summary>
        //private QuadTree.QuadTree<Marker> markerTree;


        //초기 렉탕글 ltrb 잡기
        float l = 50;
        float t = 10;
        float r = 90;
        float b = 80;

        //처음 텍스트 값
        float l_text = 95;
        float t_text = 25;
        float r_text = 20;
        float b_text = 20;

        // 마커들 크기 변수 선언
        float markers_left = 0;
        float markers_top = 0;
        float markers_right = 0;
        float markers_bottom = 0;

        // 선택된 테두리 마커들 크기 변수 선언
        float selected_marker_left = 0;
        float selected_marker_top = 0;
        float selected_marker_right = 0;
        float selected_marker_bottom = 0;

        
        //화면 실시간 좌표값 표츌
        Rectangle rawrect_x1;
        Rectangle rawrect_y1;

        //마우스 클릭 시 x,y좌표값
        private int x1 = 0;
        private int y1 = 0;

        //마우스의 현재 x,y좌표값
        private int x2 = 0;
        private int y2 = 0;

        //마우스 클릭시 x, y좌표
        private int mouseXPos;
        private int mouseYPos;

        //클릭 시 flag객체
        private bool isMousedown = false;

        //Ctrl 입력 시 flag객체
        private bool isCtrl = false;

        //마우스 왼쪽 클릭 시 flag객체
        private bool isMouseLeft = false;

        bool redraw2 = false;

        //paint 횟수 제한할때 사용하는 flag
        int a = 0;
        
        //마커 그릴 때 조건문
        bool markers_yn = true;
        
        //fit flag
        private bool firstFit = true;

        //사각형 생성 텍스트 번호
        int markers_Addnum;

        //사각형 생성 버튼 마커 크기
        float l_add;
        float r_add;

        //사각형 생성 버튼 마커 텍스트 크기
        float l_text_add;
        float r_text_add;

        //사각형 생성 버튼 flag
        bool make_marker = false;
        public MainForm()
        {

            InitializeComponent();
            ZoomScale = 1.0f;
            panOffset = new Point(0, 0);

            dataGridView1.KeyDown += FrmKeyDown_KeyDown;
            dataGridView1.KeyUp += MainForm_KeyUp;
            dataGridView1.MouseDown += dataGridView1_MouseDown;
            dataGridView1.MouseMove += dataGridView1_MouseMove;
            dataGridView1.MouseUp += dataGridView1_MouseUp;
            MouseWheel += OnMouseWheel;

            int b = 0;
            while (b < 3)
            {
            Initialize();
                b++;
            }
            
            //miplevel 3
            LoadImage(3);


            // 마커 저장용 쿼드트리 초기화
            Rectangle qtBoundingBox = new Rectangle(0, 0, Width, Height);
            //markerTree = new QuadTree.QuadTree<Marker>(qtBoundingBox);


        }

        //초기화 및 변수 값 선언, 값 초기화
        protected void Initialize()
        {

            //팩토리2D 객체 생성
            Factory2D = new SharpDX.Direct2D1.Factory();
            //팩토리Write 객체 생성
            FactoryDWrite = new SharpDX.DirectWrite.Factory();

            // 렌더타겟 속성 객체를 생성한다.
            RenderTargetProperties rtProp = new RenderTargetProperties(
                RenderTargetType.Hardware,
                new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore),
                96, 96,
                RenderTargetUsage.None,
                FeatureLevel.Level_9);

            //picturebox를 담는 hwnd
            HwndRenderTargetProperties properties = new HwndRenderTargetProperties();
            properties.Hwnd = dataGridView1.Handle;
            properties.PixelSize = new SharpDX.Size2(dataGridView1.Width, dataGridView1.Height);
            properties.PresentOptions = PresentOptions.Immediately;
                        
            TextFormat = new TextFormat(FactoryDWrite, "Calibri", 14) 
            { 
                TextAlignment = TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center 
            };

            TextFormat_xy = new TextFormat(FactoryDWrite, "Calibri", 18)
            {
                TextAlignment = TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center
            };

            textLayout1 = new TextLayout(FactoryDWrite, "이동 테스트", TextFormat, 1000.0f, 50.0f);

            RenderTarget2D = new WindowRenderTarget(Factory2D, rtProp, properties);
            RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;
            RenderTarget2D.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;

            bufferRenderTarget = new SharpDX.Direct2D1.BitmapRenderTarget(RenderTarget2D, CompatibleRenderTargetOptions.None);
            bufferRenderTarget.AntialiasMode = AntialiasMode.Aliased;

            brushes = new SortedDictionary<int, SharpDX.Direct2D1.Brush>();

            // 배경색을 설정
            clearColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f);

            SceneColorBrush = new SolidColorBrush(RenderTarget2D, new SharpDX.Mathematics.Interop.RawColor4(Color.Black.R / 255.0f, Color.Black.G / 255.0f, Color.Black.B / 255.0f, Color.Black.A / 255.0f));
            //SceneColorBrush.Color = black;
            text_SceneColorBrush = new SolidColorBrush(RenderTarget2D, new SharpDX.Mathematics.Interop.RawColor4(Color.Black.R / 255.0f, Color.Black.G / 255.0f, Color.Black.B / 255.0f, Color.Black.A / 255.0f));

            // 선택 툴 드래그 박스 브러시 초기화(빨간색)
            SelectionBoxBrush = GetBrush(Color.FromArgb(255, 255, 0, 0));

            // 선택 툴 드래그 박스 브러시 초기화(빨간색)
            SelectionLineBrush = GetBrush(Color.FromArgb(255, 255, 0, 0));

            // 선택 상태 표시 브러시 초기화(마젠타색)
            selectedMarkerBrush = GetBrush(Color.FromArgb(255, 255, 0, 255));


            Initialized = true;
        }

        /// <summary>
        /// 이미지 로드
        /// </summary>
        public void LoadImage(int mipLevels)
        {
            if (!ImageLoaded)
            {
                // jpg 이미지 파일 경로
                ImageFileName = System.Windows.Forms.Application.StartupPath.ToString() + "\\map\\B1.jpg";

                // 이미지 매니저에서 로드한다.
                Load_img(ImageFileName, mipLevels);
                if (ImageLoaded = this.Image != null) // 로드 여부 체크 및 설정
                {
                    // 이미지 픽셀 크기 가져옴.
                    ImageWidth = this.Image[0].PixelSize.Width;
                    ImageHeight = this.Image[0].PixelSize.Height;
                }
            }
        }

        /// <summary>
        /// 지정한 색상의 D2D 브러시를 생성하여 반환한다. 생성된 브러시는 렌더러 shutdown시에 자동으로 dispose된다.
        /// </summary>
        /// <param name="color">브러시 색상</param>
        /// <returns>브러시</returns>
        public SharpDX.Direct2D1.Brush GetBrush(Color color)
        {
            int argb = color.ToArgb();

            SharpDX.Direct2D1.Brush brush = null;

            if (brushes.TryGetValue(argb, out brush))
            {
                return brush;
            }
            else
            {
                brush =
                    new SharpDX.Direct2D1.SolidColorBrush(
                        RenderTarget2D,
                        new SharpDX.Mathematics.Interop.RawColor4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f));

                brushes.Add(argb, brush);

                return brush;
            }
        }

        public SharpDX.Direct2D1.Bitmap[] Load_img(string fileName, int mipLevels)
        {

            //IMAGES 객체 생성
            images = new Dictionary<string, SharpDX.Direct2D1.Bitmap[]>();
            //이미지 객체 생성
            ImagingFactory = new SharpDX.WIC.ImagingFactory();

            // 컬렉션에 없으면
            if (!images.ContainsKey(fileName))
            {   

                using (Stream imageStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    // 디코더 생성
                    using (var decoder = new SharpDX.WIC.BitmapDecoder(
                        ImagingFactory,
                        imageStream,
                        SharpDX.WIC.DecodeOptions.CacheOnLoad))
                    {
                        // 이미지 디코드
                        using (var frameDecode = decoder.GetFrame(0))
                        {
                            // D2D에서 사용하기 위한 픽셀 포맷 컨버터 생성
                            using (var formatConverter = new SharpDX.WIC.FormatConverter(ImagingFactory))
                            {
                                formatConverter.Initialize(
                                    frameDecode,
                                    SharpDX.WIC.PixelFormat.Format32bppPBGRA,
                                    SharpDX.WIC.BitmapDitherType.None,
                                    null,
                                    0.0,
                                    SharpDX.WIC.BitmapPaletteType.Custom);

                                int width = frameDecode.Size.Width;
                                int height = frameDecode.Size.Height;
                                // 밉맵 저장할 배열 생성
                                var bitmaps = new SharpDX.Direct2D1.Bitmap[mipLevels];

                                // 밉맵 레벨을 순회하며 스케일러를 생성하여 밉맵 배열에 스케일링된 비트맵을 생성하여 넣는다.
                                for (int i = 0; i < mipLevels; ++i)
                                {
                                    using (SharpDX.WIC.BitmapScaler scaler = new SharpDX.WIC.BitmapScaler(ImagingFactory))
                                    {
                                        // 밉의 크기는 밉맵 레벨에 따라 0=100%, 1=50%, 2=25%, 3=12.5%....로 각 이전 레벨의 1/2만큼 줄어든다.
                                        scaler.Initialize(formatConverter, width >> i, height >> i, SharpDX.WIC.BitmapInterpolationMode.HighQualityCubic);

                                        bitmaps[i] = null;
                                        bitmaps[i] = SharpDX.Direct2D1.Bitmap.FromWicBitmap(FirstRenderTarget, scaler);
                                    }
                                }

                                // 컬렉션에 추가
                                images.Add(fileName, bitmaps);

                                return bitmaps;
                            }
                        }
                    }
                }

                //return null;
            }
            // 컬렉션에 있으면
            else
            {
                // 있는거 반환
                return images[fileName];
            }
        }

        /// <summary>
        /// 지정한 가로 크기에 가장 적합한 밉맵 레벨 반환. 반환하는 밉 레벨에 해당하는 이미지 가로 크기는 항상 지정한 대상 크기보다 크다.
        /// 예를 들어 밉맵 이미지의 가로 크기가 각각 1000, 500, 250이고 대상 가로 크기가 400이면 밉맵 레벨은 1을 반환한다.
        /// </summary>
        /// <param name="targetWidth">대상 가로 크기</param>
        /// <returns></returns>
        public int GetMipLevel(int targetWidth)
        {
            int mipLevels = Image.Length;
            int pd = 999999;
            int selectedLevel = 0;
            for (int i = 0; i < mipLevels; ++i)
            {
                int width = ImageWidth >> i;

                int d = width - targetWidth;
                if (d >= 0)
                {
                    if (d < pd)
                    {
                        pd = d;
                        selectedLevel = i;
                    }
                }
                else
                    break; // d가 0보다 작아지기 시작했다면 이후 레벨은 계산할 필요 없으므로 종료한다.
            }

            return selectedLevel;
        }

        /// <summary>
        /// 임시 객체 그리기. 드래그시 사각형 박스를 그린다.
        /// </summary>
        public void DrawTemporaryObject()
        {

            if (isMouseLeft && !isCtrl )
            {

                var left = x1 < x2 ? x1 : x2;
                var top = y1 < y2 ? y1 : y2;

                rc = new SharpDX.Mathematics.Interop.RawRectangleF(
                    left, top,
                    left + Math.Abs(x1 - x2),
                    top + Math.Abs(y1 - y2));

                RenderTarget2D.DrawRectangle(rc, SelectionBoxBrush, 1.0f);

                Invalidate();
                
            }
            
        }

        /// <summary>
        /// 지정한 비율 만큼 도면을 축소한다.
        /// </summary>
        /// <param name="factor">축소 비율. 1.0f보다 큰 값을 지정해야 축소된다. 1.1f = 10% 축소</param>
        /// <param name="xCenter">축소 중심점 X 좌표(화면좌표)</param>
        /// <param name="yCenter">축소 중심점 X 좌표(화면좌표)</param>
        public void ZoomOut(float factor, int xCenter, int yCenter)
        {
            float preZoomX = xCenter;
            float preZoomY = yCenter;
            ScreenPosToWorkspacePos(ref preZoomX, ref preZoomY);

            ZoomScale /= factor;

            float postZoomX = xCenter;
            float postZoomY = yCenter;
            ScreenPosToWorkspacePos(ref postZoomX, ref postZoomY);

            panOffset.X -= (int)((preZoomX - postZoomX) * ZoomScale);
            panOffset.Y -= (int)((preZoomY - postZoomY) * ZoomScale);

            //Invalidated = true;
        }

        /// <summary>
        /// 지정한 비율 만큼 도면을 확대한다.
        /// </summary>
        /// <param name="factor">확대 비율. 1.0f 보다 큰 값을 지정해야 확대된다. 1.1f = 10% 확대</param>
        /// <param name="xCenter">확대 중심점 X 좌표(화면좌표)</param>
        /// <param name="yCenter">확대 중심점 Y 좌표(화면좌표)</param>
        public void ZoomIn(float factor, int xCenter, int yCenter)
        {
            float preZoomX = xCenter;
            float preZoomY = yCenter;
            ScreenPosToWorkspacePos(ref preZoomX, ref preZoomY);

            ZoomScale *= factor;

            float postZoomX = xCenter;
            float postZoomY = yCenter;
            ScreenPosToWorkspacePos(ref postZoomX, ref postZoomY);

            panOffset.X -= (int)((preZoomX - postZoomX) * ZoomScale);
            panOffset.Y -= (int)((preZoomY - postZoomY) * ZoomScale);

            //Invalidated = true;
        }

        /// <summary>
        /// 화면 좌표를 도면 좌표로 변환한다.
        /// </summary>
        /// <param name="x">화면 X 좌표</param>
        /// <param name="y">화면 Y 좌표</param>
        public void ScreenPosToWorkspacePos(ref float x, ref float y)
        {
            x = x / ZoomScale - (PanOffset.X / ZoomScale);
            y = y / ZoomScale - (PanOffset.Y / ZoomScale);
        }

        /// <summary>
        /// 화면 좌표를 도면 좌표로 변환한다.
        /// </summary>
        /// <param name="x">화면 X 좌표</param>
        /// <param name="y">화면 Y 좌표</param>
        public void ScreenPosToWorkspacePos(ref int x, ref int y)
        {
            x = (int)Math.Round(x / ZoomScale - (PanOffset.X / ZoomScale));
            y = (int)Math.Round(y / ZoomScale - (PanOffset.Y / ZoomScale));
        }

        /// <summary>
        /// 모든 마커의 선택 상태를 해제한다.
        /// </summary>
        public void DeselectAllMarkers()
        {
            selectedMarker.Clear();
        }

        /// <summary>
        /// 지정한 값 만큼 도면을 이동시킨다.
        /// </summary>
        /// <param name="dx">가로 이동 값</param>
        /// <param name="dy">세로 이동 값</param>
        public void Pan(int dx, int dy)
        {
            panOffset.Offset(dx, dy);
            //Invalidated = true;
        }

        /// <summary>
        /// 현재 작업중인 도면 배율을 지정한 렌더러 크기에 맞춘다.
        /// </summary>
        public void Fit()
        {
            //if (WorkingFloor != null)
            //{
                // 도면 이미지 크기 가져옴
                float imageWidth = ImageWidth;
                float imageHeight = ImageHeight;

                float viewWidth = dataGridView1.Width;
                float viewHeight = dataGridView1.Height;

                float targetWidth = Math.Min(imageWidth, viewWidth);
                float targetHeight = Math.Min(imageHeight, viewHeight);
                ZoomScale = Math.Min(targetWidth / imageWidth, targetHeight / imageHeight);
                panOffset.X = (int)((viewWidth - imageWidth * ZoomScale) * 0.5f);
                panOffset.Y = (int)((viewHeight - imageHeight * ZoomScale) * 0.5f);

                firstFit = true;
                //Invalidated = true;
            //}
        }

        #region Paint 이벤트
        //그리기
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {

            try
            {
                    // 처음 로드 시 Fit함수 호출
                    if (firstFit)
                    {
                        Fit();
                        firstFit = false;
                    }

                if (Initialized)
                {
                    //5회 초기화 후 paint 안그러면 paint함수가 지속적으로 타서 계속 그리게된다.
                    //if (Initialized && a < 3)
                    //{

                    //백버퍼 그리기 시작
                    bufferRenderTarget.BeginDraw();

                //바탕화면 흰색 클리어
                bufferRenderTarget.Clear(clearColor);

                string str = "X:" + rawrect_x1.X +

                            "Y:" + rawrect_y1.Y;


                // 윈도우에 그려질 사각형 영역 생성
                Rectangle targetRect = new Rectangle(
                    panOffset.X,
                    panOffset.Y,
                    (int)(ZoomScale * ImageWidth),
                    (int)(ZoomScale * ImageHeight));

                // 그릴 이미지 사각형 영역 생성
                Rectangle sourceRect = new Rectangle(
                    0,
                    0,
                    ImageWidth,
                    ImageHeight);

                // 사각형 영역을 d2d 사각형으로 변환한다.
                var d2dDestRect = new SharpDX.Mathematics.Interop.RawRectangleF(
                    targetRect.X, targetRect.Y,
                    targetRect.Right, targetRect.Bottom);

                // 사각형 영역을 d2d 사각형으로 변환한다.2
                var d2dSrcRect = new SharpDX.Mathematics.Interop.RawRectangleF(
                    sourceRect.X, sourceRect.Y,
                    sourceRect.Right, sourceRect.Bottom);

                // 도면 이미지를 버퍼에 그린다.
                bufferRenderTarget.DrawBitmap(
                    Image[GetMipLevel(targetRect.Width)],
                    d2dDestRect, 1.0f, BitmapInterpolationMode.Linear, d2dSrcRect);

                    // 렌더타겟의 트랜스폼 백업
                    var transformBackup = bufferRenderTarget.Transform;

                    // 타원 생성
                    //bufferRenderTarget.DrawEllipse(new Ellipse(new RawVector2(430, 60), 50, 50), SceneColorBrush, 2.00f);

                    // 화면 실시간 좌표 표출
                    bufferRenderTarget.DrawText(str, TextFormat_xy, new RawRectangleF(750, 12, 850, 32), text_SceneColorBrush);

                    // add를 한번만 돌게끔 true/false문 작성
                    if (markers_yn)
                    {

                        //rectangle
                        markers.Add(new RawRectangleF(l, t, r, b));
                        markers.Add(new RawRectangleF(l+60, t, r+60, b));
                        markers.Add(new RawRectangleF(l+120, t, r+120, b));
                        markers.Add(new RawRectangleF(l+180, t, r+180, b));
                        markers.Add(new RawRectangleF(l+240, t, r+240, b));

                        //text
                        markers_text.Add(new RawRectangleF(l_text, t_text, r_text, b_text));
                        markers_text.Add(new RawRectangleF(l_text+60, t_text, r_text+60, b_text));
                        markers_text.Add(new RawRectangleF(l_text+120, t_text, r_text+120, b_text));
                        markers_text.Add(new RawRectangleF(l_text+180, t_text, r_text+180, b_text));
                        markers_text.Add(new RawRectangleF(l_text+240, t_text, r_text+240, b_text));

                        markers_num.Add("1");
                        markers_num.Add("2");
                        markers_num.Add("3");
                        markers_num.Add("4");
                        markers_num.Add("5");


                        l_add = l + 300;
                        r_add = r + 300;

                        l_text_add = l_text + 300;
                        r_text_add = r_text + 300;

                        //텍스트 1 더한값
                        markers_Addnum = Convert.ToInt32(markers_num[markers_num.Count - 1]) + 1;

                        markers_yn = false;
                    }

                    //사각형 5개 그리기
                    foreach (var marker in markers)
                    {

                        //트랜스폼 업데이트
                        UpdateTransform();

                        //Draw(marker);

                        //사각형 내부 색 적용    
                        bufferRenderTarget.FillRectangle(marker, GetBrush(Color.FromArgb(192, 255, 192)));

                        //사각형 테두리 stroke 효과 적용
                        bufferRenderTarget.DrawRectangle(marker, SceneColorBrush, 2.00f);

                    }

                    //텍스트 5개 그리기
                    for (int i = 0; i < markers_num.Count; i++)
                    {

                        bufferRenderTarget.DrawText(markers_num[i], TextFormat, markers_text[i], text_SceneColorBrush);

                    }

                    // 렌더타겟의 트랜스폼 복원
                    bufferRenderTarget.Transform = transformBackup;

                    //버퍼렌더 종료
                    bufferRenderTarget.EndDraw();

                    //렌더타겟의 트랜스폼 백업
                    RenderTarget2D.BeginDraw();

                    // 백버퍼 -> 윈도우 버퍼 전송 (도면 나타냄)
                    RenderTarget2D.DrawBitmap(bufferRenderTarget.Bitmap, 1.0f, BitmapInterpolationMode.NearestNeighbor);

                    // 렌더타겟의 트랜스폼 백업
                    var transformBackup2 = RenderTarget2D.Transform;

                    //사각형 5개 그리기
                    foreach (var selected_marker in selectedMarker)
                    {

                        //사각형 테두리 stroke 효과 적용
                        RenderTarget2D.DrawRectangle(selected_marker, selectedMarkerBrush, 2.00f);

                    }

                    //드래그 시 빨간 rectangle 생성, 영역2 잡기
                    DrawTemporaryObject();

                    // 렌더타겟의 트랜스폼 복원
                    RenderTarget2D.Transform = transformBackup2;
                    //그리기 종료
                    RenderTarget2D.EndDraw();

                    a++;

                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 마커 및 텍스트를 그림
        /// </summary>
        public void Draw(RawRectangleF marker)
        {

        }
        /// <summary>
        /// 렌더타겟에 설정하기 위한 트랜스폼을 업데이트한다.
        /// </summary>
        public void UpdateTransform()
        {
            // 마커 이미지의 절반 크기 구함(마커의 위치가 좌표의 중간에 오도록 계산을 위해)
            float halfWidth = ImageWidth * 0.5f;
            float halfHeight = ImageHeight * 0.5f;

            // 스케일 매트릭스 생성
            // 스케일 구함
            var scale = SharpDX.Matrix3x2.Scaling(ZoomScale, ZoomScale);

            // 이동 매트릭스 생성
            var trans = SharpDX.Matrix3x2.Translation(
                panOffset.X + (l * ZoomScale) - (halfWidth * ZoomScale),
                panOffset.Y + (t * ZoomScale) - (halfHeight * ZoomScale));

            // 회전 매트릭스 생성
            var rot = SharpDX.Matrix3x2.Rotation(
                Rotation * ToRad,
                new SharpDX.Vector2(halfWidth, halfHeight));
            
            // 스케일x이동x회전 매트릭스 산출
            var newTransform = SharpDX.Matrix3x2.Multiply(scale, trans);
            Transform = SharpDX.Matrix3x2.Multiply(rot, newTransform);
        }


        ///// <summary>
        ///// 지정한 영역 내에 해당하는 아이템을 검색한다. 재귀호출함.
        ///// </summary>
        ///// <param name="currentNode">검색할 노드</param>
        ///// <param name="rectangle">검색 영역</param>
        ///// <param name="precisly">정확한 컬링을 수행할지 여부</param>
        ///// <param name="items">검색된 아이템 리스트 반환 인자</param>
        //private void FindItemsInRectangle(Node<ItemType> currentNode, Rectangle rectangle, bool precisly, ref List<ItemType> items)
        //{
        //    if (currentNode.BoundingBox.IntersectsWith(rectangle))
        //    {
        //        // 정확한 컬링을 수행한다면 컬링된 노드에 있는 아이템이 컬링 박스 안에 들어가는지 체크한다.
        //        if (precisly)
        //        {
        //            // 현재 노드가 보유하고 있는 아이템 리스트를 순회하면서
        //            foreach (var item in currentNode.Items)
        //            {
        //                // 검색 영역 내에 아이템이 포함되면 검색된 리스트에 추가
        //                if (item.HitTest(rectangle))
        //                    items.Add(item);
        //            }
        //        }
        //        // 정확한 컬링을 수행하지 않는다면 컬링된 노드에 있는 아이템을 그냥 결과 리스트에 넣는다.
        //        else
        //        {
        //            items.AddRange(currentNode.Items);
        //        }

        //        // 현재 노드가 자식 노드를 갖고 있으면
        //        if (currentNode.HasChildren)
        //        {
        //            // 자식 노드를 순회하면서 재귀호출
        //            for (int i = 0; i < 4; ++i)
        //            {
        //                FindItemsInRectangle(currentNode.GetChild((Node<ItemType>.Segments)i), rectangle, precisly, ref items);
        //            }
        //        }
        //    }
        //}

        #endregion

        #region 클릭 이벤트

        
        //사각형 생성 버튼 클릭
        private void CRT_parkingslot_Click(object sender, EventArgs e)
        {

            //상단에 한줄 생성
            markers.Add(new RawRectangleF(l_add, t, r_add, b));

            markers_num.Add(Convert.ToString(markers_Addnum));

            markers_text.Add(new RawRectangleF(l_text_add, 25, r_text_add, 20));

            //markers_num 리스트에서 마지막 값에서 +1씩 하기위한 마지막 값을 담은 객체 생성                               
            l_add += 60;
            r_add += 60;
            l_text_add += 60;
            r_text_add += 60;
            markers_Addnum++;

            //생성 버튼 누르면 마우스 움직일때 따라다님
            make_marker = true;

        }

        //타원 만들기
        private void button1_Click(object sender, EventArgs e)
        {
            Random ran = new Random();

            int ran_num_o1 = ran.Next(50, 200);
            int ran_num_o2 = ran.Next(30, 200);
            int ran_num_o3 = ran.Next(50, 200);
            int ran_num_o4 = ran.Next(30, 200);

            RenderTarget2D.BeginDraw();
            RenderTarget2D.DrawEllipse(new Ellipse(new RawVector2(ran_num_o1, ran_num_o2), ran_num_o3, ran_num_o4), SceneColorBrush);
            RenderTarget2D.EndDraw();
        }

             //이동 텍스트 테스트
             private void button2_Click(object sender, EventArgs e)
             {

                 //이동 아래로
                 var a = markers[0];
                 for (int i = 0; i < markers.Count; i++)
                 {

                     markers[0] = new RawRectangleF(l,t,r,b);
                     markers_text[0] = new RawRectangleF(l_text, t_text, r_text, b_text);
                     //l += 5;
                     t += 2;
                     //r += 5;
                     b += 2;

                     //텍스트
                     t_text += 2;
                     b_text += 2;

                 };

             }

             //사이즈 조정
             private void button3_Click(object sender, EventArgs e)
             {

                     markers[0] = new RawRectangleF(l, t, r, b);
                     markers_text[0] = new RawRectangleF(l_text, t_text, r_text, b_text);

                     // 사이즈 조정    
                     l += 5;
                     r += 15;
                     t += 5;
                     b += 15;

                     //텍스트
                     l_text += 5;
                     r_text += 7;
                     t_text += 5;
                     b_text += 7;

             }
             #endregion

        #region 키보드 이벤트

             //폼에서 키보드가 입력 될 때
             private void FrmKeyDown_KeyDown(object sender, KeyEventArgs e)
             {
                 //markers[0] = new RawRectangleF(l, t, r, b);

                 switch (e.KeyCode)
                 {
                     case Keys.Left:
                        if (r > 60)
                        {
                           r -= 10;   //Invalidate(); = 다시 그리려주기
                           b -= 10;
                           r_text -= 3;
                           r_text -= 3;
                        }
                        Invalidate(); break;
                     case Keys.Right:
                        r += 10;
                        b += 10;
                        Invalidate(); break;

                     case Keys.Up:
                        Invalidate(); break;

                     case Keys.Down:
                         Invalidate(); break;

                     case Keys.ControlKey:
                        isCtrl = true;

                        Invalidate(); break;
                     case Keys.Delete:

                    if(selectedMarker.Count > 0)
                    {

                   if (MessageBox.Show("삭제하시겠습니까?","삭제",MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                   {

                      for(int i=0;  i< selectedMarker.Count; i++)
                      {

                        for (int j = 0; j < markers.Count; j++)
                        {

                            if (selectedMarker[0].Left == markers[j].Left &&
                                selectedMarker[0].Top == markers[j].Top &&
                                selectedMarker[0].Right == markers[j].Right &&
                                selectedMarker[0].Bottom == markers[j].Bottom)
                          {

                                        selectedMarker[i] = new RawRectangleF(-10, -10, -10, -10);
                                        markers[j] = new RawRectangleF(-10, -10, -10, -10);
                                        markers_text[j] = new RawRectangleF(-10, -10, -10, -10);

                                        selectedMarker.Clear();

                                        markers.RemoveAt(j);

                                        markers_text.RemoveAt(j);
                          }
                        };

                      };
                    }

                     //delete시 기존 있던 마지막 마커에서 시작할 수 있도록 생성
                        l_add -= 60;
                        r_add -= 60;
                        l_text_add -= 60;
                        r_text_add -= 60;
                        markers_Addnum -= 1;
                    }

                    Invalidate(); break;
                default:
                         break;
                 }

             }

            private void MainForm_KeyUp(object sender, KeyEventArgs e)
            {
                isCtrl = false;
            }

        #endregion
        
        #region 마우스 이벤트
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {

            isMousedown = true;
            redraw2 = true;
            mouseXPos = e.X;
            mouseYPos = e.Y;

            //초기값 = mousedown위치 
            x1 = e.X;
            y1 = e.Y;


            switch (e.Button)
            {
                case MouseButtons.Left:

                    // 해당 선택된 다른 마커 초기화
                    DeselectAllMarkers();

                    //다중 말고 클릭 시 x,y좌표 안에 있는 사각형 컬렉션에 담기 
                    for (int i = 0; i < markers.Count; i++)
                    {

                        if (markers[i].Top < y1 && y1 < markers[i].Bottom &&
                            markers[i].Left < x1 && x1 < markers[i].Right)
                        {

                            selectedMarker.Add(markers[i]);

                        }
                        else
                        {
                            //selectedMarker.Clear();
                        }
                    };

                    isMouseLeft = true;

                    // 초기 검색된 x2,y2의 값을 초기화함
                    x2 = x1;
                    y2 = y1;

                    break;

                case MouseButtons.Middle:

                    break;

                case MouseButtons.Right:
                    ContextMenu = null;

                    //해당 절대위치 마커 생성
                    for (int i = 0; i < markers.Count; i++)
                    {

                        //처음 텍스트 값으로 초기화
                        float l_text = 95;
                        float t_text = 25;
                        float r_text = 20;
                        float b_text = 20;

                        //사각형 마커 처음 값으로 초기화
                        float l2 = 50;
                        float t2 = 10;
                        float r2 = 90;
                        float b2 = 80;

                        markers[i] = new RawRectangleF(l2+(i*60), t2, r2 + (i * 60), b2);
                        markers_text[i] = new RawRectangleF(l_text + (i * 60), t_text, r_text + (i * 60), b_text);
                        
                    }

                    //선택된 주차면 초기화
                    selectedMarker.Clear();
                    
                    Fit();

                    break;
            }

            if (redraw2)
            {
                Invalidate();
            }
        }


        private void dataGridView1_MouseMove(object sender, MouseEventArgs e)
        {
            
            int live_x = e.X;
            int live_y = e.Y;

            ScreenPosToWorkspacePos(ref live_x, ref live_y);

            rawrect_x1.X = live_x;
            rawrect_y1.Y = live_y;

            x1 = e.X;
            y1 = e.Y;
            isMousedown = true;

            //도면 이동 alt
            if (isMousedown && e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.Alt)
            {
                Pan(e.X - mouseXPos, e.Y - mouseYPos);

                mouseXPos = e.X;
                mouseYPos = e.Y;

                isCtrl = true;
            }

            //마커 이동 ctrl
            if (isMousedown && isCtrl && isMouseLeft)
            {

                if (selectedMarker.Count > 0)
                {
                    //selectedMarker = selectedMarker.Distinct().ToList();
                    //markers = markers.Distinct().ToList();

                    for (int j = 0; j < markers.Count; j++)
                    {

                        if (selectedMarker[0].Left == markers[j].Left &&
                            selectedMarker[0].Top == markers[j].Top &&
                            selectedMarker[0].Right == markers[j].Right &&
                            selectedMarker[0].Bottom == markers[j].Bottom)
                        {
       
                            markers_left = e.X;
                            markers_top = e.Y;
                            markers_right = e.X + 40;
                            markers_bottom = e.Y + 70;

                            selected_marker_left = e.X;
                            selected_marker_top = e.Y;
                            selected_marker_right = e.X + 40;
                            selected_marker_bottom = e.Y + 70;

                            //텍스트 위치
                            l_text = e.X + 15;
                            r_text = e.X;
                            t_text = e.Y + 20;
                            b_text = e.Y;

                            markers[j] = new RawRectangleF(markers_left, markers_top, markers_right, markers_bottom);
                            markers_text[j] = new RawRectangleF(l_text, t_text, r_text, b_text);
                            selectedMarker[0] = new RawRectangleF(selected_marker_left, selected_marker_top, selected_marker_right, selected_marker_bottom);



                        }

                    }

                }

            }

            //마커 생성버튼 누를 시 작동
            if (make_marker)
            {

            }

            if (redraw2 || isMousedown)
            {
                Invalidate();
            }
        }

        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {

            if (!isCtrl)
            {

                // 드래그 시작 좌표를 도면 좌표로 변경
                int ix1 = x1;
                int iy1 = y1;
                ScreenPosToWorkspacePos(ref ix1, ref iy1);

                // 드래그 종료 좌표를 도면 좌표로 변경
                int ix2 = x2;
                int iy2 = y2;
                ScreenPosToWorkspacePos(ref ix2, ref iy2);

                // 검색 영역 사각형 생성
                var selectionRect = Util.CreateRectangleFromPoints(ix1, iy1, ix2, iy2);

                // 검색된 마커 저장할 리스트 생성
                var search_markers = new List<Marker>();

                // 선택 사각형의 크기가 0이면(=마우스를 드래그하지 않고 클릭했으면)
                if (selectionRect.Width == 0 &&
                    selectionRect.Height == 0)
                {
                    //// Pick 수행
                    //Marker mk = null;
                    //if (ws.WorkingFloor.PickMarker(ix1, iy1, out mk))
                    //{
                    //    markers.Add(mk);
                    //}
                }
                else
                {
                    // 검색 실패를 방지하기 위해 검색 사각형의 가로/세로 중 0인것을 1로 바꾼다.
                    if (selectionRect.Width == 0) selectionRect.Width = 1;
                    if (selectionRect.Height == 0) selectionRect.Height = 1;

                    //FindItemsInRectangle(Root, selectionRect, true, ref markers);

                }

            }

            isMousedown = false;
            isMouseLeft = false;
            x2 = e.X;
            y2 = e.Y;
            redraw2 = false;

        }

        // 마우스 휠 동작
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            bool redraw = false;

            // 컨트롤 키를 누른 상태면 도면 줌 인/아웃
            if (Control.ModifierKeys == Keys.Control)
            {
                if (e.Delta < 0)
                {
                    for(int i =0; i<markers.Count; i++)
                    {

                        
                        if(selectedMarker.Count > 0)
                        {

                            if (selectedMarker[0].Left == markers[i].Left &&
                                selectedMarker[0].Top == markers[i].Top &&
                                selectedMarker[0].Right == markers[i].Right &&
                                selectedMarker[0].Bottom == markers[i].Bottom)
                            {

                                selected_marker_left = markers[i].Left;
                                selected_marker_top = markers[i].Top;
                                selected_marker_right = markers[i].Right;
                                selected_marker_bottom = markers[i].Bottom;

                                selectedMarker[0] = new RawRectangleF(selected_marker_left, selected_marker_top, selected_marker_right - 1, selected_marker_bottom - 1);
                                markers[i] = new RawRectangleF(markers[i].Left, markers[i].Top, markers[i].Right - 1, markers[i].Bottom - 1);

                            }
                        }
                        else
                        {
                            markers[i] = new RawRectangleF(markers[i].Left, markers[i].Top, markers[i].Right-1, markers[i].Bottom-1);
                            selectedMarker.Clear();

                        }
                    }
                    if (selectedMarker.Count == 0)
                    {
                        ZoomOut(1.1f, e.X, e.Y);
                    }
                    
                }
                else
                {
                    for (int i = 0; i < markers.Count; i++)
                    {


                        if (selectedMarker.Count > 0)
                        {

                            if (selectedMarker[0].Left == markers[i].Left &&
                                selectedMarker[0].Top == markers[i].Top &&
                                selectedMarker[0].Right == markers[i].Right &&
                                selectedMarker[0].Bottom == markers[i].Bottom)
                            {

                                selected_marker_left = markers[i].Left;
                                selected_marker_top = markers[i].Top;
                                selected_marker_right = markers[i].Right;
                                selected_marker_bottom = markers[i].Bottom;

                                selectedMarker[0] = new RawRectangleF(selected_marker_left, selected_marker_top, selected_marker_right + 1, selected_marker_bottom + 1);
                                markers[i] = new RawRectangleF(markers[i].Left, markers[i].Top, markers[i].Right + 1, markers[i].Bottom + 1);

                        }
                    }
                        else
                        {

                            markers[i] = new RawRectangleF(markers[i].Left, markers[i].Top, markers[i].Right + 1, markers[i].Bottom + 1);
                            selectedMarker.Clear();

                        }
                    }

                    if (selectedMarker.Count == 0)
                    {
                        ZoomIn(1.1f, e.X, e.Y);
                    }

                }

                redraw = true;
            }

            //if (MainForm.Instance.ViewMode == MainForm.ViewModes.Edit &&
            //    this == MainForm.Instance.EditView)
            //{
            //    // 아니면 워크스페이스로 전달(마커 회전)
            //    if (workspace.MouseWheel(e.Delta, GetModifierKey()))
            //    {
            //        redraw = true;
            //    }
            //}

            if (redraw)
                Invalidate();
        }

        #endregion

    }
}
