using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EcsCore.Data.FixedArrays
{
    // public class zalupaIvana
    // {
    //     public PlayerDataFixedArray _huyec;
    //
    //     public void Manda()
    //     {
    //         for (int i = 0; i < PlayerDataFixedArray.MaxSize; i++)
    //         {
    //             _huyec[i] = (uint)(i * 2);
    //         }
    //
    //         var bytes = GetBytes(_huyec);
    //
    //         var newStruct = fromBytes(bytes);
    //
    //         Console.WriteLine("HM");
    //         Console.ReadKey();
    //     }
    //     
    //     byte[] GetBytes(PlayerDataFixedArray str) {
    //         int size = Marshal.SizeOf(str);
    //         byte[] arr = new byte[size];
    //
    //         IntPtr ptr = IntPtr.Zero;
    //         try
    //         {
    //             ptr = Marshal.AllocHGlobal(size);
    //             Marshal.StructureToPtr(str, ptr, true);
    //             Marshal.Copy(ptr, arr, 0, size);
    //         }
    //         finally
    //         {
    //             Marshal.FreeHGlobal(ptr);
    //         }
    //         return arr;
    //     }
    //     
    //     PlayerDataFixedArray fromBytes(byte[] arr)
    //     {
    //         PlayerDataFixedArray str = new PlayerDataFixedArray();
    //
    //         int size = Marshal.SizeOf(str);
    //         IntPtr ptr = IntPtr.Zero;
    //         try
    //         {
    //             ptr = Marshal.AllocHGlobal(size);
    //
    //             Marshal.Copy(arr, 0, ptr, size);
    //
    //             str = (PlayerDataFixedArray)Marshal.PtrToStructure(ptr, str.GetType());
    //         }
    //         finally
    //         {
    //             Marshal.FreeHGlobal(ptr);
    //         }
    //         return str;
    //     }
    // }
}
