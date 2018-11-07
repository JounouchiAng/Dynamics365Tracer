﻿using System;
using System.IO;
using System.IO.Packaging;
using System.Net.Mime;
using System.Text;

namespace Dynamics365Tracer.AppCode
{
    /// <summary>
    /// This class helps to compress files in a zip archive
    /// </summary>
    internal class ZipManager
    {
        internal void CompressFolder(string sourceFolder, string targetFile)
        {
            using (Package package = Package.Open(targetFile, FileMode.Create))
            {
                foreach (string currentFile in Directory.GetFiles(sourceFolder, "*.log", SearchOption.TopDirectoryOnly))
                {
                    Uri relUri = GetRelativeUri(currentFile);

                    PackagePart packagePart = package.CreatePart(relUri, MediaTypeNames.Application.Octet, CompressionOption.Maximum);

                    if (packagePart == null)
                    {
                        return;
                    }

                    using (FileStream fileStream = new FileStream(currentFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        CopyStream(fileStream, packagePart.GetStream());
                    }
                }
            }
        }

        private void CopyStream(Stream source, Stream target)
        {
            const int bufSize = 16384;
            byte[] buf = new byte[bufSize];
            int bytesRead;
            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
                target.Write(buf, 0, bytesRead);
        }

        private Uri GetRelativeUri(string currentFile)
        {
            string relPath = currentFile.Substring(currentFile
            .LastIndexOf('\\')).Replace('\\', '/').Replace(' ', '_').Replace('#', '_');
            return new Uri(RemoveAccents(relPath), UriKind.Relative);
        }

        private string RemoveAccents(string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormKD);
            Encoding removal = Encoding.GetEncoding(Encoding.ASCII.CodePage, new EncoderReplacementFallback(""), new DecoderReplacementFallback(""));
            byte[] bytes = removal.GetBytes(normalized);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}