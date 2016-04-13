namespace IntelHexParser.Coshx.Com {
    internal class Record {        
        internal int Type { get; set; }
        internal UInt16 DataLength { get; set; }
        internal UInt16 Address { get; set; }
        internal byte[] Data { get; set; }
        
        internal byte Checksum {
            get {
                byte checksum;
                
                checksum = (byte) DataLength;
                checksum += (byte) Type;
                checksum += (byte) Address;
                checksum += (byte) ((Address & 0xFF00) >> 8);
                
                for (int i = 0; i < DataLength; i++) {
                    checksum += Data[i];
                }
                
                checksum = (byte) (~checksum + 1);
                
                return checksum;
            }
        }
    } 
}