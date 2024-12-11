// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Runtime.InteropServices;
// using System.Security.Cryptography;
// using EcsCore;
//
// namespace NetCodeUtils.Network.Desync
// {
//     public enum DesyncDetectionMode : byte
//     {
//         CriticalPartStateHash,
//         CriticalPartState,
//         FullState,
//         None,
//     }
//     
//     public class ClientDesyncDetector : IDisposable
//     {
//         private const int CONVERSION_BUFFER_SIZE = 8;
//
//         private const int FULL_STATE_BUFFER_SIZE = 5;
//         private const int CRITIAL_STATE_BUFFER_SIZE = 5;
//         private const int CRITIAL_STATE_HASH_BUFFER_SIZE = 5;
//
//         private volatile bool _disposed;
//         private volatile bool _alive;
//         private readonly IDesyncCheckerStrategy[] _strategies = new IDesyncCheckerStrategy[3];
//
//         public ClientDesyncDetector(DataStorage storage)
//         {
//             this[DesyncDetectionMode.FullState] = new FullStateCompareStrategy(storage);
//             this[DesyncDetectionMode.CriticalPartState] = new CriticalPartStateCompareStrategy(storage);
//             this[DesyncDetectionMode.CriticalPartStateHash] = new HashBasedCompareStrategy(storage);
//         }
//
//         private IDesyncCheckerStrategy this[DesyncDetectionMode mode]
//         {
//             get => _strategies[(int)mode];
//             set => _strategies[(int) mode] = value;
//         }
//
//         /// <summary>
//         /// Save actual debug state for desync check
//         /// </summary>
//         /// <exception cref="T:System.ArgumentException">Invalid detection mode type</exception>
//         /// <param name="dto">Data from server</param>
//         public void RegisterDesyncCheckMessageFromServer(MatchDesyncCheckDTO dto)
//         {
//             if (_disposed)
//             {
//                 return;
//             }
//
//             if (dto.DetectionMode == DesyncDetectionMode.None)
//             {
//                 throw new ArgumentException($"There are no strategy for desync detection mode '{dto.DetectionMode}'");
//             }
//
//             this[dto.DetectionMode].RegisterServerMessage(dto);
//             _alive = true;
//         }
//
//
//         /// <summary>
//         /// Check if actual server state equals to prediction-free client state (using several strategies)
//         /// Strategies order: from fast one to slow one
//         /// </summary>
//         /// <returns>True when detector alive and two states with the same tick are not equal (desync detected). False otherwise</returns>
//         public bool CheckDesync()
//         {
//             if (!_alive || _disposed)
//             {
//                 return false;
//             }
//
//
//             return this[DesyncDetectionMode.CriticalPartStateHash].CheckDesync() ||
//                    this[DesyncDetectionMode.CriticalPartState].CheckDesync() ||
//                    this[DesyncDetectionMode.FullState].CheckDesync();
//         }
//
//         public void Dispose()
//         {
//             _disposed = true;
//
//             foreach (var strategy in _strategies)
//             {
//                 strategy.Dispose();
//             }
//         }
//
//
//
//         private interface IDesyncCheckerStrategy : IDisposable
//         {
//             void RegisterServerMessage(MatchDesyncCheckDTO dto);
//             bool CheckDesync();
//         }
//
//
//         private abstract class State2StateStrategy : ClientDesyncDetectorStrategy<GameState>
//         {
//             protected ISerializer Reader;
//
//             protected State2StateStrategy(DataStorage storage, int bufferSize) : base(storage, bufferSize)
//             {
//             }
//
//             protected sealed override GameState ReadStateFromStream(MemoryStream ms)
//             {
//                 var res = new GameState();
//                 Reader = Reader ?? new StrictBitsPacker(new byte[CONVERSION_BUFFER_SIZE]);
//                 Reader.SetStream(ms);
//                 DeserializeByStrategy(res);
//                 return res;
//             }
//
//             protected sealed override GameState FilterServerState(GameState originalState)
//             {
//                 originalState.FilterByMinMaxList(Filter, ServerStateWithoutPrediction);
//                 return ServerStateWithoutPrediction;
//             }
//
//             protected sealed override void SerializeLocalState(GameState localState, MemoryStream ms) => 
//                 SerializeGameState(localState, ms);
//
//             protected sealed override void SerializeServerState(GameState serverState, MemoryStream ms) =>
//                 SerializeGameState(serverState, ms);
//
//             private void SerializeGameState(GameState gs, MemoryStream ms)
//             {
//                 gs.LocalTick = 0;
//                 gs.SimTick = 0;
//                 Writer.SetStream(ms);
//                 SerializeByStrategy(gs);
//                 Writer.Flush();
//             }
//
//             protected abstract void SerializeByStrategy(GameState gs);
//             protected abstract void DeserializeByStrategy(GameState gs);
//         }
//
//
//         private sealed class HashBasedCompareStrategy : ClientDesyncDetectorStrategy<ArraySegment<byte>>
//         {
//             private HashAlgorithm _hashProvider;
//             private MemoryStream _streamHelper;
//
//             public HashBasedCompareStrategy(DataStorage storage) : base(storage, CRITIAL_STATE_HASH_BUFFER_SIZE)
//             {
//             }
//
//             protected override ArraySegment<byte> ReadStateFromStream(MemoryStream ms)
//             {
//                 var copy = new byte[(int)ms.Length - 1];
//                 ms.Read(copy, 0, copy.Length);
//                 return new ArraySegment<byte>(copy);
//             }
//
//             protected override ArraySegment<byte> FilterServerState(ArraySegment<byte> originalState) => originalState;
//
//             protected override void SerializeLocalState(GameState localState, MemoryStream ms)
//             {
//                 localState.LocalTick = 0;
//                 localState.SimTick = 0;
//
//                 if (_hashProvider == null)
//                 {
//                     var buff = new byte[1024 * 4];
//                     _streamHelper = new MemoryStream(buff, 0, buff.Length, true, true);
//                     _hashProvider = DesyncStateCheckerUtils.CreateHashAlgorithm();
//                 }
//
//                 _streamHelper.Seek(0, SeekOrigin.Begin);
//                 _streamHelper.SetLength(0);
//
//                 Writer.SetStream(_streamHelper);
//                 localState.SimTick = 0;
//                 localState.LocalTick = 0;
//                 localState.SerSyncCriticalPart(Writer);
//                 Writer.Flush();
//
//                 var hash = DesyncStateCheckerUtils.CalculateHash(new ArraySegment<byte>(_streamHelper.GetBuffer(),
//                     0, (int) _streamHelper.Length), _hashProvider);
//                 ms.Write(hash.Array, hash.Offset, hash.Count);
//             }
//
//             protected override void SerializeServerState(ArraySegment<byte> array, MemoryStream ms) 
//                 => ms.Write(array.Array, array.Offset, array.Count);
//
//             public override void Dispose()
//             {
//                 lock (SafeDisposeLock)
//                 {
//                     base.Dispose();
//                     _hashProvider?.Dispose();
//                     _streamHelper?.Dispose();
//                 }
//             }
//
//             protected override bool IsApproachSupported(MemoryStream ms)
//             {
//                 var byteOrderFlag = ms.ReadByte();
//                 bool serverByteOrder = Convert.ToBoolean(byteOrderFlag);
//                 return serverByteOrder && BitConverter.IsLittleEndian;
//             }
//
//         }
//
//
//         private sealed class FullStateCompareStrategy : State2StateStrategy
//         {
//             private ISimpleLogger _logger;
//             public FullStateCompareStrategy(DataStorage storage, ISimpleLogger logger) : base(storage, FULL_STATE_BUFFER_SIZE)
//             {
//                 _logger = logger;
//             }
//
//             protected override void SerializeByStrategy(EcsState gs) => gs.Ser(Writer);
//             protected override void DeserializeByStrategy(EcsState gs) => gs.Deser(Reader);
//
//             protected override void OnComparisionFailed(EcsState localState, GameState serverState)
//             {
//                 base.OnComparisionFailed(localState, serverState);
//                 _logger.Info($"Local state:\n {localState.DumpState()}");
//                 _logger.Info($"Server state:\n {serverState.DumpState()}");
//             }
//         }
//
//
//         private sealed class CriticalPartStateCompareStrategy : State2StateStrategy
//         {
//             public CriticalPartStateCompareStrategy(DataStorage storage) : base(storage, CRITIAL_STATE_BUFFER_SIZE)
//             {
//             }
//
//             protected override void SerializeByStrategy(EcsState gs) => gs.SerSyncCriticalPart(Writer);
//             protected override void DeserializeByStrategy(EcsState gs) => gs.DeserSyncCriticalPart(Reader);
//
//         }
//
//
//         private abstract class ClientDesyncDetectorStrategy<TServerState> : IDesyncCheckerStrategy where TServerState: new()
//         {
//             private const string USE_COMPARISION_CALLBACKS = "USE_COMPARISION_CALLBACKS";
//
//             private volatile bool _alive;
//             private volatile bool _disposed;
//             private readonly object _lock = new object();
//
//             private readonly int _bufferSize;
//             private readonly DataStorage _storage;
//
//             private MemoryStream[] _desyncCheckerStreamHelpers;
//             private Sampler<TServerState> _debugStateSampler;
//
//             private EcsState _localStateWithoutPrediction;
//             protected TServerState ServerStateWithoutPrediction;
//             protected IPacker Writer;
//             protected readonly object SafeDisposeLock = new object();
//
//
// 			private class ServerStateComparer : IComparer<(uint serverTick, TServerState serverState)>
// 			{
// 				public int Compare(
// 					(uint serverTick, TServerState serverState) x,
// 					(uint serverTick, TServerState serverState) y)
// 				{
// 					return y.serverTick.CompareTo(x.serverTick);
// 				}
// 			}
//
//             private readonly IDictionary<uint, EcsState> _clientStatesMap = new Dictionary<uint, EcsState>();
//             private readonly List<(uint serverTick, TServerState serverState)> _receivedFullStatesFromServer
//                 = new List<(uint serverTick, TServerState serverState)>();
// 			private readonly ServerStateComparer _serverStateComparer = new ServerStateComparer();
//
//             protected static readonly List<KeyValuePair<uint, uint>> Filter = new List<KeyValuePair<uint, uint>>(1)
//             {
//                 new KeyValuePair<uint, uint>(
//                     0,
//                     (uint)PredefinedConstants.Gameplay.IdsPerEntitySpace * PredefinedConstants.Gameplay.PredictionEntitySpace - 1
//                 )
//             };
//
//
//
//             protected ClientDesyncDetectorStrategy(DataStorage storage, int bufferSize)
//             {
//                 _bufferSize = bufferSize;
//                 _storage = storage;
//             }
//
//
//             protected abstract TServerState ReadStateFromStream(MemoryStream ms);
//             protected abstract TServerState FilterServerState(TServerState originalState);
//             protected abstract void SerializeLocalState(GameState localState, MemoryStream ms);
//             protected abstract void SerializeServerState(TServerState serverState, MemoryStream ms);
//
//             [Conditional(USE_COMPARISION_CALLBACKS)]
//             private void OnComparisionReady(GameState localState, TServerState serverState, bool result)
//             {
//                 if (result)
//                 {
//                     OnComparisionFailed(localState, serverState);
//                 }
//                 else
//                 {
//                     OnComparisionSuccess(localState, serverState);
//                 }
//             }
//
//             [Conditional(USE_COMPARISION_CALLBACKS)]
//             protected virtual void OnComparisionFailed(GameState localState, TServerState serverState)
//             {
//             }
//
//             [Conditional(USE_COMPARISION_CALLBACKS)]
//             protected virtual void OnComparisionSuccess(GameState localState, TServerState serverState)
//             {
//             }
//
//             public void RegisterServerMessage(MatchDesyncCheckDTO dto)
//             {
//                 if (_disposed)
//                 {
//                     return;
//                 }
//
//                 var serverTick = dto.ServerTick;
//                 var data = dto.Data;
//
//                 using (var ms = new MemoryStream(data.Array, data.Offset, data.Count, false, true))
//                 {
//                     lock (_lock)
//                     {
//                         if (!IsApproachSupported(ms))
//                         {
//                             return;
//                         }
//
//                         if (!_alive)
//                         {
//                             _debugStateSampler = new Sampler<TServerState>();
//                             _debugStateSampler.Init(_bufferSize, 0);
//                             _localStateWithoutPrediction = new EcsState();
//                             ServerStateWithoutPrediction = new TServerState();
//                             _desyncCheckerStreamHelpers = new[] { new MemoryStream(), new MemoryStream() };
//                             Writer = new StrictBitsPacker(new byte[CONVERSION_BUFFER_SIZE]);
//                         }
//
//                         var serverState = ReadStateFromStream(ms);
//                         _debugStateSampler.CreateSample(serverTick).Data = serverState;
//
//                         _alive = true;
//                     }
//                 }
//             }
//
//             protected virtual bool IsApproachSupported(MemoryStream ms) => true;
//
//             private void FetchServerStatesToOrderedList()
//             {
//                 lock (_lock)
//                 {
//                     _receivedFullStatesFromServer.Clear();
//                     var allSamples = _debugStateSampler.GetSamples();
//                     for (int i = 0; i < allSamples.Length; i++)
//                     {
// 	                    var s = allSamples[i];
// 	                    if (s.SimulationTick != 0)
// 	                    {
// 							_receivedFullStatesFromServer.Add((s.SimulationTick, s.Data));
// 	                    }
//                     }
// 					_receivedFullStatesFromServer.Sort(_serverStateComparer);
//                 }
//             }
//
//             private void FetchClientStatesToMap()
//             {
//                 lock (_storage.ReceivedGameStates)
//                 {
//                     _clientStatesMap.Clear();
//
//                     var maxTick = _storage.ReceivedGameStates.GetMaxTick();
//                     if (maxTick != 0)
//                     {
//                         _clientStatesMap[maxTick] = _storage.ReceivedGameStates.GetSample(maxTick).Data;
//                     }
//
//                     foreach (GameState s in _storage.World.StateSampler.GetBuffer)
//                     {
//                         if (s.SimTick != 0 && !_clientStatesMap.ContainsKey(s.SimTick))
//                         {
//                             _clientStatesMap[s.SimTick] = s;
//                         }
//                     }
//                 }
//             }
//
//
//             public bool CheckDesync()
//             {
//                 if (!_alive || _disposed)
//                 {
//                     return false;
//                 }
//
//                 lock (SafeDisposeLock)
//                 {
//                     FetchServerStatesToOrderedList();
//                     FetchClientStatesToMap();
//
//                     foreach (var serverStateWithTick in _receivedFullStatesFromServer)
//                     {
//                         if (_clientStatesMap.TryGetValue(serverStateWithTick.serverTick, out var stateToCompare))
//                         {
//                             _localStateWithoutPrediction.Clear();
//
//                             stateToCompare.FilterByMinMaxList(Filter, _localStateWithoutPrediction);
//                             ServerStateWithoutPrediction = FilterServerState(serverStateWithTick.serverState);
//
//                             _desyncCheckerStreamHelpers[0].Seek(0, SeekOrigin.Begin);
//                             _desyncCheckerStreamHelpers[1].Seek(0, SeekOrigin.Begin);
//
//                             _desyncCheckerStreamHelpers[0].SetLength(0);
//                             _desyncCheckerStreamHelpers[1].SetLength(0);
//
//                             SerializeLocalState(_localStateWithoutPrediction, _desyncCheckerStreamHelpers[0]);
//                             SerializeServerState(ServerStateWithoutPrediction, _desyncCheckerStreamHelpers[1]);
//
//                             var result = !CompareBuffers(_desyncCheckerStreamHelpers[0],
//                                 _desyncCheckerStreamHelpers[1]);
//
//                             OnComparisionReady(_localStateWithoutPrediction, ServerStateWithoutPrediction, result);
//
//                             lock (_lock)
//                             {
//                                 foreach (var sample in _debugStateSampler.GetSamples())
//                                 {
//                                     if (sample.SimulationTick <= serverStateWithTick.serverTick)
//                                     {
//                                         sample.SimulationTick = 0;
//                                     }
//                                 }
//                             }
//
//                             return result;
//                         }
//                     }
//
//                     return false;
//                 }
//             }
//
//             private unsafe bool CompareBuffers(MemoryStream s1, MemoryStream s2)
//             {
//                 if (s1.Length != s2.Length)
//                 {
//                     return false;
//                 }
//
//                 if (s1.Length == s2.Length && s1.Length == 0)
//                 {
//                     return true;
//                 }
//
//                 var buf1 = s1.GetBuffer();
//                 var buf2 = s2.GetBuffer();
//
//
//                 fixed (void* ptr1 = &buf1[0], ptr2 = &buf2[0])
//                 {
//                     return UnsafeUtility.MemCmp(ptr1, ptr2, s1.Length) == 0;
//                 }
//             }
//
//             public virtual void Dispose()
//             {
//                 lock (SafeDisposeLock)
//                 {
//                     _disposed = true;
//                     if (_alive)
//                     {
//                         foreach (var ms in _desyncCheckerStreamHelpers)
//                         {
//                             ms.Dispose();
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }