﻿using Microsoft.AspNetCore.Http;
using Repair.Core.Extensions.String;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Repair.Core.Helper.IO;

/// <summary>
/// 文件辅助操作类
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 创建文件，如果文件不存在
    /// </summary>
    /// <param name="fileName">要创建的文件</param>
    public static void CreateIfNotExists(string fileName)
    {
        if (File.Exists(fileName))
        {
            return;
        }

        string dir = Path.GetDirectoryName(fileName);
        if (dir != null)
        {
            DirectoryHelper.CreateIfNotExists(dir);
        }
        File.Create(fileName);
    }

    /// <summary>
    /// 删除指定文件
    /// </summary>
    /// <param name="fileName">要删除的文件名</param>
    public static void DeleteIfExists(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return;
        }
        File.Delete(fileName);
    }

    /// <summary>
    /// 获取文件版本号
    /// </summary>
    /// <param name="fileName"> 完整文件名 </param>
    /// <returns> 文件版本号 </returns>
    public static string GetVersion(string fileName)
    {
        if (File.Exists(fileName))
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(fileName);
            return fvi.FileVersion;
        }
        return null;
    }

    /// <summary>
    /// 获取文件的MD5值
    /// </summary>
    /// <param name="fileName"> 文件名 </param>
    /// <returns> 32位MD5 </returns>
    public static string GetFileMd5(string fileName)
    {
        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            const int bufferSize = 1024 * 1024;
            byte[] buffer = new byte[bufferSize];
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                md5.Initialize();
                long offset = 0;
                while (offset < fs.Length)
                {
                    long readSize = bufferSize;
                    if (offset + readSize > fs.Length)
                    {
                        readSize = fs.Length - offset;
                    }
                    fs.Read(buffer, 0, (int)readSize);
                    if (offset + readSize < fs.Length)
                    {
                        md5.TransformBlock(buffer, 0, (int)readSize, buffer, 0);
                    }
                    else
                    {
                        md5.TransformFinalBlock(buffer, 0, (int)readSize);
                    }
                    offset += bufferSize;
                }
                fs.Close();
                byte[] result = md5.Hash;
                md5.Clear();
                StringBuilder sb = new StringBuilder(32);
                foreach (byte b in result)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// 获取文本文件的编码方式
    /// </summary>
    /// <param name="fileName"> 文件名 例如：path = @"D:\test.txt"</param>
    /// <returns>返回编码方式</returns>
    public static Encoding GetEncoding(string fileName)
    {

        return GetEncoding(fileName, Encoding.Default);
    }

    /// <summary>
    /// 获取文本流的编码方式
    /// </summary>
    /// <param name="fs">文本流</param>
    /// <returns>返回系统默认的编码方式</returns>
    public static Encoding GetEncoding(FileStream fs)
    {
        //Encoding.Default 系统默认的编码方式
        return GetEncoding(fs, Encoding.Default);
    }

    /// <summary>
    /// 获取一个文本流的编码方式
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>
    /// <returns></returns>
    public static Encoding GetEncoding(string fileName, Encoding defaultEncoding)
    {
        using (FileStream fs = File.Open(fileName, FileMode.Open))
        {
            return GetEncoding(fs, defaultEncoding);
        }
    }

    /// <summary>
    /// 获取一个文本流的编码方式
    /// </summary>
    /// <param name="fs">文本流</param>
    /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>
    /// <returns></returns>
    public static Encoding GetEncoding(FileStream fs, Encoding defaultEncoding)
    {
        Encoding targetEncoding = defaultEncoding;
        if (fs != null && fs.Length >= 2)
        {
            byte b1 = 0;
            byte b2 = 0;
            byte b3 = 0;
            byte b4 = 0;

            long oriPos = fs.Seek(0, SeekOrigin.Begin);
            fs.Seek(0, SeekOrigin.Begin);

            b1 = Convert.ToByte(fs.ReadByte());
            b2 = Convert.ToByte(fs.ReadByte());
            if (fs.Length > 2)
            {
                b3 = Convert.ToByte(fs.ReadByte());
            }
            if (fs.Length > 3)
            {
                b4 = Convert.ToByte(fs.ReadByte());
            }

            //根据文件流的前4个字节判断Encoding
            //Unicode {0xFF, 0xFE};
            //BE-Unicode {0xFE, 0xFF};
            //UTF8 = {0xEF, 0xBB, 0xBF};
            if (b1 == 0xFE && b2 == 0xFF)//UnicodeBe
            {
                targetEncoding = Encoding.BigEndianUnicode;
            }
            if (b1 == 0xFF && b2 == 0xFE && b3 != 0xFF)//Unicode
            {
                targetEncoding = Encoding.Unicode;
            }
            if (b1 == 0xEF && b2 == 0xBB && b3 == 0xBF)//UTF8
            {
                targetEncoding = Encoding.UTF8;
            }

            fs.Seek(0, SeekOrigin.Begin);
        }
        return targetEncoding;
    }


    public static string GetFileType(this IFormFile formFile)
    {
        return formFile.FileName.Substring(formFile.FileName.LastIndexOf(".") + 1);
    }
    public static string GetFileName(this IFormFile formFile)
    {
        return formFile.FileName.Substring(0, formFile.FileName.LastIndexOf("."));
    }

    public static string GetFileType(this string url)
    {
        return url.Substring(url.LastIndexOf(".") + 1);
    }
    public static string GetFileName(this string url)
    {
        return url.Substring(url.LastIndexOf("/") + 1, url.LastIndexOf(".") - url.LastIndexOf("/") - 1);
    }
    public static List<FileResult> GetFileResultList(this string urls)
    {
        var result = new List<FileResult>();
        try
        {
            var collect = urls.JoinAsList(";");

            foreach (var item in collect)
            {
                result.Add(new FileResult()
                {
                    Name = item.GetFileName(),
                    Url = item
                });
            }

        }
        catch (Exception ex)
        {

        }

        return result;
    }


}
