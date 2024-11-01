// 有一个超大文件，无法一次性加载到内存，如何排序？
// 1、文件分割：
//  将大文件分割成多个小文件，每个小文件的大小应适合内存容量，以便可以一次性加载到内存中进行排序。
//  分割时可以使用文件读写操作，每次读取一定大小的数据块，将其写入一个小文件中。
//  2.内部排序：
//  对每个小文件进行内部排序。可以使用任何高效的内部排序算法，如快速排序（Quick Sort）、归并排序（Merge Sort）等。
//  排序后的每个小文件都是有序的，可以将其写回到磁盘上。
//  3.多路归并：
//  将所有已排序的小文件合并成一个有序的大文件。这是通过多路归并算法实现的。
//  多路归并的基本思想是从每个已排序的小文件中读取一部分数据（例如，每个文件读取1MB的数据），将这些数据放入一个优先队列（最小堆）中。
//  从优先队列中取出最小的元素，将其写入最终的输出文件中，然后从对应的文件中读取下一个元素放入优先队列中。
//  重复上述过程，直到所有小文件中的数据都被处理完毕。

// 文件分割
#include <iostream>
#include <fstream>
#include <vector>
#include <string>

using namespace std;

void split_file(const string &input_file, const string &output_dir, size_t chunk_size)
{
    ifstream infile(input_file);
    if (!infile)
    {
        cerr << "Error opening input file" << endl;
        return;
    }
    size_t chunk_num = 0;
    string line;
    vector<string> chunk;

    while (getline(infile, line))
    {
        chunk.push_back(line);
        if (chunk.size() * line.size() >= chunk_size)
        {
            ofstream outfile(output_dir + "/chunk_" + to_string(chunk_num) + ".txt");
            for (const auto &l : chunk)
            {
                outfile << l << "\n";
            }
            chunk.clear();
            ++chunk_num;
        }
    }

    if (!chunk.empty())
    {
        ofstream outfile(output_dir + "/chunk_" + to_string(chunk_num) + ".txt");
        for (const auto &l : chunk)
        {
            outfile << l << "\n";
        }
    }
}

// 内部排序
#include <algorithm>

void sort_chunk(const string &input_file, const string &output_file)
{
    ifstream infile(input_file);
    if (!infile)
    {
        cerr << "Error opening input file" << endl;
        return;
    }
    vector<long long> numbers;
    string line;
    while (getline(infile, line))
    {
        numbers.push_back(stoll(line));
    }
    sort(numbers.begin(), numbers.end());
    ofstream outfile(output_file);
    for (const auto &num : numbers)
    {
        outfile << num << "\n";
    }
}

void sort_chunks(const string &output_dir, const vector<string> &chunk_files)
{
    for (const auto &file : chunk_files)
    {
        sort_chunk(file, output_dir);
    }
}

// 多路归并
#include <queue>
#include <memory>
#include <functional>
using namespace std;

struct FileIterator
{
    ifstream *file;
    string current_line;
    bool has_next;

    FileIterator(ifstream *file) : file(file), has_next(true)
    {
        read_next();
    }
    void read_next()
    {
        if (getline(*file, current_line))
        {
            current_line = to_string(stoll(current_line));
        }
        else
        {
            has_next = false;
        }
    }
};
struct Compare
{
    bool operator()(const shared_ptr<FileIterator> &a, const shared_ptr<FileIterator> &b)
    {
        return a->current_line > b->current_line;
    }
};

void merge_sorted_files(const vector<string> &chunk_files, const string &output_file)
{
    priority_queue<shared_ptr<FileIterator>, vector<shared_ptr<FileIterator>>, Compare> min_heap;
    vector<unique_ptr<ifstream>> files;

    for (const auto &file : chunk_files)
    {
        files.emplace_back(new ifstream(file));
        if (files.back()->is_open())
        {
            min_heap.push(make_shared<FileIterator>(files.back().get()));
        }
    }

    ofstream outfile(output_file);
    while (!min_heap.empty())
    {
        auto top = min_heap.top();
        min_heap.pop();

        outfile << top->current_line << "\n";
        top->read_next();
        if (top->has_next)
        {
            min_heap.push(top);
        }
    }
}
#include <filesystem>
int main()
{
    const string input_file = "large_file.txt";
    const string output_dir = "chucks";
    const string output_file = "sorted_file.txt";
    const size_t chunk_size = 1024 * 1024 * 100;
    std::filesystem::create_directory(output_dir);

    split_file(input_file,output_dir,chunk_size);

    vector<string> chunk_files;
    for (const auto &entry : filesystem::directory_iterator(output_dir))
    {
        if (entry.is_regular_file())
        {
            chunk_files.push_back(entry.path().string());
        }
    }

    sort_chunks(output_dir, chunk_files);

    merge_sorted_files(chunk_files, output_file);

    cout << "排序完成" << output_file << endl;
    return 0;
}