using Veldrid;
using Veldrid.ImageSharp;

namespace VdGfx
{
    public class VxTexture
    {
        internal Texture Tex { get; }
        internal TextureView View { get; }

        public VxTexture(string imagePath)
        {
            ImageSharpTexture imageSharpTex = new ImageSharpTexture(imagePath);
            Tex = imageSharpTex.CreateDeviceTexture(VxContext.Instance.Device, VxContext.Instance.Factory);
            View = VxContext.Instance.Factory.CreateTextureView(Tex);
        }
    }
}
