// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: watch_input.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Psix.Proto {

  /// <summary>Holder for reflection information generated from watch_input.proto</summary>
  public static partial class WatchInputReflection {

    #region Descriptor
    /// <summary>File descriptor for watch_input.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static WatchInputReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChF3YXRjaF9pbnB1dC5wcm90bxoMY29tbW9uLnByb3RvIn4KC0hhcHRpY0V2",
            "ZW50EiUKBHR5cGUYASABKA4yFy5IYXB0aWNFdmVudC5IYXB0aWNUeXBlEhEK",
            "CWludGVuc2l0eRgCIAEoAhIOCgZsZW5ndGgYAyABKAUiJQoKSGFwdGljVHlw",
            "ZRIKCgZDQU5DRUwQABILCgdPTkVTSE9UEAEiTAoKQ2xpZW50SW5mbxIPCgdh",
            "cHBOYW1lGAEgASgJEhIKCmRldmljZU5hbWUYAiABKAkSDQoFdGl0bGUYAyAB",
            "KAkSCgoCb3MYBCABKAkibwoLSW5wdXRVcGRhdGUSIQoLaGFwdGljRXZlbnQY",
            "ASABKAsyDC5IYXB0aWNFdmVudBIfCgpjbGllbnRJbmZvGAIgASgLMgsuQ2xp",
            "ZW50SW5mbxIcCgxtb2RlbFJlcXVlc3QYAyABKAsyBi5Nb2RlbEINqgIKUHNp",
            "eC5Qcm90b2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Psix.Proto.CommonReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Psix.Proto.HapticEvent), global::Psix.Proto.HapticEvent.Parser, new[]{ "Type", "Intensity", "Length" }, null, new[]{ typeof(global::Psix.Proto.HapticEvent.Types.HapticType) }, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Psix.Proto.ClientInfo), global::Psix.Proto.ClientInfo.Parser, new[]{ "AppName", "DeviceName", "Title", "Os" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Psix.Proto.InputUpdate), global::Psix.Proto.InputUpdate.Parser, new[]{ "HapticEvent", "ClientInfo", "ModelRequest" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// Haptic event to be triggered on the peripheral device.
  /// </summary>
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class HapticEvent : pb::IMessage<HapticEvent>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<HapticEvent> _parser = new pb::MessageParser<HapticEvent>(() => new HapticEvent());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<HapticEvent> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Psix.Proto.WatchInputReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public HapticEvent() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public HapticEvent(HapticEvent other) : this() {
      type_ = other.type_;
      intensity_ = other.intensity_;
      length_ = other.length_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public HapticEvent Clone() {
      return new HapticEvent(this);
    }

    /// <summary>Field number for the "type" field.</summary>
    public const int TypeFieldNumber = 1;
    private global::Psix.Proto.HapticEvent.Types.HapticType type_ = global::Psix.Proto.HapticEvent.Types.HapticType.Cancel;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Psix.Proto.HapticEvent.Types.HapticType Type {
      get { return type_; }
      set {
        type_ = value;
      }
    }

    /// <summary>Field number for the "intensity" field.</summary>
    public const int IntensityFieldNumber = 2;
    private float intensity_;
    /// <summary>
    /// Intensity of effect, between 0 and 1.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Intensity {
      get { return intensity_; }
      set {
        intensity_ = value;
      }
    }

    /// <summary>Field number for the "length" field.</summary>
    public const int LengthFieldNumber = 3;
    private int length_;
    /// <summary>
    /// Duration of effect in milliseconds. Peripheral may impose a maximum duration.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Length {
      get { return length_; }
      set {
        length_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as HapticEvent);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(HapticEvent other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Type != other.Type) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Intensity, other.Intensity)) return false;
      if (Length != other.Length) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Type != global::Psix.Proto.HapticEvent.Types.HapticType.Cancel) hash ^= Type.GetHashCode();
      if (Intensity != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Intensity);
      if (Length != 0) hash ^= Length.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Type != global::Psix.Proto.HapticEvent.Types.HapticType.Cancel) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Type);
      }
      if (Intensity != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Intensity);
      }
      if (Length != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Length);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Type != global::Psix.Proto.HapticEvent.Types.HapticType.Cancel) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Type);
      }
      if (Intensity != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Intensity);
      }
      if (Length != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Length);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (Type != global::Psix.Proto.HapticEvent.Types.HapticType.Cancel) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Type);
      }
      if (Intensity != 0F) {
        size += 1 + 4;
      }
      if (Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Length);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(HapticEvent other) {
      if (other == null) {
        return;
      }
      if (other.Type != global::Psix.Proto.HapticEvent.Types.HapticType.Cancel) {
        Type = other.Type;
      }
      if (other.Intensity != 0F) {
        Intensity = other.Intensity;
      }
      if (other.Length != 0) {
        Length = other.Length;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Type = (global::Psix.Proto.HapticEvent.Types.HapticType) input.ReadEnum();
            break;
          }
          case 21: {
            Intensity = input.ReadFloat();
            break;
          }
          case 24: {
            Length = input.ReadInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            Type = (global::Psix.Proto.HapticEvent.Types.HapticType) input.ReadEnum();
            break;
          }
          case 21: {
            Intensity = input.ReadFloat();
            break;
          }
          case 24: {
            Length = input.ReadInt32();
            break;
          }
        }
      }
    }
    #endif

    #region Nested types
    /// <summary>Container for nested types declared in the HapticEvent message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Types {
      /// <summary>
      /// Type of the haptic event.
      /// </summary>
      public enum HapticType {
        /// <summary>
        /// Cancel any ongoing haptic feedback.
        /// </summary>
        [pbr::OriginalName("CANCEL")] Cancel = 0,
        /// <summary>
        /// Trigger a one-shot haptic feedback.
        /// </summary>
        [pbr::OriginalName("ONESHOT")] Oneshot = 1,
      }

    }
    #endregion

  }

  /// <summary>
  /// Information about the client application.
  /// </summary>
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class ClientInfo : pb::IMessage<ClientInfo>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ClientInfo> _parser = new pb::MessageParser<ClientInfo>(() => new ClientInfo());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ClientInfo> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Psix.Proto.WatchInputReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ClientInfo() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ClientInfo(ClientInfo other) : this() {
      appName_ = other.appName_;
      deviceName_ = other.deviceName_;
      title_ = other.title_;
      os_ = other.os_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ClientInfo Clone() {
      return new ClientInfo(this);
    }

    /// <summary>Field number for the "appName" field.</summary>
    public const int AppNameFieldNumber = 1;
    private string appName_ = "";
    /// <summary>
    /// User-readable name of the application.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string AppName {
      get { return appName_; }
      set {
        appName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "deviceName" field.</summary>
    public const int DeviceNameFieldNumber = 2;
    private string deviceName_ = "";
    /// <summary>
    /// User-readable name of the device running the application.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string DeviceName {
      get { return deviceName_; }
      set {
        deviceName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "title" field.</summary>
    public const int TitleFieldNumber = 3;
    private string title_ = "";
    /// <summary>
    /// Not used.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Title {
      get { return title_; }
      set {
        title_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "os" field.</summary>
    public const int OsFieldNumber = 4;
    private string os_ = "";
    /// <summary>
    /// Operating system of the device running the application.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Os {
      get { return os_; }
      set {
        os_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ClientInfo);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ClientInfo other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (AppName != other.AppName) return false;
      if (DeviceName != other.DeviceName) return false;
      if (Title != other.Title) return false;
      if (Os != other.Os) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (AppName.Length != 0) hash ^= AppName.GetHashCode();
      if (DeviceName.Length != 0) hash ^= DeviceName.GetHashCode();
      if (Title.Length != 0) hash ^= Title.GetHashCode();
      if (Os.Length != 0) hash ^= Os.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (AppName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(AppName);
      }
      if (DeviceName.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(DeviceName);
      }
      if (Title.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Title);
      }
      if (Os.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(Os);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (AppName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(AppName);
      }
      if (DeviceName.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(DeviceName);
      }
      if (Title.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Title);
      }
      if (Os.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(Os);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (AppName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(AppName);
      }
      if (DeviceName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(DeviceName);
      }
      if (Title.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Title);
      }
      if (Os.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Os);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ClientInfo other) {
      if (other == null) {
        return;
      }
      if (other.AppName.Length != 0) {
        AppName = other.AppName;
      }
      if (other.DeviceName.Length != 0) {
        DeviceName = other.DeviceName;
      }
      if (other.Title.Length != 0) {
        Title = other.Title;
      }
      if (other.Os.Length != 0) {
        Os = other.Os;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            AppName = input.ReadString();
            break;
          }
          case 18: {
            DeviceName = input.ReadString();
            break;
          }
          case 26: {
            Title = input.ReadString();
            break;
          }
          case 34: {
            Os = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            AppName = input.ReadString();
            break;
          }
          case 18: {
            DeviceName = input.ReadString();
            break;
          }
          case 26: {
            Title = input.ReadString();
            break;
          }
          case 34: {
            Os = input.ReadString();
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  /// Message type sent by the client (application) to the server (peripheral device)
  /// over the "input" GATT characteristic by a GATT write operation.
  /// </summary>
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class InputUpdate : pb::IMessage<InputUpdate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<InputUpdate> _parser = new pb::MessageParser<InputUpdate>(() => new InputUpdate());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<InputUpdate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Psix.Proto.WatchInputReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public InputUpdate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public InputUpdate(InputUpdate other) : this() {
      hapticEvent_ = other.hapticEvent_ != null ? other.hapticEvent_.Clone() : null;
      clientInfo_ = other.clientInfo_ != null ? other.clientInfo_.Clone() : null;
      modelRequest_ = other.modelRequest_ != null ? other.modelRequest_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public InputUpdate Clone() {
      return new InputUpdate(this);
    }

    /// <summary>Field number for the "hapticEvent" field.</summary>
    public const int HapticEventFieldNumber = 1;
    private global::Psix.Proto.HapticEvent hapticEvent_;
    /// <summary>
    /// Application wants to trigger haptic feedback on the peripheral device.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Psix.Proto.HapticEvent HapticEvent {
      get { return hapticEvent_; }
      set {
        hapticEvent_ = value;
      }
    }

    /// <summary>Field number for the "clientInfo" field.</summary>
    public const int ClientInfoFieldNumber = 2;
    private global::Psix.Proto.ClientInfo clientInfo_;
    /// <summary>
    /// Information about the client application.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Psix.Proto.ClientInfo ClientInfo {
      get { return clientInfo_; }
      set {
        clientInfo_ = value;
      }
    }

    /// <summary>Field number for the "modelRequest" field.</summary>
    public const int ModelRequestFieldNumber = 3;
    private global::Psix.Proto.Model modelRequest_;
    /// <summary>
    /// Application requests a gesture detection model to be activated on the peripheral device.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Psix.Proto.Model ModelRequest {
      get { return modelRequest_; }
      set {
        modelRequest_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as InputUpdate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(InputUpdate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(HapticEvent, other.HapticEvent)) return false;
      if (!object.Equals(ClientInfo, other.ClientInfo)) return false;
      if (!object.Equals(ModelRequest, other.ModelRequest)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (hapticEvent_ != null) hash ^= HapticEvent.GetHashCode();
      if (clientInfo_ != null) hash ^= ClientInfo.GetHashCode();
      if (modelRequest_ != null) hash ^= ModelRequest.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (hapticEvent_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(HapticEvent);
      }
      if (clientInfo_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(ClientInfo);
      }
      if (modelRequest_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(ModelRequest);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (hapticEvent_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(HapticEvent);
      }
      if (clientInfo_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(ClientInfo);
      }
      if (modelRequest_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(ModelRequest);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (hapticEvent_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(HapticEvent);
      }
      if (clientInfo_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ClientInfo);
      }
      if (modelRequest_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ModelRequest);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(InputUpdate other) {
      if (other == null) {
        return;
      }
      if (other.hapticEvent_ != null) {
        if (hapticEvent_ == null) {
          HapticEvent = new global::Psix.Proto.HapticEvent();
        }
        HapticEvent.MergeFrom(other.HapticEvent);
      }
      if (other.clientInfo_ != null) {
        if (clientInfo_ == null) {
          ClientInfo = new global::Psix.Proto.ClientInfo();
        }
        ClientInfo.MergeFrom(other.ClientInfo);
      }
      if (other.modelRequest_ != null) {
        if (modelRequest_ == null) {
          ModelRequest = new global::Psix.Proto.Model();
        }
        ModelRequest.MergeFrom(other.ModelRequest);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (hapticEvent_ == null) {
              HapticEvent = new global::Psix.Proto.HapticEvent();
            }
            input.ReadMessage(HapticEvent);
            break;
          }
          case 18: {
            if (clientInfo_ == null) {
              ClientInfo = new global::Psix.Proto.ClientInfo();
            }
            input.ReadMessage(ClientInfo);
            break;
          }
          case 26: {
            if (modelRequest_ == null) {
              ModelRequest = new global::Psix.Proto.Model();
            }
            input.ReadMessage(ModelRequest);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (hapticEvent_ == null) {
              HapticEvent = new global::Psix.Proto.HapticEvent();
            }
            input.ReadMessage(HapticEvent);
            break;
          }
          case 18: {
            if (clientInfo_ == null) {
              ClientInfo = new global::Psix.Proto.ClientInfo();
            }
            input.ReadMessage(ClientInfo);
            break;
          }
          case 26: {
            if (modelRequest_ == null) {
              ModelRequest = new global::Psix.Proto.Model();
            }
            input.ReadMessage(ModelRequest);
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
