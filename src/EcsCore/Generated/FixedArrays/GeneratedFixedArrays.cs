// using System;
// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;
//
// namespace EcsCore.Data.FixedArrays
// {
//     [StructLayout(LayoutKind.Sequential, Pack = 0), System.Runtime.CompilerServices.UnsafeValueType, System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Required for fixed-size arrays")]
//     public partial struct PlayerDataFixedArray
//     {
//         public const int MaxSize = 32;
//         public int CurrentCount;
//         private uint _el0;
//         private uint _el1;
//         private uint _el2;
//         private uint _el3;
//         private uint _el4;
//         private uint _el5;
//         private uint _el6;
//         private uint _el7;
//         private uint _el8;
//         private uint _el9;
//         private uint _el10;
//         private uint _el11;
//         private uint _el12;
//         private uint _el13;
//         private uint _el14;
//         private uint _el15;
//         private uint _el16;
//         private uint _el17;
//         private uint _el18;
//         private uint _el19;
//         private uint _el20;
//         private uint _el21;
//         private uint _el22;
//         private uint _el23;
//         private uint _el24;
//         private uint _el25;
//         private uint _el26;
//         private uint _el27;
//         private uint _el28;
//         private uint _el29;
//         private uint _el30;
//         private uint _el31;
//         public unsafe ref  uint  this[int index]
//         {
//             get
//             {
//                 if (index >= MaxSize || index < 0)
//                 {
//                 throw new IndexOutOfRangeException();
//                  }
//                 var ptr = System.Runtime.CompilerServices.Unsafe.AsPointer<uint>(ref _el0);
//                 var elementPtr = System.Runtime.CompilerServices.Unsafe.Add<uint>(ptr, index);
//                 return ref System.Runtime.CompilerServices.Unsafe.AsRef<uint>(elementPtr);
//             }
//         }
//
//         public unsafe Span<uint> AsSpan()
//         {
//             return new System.Span<uint>(System.Runtime.CompilerServices.Unsafe.AsPointer<uint>(ref _el0), MaxSize);
//         }
//
//         public unsafe Enumerator GetEnumerator()
//         {
//             return new Enumerator(System.Runtime.CompilerServices.Unsafe.AsPointer<uint>(ref _el0));
//         }
//
//         public int Count
//         {
//             get
//             {
//                 return CurrentCount;
//             }
//         }
//
//         public unsafe struct Enumerator
//         {
//             private readonly void* _ptr;
//             private int* _index;
//             public Enumerator(void* ptr)
//             {
//                 _ptr = ptr;
//                 _index = -1;
//             }
//
//             public ref uint Current
//             {
//                 get
//                 {
//                     var ptr = System.Runtime.CompilerServices.Unsafe.Add<uint>(_ptr, _index);
//                     return ref System.Runtime.CompilerServices.Unsafe.AsRef<uint>(ptr);
//                 }
//             }
//
//             public bool MoveNext()
//             {
//                 return ++_index < MaxSize;
//             }
//
//             public void Reset()
//             {
//                 _index = -1;
//             }
//         }
//     }
// }
