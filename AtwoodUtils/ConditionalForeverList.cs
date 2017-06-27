using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwoodUtils
{
    /// <summary>
    /// A list which returns elements forever. 
    /// <para />
    /// If elements do not pass the given predicate during selection, they will be returned to the top of the stack for consideration during the next selection process.
    /// <para />
    /// Thanks in no small part to McLean for the implementation of this algorithm... and spell checking my comments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConditionalForeverList<T>
    {

        private Stack<T> _remaining;
        private Stack<T> _original;

        List<T> visitedItems = new List<T>();

        /// <summary>
        /// Creates a new forever list from the given source.
        /// </summary>
        /// <param name="source"></param>
        public ConditionalForeverList(IEnumerable<T> source)
        {
            if (!source.Any())
                throw new ArgumentException("Your source has no elements.");

            _remaining = new Stack<T>(source);
            _original = new Stack<T>(source);
        }

        /// <summary>
        /// Gets the next element in the collection that satisfies the given predicate.  Any elements that don't satisfy the predicate will be placed back on top for consideration.
        /// <para/>
        /// Returns false if no elements match the predicate.
        /// </summary>
        /// <returns></returns>
        public bool TryNext(Func<T, bool> predicate, out T item)
        {
            int attempts = 0;
            Stack<T> failures = new Stack<T>();
            visitedItems = new List<T>();

            while (attempts < _original.Count)
            {
                if (!_remaining.Any())
                {
                    _remaining = new Stack<T>(_original);
                }

                item = _remaining.Pop();
                visitedItems.Add(item);

                if (predicate(item))
                {
                    //We need to take everything off the failures stack and put it back onto the remaining stack.
                    while (failures.Any())
                    {
                        _remaining.Push(failures.Pop());
                    }

                    return true;
                }
                else
                {
                    if (!failures.Contains(item))
                    {
                        failures.Push(item);
                        attempts++;
                    }
                }
            }

            item = default(T);
            return false;
        }
    }
}