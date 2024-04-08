#nullable enable

using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Psix {

    public class MessageDispatcher : MonoBehaviour
    {

        private ConcurrentQueue<byte[]> messageQueue = new ConcurrentQueue<byte[]>();

        public Action<byte[]>? OnMessage = null;
        public Action? OnDisconnect = null;

        private bool shouldDisconnect = false;

        void Update() {
            byte[] output;
            while (messageQueue.TryDequeue(out output)) {
                OnMessage?.Invoke(output);
            }

            if (shouldDisconnect) {
                OnDisconnect?.Invoke();
                lock(this) shouldDisconnect = false;
            }
        }

        public void Dispatch(byte[] message) {
            messageQueue.Enqueue(message);
        }

        public void Disconnect() {
            lock(this) shouldDisconnect = true;
        }

    }
}