using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CodeGenerator.TestDatas.NestedComponents.FirstCase;

namespace CodeGenerator.TestDatas
{
    [StructLayout(LayoutKind.Sequential, Pack = 0), System.Runtime.CompilerServices.UnsafeValueType, System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Required for fixed-size arrays")]
    public partial struct StructFixedArray
    {
        public const int MaxSize = 4;
        public int CurrentCount;
        
        private HealthComponent _el0;
        private HealthComponent _el1;
        private HealthComponent _el2;
        private HealthComponent _el3;

        public unsafe ref  HealthComponent  this[int index]
        {
            get
            {
                if (index >= MaxSize || index < 0)
                {
                    throw new IndexOutOfRangeException();
                }
                var ptr = System.Runtime.CompilerServices.Unsafe.AsPointer<HealthComponent>(ref _el0);
                var elementPtr = System.Runtime.CompilerServices.Unsafe.Add<HealthComponent>(ptr, index);
                return ref System.Runtime.CompilerServices.Unsafe.AsRef<HealthComponent>(elementPtr);
            }
        }

        public unsafe Span<HealthComponent> AsSpan()
        {
            return new System.Span<HealthComponent>(System.Runtime.CompilerServices.Unsafe.AsPointer<HealthComponent>(ref _el0), MaxSize);
        }

        public unsafe Enumerator GetEnumerator()
        {
            return new Enumerator(System.Runtime.CompilerServices.Unsafe.AsPointer<HealthComponent>(ref _el0));
        }

        public int Count
        {
            get
            {
                return CurrentCount;
            }
        }

        public unsafe struct Enumerator
        {
            private readonly void* _ptr;
            private int _index;
            public Enumerator(void* ptr)
            {
                _ptr = ptr;
                _index = -1;
            }

            public ref HealthComponent Current
            {
                get
                {
                    var ptr = System.Runtime.CompilerServices.Unsafe.Add<HealthComponent>(_ptr, _index);
                    return ref System.Runtime.CompilerServices.Unsafe.AsRef<HealthComponent>(ptr);
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
    
    [StructLayout(LayoutKind.Sequential, Pack = 0), System.Runtime.CompilerServices.UnsafeValueType, System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Required for fixed-size arrays")]
    public partial struct PlayerDataFixedArray
    {
        public const int MaxSize = 4;
        public int CurrentCount;
        private uint _el0;
        private uint _el1;
        private uint _el2;
        private uint _el3;

        public unsafe ref  uint  this[int index]
        {
            get
            {
                if (index >= MaxSize || index < 0)
                {
                throw new IndexOutOfRangeException();
                 }
                var ptr = System.Runtime.CompilerServices.Unsafe.AsPointer<uint>(ref _el0);
                var elementPtr = System.Runtime.CompilerServices.Unsafe.Add<uint>(ptr, index);
                return ref System.Runtime.CompilerServices.Unsafe.AsRef<uint>(elementPtr);
            }
        }

        public unsafe Span<uint> AsSpan()
        {
            return new System.Span<uint>(System.Runtime.CompilerServices.Unsafe.AsPointer<uint>(ref _el0), MaxSize);
        }

        public unsafe Enumerator GetEnumerator()
        {
            return new Enumerator(System.Runtime.CompilerServices.Unsafe.AsPointer<uint>(ref _el0));
        }

        public int Count
        {
            get
            {
                return CurrentCount;
            }
        }

        public unsafe struct Enumerator
        {
            private readonly void* _ptr;
            private int _index;
            public Enumerator(void* ptr)
            {
                _ptr = ptr;
                _index = -1;
            }

            public ref uint Current
            {
                get
                {
                    var ptr = System.Runtime.CompilerServices.Unsafe.Add<uint>(_ptr, _index);
                    return ref System.Runtime.CompilerServices.Unsafe.AsRef<uint>(ptr);
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
