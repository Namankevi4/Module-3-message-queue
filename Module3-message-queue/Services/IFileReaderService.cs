using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ModelObjects;

namespace Services
{
    public interface IFileReaderService
    {
        IEnumerable<FilePortionModel> ReadFileByPortion(int bufferSize, string filePath);
        void WriteFileByPortion(int bufferSize, string folderPath, FilePortionModel fileModel);
    }
}
