///<summary>
///A red-black tree must satisfy these properties:
///
///1. The root is black. 
///2. All leaves are black. 
///3. Red nodes can only have black children. 
///4. All paths from a node to its leaves contain the same number of black nodes.
///</summary>

using System.Collections;
using System.Text;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KetamaHash;

namespace RedBlackCS
{
    public class RedBlack<TKey, TValue> where TKey : IComparable
    {
        // the number of nodes contained in the tree
        private int intCount;
        // a simple randomized hash code. The hash code could be used as a key
        // if it is "unique" enough. Note: The IComparable interface would need to 
        // be replaced with int.
        private int intHashCode;
        // identifies the owner of the tree
        private string strIdentifier;
        // the tree
        private RedBlackNode<TKey, TValue> rbTree;
        //  sentinelNode is convenient way of indicating a leaf node.
        public static RedBlackNode<TKey, TValue> sentinelNode;
        // the node that was last found; used to optimize searches
        private RedBlackNode<TKey, TValue> lastNodeFound;
        private Random rand = new Random();
        private SortedList<TKey, TValue> sortList = null;//�����������
        private TKey minKey;
        private TKey maxKey;
        private RedBlackNode<TKey, TValue> minNode = null;
        private RedBlackNode<TKey, TValue> maxNode = null;

        public  double sumTime = 0;
        /// <summary>
        /// ���Key
        /// </summary>
        public TKey MaxKey { get { return maxKey; } }

        /// <summary>
        /// ��СKey;
        /// </summary>
        public TKey MinKey { get { return minKey; } }


        public RedBlack()
        {
            strIdentifier = base.ToString() + rand.Next();
            intHashCode = rand.Next();

            // set up the sentinel node. the sentinel node is the key to a successfull
            // implementation and for understanding the red-black tree properties.
            sentinelNode = new RedBlackNode<TKey, TValue>();
            sentinelNode.Left = null;
            sentinelNode.Right = null;
            sentinelNode.Parent = null;
            sentinelNode.Color = RedBlackNode<TKey, TValue>.BLACK;
            rbTree = sentinelNode;
            lastNodeFound = sentinelNode;
        }

        public RedBlack(string strIdentifier)
        {
            intHashCode = rand.Next();
            this.strIdentifier = strIdentifier;
        }

        ///<summary>
        /// Add
        /// args: ByVal key As IComparable, ByVal data As Object
        /// key is object that implements IComparable interface
        /// performance tip: change to use use int type (such as the hashcode)
        ///</summary>
        public void Add(TKey key, TValue data)
        {
            if (key == null || data == null)
                throw (new RedBlackException("RedBlackNode<TKey,TValue> key and data must not be null") { Error = ReadBlackCode.DataNull });

            // traverse tree - find where node belongs
            int result = 0;
            // create new node
            RedBlackNode<TKey, TValue> node = new RedBlackNode<TKey, TValue>();
            RedBlackNode<TKey, TValue> temp = rbTree;               // grab the rbTree node of the tree

            while (temp != sentinelNode)
            {   // find Parent
                node.Parent = temp;
                result = key.CompareTo(temp.Key);
                if (result == 0)
                    throw (new RedBlackException("A Node with the same key already exists") { Error = ReadBlackCode.KeyExists });
                if (result > 0)
                    temp = temp.Right;
                else
                    temp = temp.Left;
            }

            // setup node
            node.Key = key;
            node.Value = data;
            node.Left = sentinelNode;
            node.Right = sentinelNode;

            // insert node into tree starting at parent's location
            if (node.Parent != null)
            {
                result = node.Key.CompareTo(node.Parent.Key);
                if (result > 0)
                    node.Parent.Right = node;
                else
                    node.Parent.Left = node;
            }
            else
            {
                rbTree = node;                  // first node added
                maxKey = minKey = key;
                minNode = new RedBlackNode<TKey, TValue>() { Key = minKey, Value = data };
                maxNode = new RedBlackNode<TKey, TValue>() { Key = maxKey, Value = data };
            }

            RestoreAfterInsert(node);           // restore red-black properities

            lastNodeFound = node;

            intCount = intCount + 1;
            if (minKey.CompareTo(key) > 0)
            {
                minKey = key;
                minNode.Key = key;
                minNode.Value = data;
            }
            if (maxKey.CompareTo(key) < 0)
            {
                maxKey = key;
                maxNode.Key = key;
                maxNode.Value = data;
            }
        }


        ///<summary>
        /// RestoreAfterInsert
        /// Additions to red-black trees usually destroy the red-black 
        /// properties. Examine the tree and restore. Rotations are normally 
        /// required to restore it
        ///</summary>
        private void RestoreAfterInsert(RedBlackNode<TKey, TValue> x)
        {
            // x and y are used as variable names for brevity, in a more formal
            // implementation, you should probably change the names

            RedBlackNode<TKey, TValue> y;

            // maintain red-black tree properties after adding x
            while (x != rbTree && x.Parent.Color == RedBlackNode<TKey, TValue>.RED)
            {
                // Parent node is .Colored red; 
                if (x.Parent == x.Parent.Parent.Left)   // determine traversal path			
                {                                       // is it on the Left or Right subtree?
                    y = x.Parent.Parent.Right;          // get uncle
                    if (y != null && y.Color == RedBlackNode<TKey, TValue>.RED)
                    {   // uncle is red; change x's Parent and uncle to black
                        x.Parent.Color = RedBlackNode<TKey, TValue>.BLACK;
                        y.Color = RedBlackNode<TKey, TValue>.BLACK;
                        // grandparent must be red. Why? Every red node that is not 
                        // a leaf has only black children 
                        x.Parent.Parent.Color = RedBlackNode<TKey, TValue>.RED;
                        x = x.Parent.Parent;    // continue loop with grandparent
                    }
                    else
                    {
                        // uncle is black; determine if x is greater than Parent
                        if (x == x.Parent.Right)
                        {   // yes, x is greater than Parent; rotate Left
                            // make x a Left child
                            x = x.Parent;
                            RotateLeft(x);
                        }
                        // no, x is less than Parent
                        x.Parent.Color = RedBlackNode<TKey, TValue>.BLACK;  // make Parent black
                        x.Parent.Parent.Color = RedBlackNode<TKey, TValue>.RED;     // make grandparent black
                        RotateRight(x.Parent.Parent);                   // rotate right
                    }
                }
                else
                {   // x's Parent is on the Right subtree
                    // this code is the same as above with "Left" and "Right" swapped
                    y = x.Parent.Parent.Left;
                    if (y != null && y.Color == RedBlackNode<TKey, TValue>.RED)
                    {
                        x.Parent.Color = RedBlackNode<TKey, TValue>.BLACK;
                        y.Color = RedBlackNode<TKey, TValue>.BLACK;
                        x.Parent.Parent.Color = RedBlackNode<TKey, TValue>.RED;
                        x = x.Parent.Parent;
                    }
                    else
                    {
                        if (x == x.Parent.Left)
                        {
                            x = x.Parent;
                            RotateRight(x);
                        }
                        x.Parent.Color = RedBlackNode<TKey, TValue>.BLACK;
                        x.Parent.Parent.Color = RedBlackNode<TKey, TValue>.RED;
                        RotateLeft(x.Parent.Parent);
                    }
                }
            }
            rbTree.Color = RedBlackNode<TKey, TValue>.BLACK;        // rbTree should always be black
        }


        /// <summary>
        /// ����Key��С�Ľڵ�
        /// </summary>
        /// <returns></returns>
        public RedBlackNode<TKey, TValue> First()
        {
            RedBlackNode<TKey, TValue> node = new RedBlackNode<TKey, TValue>() { Key = minNode.Key, Value = minNode.Value };
            return node;
        }

        ///<summary>
        /// RotateLeft
        /// Rebalance the tree by rotating the nodes to the left
        ///</summary>
        public void RotateLeft(RedBlackNode<TKey, TValue> x)
        {
            // pushing node x down and to the Left to balance the tree. x's Right child (y)
            // replaces x (since y > x), and y's Left child becomes x's Right child 
            // (since it's < y but > x).

            RedBlackNode<TKey, TValue> y = x.Right;         // get x's Right node, this becomes y

            // set x's Right link
            x.Right = y.Left;                   // y's Left child's becomes x's Right child

            // modify parents
            if (y.Left != sentinelNode)
                y.Left.Parent = x;				// sets y's Left Parent to x

            if (y != sentinelNode)
                y.Parent = x.Parent;            // set y's Parent to x's Parent

            if (x.Parent != null)
            {   // determine which side of it's Parent x was on
                if (x == x.Parent.Left)
                    x.Parent.Left = y;          // set Left Parent to y
                else
                    x.Parent.Right = y;         // set Right Parent to y
            }
            else
                rbTree = y;                     // at rbTree, set it to y

            // link x and y 
            y.Left = x;                         // put x on y's Left 
            if (x != sentinelNode)                      // set y as x's Parent
                x.Parent = y;
        }
        ///<summary>
        /// RotateRight
        /// Rebalance the tree by rotating the nodes to the right
        ///</summary>
        public void RotateRight(RedBlackNode<TKey, TValue> x)
        {
            // pushing node x down and to the Right to balance the tree. x's Left child (y)
            // replaces x (since x < y), and y's Right child becomes x's Left child 
            // (since it's < x but > y).

            RedBlackNode<TKey, TValue> y = x.Left;          // get x's Left node, this becomes y

            // set x's Right link
            x.Left = y.Right;                   // y's Right child becomes x's Left child

            // modify parents
            if (y.Right != sentinelNode)
                y.Right.Parent = x;				// sets y's Right Parent to x

            if (y != sentinelNode)
                y.Parent = x.Parent;            // set y's Parent to x's Parent

            if (x.Parent != null)               // null=rbTree, could also have used rbTree
            {   // determine which side of it's Parent x was on
                if (x == x.Parent.Right)
                    x.Parent.Right = y;         // set Right Parent to y
                else
                    x.Parent.Left = y;          // set Left Parent to y
            }
            else
                rbTree = y;                     // at rbTree, set it to y

            // link x and y 
            y.Right = x;                        // put x on y's Right
            if (x != sentinelNode)              // set y as x's Parent
                x.Parent = y;
        }
        ///<summary>
        /// GetData
        /// Gets the data object associated with the specified key
        ///<summary>
        public TValue GetData(TKey key)
        {
            if (sortList != null)
            {
                TValue value = default(TValue);
                if (!sortList.TryGetValue(key, out value))
                {
                    throw (new RedBlackException("RedBlackNode<TKey,TValue> key was not found") { Error = ReadBlackCode.KeyNotExists });
                }
                return value;
            }
            int result;

            RedBlackNode<TKey, TValue> treeNode = rbTree;     // begin at root
            // traverse tree until node is found
            while (treeNode != sentinelNode)
            {
                result = key.CompareTo(treeNode.Key);
                if (result == 0)
                {
                    lastNodeFound = treeNode;
                    return treeNode.Value;
                }
                if (result < 0)
                    treeNode = treeNode.Left;
                else
                    treeNode = treeNode.Right;
            }
            throw (new RedBlackException("RedBlackNode<TKey,TValue> key was not found") { Error = ReadBlackCode.KeyNotExists });
        }


        ///<summary>
        /// GetMinKey
        /// Returns the minimum key value
        ///<summary>
        public TKey GetMinKey()
        {
            RedBlackNode<TKey, TValue> treeNode = rbTree;

            if (treeNode == null || treeNode == sentinelNode)
                throw (new RedBlackException("RedBlack tree is empty") { Error = ReadBlackCode.Empty });

            // traverse to the extreme left to find the smallest key
            while (treeNode.Left != sentinelNode)
                treeNode = treeNode.Left;

            lastNodeFound = treeNode;

            return treeNode.Key;

        }
        ///<summary>
        /// GetMaxKey
        /// Returns the maximum key value
        ///<summary>
        public TKey GetMaxKey()
        {
            RedBlackNode<TKey, TValue> treeNode = rbTree;

            if (treeNode == null || treeNode == sentinelNode)
                throw (new RedBlackException("RedBlack tree is empty") { Error = ReadBlackCode.Empty });

            // traverse to the extreme right to find the largest key
            while (treeNode.Right != sentinelNode)
                treeNode = treeNode.Right;

            lastNodeFound = treeNode;

            return treeNode.Key;

        }
        ///<summary>
        /// GetMinValue
        /// Returns the object having the minimum key value
        ///<summary>
        public TValue GetMinValue()
        {
            return GetData(GetMinKey());
        }
        ///<summary>
        /// GetMaxValue
        /// Returns the object having the maximum key
        ///<summary>
        public TValue GetMaxValue()
        {
            return GetData(GetMaxKey());
        }
        ///<summary>
        /// GetEnumerator
        /// return an enumerator that returns the tree nodes in order
        ///<summary>
        public RedBlackEnumerator<TKey, TValue> GetEnumerator()
        {
            // elements is simply a generic name to refer to the 
            // data objects the nodes contain
            return Elements(true);
        }
        ///<summary>
        /// Keys
        /// if(ascending is true, the keys will be returned in ascending order, else
        /// the keys will be returned in descending order.
        ///<summary>
        public RedBlackEnumerator<TKey, TValue> Keys()
        {
            return Keys(true);
        }
        public RedBlackEnumerator<TKey, TValue> Keys(bool ascending)
        {
            return new RedBlackEnumerator<TKey, TValue>(rbTree, true, ascending);
        }
        ///<summary>
        /// Values
        /// Provided for .NET compatibility. 
        ///<summary>
        public RedBlackEnumerator<TKey, TValue> Values()
        {
            return Elements(true);
        }
        ///<summary>
        /// Elements
        /// Returns an enumeration of the data objects.
        /// if(ascending is true, the objects will be returned in ascending order,
        /// else the objects will be returned in descending order.
        ///<summary>
        public RedBlackEnumerator<TKey, TValue> Elements()
        {
            return Elements(true);
        }
        public RedBlackEnumerator<TKey, TValue> Elements(bool ascending)
        {
            return new RedBlackEnumerator<TKey, TValue>(rbTree, false, ascending);
        }
        ///<summary>
        /// IsEmpty
        /// Is the tree empty?
        ///<summary>
        public bool IsEmpty()
        {
            return (rbTree == null);
        }
        ///<summary>
        /// Remove
        /// removes the key and data object (delete)
        ///<summary>
        public void Remove(TKey key)
        {
            if (key == null)
                throw (new RedBlackException("RedBlackNode<TKey,TValue> key is null") { Error = ReadBlackCode.KeyNull });

            // find node
            int result;
            RedBlackNode<TKey, TValue> node;

            // see if node to be deleted was the last one found
            result = key.CompareTo(lastNodeFound.Key);
            if (result == 0)
                node = lastNodeFound;
            else
            {   // not found, must search		
                node = rbTree;
                while (node != sentinelNode)
                {
                    result = key.CompareTo(node.Key);
                    if (result == 0)
                        break;
                    if (result < 0)
                        node = node.Left;
                    else
                        node = node.Right;
                }

                if (node == sentinelNode)
                    return;             // key not found
            }

            Delete(node);

            intCount = intCount - 1;
        }
        ///<summary>
        /// Delete
        /// Delete a node from the tree and restore red black properties
        ///<summary>
        private void Delete(RedBlackNode<TKey, TValue> z)
        {
            // A node to be deleted will be: 
            //		1. a leaf with no children
            //		2. have one child
            //		3. have two children
            // If the deleted node is red, the red black properties still hold.
            // If the deleted node is black, the tree needs rebalancing

            RedBlackNode<TKey, TValue> x = new RedBlackNode<TKey, TValue>();    // work node to contain the replacement node
            RedBlackNode<TKey, TValue> y;                   // work node 

            // find the replacement node (the successor to x) - the node one with 
            // at *most* one child. 
            if (z.Left == sentinelNode || z.Right == sentinelNode)
                y = z;                      // node has sentinel as a child
            else
            {
                // z has two children, find replacement node which will 
                // be the leftmost node greater than z
                y = z.Right;                        // traverse right subtree	
                while (y.Left != sentinelNode)      // to find next node in sequence
                    y = y.Left;
            }

            // at this point, y contains the replacement node. it's content will be copied 
            // to the valules in the node to be deleted

            // x (y's only child) is the node that will be linked to y's old parent. 
            if (y.Left != sentinelNode)
                x = y.Left;
            else
                x = y.Right;

            // replace x's parent with y's parent and
            // link x to proper subtree in parent
            // this removes y from the chain
            x.Parent = y.Parent;
            if (y.Parent != null)
                if (y == y.Parent.Left)
                    y.Parent.Left = x;
                else
                    y.Parent.Right = x;
            else
                rbTree = x;         // make x the root node

            // copy the values from y (the replacement node) to the node being deleted.
            // note: this effectively deletes the node. 
            if (y != z)
            {
                z.Key = y.Key;
                z.Value = y.Value;
            }

            if (y.Color == RedBlackNode<TKey, TValue>.BLACK)
                RestoreAfterDelete(x);

            lastNodeFound = sentinelNode;
        }

        ///<summary>
        /// RestoreAfterDelete
        /// Deletions from red-black trees may destroy the red-black 
        /// properties. Examine the tree and restore. Rotations are normally 
        /// required to restore it
        ///</summary>
		private void RestoreAfterDelete(RedBlackNode<TKey, TValue> x)
        {
            // maintain Red-Black tree balance after deleting node 			

            RedBlackNode<TKey, TValue> y;

            while (x != rbTree && x.Color == RedBlackNode<TKey, TValue>.BLACK)
            {
                if (x == x.Parent.Left)         // determine sub tree from parent
                {
                    y = x.Parent.Right;         // y is x's sibling 
                    if (y.Color == RedBlackNode<TKey, TValue>.RED)
                    {   // x is black, y is red - make both black and rotate
                        y.Color = RedBlackNode<TKey, TValue>.BLACK;
                        x.Parent.Color = RedBlackNode<TKey, TValue>.RED;
                        RotateLeft(x.Parent);
                        y = x.Parent.Right;
                    }
                    if (y.Left.Color == RedBlackNode<TKey, TValue>.BLACK &&
                        y.Right.Color == RedBlackNode<TKey, TValue>.BLACK)
                    {   // children are both black
                        y.Color = RedBlackNode<TKey, TValue>.RED;       // change parent to red
                        x = x.Parent;                   // move up the tree
                    }
                    else
                    {
                        if (y.Right.Color == RedBlackNode<TKey, TValue>.BLACK)
                        {
                            y.Left.Color = RedBlackNode<TKey, TValue>.BLACK;
                            y.Color = RedBlackNode<TKey, TValue>.RED;
                            RotateRight(y);
                            y = x.Parent.Right;
                        }
                        y.Color = x.Parent.Color;
                        x.Parent.Color = RedBlackNode<TKey, TValue>.BLACK;
                        y.Right.Color = RedBlackNode<TKey, TValue>.BLACK;
                        RotateLeft(x.Parent);
                        x = rbTree;
                    }
                }
                else
                {   // right subtree - same as code above with right and left swapped
                    y = x.Parent.Left;
                    if (y.Color == RedBlackNode<TKey, TValue>.RED)
                    {
                        y.Color = RedBlackNode<TKey, TValue>.BLACK;
                        x.Parent.Color = RedBlackNode<TKey, TValue>.RED;
                        RotateRight(x.Parent);
                        y = x.Parent.Left;
                    }
                    if (y.Right.Color == RedBlackNode<TKey, TValue>.BLACK &&
                        y.Left.Color == RedBlackNode<TKey, TValue>.BLACK)
                    {
                        y.Color = RedBlackNode<TKey, TValue>.RED;
                        x = x.Parent;
                    }
                    else
                    {
                        if (y.Left.Color == RedBlackNode<TKey, TValue>.BLACK)
                        {
                            y.Right.Color = RedBlackNode<TKey, TValue>.BLACK;
                            y.Color = RedBlackNode<TKey, TValue>.RED;
                            RotateLeft(y);
                            y = x.Parent.Left;
                        }
                        y.Color = x.Parent.Color;
                        x.Parent.Color = RedBlackNode<TKey, TValue>.BLACK;
                        y.Left.Color = RedBlackNode<TKey, TValue>.BLACK;
                        RotateRight(x.Parent);
                        x = rbTree;
                    }
                }
            }
            x.Color = RedBlackNode<TKey, TValue>.BLACK;
        }

        ///<summary>
        /// RemoveMin
        /// removes the node with the minimum key
        ///<summary>
        public void RemoveMin()
        {
            if (sortList != null && sortList.Count > 0)
            {

                sortList.RemoveAt(0);
                minKey = sortList.Keys[0];
                intCount--;
                return;
            }
            if (rbTree == null)
                throw (new RedBlackException("RedBlackNode<TKey,TValue> is null") { Error = ReadBlackCode.TreeNull });

            Remove(GetMinKey());
        }
        ///<summary>
        /// RemoveMax
        /// removes the node with the maximum key
        ///<summary>
        public void RemoveMax()
        {
            if (sortList != null && sortList.Count > 0)
            {

                sortList.RemoveAt(sortList.Count - 1);
                maxKey = sortList.Keys[sortList.Count - 1];
                intCount--;
                return;
            }
            if (rbTree == null)
                throw (new RedBlackException("RedBlackNode<TKey,TValue> is null") { Error = ReadBlackCode.TreeNull });

            Remove(GetMaxKey());
        }
        ///<summary>
        /// Clear
        /// Empties or clears the tree
        ///<summary>
        public void Clear()
        {
            rbTree = sentinelNode;
            intCount = 0;
            sortList = null;
        }
        ///<summary>
        /// Size
        /// returns the size (number of nodes) in the tree
        ///<summary>
        public int Size()
        {
            // number of keys
            return intCount;
        }
        ///<summary>
        /// Equals
        ///<summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is RedBlackNode<TKey, TValue>))
                return false;

            if (this == obj)
                return true;

            return (ToString().Equals(((RedBlackNode<TKey, TValue>)(obj)).ToString()));

        }
        ///<summary>
        /// HashCode
        ///<summary>
        public override int GetHashCode()
        {
            return intHashCode;
        }
        ///<summary>
        /// ToString
        ///<summary>
        public override string ToString()
        {
            return strIdentifier.ToString();
        }


        #region ���Hash

        /// <summary>
        /// ��������,�����б��ܣ��������Ч
        /// ר��ΪHashһ�·�����ӵ�
        /// </summary>
        public void UpdateSort()
        {
            //����֧·��һ���̲߳���;ֻ�ܲ���
            var taskLeft = Task.Factory.StartNew(() =>
            {
                LinkedList<RedBlackNode<TKey, TValue>> lstKeys = new LinkedList<RedBlackNode<TKey, TValue>>();
                PreOrderNode(rbTree.Left, lstKeys);
                return lstKeys;
            });
            var taskRight = Task.Factory.StartNew(() =>
            {
                LinkedList<RedBlackNode<TKey, TValue>> lstKeys = new LinkedList<RedBlackNode<TKey, TValue>>();
                PreOrderNode(rbTree.Right, lstKeys);
                return lstKeys;
            });
            sortList = new SortedList<TKey, TValue>(intCount);
            sortList.Add(rbTree.Key, rbTree.Value);
            LinkedList<RedBlackNode<TKey, TValue>> lst = taskLeft.Result;
            foreach (RedBlackNode<TKey, TValue> node in lst)
            {
                sortList.Add(node.Key, node.Value);
            }
            lst.Clear();
            lst = taskRight.Result;
            foreach (RedBlackNode<TKey, TValue> node in lst)
            {
                sortList.Add(node.Key, node.Value);
            }
            lst.Clear();
            //
            rbTree = null;//���
                        

        }


        /// <summary>
        /// ����������
        /// </summary>
        public void ResetTree()
        {
            if (rbTree == null && sortList != null)
            {
                foreach (var item in sortList)
                {
                    Add(item.Key, item.Value);
                }
            }
        }

        public RedBlackNode<TKey, TValue> TailNode(TKey key)
        {
            if(sortList==null||sortList.Count==0)
            {
                return null;
            }
            //
            TKey first = sortList.Keys[0];
            TKey last = sortList.Keys[sortList.Count - 1];
            if(first.CompareTo(key)>=0)
            {
                RedBlackNode<TKey, TValue>  tmp = new RedBlackNode<TKey, TValue>();
                tmp.Key = first;
                tmp.Value = sortList[first];
                return tmp;
            }
            else if(last.CompareTo(key)==0)
            {
                RedBlackNode<TKey, TValue> tmp = new RedBlackNode<TKey, TValue>();
                tmp.Key = last;
                tmp.Value = sortList[last];
                return tmp;
            }
            else if(last.CompareTo(key) < 0)
            {
                RedBlackNode<TKey, TValue> tmp = new RedBlackNode<TKey, TValue>();
                tmp.Key = first;
                tmp.Value = sortList[first];
                return tmp;
            }
           //return TailNodeSplice(key);//����֤�����е���������Ƭ�ٶȺܵ�
             return TailNodeBlock(0, sortList.Count, key);
        }

        /// <summary>
        /// ��Ƭ
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private RedBlackNode<TKey, TValue> TailNodeSplice(TKey key)
        {
           // DateTime start = DateTime.Now;
            int num = Environment.ProcessorCount;
            if (num > 2)
            {
                num = num / 2;
            }
            Task<RedBlackNode<TKey, TValue>>[] taskNum = new Task<RedBlackNode<TKey, TValue>>[num];
            int cpuTaskNum = sortList.Count / num;
            int left = sortList.Count % num;

            for (int i = 0; i < num; i++)
            {
                var task = Task.Factory.StartNew((k) =>
                {
                    int j = (int)k;
                    int curNum = 0;
                    if (j == num - 1)
                    {
                        //���һ��
                        curNum = (j + 1) * cpuTaskNum + left;
                    }
                    else
                    {
                        curNum = (j + 1) * cpuTaskNum;
                    }
                    // Console.WriteLine(j + "," + curNum);
                    return TailNodeBlock(j * cpuTaskNum, curNum, key);
                }, i);
                taskNum[i] = task;
            }
            Task.WaitAll(taskNum);
            //Console.WriteLine(DateTime.Now - start);
           // sumTime += (DateTime.Now - start).TotalMilliseconds;
            SortedList<TKey, TValue> result = new SortedList<TKey, TValue>(num);
            foreach (var item in taskNum)
            {
                if (item.Result != null)
                {

                    result[item.Result.Key] = item.Result.Value;
                }
            }
            //
            if (result.Count > 0)
            {
                RedBlackNode<TKey, TValue> node = new RedBlackNode<TKey, TValue>();
                node.Key = result.Keys[0];
                node.Value = result[node.Key];
                return node;
            }
            return null;
        }

        private RedBlackNode<TKey, TValue> TailNodeBlock(int low, int high,TKey key)
        {
            //���ַ�����
            var lst = sortList;
            TKey findKey = default(TKey);
            bool isFind = false;
            int middle = 0;
            TKey first = sortList.Keys[low];
            TKey last = sortList.Keys[high-1];
            if (first.CompareTo(key) >= 0)
            {
                RedBlackNode<TKey, TValue> tmp = new RedBlackNode<TKey, TValue>();
                tmp.Key = first;
                tmp.Value = sortList[first];
                return tmp;
            }
            else if (last.CompareTo(key) == 0)
            {
                RedBlackNode<TKey, TValue> tmp = new RedBlackNode<TKey, TValue>();
                tmp.Key = last;
                tmp.Value = sortList[last];
                return tmp;
            }
            else if (last.CompareTo(key) < 0)
            {
                //RedBlackNode<TKey, TValue> tmp = new RedBlackNode<TKey, TValue>();
                //tmp.Key = first;
                //tmp.Value = sortList[first];
                //return tmp;
                return null;
            }
            while (low <= high)
            {
                middle = (low + high) / 2;
                if (middle == lst.Count)
                {
                    if (key.CompareTo(lst.Keys[middle - 1]) > 0)
                    {
                        return null;
                    }
                    else
                    {
                        findKey = lst.Keys[middle - 1];
                        break;
                    }
                }
                int result = key.CompareTo(lst.Keys[middle]);
                if (result == 0)
                {
                    findKey = key;
                    isFind = true;
                    break;
                }
                else if (result > 0)
                {
                    //���ڵ�ǰֵ
                    low = middle + 1;
                    if (middle < lst.Keys.Count - 1 && key.CompareTo(lst.Keys[middle + 1]) <= 0)
                    {
                        findKey = lst.Keys[middle + 1];
                        isFind = true;
                        break;
                    }
                }
                else if (result < 0)
                {
                    high = middle - 1;
                    if (middle > 0 && key.CompareTo(lst.Keys[middle - 1]) >= 0)
                    {
                        findKey = lst.Keys[middle];
                        isFind = true;
                        break;
                    }
                }
            }
            //
            if (!isFind)
            {
                if(middle>=lst.Count)
                {
                    middle = lst.Count - 1;
                }
                int r = key.CompareTo(lst.Keys[middle]);
                if (r > 0)
                {
                    for (int i = middle; i < high; i++)
                    {
                        if (key.CompareTo(lst.Keys[i]) <= 0)
                        {
                            findKey = lst.Keys[i];
                            isFind = true;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = middle; i > 0; i--)
                    {
                        if (key.CompareTo(lst.Keys[i]) >= 0)
                        {
                            findKey = lst.Keys[i + 1];
                            isFind = true;
                            break;
                        }
                    }
                }
            }
            //
          
            if (isFind)
            {
                //
                RedBlackNode<TKey, TValue> node = new RedBlackNode<TKey, TValue>() { Key = findKey };
                TValue value = default(TValue);
                lst.TryGetValue(findKey, out value);
                node.Value = value;
                return node;
            }
            return null;
        }

        public SortedList<TKey, TValue> TailListMap(TKey key)
        {
            if(sortList==null)
            {
                return null;
            }
            var lst = sortList;
            SortedList<TKey, TValue> lstR = new SortedList<TKey, TValue>();
            var map = lst.AsParallel().Where((k) =>
            {
                int result = k.Key.CompareTo(key);
                if (result >= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });
            var r = map.ToList();
            lstR = new SortedList<TKey, TValue>(r.Count);
            foreach (var item in r)
            {
                lstR.Add(item.Key, item.Value);
            }
            return lstR;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedBlack<TKey, TValue> TailMap(TKey key)
        {
            RedBlack<TKey, TValue> tree = new RedBlack<TKey, TValue>(this.strIdentifier);
            if (sortList!=null)
            {
                foreach(var item in sortList)
                {
                    tree.Add(item.Key, item.Value);
                }
                return tree;
            }
           
            RedBlackEnumerator<TKey, TValue> enumerator = Keys(true);
            while (enumerator.MoveNext())
            {
                //����
                if (enumerator.Key.CompareTo(key) < 0)
                {
                    continue;
                }
                tree.Add(enumerator.Key, enumerator.Value);
            }
            return tree;
        }

        /// <summary>
        /// ����С�ڵ��ڵĽڵ�
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedBlackNode<TKey, TValue> HeadNode(TKey key)
        {
            int result;
            RedBlackNode<TKey, TValue> treeNode = rbTree;     // begin at root
            // traverse tree until node is found
            while (treeNode != sentinelNode)
            {
                result = key.CompareTo(treeNode.Key);
                if (result == 0)
                {
                    lastNodeFound = treeNode;
                    return treeNode;
                }
                if (result < 0)
                {
                    treeNode = treeNode.Left;
                }
                else
                {
                    return treeNode;
                }
            }
            return null;
        }


        /// <summary>
        /// ���ҷ��ϵ�Keys
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<TKey> FindKeys(TValue value)
        {
            List<TKey> lst = new List<TKey>(100);
            if (rbTree == null)
            {
                return lst;
            }
            //����֧·��һ���̲߳���;ֻ�ܲ���
            var taskLeft = Task.Factory.StartNew(() =>
            {
                LinkedList<TKey> lstKeys = new LinkedList<TKey>();
                PreOrderNode(rbTree.Left, lstKeys, value);
                return lstKeys;
            });
            var taskRight = Task.Factory.StartNew(() =>
            {
                LinkedList<TKey> lstKeys = new LinkedList<TKey>();
                PreOrderNode(rbTree.Left, lstKeys, value);
                return lstKeys;
            });
            //
            lst.AddRange(taskLeft.Result);
            taskLeft.Result.Clear();
            lst.AddRange(taskRight.Result);
            taskRight.Result.Clear();
            return lst;
        }



        /// <summary>
        /// ��ȡ�ض�Key
        /// </summary>
        /// <param name="node">��ʼ�ڵ�</param>
        /// <param name="lstValue">����ڵ�</param>
        public void PreOrderNode(RedBlackNode<TKey, TValue> node, LinkedList<TKey> lstKey, TValue value)
        {
            if (node == null)
                return;

            Stack<RedBlackNode<TKey, TValue>> stack = new Stack<RedBlackNode<TKey, TValue>>();
            while (node != null || stack.Any())
            {
                //�������
                if (node != null)
                {
                    stack.Push(node);
                    if (value.Equals(node.Value))
                    {
                        lstKey.AddLast(node.Key);
                    }
                    node = node.Left;

                }
                else
                {
                    var item = stack.Pop();
                    node = item.Right;
                }
            }
        }


        /// <summary>
        /// ������ȡ���нڵ�
        /// </summary>
        /// <param name="node">��ʼ�ڵ�</param>
        /// <param name="lstValue">����ڵ�</param>
        public void PreOrderNode(RedBlackNode<TKey, TValue> node, LinkedList<RedBlackNode<TKey, TValue>> lstValue)
        {
            if (node == null)
                return;

            Stack<RedBlackNode<TKey, TValue>> stack = new Stack<RedBlackNode<TKey, TValue>>();
            while (node != null || stack.Any())
            {
                //�������
                if (node != null)
                {
                    stack.Push(node);
                    lstValue.AddLast(node);
                    node = node.Left;

                }
                else
                {
                    var item = stack.Pop();
                    node = item.Right;
                }
            }
        }

        #endregion

    }
}
