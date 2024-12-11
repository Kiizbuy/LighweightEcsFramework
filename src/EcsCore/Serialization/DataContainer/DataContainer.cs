using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NetCodeUtils.Attributes;

namespace EcsCore.Serialization.DataContainer
{
    public struct Huy
    {
        public int min;
        public int max;
    }


    [FixedArrayGeneration(4)]
    public partial struct Pizda : IFixedArray<Huy>
    {
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0), System.Runtime.CompilerServices.UnsafeValueType, System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Required for fixed-size arrays")]

    public partial struct Pizda : IEquatable<Pizda>
    {
        private Huy _el0;
        private Huy _el1;
        private Huy _el2;
        private Huy _el3;

        public int CurrentCount;
        public const int MaxSize = 4;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CurrentCount;
        }


        public unsafe ref Huy this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG
                if (index >= Count || index < 0)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                var ptr = System.Runtime.CompilerServices.Unsafe.AsPointer<Huy>(ref _el0);
                var elementPtr = System.Runtime.CompilerServices.Unsafe.Add<Huy>(ptr, index);

                return ref System.Runtime.CompilerServices.Unsafe.AsRef<Huy>(elementPtr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Pizda other)
        {
            return IsEqual(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEqual(Pizda other)
        {
            for (var i = 0; i < MaxSize; i++)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<Huy> AsSpan() =>
            new System.Span<Huy>(System.Runtime.CompilerServices.Unsafe.AsPointer<Huy>(ref _el0),
                MaxSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Enumerator GetEnumerator() =>
            new Enumerator(System.Runtime.CompilerServices.Unsafe.AsPointer<Huy>(ref _el0));

        public unsafe struct Enumerator
        {
            private readonly void* _ptr;
            private int _index;

            public Enumerator(void* ptr)
            {
                _ptr = ptr;
                _index = -1;
            }

            public ref Huy Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var ptr = System.Runtime.CompilerServices.Unsafe.Add<Huy>(_ptr, _index);
                    return ref System.Runtime.CompilerServices.Unsafe.AsRef<Huy>(ptr);
                }
            }

            public bool MoveNext()
            {
                return ++_index < MaxSize;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}