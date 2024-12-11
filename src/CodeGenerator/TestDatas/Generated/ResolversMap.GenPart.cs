using CodeGenerator.TestDatas;
using CodeGenerator.TestDatas.NestedComponents.FirstCase;
using EcsCore.Serialization.Extensions;

namespace EcsCore.Serialization.Resolvers
{
    public sealed class TransformComponentComponentResolver : ComponentResolver<TransformComponent, TransformComponentComponentResolver.Data>
    {
        public struct Data : ISerializableData
        {
            public PlayerDataFixedArray FixedArray;
            public StructFixedArray Huy;
            public float X;
            public float Y;
            public float Z;
            public string Name;
            public void Serialize(ISerializePacker packer)
            {
                packer.Write(Z);
                packer.Write(Name);
                packer.Write(Huy.Count);
                for (int i = 0; i < Huy.Count; i++)
                {
                    packer.Write(Huy[i].Amount);
                    packer.Write(Huy[i].Max);
                    packer.Write(Huy[i].Min);
                }
                //
                // packer.Write(FixedArray.Count);
                // for (int i = 0; i < FixedArray.CurrentCount; i++)
                // {
                //     packer.Write(FixedArray[i]);
                // }
            }

            public void Deserialize(ISerializePacker packer)
            {
                Z = packer.ReadFloat();
                Name = packer.ReadString();
                var HuyCount = packer.ReadInt();
                for (int i = 0; i < HuyCount; i++)
                {
                    Huy[i].Amount = packer.ReadInt();
                    Huy[i].Min = packer.ReadInt();
                    Huy[i].Max = packer.ReadInt();
                }
                // var FixedArrayCount = packer.ReadInt();
                // for (int i = 0; i < FixedArrayCount; i++)
                // {
                //     FixedArray[i] = packer.ReadUInt();
                // }
            }
        }

        protected override Data FillSerializableDataFrom(ref TransformComponent component)
        {
            Data data;
            data.X = component.X;
            data.Y = component.Y;
            data.Z = component.Z;
            data.Name = component.Name;
            data.FixedArray = component.FixedArray;
            data.Huy = default;
            return data;
        }

        protected override void FillComponent(ref Data data, ref TransformComponent component)
        {
            component.X = data.X;
            component.Y = data.Y;
            component.Z = data.Z;
            component.FixedArray = data.FixedArray;
            component.Name = data.Name;
        }
    }

    public sealed class HealthComponentComponentResolver : ComponentResolver<HealthComponent, HealthComponentComponentResolver.Data>
    {
        public struct Data : ISerializableData
        {
            public float Min;
            public int Max;
            public int Amount;
            public void Serialize(ISerializePacker packer)
            {
                packer.Write(Min, -100, 100.001f, 0.01f);
                packer.Write(Max, -12, 12);
            }

            public void Deserialize(ISerializePacker packer)
            {
                Min = packer.ReadFloat(-100, 100.001f, 0.01f);
                Max = packer.ReadInt(-12, 12);
            }
        }

        protected override Data FillSerializableDataFrom(ref HealthComponent component)
        {
            Data data;
            data.Min = component.Min;
            data.Max = component.Max;
            data.Amount = component.Amount;
            return data;
        }

        protected override void FillComponent(ref Data data, ref HealthComponent component)
        {
            component.Min = data.Min;
            component.Max = data.Max;
            component.Amount = data.Amount;
        }
    }

    public sealed class SpeedComponentComponentResolver : ComponentResolver<SpeedComponent, SpeedComponentComponentResolver.Data>
    {
        public struct Data : ISerializableData
        {
            public byte SpeedId;
            public ushort SpeedGen;
            public int TargetId;
            public void Serialize(ISerializePacker packer)
            {
                packer.Write(SpeedId, 8);
                packer.Write(SpeedGen, 16);
                packer.Write(TargetId, 32);
            }

            public void Deserialize(ISerializePacker packer)
            {
                SpeedId = packer.ReadByte(8);
                SpeedGen = packer.ReadUShort(16);
                TargetId = packer.ReadInt(32);
            }
        }

        protected override Data FillSerializableDataFrom(ref SpeedComponent component)
        {
            Data data;
            data.SpeedId = component.SpeedId;
            data.SpeedGen = component.SpeedGen;
            data.TargetId = component.TargetId;
            return data;
        }

        protected override void FillComponent(ref Data data, ref SpeedComponent component)
        {
            component.SpeedId = data.SpeedId;
            component.SpeedGen = data.SpeedGen;
            component.TargetId = data.TargetId;
        }
    }

    public sealed class MoveComponentTagComponentResolver : ComponentResolver<MoveComponentTag, MoveComponentTagComponentResolver.Data>
    {
        public struct Data : ISerializableData
        {
            public void Serialize(ISerializePacker packer)
            {
            }

            public void Deserialize(ISerializePacker packer)
            {
            }
        }

        protected override Data FillSerializableDataFrom(ref MoveComponentTag component)
        {
            Data data;
            return data;
        }

        protected override void FillComponent(ref Data data, ref MoveComponentTag component)
        {
        }
    }

    internal static partial class ResolversMap
    {
        static partial void Initialize()
        {
            _componentResolvers.TryAdd(nameof(TransformComponent), new TransformComponentComponentResolver());
            _componentResolvers.TryAdd(nameof(HealthComponent), new HealthComponentComponentResolver());
            _componentResolvers.TryAdd(nameof(SpeedComponent), new SpeedComponentComponentResolver());
            _componentResolvers.TryAdd(nameof(MoveComponentTag), new MoveComponentTagComponentResolver());
        }
    }
}
