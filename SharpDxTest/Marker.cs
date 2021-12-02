using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace SharpDxTest
{
    class Marker
    {
        
        /// <summary>
        /// 마커ID
        /// </summary>
        /// public int ID{
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 마커 이름
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        private int x = 0;
        private int y = 0;

        /// <summary>
        /// 마커의 X 위치(도면 좌표)
        /// </summary>
        public int X
        {
            get
            {
                return this.x;
            }
        }

        /// <summary>
        /// 마커의 Y 위치(도면 좌표)
        /// </summary>
        public int Y
        {
            get
            {
                return this.y;
            }
        }


        /// <summary>
        /// 이미지 객체의 참조. 실제 보유는 ImageManager가 함.
        /// </summary>
        //public SharpDX.Direct2D1.Bitmap Image;
        public SharpDX.Direct2D1.Bitmap Image
        {
            get
            {
                return Images[imageFileName][0];
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
        /// 마커의 회전 방향(각도)
        /// </summary>
        public float Rotation;

        /// <summary>
        /// 가로 크기
        /// </summary>
        public int Width;

        /// <summary>
        /// 세로 크기
        /// </summary>
        public int Height;

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
                
            }
        }

        MainForm mf = new MainForm();
        /// <summary>
        /// 마커의 위치와 회전 값을 워크스페이스의 이동 및 줌 값과 계산하여 얻은 트랜스폼.
        /// 렌더타겟에 마커를 그릴 때 사용.
        /// </summary>
        public SharpDX.Mathematics.Interop.RawMatrix3x2 Transform
        {
            get;
            protected set;
        }

        public Marker(String Name, int X, int Y, int W, int H)
        {
            images = new Dictionary<string, SharpDX.Direct2D1.Bitmap[]>();
        }
        public void DrawMarker(MainForm mf, RawRectangleF rf)

        {
            var mk = mf.BufferRenderTarget;
            //mk.Transform = Transform;

            //RawRectangleF rf = new RawRectangleF();
            //rf.Left = -(int)((mf.Width - mf.ImageWidth) * 0.5f);
            //rf.Top = -(int)((mf.Height - mf.ImageHeight) * 0.5f);
            //rf.Right = rf.Left + mf.Width;
            //rf.Bottom = rf.Top + mf.Height;

            //rf.Left = rf;
            //rf.Top = -(int)((mf.Height - mf.ImageHeight) * 0.5f);
            //rf.Right = rf.Left + mf.Width;
            //rf.Bottom = rf.Top + mf.Height;


            // 마커 이미지 그리기
            mk.DrawBitmap(this.Image, rf, 1.0f, BitmapInterpolationMode.NearestNeighbor);
        }

    }
}
