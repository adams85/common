using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace Karambolo.Common
{
    /// <remarks>
    /// <see href="http://stackoverflow.com/questions/17828518/parallel-foreach-stalled-when-integarated-with-blockingcollection" />
    /// <see href="http://blogs.msdn.com/b/pfxteam/archive/2010/04/06/9990420.aspx" />
    /// </remarks>
    public static class ConcurrencyUtils
    {
        class BlockingCollectionPartitioner<T> : Partitioner<T>
        {
            readonly BlockingCollection<T> _collection;

            internal BlockingCollectionPartitioner(BlockingCollection<T> collection)
            {
                if (collection == null)
                    throw new ArgumentNullException(nameof(collection));

                _collection = collection;
            }

            public override bool SupportsDynamicPartitions => true;

            public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
            {
                if (!(partitionCount > 0))
                    throw new ArgumentException(null, nameof(partitionCount));

                var dynamicPartitioner = GetDynamicPartitions();
                return Enumerable.Range(0, partitionCount).Select(_ => dynamicPartitioner.GetEnumerator()).ToArray();
            }

            public override IEnumerable<T> GetDynamicPartitions()
            {
                return _collection.GetConsumingEnumerable();
            }
        }

        public static Partitioner<T> GetConsumingPartitioner<T>(this BlockingCollection<T> @this)
        {
            if (@this == null)
                throw new NullReferenceException();

            return new BlockingCollectionPartitioner<T>(@this);
        }
    }
}
