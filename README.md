# CyoEncode

__Latest stable version__ : [![NuGet](https://img.shields.io/nuget/v/CyoEncode.svg)](https://www.nuget.org/packages/CyoEncode/)

Provides classes for encoding binary data into a printable representation using Base64, Base32, Base16 (hex), or Base85/Ascii85 character sets, plus equivalent functions for the decoding of such encoded data back into its binary form.

Features:
- Targets .NET Standard 2.1;
- Works with .NET Core 3.1, .NET 5, or .NET 6;
- Available as a [NuGet package](https://www.nuget.org/packages/CyoEncode).

## Supported encoders

- Base85/Ascii85
- Base64
- Base32
- Base16/Hexadecimal

## Usage

CyoEncode is simple to use:

Encoding takes an array of bytes, and outputs an encoded string:

```csharp
var encoder = new CyoEncode.Base85();

byte[] bytes = ...
string encoded = encoder.Encode(bytes);
```

To use another encoder, simply instantiate the relevant class; for example:

```csharp
var encoder = new CyoEncode.Base32();
```

Decoding takes the encoded string, and outputs an array of decoded bytes:

```csharp
var decoder = new CyoEncode.Base85();

string encoded = ...;
byte[] decoded = decoder.Decode(encoded);
```

### Streams

CyoEncode can asynchronously encode and decode streams:

```csharp
var encoder = new CyoEncode.Base64();

byte[] bytes = ...
using var input = new MemoryStream(bytes);

using var output = new MemoryStream();

await encoder.EncodeStreamAsync(input, output);

string encoded = Encoding.ASCII.GetString(output.ToArray());
```

Decode:

```csharp
var decoder = new CyoEncode.Base64();

string encoded = ...
using var input = new MemoryStream(Encoding.ASCII.GetBytes(encoded));

using var output = new MemoryStream();

await decoder.DecodeStreamAsync(input, output);

byte[] decoded = output.ToArray();
```

Even files can be encoded:

```csharp
using var input = new FileStream("...", FileMode.Open);

using var output = new FileStream("...", FileMode.Create);

await encoder.EncodeStreamAsync(input, output);
```

and decoded:

```csharp
using var input = new FileStream("...", FileMode.Open);

using var output = new FileStream("...", FileMode.Create);

await encoder.DecodeStreamAsync(input, output);
```

Of course, assume the source file cannot be trusted! Ensure its size is reasonable before decoding.

## License

### The MIT License (MIT)

Copyright (c) 2017-2021 Graham Bull

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
