using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MiniTwitter.Net.Twitter;

namespace MiniTwitter.Extensions
{
    public static class FiltersExtension
    {
        public static bool CheckEach(this List<Filter> filters, ITwitterItem item)
        {
            Stack<Tuple<bool, int>> results = new Stack<Tuple<bool, int>>();
            for (int i = 0; i < filters.Count - 1; i++)
            {
                if (results.Count == 0 || results.Peek().Item2 < filters[i].Level)
                {
                    results.Push(Tuple.Create(filters[i].Process(item), filters[i].Level));
                }
                else if (results.Peek().Item2 == filters[i].Level)
                {
                    var t = results.Pop();
                    if (filters[i].AndCombine)
                    {
                        results.Push(Tuple.Create(t.Item1 && filters[i].Process(item), filters[i].Level));
                    }
                    else
                    {
                        results.Push(Tuple.Create(t.Item1 || filters[i].Process(item), filters[i].Level));
                    }
                }
                else
                {
                    bool r = filters[i].Process(item);
                    if (filters[i].AndCombine)
                    {
                        do
                        {
                            r = r && results.Pop().Item1;
                        } while (results.Count == 0 || results.Peek().Item2 >= filters[i].Level);
                    }
                    else
                    {
                        do
                        {
                            r = r || results.Pop().Item1;
                        } while (results.Count == 0 || results.Peek().Item2 >= filters[i].Level);
                    }
                    results.Push(Tuple.Create(r, filters[i].Level));
                }
            }
            if (filters.Last().AndCombine)
            {
                results.Push(Tuple.Create(filters.Last().Process(item), filters.Last().Level));
                return results.All(t => t.Item1);
            }
            else
            {
                results.Push(Tuple.Create(filters.Last().Process(item), filters.Last().Level));
                return results.Any(t => t.Item1);
            }
            //return false;
        }
    }
}
