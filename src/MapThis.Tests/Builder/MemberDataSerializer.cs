using Newtonsoft.Json;
using Xunit.Abstractions;

namespace MapThis.Tests.Builder
{
    public class MemberDataSerializer<T> : IXunitSerializable
    {
        public T Object { get; private set; }

        // required for deserializer
        public MemberDataSerializer()
        {
        }

        public MemberDataSerializer(T objectToSerialize)
        {
            Object = objectToSerialize;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Object = JsonConvert.DeserializeObject<T>(info.GetValue<string>("objValue"));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            var json = JsonConvert.SerializeObject(Object);
            info.AddValue("objValue", json);
        }

        public override string ToString()
        {
            return $"Serializer";
        }
    }
}
