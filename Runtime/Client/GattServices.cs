using System;

namespace Psix
{
    public struct Subscription
    {
        public string service;
        public string characteristic;
        public Action<byte[]> callback;
        public Subscription(string _service, string _characteristic, Action<byte[]> _callback)
        {
            service = _service;
            characteristic = _characteristic;
            callback = _callback;
        }
    }
    public static class GattServices
    {
        public static string ProtobufServiceUUID = "f9d60370-5325-4c64-b874-a68c7c555bad";
        public static string ProtobufOutputUUID = "f9d60371-5325-4c64-b874-a68c7c555bad";
        public static string ProtobufInputUUID = "f9d60372-5325-4c64-b874-a68c7c555bad";

        public static string InteractionServiceUUID = "008e74d0-7bb3-4ac5-8baf-e5e372cced76";

        public static string[] RelevantServiceUuids = new[] { ProtobufServiceUUID };

    }
}
