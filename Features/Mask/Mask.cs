using Godot;
using System.Collections.Generic;
public enum MaskMode
{
    Union,
    Intersection
}
public partial class Mask : Resource
{
    public static readonly List<int[]> PersonaTraits = new()
    {
        new int[] { 1, 2, 3 },
        new int[] { 1, 2, 4 },
        new int[] { 1, 3, 5 },
        new int[] { 1, 4, 6 },
        new int[] { 2, 5, 6 },
        new int[] { 3, 4, 5 },
        new int[] { 3, 4, 6 },
        new int[] { 2, 3, 6 },
    };
    public Texture2D MaskTexture { get; set; } = null;
    public List<int> Traits { get; set; } = new();
    public MaskMode Mode { get; set; } = MaskMode.Union;
    private Mask() { }

    public partial class MaskBuilder
    {
        private Mask _currentMask = new();
        public static MaskBuilder New() => new MaskBuilder();
        public MaskBuilder WithTexture(Texture2D texture)
        {
            _currentMask.MaskTexture = texture;
            return this;
        }
        public MaskBuilder WithTrait(params int[] traits)
        {
            _currentMask.Traits.AddRange(traits);
            return this;
        }
        public MaskBuilder WithMode(MaskMode mode)
        {
            _currentMask.Mode = mode;
            return this;
        }
        public Mask Get() => _currentMask;
    }
}
