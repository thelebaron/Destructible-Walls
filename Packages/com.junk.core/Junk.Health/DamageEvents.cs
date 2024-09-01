using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Junk.Health
{
    /// <summary>
    /// See ImpulseEvents for more information
    /// A stream of impulse events. This is a value type, which means it can be used in Burst jobs
    /// (unlike IEnumerable&lt;ImpulseEvent&gt;).
    /// </summary>
    public struct DamageEvents
    {
        //@TODO: Unity should have a Allow null safety restriction
        [NativeDisableContainerSafetyRestriction]
        private readonly NativeStream m_EventDataStream;

        internal DamageEvents(NativeStream eventDataStream)
        {
            m_EventDataStream = eventDataStream;
        }

        /// <summary>   Gets the enumerator. </summary>
        ///
        /// <returns>   The enumerator. </returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_EventDataStream);
        }

        /// <summary>   An enumerator. </summary>
        public struct Enumerator
        {
            private NativeStream.Reader m_Reader;
            private int m_CurrentWorkItem;
            private readonly int m_NumWorkItems;

            /// <summary>   Gets or sets the current. </summary>
            ///
            /// <value> The current. </value>
            public DamageData Current { get; private set; }

            internal Enumerator(NativeStream stream)
            {
                m_Reader = stream.IsCreated ? stream.AsReader() : new NativeStream.Reader();
                m_CurrentWorkItem = 0;
                m_NumWorkItems = stream.IsCreated ? stream.ForEachCount : 0;
                Current = default;

                AdvanceReader();
            }

            /// <summary>   Determines if we can move next. </summary>
            ///
            /// <returns>   True if we can, false otherwise. </returns>
            public bool MoveNext()
            {
                if (m_Reader.RemainingItemCount > 0)
                {
                    var eventData = m_Reader.Read<DamageEventData>();

                    Current = eventData.CreateDamageEvent();

                    AdvanceReader();
                    return true;
                }
                return false;
            }

            private void AdvanceReader()
            {
                while (m_Reader.RemainingItemCount == 0 && m_CurrentWorkItem < m_NumWorkItems)
                {
                    m_Reader.BeginForEachIndex(m_CurrentWorkItem);
                    m_CurrentWorkItem++;
                }
            }
        }
    }
}