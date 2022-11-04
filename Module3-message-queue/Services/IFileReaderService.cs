using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ModelObjects;

namespace Services
{
    public interface IFileReaderService
    {
        Task ReadFileByPortionAsync(int bufferSize, string filePath, Func<FilePortionModel, Task> portionHandler);
        void WriteFileByPortion(int bufferSize, string folderPath, FilePortionModel fileModel);
    }
}
