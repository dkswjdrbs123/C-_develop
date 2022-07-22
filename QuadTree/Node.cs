
using System.Collections.Generic;

using System.Drawing;



namespace QuadTree
{
    // http://stackoverflow.com/questions/6502666/access-class-property-passed-as-generic-type

    /// <summary>
    /// 쿼드트리 노드 클래스
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    public class Node<ItemType> where ItemType : INodeItem<ItemType>
    {
        /// <summary>
        /// 노드를 정4등분한 각 영역 종류
        /// </summary>
        public enum Segments : int
        {
            /// <summary>좌상</summary>
            TopLeft = 0,
            /// <summary>우상</summary>
            TopRight,
            /// <summary>좌하</summary>
            BottomLeft,
            /// <summary>우하</summary>
            BottomRight
        }

        /// <summary>
        /// 노드가 소유한 항목
        /// </summary>
        public List<ItemType> Items = new List<ItemType>();

        /// <summary>
        /// 부모 노드
        /// </summary>
        public Node<ItemType> Parent;

        /// <summary>
        /// 소유자 쿼드 트리
        /// </summary>
        public QuadTree<ItemType> OwnerTree;

        /// <summary>
        /// 자식 노드들
        /// </summary>
        public Node<ItemType>[] Children;

        /// <summary>
        /// 노드의 영역
        /// </summary>
        public Rectangle BoundingBox
        {
            get;
            private set;
        }

        /// <summary>
        /// 노드 생성자
        /// </summary>
        /// <param name="owner">소유 쿼드트리</param>
        /// <param name="parent">부모 노드</param>
        /// <param name="boundingBox">영역</param>
        public Node(QuadTree<ItemType> owner, Node<ItemType> parent, Rectangle boundingBox)
        {
            OwnerTree = owner;
            Parent = parent;
            BoundingBox = boundingBox;
            Children = null;
        }

        /// <summary>
        /// 각 등분에 해당하는 영역 반환
        /// </summary>
        /// <param name="segment">등분 종류</param>
        /// <returns>영역</returns>
        public Rectangle GetSegmentBoundingBox(Segments segment)
        {
            //int halfWidth = (int)(BoundingBox.Width * 0.5f);
            //int halfHeight = (int)(BoundingBox.Height * 0.5f);

            int halfWidth = (int)(BoundingBox.Width);
            int halfHeight = (int)(BoundingBox.Height);

            switch (segment)
            {
                case Segments.TopLeft:
                    return new Rectangle(BoundingBox.Left, BoundingBox.Top, halfWidth, halfHeight);

                case Segments.TopRight:
                    return new Rectangle(BoundingBox.Left + halfWidth, BoundingBox.Top, halfWidth, halfHeight);

                case Segments.BottomLeft:
                    return new Rectangle(BoundingBox.Left, BoundingBox.Top + halfHeight, halfWidth, halfHeight);

                case Segments.BottomRight:
                    return new Rectangle(BoundingBox.Left + halfWidth, BoundingBox.Top + halfHeight, halfWidth, halfHeight);
            }

            return new Rectangle();
        }

        /// <summary>
        /// 지정한 점이 속하는 등분 종류 반환
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>등분 종류</returns>
        public Segments GetSegmentFromPosition(int x, int y)
        {
            //int xCenter = (int)(BoundingBox.Left + BoundingBox.Width * 0.5f);
            //int yCenter = (int)(BoundingBox.Top + BoundingBox.Height * 0.5f);

            int xCenter = (int)(BoundingBox.Left + BoundingBox.Width);
            int yCenter = (int)(BoundingBox.Top + BoundingBox.Height);

            if (x < xCenter)
            {
                if (y < yCenter)
                    return Segments.TopLeft;
                else
                    return Segments.BottomLeft;
            }
            else
            {
                if (y < yCenter)
                    return Segments.TopRight;
                else
                    return Segments.BottomRight;
            }
        }

        /// <summary>
        /// 자식 노드 소유 여부
        /// </summary>
        public bool HasChildren
        {
            get
            {
                return Children != null;
            }
        }

        /// <summary>
        /// 자식 노드 생성
        /// </summary>
        public void CreateChildren()
        {
            Children = new Node<ItemType>[4]
            {
                new Node<ItemType>(this.OwnerTree, this, GetSegmentBoundingBox(Segments.TopLeft)),
                new Node<ItemType>(this.OwnerTree, this, GetSegmentBoundingBox(Segments.TopRight)),
                new Node<ItemType>(this.OwnerTree, this, GetSegmentBoundingBox(Segments.BottomLeft)),
                new Node<ItemType>(this.OwnerTree, this, GetSegmentBoundingBox(Segments.BottomRight))
            };
        }

        /// <summary>
        /// 등분에 해당하는 자식 노드 반환
        /// </summary>
        /// <param name="segment">등분 종류</param>
        /// <returns>자식 노드</returns>
        public Node<ItemType> GetChild(Segments segment)
        {
            //if (!HasChildren)
                //throw new EditorException(cl_gl_globalization.Instance.fngetglobalization_value("@G00424"));//"자식 노드가 존재하지 않습니다."

            return Children[(int)segment];
        }

        /// <summary>
        /// 자식 노드 삭제
        /// </summary>
        public void RemoveChildren()
        {
            Children = null;
        }
    }
}
