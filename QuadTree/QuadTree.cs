
using System.Collections.Generic;

using System.Drawing;



namespace QuadTree
{
    /// <summary>
    /// 위치를 갖는 아이템을 빠르게 검색하기 위한 쿼드트리 클래스
    /// </summary>
    /// <typeparam name="ItemType">아이템 타입</typeparam>
    public class QuadTree<ItemType> where ItemType : INodeItem<ItemType>
    {
        /// <summary>
        /// 쿼드트리의 깊이 제한 값. 노드의 깊이가 제한 미만이라면 노드는 단 하나의 아이템만 가질 수 있으며
        /// 아이템을 가지고 있는 노드에 또 다른 아이템을 추가하면 노드는 자식노드를 생성하여 보유하고
        /// 있는 아이템을 자식 노드에 분배한다.
        /// 깊이 제한에 다다른 노드는 더 이상 자식 노드를 생성하지 않으며 다수의 아이템을 보유할 수 있다.
        /// </summary>
        private int depthLimit;

        /// <summary>
        /// 깊이 제한 프로퍼티
        /// </summary>
        public int DepthLimit
        {
            get
            {
                return depthLimit;
            }

            set
            {
                if (Empty)
                {
                    // 쿼드트리에 아이템을 하나라도 넣었다면 깊이를 변경할 수 없다.
                    //throw new Exception(cl_gl_globalization.Instance.fngetglobalization_value("@G00654"));//"쿼드트리가 변경된 이후에는 DepthLimit를 설정할 수 없습니다."
                }

                depthLimit = value;
            }
        }

        /// <summary>
        /// 트리가 비어있는지 여부 반환. 루트가 자식을 갖고 있지 않고 보유한 아이템이 없어야 한다.
        /// </summary>
        public bool Empty
        {
            get
            {
                return Root.Items.Count > 0 || Root.HasChildren;
            }
        }

        /// <summary>
        /// 루트 노드
        /// </summary>
        public Node<ItemType> Root
        {
            get;
            private set;
        }

        /// <summary>
        /// 쿼드트리 생성자
        /// </summary>
        /// <param name="initialBoundingBox">루트 노드의 영역</param>
        public QuadTree(Rectangle initialBoundingBox)
        {
            Root = new Node<ItemType>(this, null, initialBoundingBox);
            // 깊이 제한을 8단계로 설정한다.
            DepthLimit = 8;
        }

        /// <summary>
        /// 쿼드트리에 아이템을 추가하는 실제 로직. 재귀호출한다.
        /// </summary>
        /// <param name="newItem">추가될 새 아이템</param>
        /// <param name="currentNode">추가될 노드</param>
        /// <param name="currentDepth">현재 깊이</param>
        /// <returns>추가된 아이템을 소유한 노드</returns>
        private Node<ItemType> Add(ItemType newItem, Node<ItemType> currentNode, int currentDepth)
        {
            // 현재 깊이가 제한에 해당하면 현재 노드에 아이템을 추가하고 종료한다.
            if (currentDepth >= DepthLimit)
            {
                newItem.OwnerNode = currentNode;
                currentNode.Items.Add(newItem);
                return currentNode;
            }

            // 현재 노드가 아이템을 소유 중이고 자식 노드가 없다면
            if (currentNode.Items.Count > 0 && !currentNode.HasChildren )
            {
                // 먼저 현재 노드에 달려있는 모든 아이템을 자식 노드로 전달한다.

                // 현재 노드에 자식 노드를 생성하고
                currentNode.CreateChildren();

                // 아이템 리스트를 순회하면서 각 자식 노드의 바운딩 박스에 들어가는 아이템을
                // 해당 자식 노드에 추가하도록 Add를 재귀호출한다.
                foreach (var currentItem in currentNode.Items)
                {
                    var segment = currentNode.GetSegmentFromPosition(currentItem.X, currentItem.Y);
                    var childNode = currentNode.GetChild(segment);
                    Add(currentItem, childNode, currentDepth + 1);
                }

                // 추가가 끝났으면 현재 노드의 아이템을 모두 삭제한다.(자식으로 모두 전달되었을 것이므로)
                currentNode.Items.Clear();

                // 새 아이템을 추가한다.
                var newItemSegment = currentNode.GetSegmentFromPosition(newItem.X, newItem.Y);
                var newItemChildNode = currentNode.GetChild(newItemSegment);
                return Add(newItem, newItemChildNode, currentDepth + 1);
            }
            // 현재 노드가 아이템을 소유하지 않고 자식 노드가 존재하면
            else if (currentNode.Items.Count == 0 && currentNode.HasChildren)
            {
                // 새 아이템의 위치에 해당하는 자식 노드를 가져와 추가 시도한다.
                var segment = currentNode.GetSegmentFromPosition(newItem.X, newItem.Y);
                var childNode = currentNode.GetChild(segment);
                return Add(newItem, childNode, currentDepth + 1);
            }
            // 현재 노드가 아이템을 소유하지 않는다면
            else
            {
                // 현재 노드에 아이템 추가한다.
                newItem.OwnerNode = currentNode;
                currentNode.Items.Add(newItem);
                return currentNode;
            }

            //return null;
        }

        /// <summary>
        /// 쿼드트리에 아이템 추가
        /// </summary>
        /// <param name="item">추가할 아이템</param>
        /// <returns>추가된 아이템을 소유한 노드</returns>
        public Node<ItemType> Add(ItemType item)
        {
            if (item.OwnerNode != null)
            {
                //throw new EditorException(cl_gl_globalization.Instance.fngetglobalization_value("@G00634"));//"추가하려는 항목이 이미 다른 노드에 소속돼 있습니다."
            }
                

            // 아이템의 위치가 루트 바운딩 박스 밖이면 추가 안 함. 예외발생이 좋을까?
            if (!Root.BoundingBox.Contains(item.X, item.Y))
                return null;

            // 아이템 추가
            return Add(item, Root, 0);

            // 최대 깊이에 해당하지 않는 노드는 단 하나의 아이템만 가질 수 있다.
            // 최대 깊이에 해당하는 노드는 소유할 수 있는 아이템 개수에 제한이 없다.
            // 아이템을 소유하고 있는 중간 깊이 노드에 새 아이템을 추가할 경우
            // 자식노드를 생성하여 새 아이템과 소유중인 아이템을 자식으로 전달한다.

            // 트리의 자식을 찾아 내려가면서 아이템을 넣을 노드를 선택한다.
            // 현재 노드에 아이템이 붙어있고 깊이 제한에 다다르지 않는다면 루프 수행.
            // 쿼드트리가 빈(Empty) 상태이면 이 루프를 타지 않는다.
        }

        /// <summary>
        /// 아이템을 트리에서 제거한다.
        /// </summary>
        /// <param name="item">제거할 아이템</param>
        public void Remove(ItemType item)
        {
            // 아이템의 소유 노드에서 아이템을 제거한다.
            item.OwnerNode.Items.Remove(item);
            // 소유 노드를 null로 설정한다.
            item.OwnerNode = null;
        }

        /// <summary>
        /// 모든 아이템을 삭제한다.
        /// </summary>
        public void RemoveAll()
        {
            Root.Items.Clear();
            Root.RemoveChildren();
        }

        /// <summary>
        /// 지정한 영역 내에 해당하는 아이템을 검색한다. 재귀호출함.
        /// </summary>
        /// <param name="currentNode">검색할 노드</param>
        /// <param name="rectangle">검색 영역</param>
        /// <param name="precisly">정확한 컬링을 수행할지 여부</param>
        /// <param name="items">검색된 아이템 리스트 반환 인자</param>
        private void FindItemsInRectangle(Node<ItemType> currentNode, Rectangle rectangle, bool precisly, ref List<ItemType> items)
        {
            if (currentNode.BoundingBox.IntersectsWith(rectangle))
            {
                // 정확한 컬링을 수행한다면 컬링된 노드에 있는 아이템이 컬링 박스 안에 들어가는지 체크한다.
                if (precisly)
                {
                    // 현재 노드가 보유하고 있는 아이템 리스트를 순회하면서
                    foreach (var item in currentNode.Items)
                    {
                        // 검색 영역 내에 아이템이 포함되면 검색된 리스트에 추가
                        if (item.HitTest(rectangle))
                            items.Add(item);
                    }
                }
                // 정확한 컬링을 수행하지 않는다면 컬링된 노드에 있는 아이템을 그냥 결과 리스트에 넣는다.
                else
                {
                    items.AddRange(currentNode.Items);
                }

                // 현재 노드가 자식 노드를 갖고 있으면
                if (currentNode.HasChildren)
                {
                    // 자식 노드를 순회하면서 재귀호출
                    for (int i = 0; i < 4; ++i)
                    {
                        FindItemsInRectangle(currentNode.GetChild((Node<ItemType>.Segments)i), rectangle, precisly, ref items);
                    }
                }
            }
        }

        /// <summary>
        /// 지정한 영역 내에 해당하는 아이템을 검색한다.
        /// </summary>
        /// <param name="rectangle">검색 영역</param>
        /// <param name="precisly">정확한 컬링을 수행할지 여부</param>
        /// <param name="items">검색된 아이템 반환 인자</param>
        public void FindItemsInRectangle(Rectangle rectangle, bool precisly, ref List<ItemType> items)
        {
            FindItemsInRectangle(Root, rectangle, precisly, ref items);
        }

        /// <summary>
        /// 지정한 좌표와 충돌하는 하나의 아이템 검색
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pickedItem"></param>
        /// <returns>검색 성공 여부</returns>
        private bool PickItem(Node<ItemType> currentNode, int x, int y, out ItemType pickedItem)
        {
            foreach (var item in currentNode.Items)
            {
                if (item.HitTest(x, y))
                {
                    pickedItem = item;
                    return true;
                }
            }

            if (currentNode.HasChildren)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (PickItem(currentNode.GetChild((Node<ItemType>.Segments)i), x, y, out pickedItem))
                    {
                        return true;
                    }
                }
            }

            pickedItem = default(ItemType);
            return false;
        }

        /// <summary>
        /// 지정한 좌표의 아이템을 검색한다.
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <param name="pickedItem">검색된 아이템</param>
        /// <returns>검색 성공 여부</returns>
        public bool PickItem(int x, int y, out ItemType pickedItem)
        {
            return PickItem(Root, x, y, out pickedItem);
        }

        /// <summary>
        /// 디버그용 쿼드트리 가시화 메서드
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="graphics"></param>
        /// <param name="pen"></param>
        private void Draw(Node<ItemType> currentNode, Graphics graphics, Pen pen)
        {
            graphics.DrawRectangle(pen, currentNode.BoundingBox);

            foreach (var item in currentNode.Items)
            {
                Rectangle rc = new Rectangle(item.X - 1, item.Y - 1, 3, 3);
                graphics.DrawRectangle(pen, rc);
            }

            if (currentNode.HasChildren)
            {
                for (int i = 0; i < 4; ++i)
                {
                    Draw(currentNode.GetChild((Node<ItemType>.Segments)i), graphics, pen);
                }
            }
        }

        /// <summary>
        /// 디버그용 쿼드트리 가시화 
        /// </summary>
        /// <param name="graphics">그려질 그래픽 객체</param>
        private void Draw(Graphics graphics)
        {
            Pen pen = new Pen(Color.BlueViolet, 1);

            Draw(Root, graphics, pen);

            pen.Dispose();
        }
    }
}
