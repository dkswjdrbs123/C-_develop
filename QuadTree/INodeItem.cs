
using System.Drawing;

namespace QuadTree
{
    /// <summary>
    /// 쿼드트리 노드의 아이템 타입 인터페이스.
    /// 어떤 클래스를 노드의 아이템으로 사용하려면 이 인터페이스를 상속하여
    /// 멤버를 구현해야 한다. 자세한 구현 방식은 Marker 클래스 참조.
    /// </summary>
    /// <typeparam name="ItemType">아이템 타입</typeparam>
    public interface INodeItem<ItemType> where ItemType : INodeItem<ItemType>
    {
        /// <summary>
        /// 항목의 X 위치
        /// </summary>
        int X
        {
            get;
        }

        /// <summary>
        /// 항목의 Y 위치
        /// </summary>
        int Y
        {
            get;
        }

        /// <summary>
        /// 항목의 바운딩 영역
        /// </summary>
        Rectangle BoundingBox
        {
            get;
        }

        /// <summary>
        /// 항목의 소유 노드
        /// </summary>
        Node<ItemType> OwnerNode
        {
            get;
            set;
        }

        /// <summary>
        /// 항목의 위치 설정
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        void SetPosition(int x, int y);

        /// <summary>
        /// 항목이 지정한 좌표와 충돌하는지 체크
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>충돌 여부</returns>
        bool HitTest(int x, int y);

        /// <summary>
        /// 항목이 지정한 좌표와 충돌하는지 체크. 지정한 버퍼 크기 만큼 오차를 허용한다.
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <param name="buffer">허용 오차</param>
        /// <returns>충돌 여부</returns>
        bool HitTest(int x, int y, int buffer);

        /// <summary>
        /// 항목이 지정한 사각형과 충돌하는지 체크.
        /// </summary>
        /// <param name="rect">사각형</param>
        /// <returns>충돌 여부</returns>
        bool HitTest(Rectangle rect);
    }
}
