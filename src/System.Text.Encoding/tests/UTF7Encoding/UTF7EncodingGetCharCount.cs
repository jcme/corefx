// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace System.Text.Tests
{
    public class UTF7EncodingGetCharCount
    {
        // PosTest1: Verify method GetCharCount with a non-null Byte[]
        [Fact]
        public void PosTest1()
        {
            Byte[] bytes = new Byte[] {
                                         85,  84,  70,  56,  32,  69, 110,
                                         99, 111, 100, 105, 110, 103,  32,
                                         69, 120,  97, 109, 112, 108, 101};
            UTF7Encoding UTF7 = new UTF7Encoding();
            int charCount = UTF7.GetCharCount(bytes, 2, 8);
            Assert.Equal(8, charCount);
        }

        // PosTest2: Verify method GetCharCount with a null Byte[]
        [Fact]
        public void PosTest2()
        {
            Byte[] bytes = new Byte[] { };
            UTF7Encoding UTF7 = new UTF7Encoding();
            int charCount = UTF7.GetCharCount(bytes, 0, 0);
            Assert.Equal(0, charCount);
        }
    }
}
