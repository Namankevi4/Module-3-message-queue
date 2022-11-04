using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ModelObjects;

namespace Services
{
    public class FileReaderService : IFileReaderService
    {
        private ConcurrentDictionary<string, object> _fileAlreadyInWritingState = new ConcurrentDictionary<string, object>();

        public IEnumerable<FilePortionModel> ReadFileByPortion(int bufferSize, string filePath)
        {
            Guid fileId = Guid.NewGuid();
            string fileName = Path.GetFileName(filePath);
            int sequenceNumber = 0;

             using (FileStream fs = File.OpenRead(filePath))
            {
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        var fileData = binaryReader.ReadBytes(bufferSize);
                        var filePortionModel = new FilePortionModel()
                        {
                            FileId = fileId,
                            FileName = fileName,
                            SequenceNumber = sequenceNumber++,
                            Body = fileData,
                            FileSize = binaryReader.BaseStream.Length,
                            BufferSize = bufferSize
                        };

                        yield return filePortionModel;
                    }
                }
            }
        }

        public void WriteFileByPortion(int bufferSize, string folderPath, FilePortionModel fileModel)
        {
            if(!_fileAlreadyInWritingState.TryGetValue(fileModel.FileName, out _))
            {
                _fileAlreadyInWritingState.TryAdd(fileModel.FileName, 0);
            }

            lock (_fileAlreadyInWritingState[fileModel.FileName])
            {
                var filepath = Path.Combine(folderPath, fileModel.FileName);

                using (var fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    fs.SetLength(fileModel.FileSize);

                    long offset = fileModel.SequenceNumber * bufferSize;
                    fs.Seek(offset, SeekOrigin.Begin);
                    fs.Write(fileModel.Body, 0, fileModel.Body.Length);
                }
            }
        }
    }
}
