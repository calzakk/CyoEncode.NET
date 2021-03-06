using System.IO;
using System.Threading.Tasks;

namespace CyoEncode
{
    public interface IEncoder
    {
        void Encode(Stream input, Stream output);

        void Decode(Stream input, Stream output);

        Task EncodeAsync(Stream input, Stream output);

        Task DecodeAsync(Stream input, Stream output);

        string Encode(byte[] input);

        byte[] Decode(string input);
    }
}
