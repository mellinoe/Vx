using System.Runtime.CompilerServices;
using Veldrid;

namespace VdGfx
{
    internal static class ResourceFactoryExtensions
    {
        internal static DeviceBuffer CreateBufferFor<T>(this ResourceFactory factory, BufferUsage usage)
        {
            return factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<T>(), usage));
        }
    }
}
