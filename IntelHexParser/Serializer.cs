using System;
using System.Linq;

namespace Coshx.IntelHexParser {
    public class Serializer {
        private const int START_CODE_OFFSET = 0;
        private const int START_CODE_LENGTH = 1;
        private const int BYTE_COUNT_OFFSET = START_CODE_OFFSET + START_CODE_LENGTH;
        private const int BYTE_COUNT_LENGTH = 2;
        private const int ADDRESS_OFFSET = BYTE_COUNT_OFFSET + BYTE_COUNT_LENGTH;
        private const int ADDRESS_LENGTH = 4;
        private const int RECORD_TYPE_OFFSET = ADDRESS_OFFSET + ADDRESS_LENGTH;
        private const int RECORD_TYPE_LENGTH = 2;
        private const int DATA_OFFSET = RECORD_TYPE_OFFSET + RECORD_TYPE_LENGTH;
        private const int CHECKSUM_LENGTH = 2; 
        
        private int MinimumLineLength {
            get {
                return START_CODE_LENGTH + BYTE_COUNT_LENGTH + ADDRESS_LENGTH + RECORD_TYPE_LENGTH + CHECKSUM_LENGTH;
            }
        }
        
        private bool IsChecksumValid(String line, Record currentRecord) {
            byte current = Convert.ToByte(line.Substring(DATA_OFFSET + currentRecord.DataLength * 2, CHECKSUM_LENGTH), 16);
            
            return currentRecord.Checksum == current;
        }
        
        public String Serialize(byte[] source) {
            int i = 0, currentRecordIndex;
            Record[] records;
            String outcome;
            Record currentRecord;
            
            if (source.Length == 0) {
                return "";
            }

            records = new Record[source.Length / 255 + 2];
            currentRecord = null;
            currentRecordIndex = 0;
            
            for (i = 0; i < source.Length; i++) {
                if (i % 255 == 0) {
                    if (currentRecord != null) {
                        records[currentRecordIndex++] = currentRecord;
                    }
                    currentRecord = new Record();
                    currentRecord.Type = 0;
                    currentRecord.Data = new byte[255];
                    currentRecord.Address = (ushort) ((i / 255) * (256 + 12));
                    currentRecord.DataLength = 0;
                }
                
                currentRecord.Data[i % 255] = source[i];
                currentRecord.DataLength++;
            }
            
            if (currentRecord.DataLength > 0 && i % 255 != 0) {
                records[currentRecordIndex] = currentRecord;
            }

            var lastRecord = new Record();
            lastRecord.Type = 1;
            lastRecord.Address = 0;
            lastRecord.DataLength = 0;
            records[records.Length - 1] = lastRecord;
            
            outcome = "";
            i = 0;
            foreach (Record r in records) {
                outcome += r.ToString();
                if (i < records.Length - 1) {
                    outcome += Environment.NewLine;
                }
                i++;
            }
            
            return outcome;
        }
        
        public byte[] Deserialize(String source) {
            String[] lines = source.Split(Environment.NewLine.ToCharArray());
            lines = lines.Where(line => line.Length > 0).ToArray();
            Record[] records = new Record[lines.Length];
            byte[] outcome;
            int recordIndex = 0, finalDataSize = 0, outcomeIndex;
            
            foreach (String l in lines)
            {
                if (l.Length < MinimumLineLength)
                {
                    throw new IntelHexParserException(IntelHexParserException.Kind.INVALID_LINE);
                }

                Record current = new Record();                
                                
                if (l[0] != ':') {
                    throw new IntelHexParserException(IntelHexParserException.Kind.MISSING_START_CODE);
                }
                
                current.DataLength = Convert.ToInt16(l.Substring(BYTE_COUNT_OFFSET, BYTE_COUNT_LENGTH), 16);
                current.Address = Convert.ToUInt16(l.Substring(ADDRESS_OFFSET, ADDRESS_LENGTH), 16);
                current.Type = Convert.ToInt16(l.Substring(RECORD_TYPE_OFFSET, RECORD_TYPE_LENGTH), 16);
                
                if (current.Type != 0 && current.Type != 1) {
                    throw new IntelHexParserException(IntelHexParserException.Kind.UNSUPPORTED_RECORD_TYPE);
                }
                
                current.Data = new byte[current.DataLength];
                
                for (int i = 0; i < current.DataLength; i++) {
                    current.Data[i] = Convert.ToByte(l.Substring(DATA_OFFSET + 2 * i, 2), 16);
                }
                
                if (!IsChecksumValid(l, current)) {
                    throw new IntelHexParserException(IntelHexParserException.Kind.INVALID_CHECKSUM);
                }
                
                records[recordIndex++] = current;
                finalDataSize += current.DataLength;
            }
            
            outcome = new byte[finalDataSize];
            outcomeIndex = 0;
            for (int i = 0; i < records.Length; i++) {
                Record r = records[i];
                for (int j = 0; j < r.DataLength; j++) {
                    outcome[outcomeIndex++] = r.Data[j];                 
                }
            }
            
            return outcome;
        }
    }
}