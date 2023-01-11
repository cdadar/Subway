using System;
using System.Collections;
using System.Data;

namespace BusinessObject
{
    /// <summary>
    ///     组合数学函数集
    /// </summary>
    public class Combinatorics
    {
        #region 公共函数

        /// <summary>
        ///     把二维整形数组转换为数据表
        /// </summary>
        public static DataTable TwoDemisionIntArrayToDataTable(int[,] source)
        {
            var dt = new DataTable();
            DataRow dr;
            int i, j;

            int b1 = source.GetUpperBound(0), b2 = source.GetUpperBound(1); //获取二维表的各维长度                         

            for (i = 0; i <= b1; i++) //以第二维长度创建数据表的各字段
                dt.Columns.Add(i.ToString(), Type.GetType("System.Int32"));

            for (i = 0; i <= b2; i++) //对各返回排列循环
            {
                dr = dt.NewRow(); //准备插入新行
                for (j = 0; j <= b1; j++) //在新行中逐个填入返回排列的各元素次序                
                    dr[j.ToString()] = source[j, i]; //用序数指针获取原元素的值
                dt.Rows.Add(dr); //插入新行
            }

            return dt;
        }

        /// <summary>
        ///     连乘积函数
        /// </summary>
        public static int Product(int start, int finish)
        {
            var factorial = 1;
            for (var i = start; i <= finish; i++)
                factorial *= i;
            return factorial;
        }

        /// <summary>
        ///     阶乘函数
        /// </summary>
        public static int Factorial(int n)
        {
            return Product(2, n);
        }

        /// <summary>
        ///     排列数函数
        /// </summary>
        public static int ArrangeCount(int m, int n)
        {
            return Product(n - m + 1, n);
        }

        /// <summary>
        ///     生成排列表函数
        /// </summary>
        public static int[,] Arrange(int m, int n)
        {
            var A = ArrangeCount(m, n); //求得排列数，安排返回数组的第一维
            var arrange = new int[m, A]; //定义返回数组
            var e = new ArrayList(); //设置元素表
            for (var i = 0; i < n; i++)
                e.Add(i + 1);
            Arrange(ref arrange, e, m, 0, 0);
            return arrange;
        }

        /// <summary>
        ///     组合数函数
        /// </summary>
        public static int CombinationCount(int m, int n)
        {
            int a = Product(n - m + 1, n), b = Product(2, m); //a=n-m+1 * ... * n ; b = m!
            return a / b; //c=a/b                              
        }

        /// <summary>
        ///     生成组合表函数
        /// </summary>
        public static int[,] Combination(int m, int n)
        {
            var A = CombinationCount(m, n); //求得排列数，安排返回数组的第一维
            var combination = new int[m, A]; //定义返回数组
            var e = new ArrayList(); //设置元素表
            for (var i = 0; i < n; i++)
                e.Add(i + 1);
            Combination(ref combination, e, m, 0, 0);
            return combination;
        }

        #endregion

        #region 内部核心

        /// <summary>
        ///     排列函数
        /// </summary>
        /// <param name="result">返回值数组</param>
        /// <param name="elements">可供选择的元素数组</param>
        /// <param name="m">目标选定元素个数</param>
        /// <param name="x">当前返回值数组的列坐标</param>
        /// <param name="y">当前返回值数组的行坐标</param>
        private static void Arrange(ref int[,] result, ArrayList elements, int m, int x, int y)
        {
            var sub = ArrangeCount(m - 1, elements.Count - 1); //求取当前子排列的个数
            for (var i = 0; i < elements.Count; i++, y += sub) //每个元素均循环一次,每次循环后移动行指针
            {
                var val = RemoveAndWrite(elements, i, ref result, x, y, sub);
                if (m > 1) //递归条件为子排列数大于1
                    Arrange(ref result, elements, m - 1, x + 1, y);
                elements.Insert(i, val); //恢复刚才删除的元素                  
            }
        }

        /// <summary>
        ///     组合函数
        /// </summary>
        /// <param name="result">返回值数组</param>
        /// <param name="elements">可供选择的元素数组</param>
        /// <param name="m">目标选定元素个数</param>
        /// <param name="x">当前返回值数组的列坐标</param>
        /// <param name="y">当前返回值数组的行坐标</param>
        private static void Combination(ref int[,] result, ArrayList elements, int m, int x, int y)
        {
            var tmpElements = new ArrayList(); //所有本循环使用的元素都将暂时存放在这个数组
            var elementsCount = elements.Count; //先记录可选元素个数
            int sub;
            for (var i = elementsCount - 1; i >= m - 1; i--, y += sub) //从elementsCount-1(即n-1)到m-1的循环,每次循环后移动行指针
            {
                sub = CombinationCount(m - 1, i); //求取当前子组合的个数
                var val = RemoveAndWrite(elements, 0, ref result, x, y, sub);
                tmpElements.Add(val); //把这个可选元素存放到临时数组,循环结束后一并恢复到elements数组中                 
                if (sub > 1 || (elements.Count + 1 == m &&
                                elements.Count > 0)) //递归条件为 子组合数大于1 或 可选元素个数+1等于当前目标选择元素个数且可选元素个数大于1
                    Combination(ref result, elements, m - 1, x + 1, y);
            }

            elements.InsertRange(0, tmpElements); //一次性把上述循环删除的可选元素恢复到可选元素数组中           
        }

        /// <summary>
        ///     返回由Index指定的可选元素值,并在数组中删除之,再从y行开始在x列中连续写入subComb个值
        /// </summary>
        private static int RemoveAndWrite(ArrayList elements, int index, ref int[,] result, int x, int y, int count)
        {
            var val = (int)elements[index];
            elements.RemoveAt(index);
            for (var i = 0; i < count; i++)
                result[x, y + i] = val;
            return val;
        }

        #endregion
    }
}