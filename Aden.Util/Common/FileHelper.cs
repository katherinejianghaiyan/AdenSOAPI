using System;
using System.IO;
using System.Text;

namespace Aden.Util.Common
{
    public static class FileHelper
    {
        /// <summary>
        /// 从文件中读取内容字符串
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>内容字符串</returns>
        public static string Read(string filePath)
        {
            string outStr = string.Empty;
            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    outStr = sr.ReadToEnd();
                    sr.Close();
                }
            }
            return outStr;
        }

        /// <summary>
        /// 打开或创建文件,并写入
        /// </summary>
        /// <param name="filePath">文本文件路径包括文件名</param>
        /// <param name="content">内容</param>
        /// <param name="encoding">编码格式</param>
        public static void Write(string filePath, string content, Encoding encoding)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(filePath)) return;
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fileStream, encoding))
                {
                    sw.Write(content);
                    sw.Close();
                }
            }

        }

        /// <summary>
        /// 打开或创建文件,并写入文本,按UTF-8编码
        /// </summary>
        /// <param name="filePath">文本文件路径包括文件名</param>
        /// <param name="content">内容</param>
        public static void Write(string filePath, string content)
        {
            Write(filePath, content, Encoding.UTF8);
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="sourceFileName">源文件路径包括文件名</param>
        /// <param name="destFileName">目标文件路径包括文件名</param>
        /// <param name="overWrite">是否覆盖已存在文件</param>
        public static void CopyFile(string sourceFileName, string destFileName, bool overWrite = true)
        {
            if (!File.Exists(sourceFileName)) throw new FileNotFoundException(sourceFileName + "文件不存在！");
            File.Copy(sourceFileName, destFileName, overWrite);
        }

        /// <summary>
        /// 递归复制文件夹以及文件
        /// </summary>
        /// <param name="srcDir">源文件夹根目录</param>
        /// <param name="destDir">目标文件夹根目录</param>
        public static void CopyDirFiles(string srcDir, string destDir)
        {
            if (Directory.Exists(srcDir))
            {
                foreach (string str in Directory.GetFiles(srcDir))
                {
                    string destFileName = Path.Combine(destDir, Path.GetFileName(str));
                    File.Copy(str, destFileName, true);
                }

                foreach (string str3 in Directory.GetDirectories(srcDir))
                {
                    string str4 = str3.Substring(str3.LastIndexOf(@"\") + 1);
                    CopyDirFiles(Path.Combine(srcDir, str4), Path.Combine(destDir, str4));
                }
            }
        }

        /// <summary>
        /// 删除文件夹文件
        /// </summary>
        /// <param name="dir">文件夹根目录</param>
        public static void DeleteDirFiles(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (string str in Directory.GetFiles(dir))
                {
                    File.Delete(str);
                }
                foreach (string str2 in Directory.GetDirectories(dir))
                {
                    Directory.Delete(str2, true);
                }
            }
        }

        /// <summary>
        /// 获得文件最后修改时间
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static DateTime GetFileLastWriteTime(string file)
        {
            if (File.Exists(file)) return File.GetLastWriteTime(file);
            else throw new FileNotFoundException(file);
        }

        /// <summary>
        /// 取消文件的只读属性
        /// </summary>
        /// <param name="file"></param>
        public static void RemoveFileReadOnlyAttribute(string file)
        {
            File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
        }


        //文件读到流中
        public static Stream FileToStream(string file)
        {
            byte[] bytes = File.ReadAllBytes(file);
            return new MemoryStream(bytes);
        }
    }
}
