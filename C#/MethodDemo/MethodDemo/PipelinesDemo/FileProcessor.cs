using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelinesDemo
{
    public class FileProcessor
    {
        public async Task ProcessFileAsync(string filePath)
        {
            await using var fileStream = File.OpenRead(filePath);
            var pipe = new Pipe();

            var writing = FillPipeAsync(fileStream, pipe.Writer);
            var reading = ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writing);
            
        }
        public async Task FillPipeAsync(FileStream fileStream,PipeWriter writer)
        {
            const int minimumBufferSize = 1024;
            try
            {
                while (true)
                {
                    Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                    try
                    {
                        int bytesRead = await fileStream.ReadAsync(memory);
                        if (bytesRead == 0)
                            break;
                        //表示数据已经写入,读取端感知新数据的存在。
                        writer.Advance(bytesRead);
                    }
                    catch(Exception ex)
                    {
                        break;
                    }
                    //FlushAsync负责将数据从管道的写入端推送到读取端。
                    var result = await writer.FlushAsync();
                    if (result.IsCompleted)
                        break;
                }
            }
            finally
            {
                //当写入完成后，表示不再写入新数据，此时管道会通知读取端处理剩余数据并结束。
                writer.Complete();
            }
        }
        public async Task ReadPipeAsync(PipeReader reader)
        {
            try
            {
                while(true)
                {
                    ReadResult result = await reader.ReadAsync();
                    ReadOnlySequence<byte> buffer = result.Buffer;

                    //ProcessBuffer

                    reader.AdvanceTo(buffer.End);

                    if (result.IsCompleted)
                        break;
                }
            }
            finally
            {
                reader.Complete();
            }
        }
    }
}
