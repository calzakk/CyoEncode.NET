// TestBase85.cs - part of the CyoEncode.NET library
//
// MIT License
//
// Copyright(c) 2017-2024 Graham Bull
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CyoEncode;
using FluentAssertions;
using Xunit;

namespace UnitTests;

public class TestBase85
{
    private readonly IBase85 _base85 = new Base85();

    //test taken from: https://en.wikipedia.org/wiki/Ascii85
    private const string Original = "Man is distinguished, not only by his reason, but by this singular passion from other "
                                    + "animals, which is a lust of the mind, that by a perseverance of delight in the continued and "
                                    + "indefatigable generation of knowledge, exceeds the short vehemence of any carnal pleasure.";

    private const string Encoding = "9jqo^BlbD-BleB1DJ+*+F(f,q/0JhKF<GL>Cj@.4Gp$d7F!,L7@<6@)/0JDEF<G%<+EV:2F!,O<DJ+"
                                    + "*.@<*K0@<6L(Df-\\0Ec5e;DffZ(EZee.Bl.9pF\"AGXBPCsi+DGm>@3BB/F*&OCAfu2/AKYi(DIb:@FD,*)+C]U"
                                    + "=@3BN#EcYf8ATD3s@q?d$AftVqCh[NqF<G:8+EV:.+Cf>-FD5W8ARlolDIal(DId<j@<?3r@:F%a+D58'ATD4$B"
                                    + "l@l3De:,-DJs`8ARoFb/0JMK@qB4^F!,R<AKZ&-DfTqBG%G>uD.RTpAKYo'+CT/5+Cei#DII?(E,9)oF*2M7/c";

    [Fact]
    public void TestVectors_should_encode_successfully()
    {
        var encoded = _base85.Encode(System.Text.Encoding.ASCII.GetBytes(Original));
        encoded.Should().Be(Encoding);
    }

    [Fact]
    public void TestVectors_should_decode_successfully()
    {
        var decoded = System.Text.Encoding.ASCII.GetString(_base85.Decode(Encoding));
        decoded.Should().Be(Original);
    }

    [Fact]
    public async Task TestVectors_should_encode_successfully_using_streams()
    {
        using var input = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(Original));
        using var output = new MemoryStream();

        await _base85.EncodeStreamAsync(input, output);

        output.Flush();
        var outputText = System.Text.Encoding.ASCII.GetString(output.ToArray());
        outputText.Should().Be(Encoding);
    }

    [Fact]
    public async Task TestVectors_should_decode_successfully_using_streams()
    {
        using var input = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(Encoding));
        using var output = new MemoryStream();

        await _base85.DecodeStreamAsync(input, output);

        output.Flush();
        var outputText = System.Text.Encoding.ASCII.GetString(output.ToArray());
        outputText.Should().Be(Original);
    }

    [Fact]
    public void Encode_should_fold_zeros_when_FoldZero_is_enabled()
    {
        _base85.FoldZero = true;

        var encoded = _base85.Encode(new byte[] { 0, 0, 0, 0 });
        encoded.Should().Be("z");
    }

    [Fact]
    public void Encode_should_not_fold_zeros_when_FoldZero_is_disabled()
    {
        _base85.FoldZero = false;

        var encoded = _base85.Encode(new byte[] { 0, 0, 0, 0 });
        encoded.Should().Be("!!!!!");
    }

    [Fact]
    public void Decode_should_unfold_zeros_when_FoldZero_is_enabled()
    {
        _base85.FoldZero = true;

        var decoded = _base85.Decode("z");
        decoded.Should().BeEquivalentTo(new byte[] { 0, 0, 0, 0 });
    }

    [Fact]
    public void Decode_should_unfold_zeros_for_five_exclamations_when_FoldZero_is_enabled()
    {
        _base85.FoldZero = true;

        var decoded = _base85.Decode("!!!!!");
        decoded.Should().BeEquivalentTo(new byte[] { 0, 0, 0, 0 });
    }

    [Fact]
    public void Decode_should_throw_exception_when_FoldZero_is_disabled()
    {
        _base85.FoldZero = false;

        Action action = () => _base85.Decode("z");
        action.Should().Throw<BadCharacterException>().WithMessage("Bad character at offset 0");
    }
}