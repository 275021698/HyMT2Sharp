using System.Runtime.InteropServices;
using Sdcb.HyMT2Sharp.Backends.Vulkan;

namespace Sdcb.HyMT2Sharp.Tests;

public sealed class VulkanStructLayoutTests
{
    [Fact]
    public unsafe void VkMemoryHeap_HasExpectedSizeAndLayout()
    {
        Assert.Equal(16, sizeof(Vk.VkMemoryHeap));
        Assert.Equal(16, Marshal.SizeOf<Vk.VkMemoryHeap>());

        Vk.VkMemoryHeap heap = new()
        {
            Size = 0x0123_4567_89AB_CDEFul,
            Flags = 0xFEDC_BA98u,
        };

        byte* p = (byte*)&heap;
        ulong size = *(ulong*)p;
        uint flags = *(uint*)(p + 8);

        Assert.Equal(0x0123_4567_89AB_CDEFul, size);
        Assert.Equal(0xFEDC_BA98u, flags);
    }

    [Fact]
    public unsafe void VkPhysicalDeviceMemoryProperties_HeapAtAndTypeAtWorkCorrectly()
    {
        Assert.Equal(8, sizeof(Vk.VkMemoryType));
        Assert.Equal(520, sizeof(Vk.VkPhysicalDeviceMemoryProperties));

        Vk.VkPhysicalDeviceMemoryProperties props = default;
        props.MemoryTypeCount = 2;
        props.MemoryHeapCount = 2;

        Vk.VkMemoryType* types = (Vk.VkMemoryType*)props.MemoryTypes;
        types[0] = new() { PropertyFlags = 1, HeapIndex = 0 };
        types[1] = new() { PropertyFlags = 15, HeapIndex = 1 };

        Vk.VkMemoryHeap* heaps = (Vk.VkMemoryHeap*)props.MemoryHeaps;
        heaps[0] = new() { Size = 1024 * 1024 * 64, Flags = 1 };
        heaps[1] = new() { Size = 1024 * 1024 * 1024, Flags = 0 };

        Vk.VkMemoryType t0 = props.TypeAt(0);
        Vk.VkMemoryType t1 = props.TypeAt(1);
        Assert.Equal(1u, t0.PropertyFlags);
        Assert.Equal(0u, t0.HeapIndex);
        Assert.Equal(15u, t1.PropertyFlags);
        Assert.Equal(1u, t1.HeapIndex);

        Vk.VkMemoryHeap h0 = props.HeapAt(0);
        Vk.VkMemoryHeap h1 = props.HeapAt(1);
        Assert.Equal(1024ul * 1024 * 64, h0.Size);
        Assert.Equal(1u, h0.Flags);
        Assert.Equal(1024ul * 1024 * 1024, h1.Size);
        Assert.Equal(0u, h1.Flags);
    }
}
