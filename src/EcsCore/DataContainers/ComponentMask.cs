using System;
using System.Runtime.CompilerServices;
using EcsCore.Components;

namespace EcsCore
{
    public struct ComponentMask
    {
        private int[] _includeComponentIndexes;
        private int[] _excludeComponentIndexes;
        private int _includeCount;
        private int _excludeCount;
        private bool _includeAny;
        private bool _excludeAny;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentMask IncludeAny()
        {
            _includeAny = true;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentMask ExcludeAny()
        {
            _excludeAny = true;
            return this;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentMask Include<T>() where T : struct, IComponentData
        {
            if (_includeComponentIndexes == null)
            {
                _includeComponentIndexes = new int[32];
            }

            if (_excludeComponentIndexes == null)
            {
                _excludeComponentIndexes = new int[32];
            }

            var typeBitMask = EcsComponentTypes<T>.TypeBitMask;
            if (_includeCount == _includeComponentIndexes.Length)
            {
                Array.Resize(ref _includeComponentIndexes, _includeCount << 1);
            }

            _includeComponentIndexes[_includeCount++] = typeBitMask;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentMask Exclude<T>() where T : struct, IComponentData
        {
            if (_includeComponentIndexes == null)
            {
                _includeComponentIndexes = new int[32];
            }

            if (_excludeComponentIndexes == null)
            {
                _excludeComponentIndexes = new int[32];
            }

            var typeBitMask = EcsComponentTypes<T>.TypeBitMask;
            if (_excludeCount == _excludeComponentIndexes.Length)
            {
                Array.Resize(ref _excludeComponentIndexes, _excludeCount << 1);
            }

            _excludeComponentIndexes[_excludeCount++] = typeBitMask;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIncluded(in Entity entity)
        {
            var included = false;
            for (var i = 0; i < _includeCount; i++)
            {
                var typeIndex = _includeComponentIndexes[i];
                included = (entity.AddedComponentsMask & typeIndex) == typeIndex;
                if (_includeAny && included)
                {
                    return true;
                }
            }

            return included;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsExcluded(in Entity entity)
        {
            var excluded = false;
            for (var i = 0; i < _excludeCount; i++)
            {
                var typeIndex = _excludeComponentIndexes[i];
                excluded = (entity.AddedComponentsMask & typeIndex) == typeIndex;
                if (_excludeAny && excluded)
                {
                    return true;
                }
            }

            return excluded;
        }
    }
}