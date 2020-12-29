using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Empresa.integracao.ftp.log;

namespace Empresa.integracao.ftp.filesystem
{
    public delegate bool OpAmp(string pattern);
    public delegate string OpBack(string stringtoo);
    public delegate IEnumerable<string> OpAny(string stringOf);
    public delegate Task OpConBack(string stringOf);
    class ReadFILE
    {
        private static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public static string GetSha256(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(stream);
                    // Convert byte array to a string   
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
        }
        public static string sha256 (string data)
        {
            try
            {
                return ComputeSha256Hash(data);
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR Sha256Sum ERROR::");
                return "";
            }
        }
        static readonly private Func<string, bool> DeleteDirectory = new Func<string, bool>(directory =>
        {
            foreach (string directoryof in Directory.GetDirectories(directory))
            {
                DeleteDirectory(directoryof);
            }

            try
            {
                Directory.Delete(directory, true);
            }
            catch (IOException e)
            {
                CentralLog.LogError(e, "::ERROR IN REMOVE DIRECTORY <IO>::");
                Directory.Delete(directory, true);
            }
            catch (UnauthorizedAccessException e)
            {
                CentralLog.LogError(e, "::ERROR IN REMOVE DIRECTORY <UAcess>::");
                Directory.Delete(directory, true);
            } 

            return true;
        });
        public static string ReadFile (string pathTo)
        {
            try
            {
                return File.ReadAllText(pathTo);
            } catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN REMOVE DIRECTORY::");
                return "";
            }
        }

        public static string[] showDirectory (string pathTo)
        {
            try
            {
                return Directory.GetFileSystemEntries(pathTo, "*", SearchOption.TopDirectoryOnly);
            } catch (UnauthorizedAccessException e )
            {
                CentralLog.LogError(e, "::ERROR ACESS DIRECTORY::");
                return new string[] { };
            }
        }
        public static string fileNameInPath (string pathOfFile)
        {
            try
            {
                return Path.GetFileName(pathOfFile);
            } catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN FIND PATH OF FILE::");
                return "";
            }
        }

        private static bool Not (bool something)
        {
            return !something;
        }

        public static string pathName (string somePathFile)
        {
            try
            {
                return Path.GetDirectoryName(somePathFile);
            } catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN GET DIRECTORY NAME::");
                return "";
            }
        }

        public static string moveFile (string source, string destinyTo)
        {
            try
            {
                if (Not(isDirectory(source)))
                    if (Not(existsFile(destinyTo)))
                        File.Move(source, destinyTo);
                return source;
            } catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN MOVE FILE:: Source " + source + " Destiny " + destinyTo);
                return "";
            }
        }

        public static bool existsFile(string path)
        {
            try
            {
                return File.Exists(path);
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN CHECK FILE::");
                return false;
            }
        }

        public static bool existsDirectory (string path)
        {
            try
            {
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN CHECK DIRECTORY::");
                return false;
            }
        }

        public static string getParent (string path)
        {
            try
            {
                return (Directory.GetParent(path)).Parent.FullName;
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN GET PARENT DIRECTORY::");
                return "";
            }
        }
        public static string extracZip (string Path, string ToPath)
        {
            try
            {
                if (Not(existsDirectory(Path.Substring(0, Path.Length - 4))))
                    ZipFile.ExtractToDirectory(Path, ToPath);

                return Path;
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN EXTRACT FILE ZIP::");
                return "";
            }
        }
        public static void deleteFile (string pathFile)
        {
            try
            {
                File.Delete(pathFile);
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN DELETE FILE::");
            }
        }

        public static bool deleteDirectory (string pathDirectory, bool recursiveValue)
        {
            try
            {
                return DeleteDirectory(pathDirectory);
            }
            catch
            {
                CentralLog.LogError(new Exception(pathDirectory), "::ERROR IN DELETE DIRECTORY::");
             
                System.Threading.Thread.Sleep(2*1000);     //wait 2 seconds
                Directory.Delete(pathDirectory, recursive: recursiveValue);

                return false;
            }
        }

        public static bool isFile (string pathTo)
        {
            try
            {
                return ((File.GetAttributes(pathTo) & FileAttributes.Archive) == FileAttributes.Archive);
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN CHECK IF ISFILE ::");
                return false;
            }
        }

        public static bool isDirectory (string pathTo)
        {
            try
            {
                return ((File.GetAttributes(pathTo) & FileAttributes.Directory) == FileAttributes.Directory);
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN CHECK IF ISDIRECTORY::");
                return false;
            }
        }

        public static int numberOfRegexMatcher (string text, string regex)
        {
            return ((new Regex(regex)).Matches(text)).Count;
        }

        public static string recursiveCallByDirectory(string path, string patternForFiles)
        {
            try
            {
                if (ReadFILE.isDirectory(path))
                {
                    return (ReadFILE.showDirectory(path)).Aggregate("", (acc, now) =>
                      {
                          if (ReadFILE.isFile(now))
                          {
                              if (numberOfRegexMatcher(path, patternForFiles) == 1)
                                  return acc + ReadFILE.recursiveCallByDirectory(now, patternForFiles);
                          }
                          if (ReadFILE.isDirectory(now))
                          {
                              return acc + ReadFILE.recursiveCallByDirectory(Path.Combine(path, now), patternForFiles);
                          }

                          return acc;
                      });
                }
                return ReadFILE.ReadFile(path);
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN RECURSIVY CALL BY DIRECTORY::");
                return "";
            }
        }
        public static async Task linearCallByFile(string path, OpAmp patternForFilesChecker, OpConBack comutateFishOperation)
        {
            try
            {
                foreach (string s in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    if (patternForFilesChecker(s))
                    {
                        await comutateFishOperation(s);
                    }
                }
            } catch(Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN LINEAR CALL BY FILE::");
            }
        }
        public static string linearCallByFileForDeleting(string path, OpAmp patternForFilesChecker, OpBack comutateFishOperation)
        {
            try
            {
                string result = "";

                foreach (string s in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    if (patternForFilesChecker(s))
                    {
                        result = result + comutateFishOperation(s);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN LINEAR CALL BY FILE::");
                return "";
            }
        }
        public static string recursiveCallByDelegate(string path, OpAmp patternForFilesChecker, OpBack comutateFishOperation)
        {
            try
            {
                if (ReadFILE.isDirectory(path))
                {
                    return (ReadFILE.showDirectory(path)).Aggregate("", (acc, now) =>
                    {
                        if (ReadFILE.isFile(now))
                        {
                            if (patternForFilesChecker(now))
                                return acc + ReadFILE.recursiveCallByDelegate(now, patternForFilesChecker, comutateFishOperation);
                        }
                        if (ReadFILE.isDirectory(now))
                        {
                            return acc + ReadFILE.recursiveCallByDelegate(Path.Combine(path, now), patternForFilesChecker, comutateFishOperation);
                        }

                        return acc;
                    });
                }
                return comutateFishOperation(path);
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN RECURSIVY CALL BY DELEGATE::");
                return "";
            }
        }
        public static string recursiveCallByDelegateForDeleting(string path, OpAmp patternForFilesChecker, OpBack comutateFishOperation)
        {
            try
            {
                CentralLog.LogInfo(path);
                if (patternForFilesChecker(path))
                {
                    return comutateFishOperation(path);
                }
                return (ReadFILE.showDirectory(path)).Aggregate("", (acc, now) =>
                    {
                        if (ReadFILE.isDirectory(now))
                        {
                            return acc + ReadFILE.recursiveCallByDelegateForDeleting(Path.Combine(path, now), patternForFilesChecker, comutateFishOperation);
                        }

                        return acc;
                    });
            }
            catch (Exception e)
            {
                CentralLog.LogError(e, "::ERROR IN RECURSIVY CALL FOR DELETION:: " + path);
                return "";
            }
        }

    }
}
