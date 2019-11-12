using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    class PrintUtility
    {
        public static void Print(String str)
        {
            Console.WriteLine("------------------------");
            Console.WriteLine(str);
            Console.WriteLine("------------------------");
        }

        public static void Print(HashSet<String> set)
        {
            Console.WriteLine("------------------------");
            foreach (String item in set)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("------------------------");
        }

        public static void Print(List<String> list)
        {
            Console.WriteLine("------------------------");
            foreach (String item in list)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("------------------------");
        }


        public static void Print(String str, String Header)
        {
            Console.WriteLine("------------" + Header + "------------");
            Console.WriteLine(str);
            Console.WriteLine("------------------------");
        }

        public static void Print(HashSet<String> set, String Header)
        {
            Console.WriteLine("------------" + Header + "------------");
            foreach (String item in set)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("------------------------");
        }

        public static void Print(List<String> list, String Header)
        {
            Console.WriteLine("------------" + Header + "------------");
            foreach (String item in list)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("------------------------");
        }
        public static void PrintChain(List<String> list)
        {
            foreach (String item in list)
            {
                Console.Write(" -> " + item);
            }
            Console.WriteLine();
        }
    }
}
