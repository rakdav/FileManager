using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Security.AccessControl;

namespace FileManager
{
    public class FileManager
    {
        public ESection Esection { get; set; }
        public string FileManagerName { get; set; }

        public LeftSection LeftSection { get; set; }
        public RightSection RightSection { get; set; }

        public ArrayList LFiles { get; set; } = new ArrayList();
        public ArrayList RFiles { get; set; } = new ArrayList();

        public string LPath { get => LDirectory.FullName; }
        public string RPath { get => RDirectory.FullName; }

        public DirectoryInfo LDirectory { get; set; } = new DirectoryInfo(Config.LeftSectionPath);
        public DirectoryInfo RDirectory { get; set; } = new DirectoryInfo(Config.RightSectionPath);

        private Searcher _searcher;
        private Drives _drives;

        public FileManager(string fileManagerName)
        {
            _searcher = new Searcher();
            _drives = new Drives();

            Console.BackgroundColor = Config.BackgroundColor;
            Console.ForegroundColor = Config.ForegroundColor;
            Console.SetWindowSize(Config.StartupWindowWidth, Config.StartupWindowHeight);
            Console.SetBufferSize(Config.StartupWindowWidth, Config.StartupWindowHeight);

            FileManagerName = fileManagerName;
            Esection = ESection.Left;
            Directory.SetCurrentDirectory(Config.LeftSectionPath);

            // -------------------------------------------------------

            LFiles = new ArrayList();
            LFiles.Add(new FolderUp());
            LFiles.AddRange(LDirectory.GetDirectories());
            LFiles.AddRange(LDirectory.GetFiles());

            LeftSection = new LeftSection(LFiles);

            RFiles = new ArrayList();
            RFiles.Add(new FolderUp());
            RFiles.AddRange(RDirectory.GetDirectories());
            RFiles.AddRange(RDirectory.GetFiles());

            RightSection = new RightSection(RFiles);
        }

        public void ChangeSection()
        {
            if (Esection == ESection.Right)
            {
                RightSection.Cursor.HideSelection();
                LeftSection.Cursor.UnmaskSelection();
                Console.BackgroundColor = Config.AdditionalBackgroundColor;

                Esection = ESection.Left;

                // переписать текст слева
                Console.SetCursorPosition(2, 3);
                Console.Write($"Current path: {GUI.GetNormalString(LPath, Config.HalfWindowWidth - 19)}");

                // переписать текст справа
                Console.BackgroundColor = Config.BackgroundColor;
                Console.SetCursorPosition(Config.HalfWindowWidth + 2, 3);
                Console.Write($"Current path: {GUI.GetNormalString(RPath, Config.HalfWindowWidth - 19)}");
                
                // Exception
                Directory.SetCurrentDirectory(LDirectory.FullName);
            }
            else if (Esection == ESection.Left)
            {
                RightSection.Cursor.UnmaskSelection();
                LeftSection.Cursor.HideSelection();
                Console.BackgroundColor = Config.AdditionalBackgroundColor;

                Esection = ESection.Right;

                // переписать текст справа
                Console.SetCursorPosition(Config.HalfWindowWidth + 2, 3);
                Console.Write($"Current path: {GUI.GetNormalString(RPath, Config.HalfWindowWidth - 19)}");

                // переписать текст слева
                Console.BackgroundColor = Config.BackgroundColor;
                Console.SetCursorPosition(2, 3);
                Console.Write($"Current path: {GUI.GetNormalString(LPath, Config.HalfWindowWidth - 19)}");

                Directory.SetCurrentDirectory(RDirectory.FullName);
            }
        }

        public void SetSearchResults(ArrayList results, bool advMode = false, bool flag = false)
        {
            if (advMode)
            {
                if (flag)
                {
                    LFiles = new ArrayList();
                    LFiles.Add(new FolderUp());
                    LFiles.AddRange(results);

                    LeftSection = new LeftSection(LFiles);
                    LeftSection.Displaying = IsDisplaying.Search;
                }
                else
                {
                    RFiles = new ArrayList();
                    RFiles.Add(new FolderUp());
                    RFiles.AddRange(results);

                    RightSection = new RightSection(RFiles);
                    RightSection.Displaying = IsDisplaying.Search;
                }
                return;
            }

            if (Esection == ESection.Left)
            {
                LFiles = new ArrayList();
                LFiles.Add(new FolderUp());
                LFiles.AddRange(results);

                LeftSection = new LeftSection(LFiles);
                LeftSection.Displaying = IsDisplaying.Search;
            }
            else
            {
                RFiles = new ArrayList();
                RFiles.Add(new FolderUp());
                RFiles.AddRange(results);

                RightSection = new RightSection(RFiles);
                RightSection.Displaying = IsDisplaying.Search;
            }
        }

        public void SetDisks(List<DriveInfo> results)
        {
            if (Esection == ESection.Left)
            {
                LFiles = new ArrayList();
                LFiles.Add(new FolderUp());
                LFiles.AddRange(results);

                LeftSection = new LeftSection(LFiles);
                LeftSection.Displaying = IsDisplaying.Disks;
            }
            else
            {
                RFiles = new ArrayList();
                RFiles.Add(new FolderUp());
                RFiles.AddRange(results);

                RightSection = new RightSection(RFiles);
                RightSection.Displaying = IsDisplaying.Disks;
            }
        }

        public void SetLeftSectionFiles()
        {
            if (LeftSection.Displaying == IsDisplaying.Search)
            {
                SetSearchResults(_searcher.GetResults(), true, true);
                return;
            }
            else if (LeftSection.Displaying == IsDisplaying.Disks)
            {
                SetDisks(_drives.Disks);
                return;
            }

            LFiles = new ArrayList();
            LFiles.Add(new FolderUp());
            LFiles.AddRange(LDirectory.GetDirectories());
            LFiles.AddRange(LDirectory.GetFiles());

            LeftSection = new LeftSection(LFiles);
        }

        public void SetRightSectionFiles()
        {
            if (RightSection.Displaying == IsDisplaying.Search)
            {
                SetSearchResults(_searcher.GetResults(), true, false);
                return;
            }
            else if (RightSection.Displaying == IsDisplaying.Disks)
            {
                SetDisks(_drives.Disks);
                return;
            }

            RFiles = new ArrayList();
            RFiles.Add(new FolderUp());
            RFiles.AddRange(RDirectory.GetDirectories());
            RFiles.AddRange(RDirectory.GetFiles());

            RightSection = new RightSection(RFiles);
        }

        public void MoveUp()
        {
            if (Esection == ESection.Left)
                LeftSection.Cursor.MoveUp();
            else
                RightSection.Cursor.MoveUp();
        }

        public void MoveDown()
        {
            if (Esection == ESection.Left)
                LeftSection.Cursor.MoveDown();
            else
                RightSection.Cursor.MoveDown();
        }

        public void ChangeDirectory(string newPath)
        {
            // check if user have access to folder
            var tmp = new DirectoryInfo(newPath);
            tmp.GetFiles();

            if (Esection == ESection.Left)
            {
                LDirectory = tmp;          
                LeftSection.Displaying = IsDisplaying.Files;
                SetLeftSectionFiles();
            }
            else
            {
                RDirectory = tmp;
                RightSection.Displaying = IsDisplaying.Files;
                SetRightSectionFiles();
            }

            Directory.SetCurrentDirectory(newPath);
        }

        public object GetCurrentFile()
        {
            if (Esection == ESection.Left)
                return LeftSection.Cursor.Files[LeftSection.Cursor.Index];
            else
                return RightSection.Cursor.Files[RightSection.Cursor.Index];
        }

        public void DisplayFiles()
        {
            if (Esection == ESection.Left)
                LeftSection.DisplayFiles();
            else
                RightSection.DisplayFiles();
        }

        public string GetSelectedPath(bool FullName = true)
        {
            if (!FullName)
            {
                if (Esection == ESection.Left)
                    return LeftSection.Cursor.FileNames[LeftSection.Cursor.Index];
                else
                    return RightSection.Cursor.FileNames[RightSection.Cursor.Index];
            }

            if (Esection == ESection.Left)
            {
                if (LeftSection.Cursor.Files[LeftSection.Cursor.Index] is DirectoryInfo)
                    return (LeftSection.Cursor.Files[LeftSection.Cursor.Index] as DirectoryInfo).FullName;
                else if (LeftSection.Cursor.Files[LeftSection.Cursor.Index] is FileInfo)
                    return (LeftSection.Cursor.Files[LeftSection.Cursor.Index] as FileInfo).FullName;
                else
                    return LeftSection.Cursor.FileNames[LeftSection.Cursor.Index];               
            }
            else
            {
                if (RightSection.Cursor.Files[RightSection.Cursor.Index] is DirectoryInfo)
                    return (RightSection.Cursor.Files[RightSection.Cursor.Index] as DirectoryInfo).FullName;
                else if (RightSection.Cursor.Files[RightSection.Cursor.Index] is FileInfo)
                    return (RightSection.Cursor.Files[RightSection.Cursor.Index] as FileInfo).FullName;
                else
                    return RightSection.Cursor.FileNames[RightSection.Cursor.Index];
            }
        }

        public void Reload(bool RefreshFilesList = true)
        {
            if (RefreshFilesList)
            {
                SetLeftSectionFiles();
                SetRightSectionFiles();
            }
            
            GUI.DrawMenu(FileManagerName);

            LeftSection.DisplayFiles();
            RightSection.DisplayFiles();

            GUI.DrawName();
        }

        public bool CanCreate()
        {
            if (Esection == ESection.Left)
                if (LeftSection.Displaying == IsDisplaying.Disks || LeftSection.Displaying == IsDisplaying.Search)
                    return false;
            else
                if (RightSection.Displaying == IsDisplaying.Disks || RightSection.Displaying == IsDisplaying.Search)
                    return false;

            return true;
        }

        public bool IsDispayingDisks()
        {
            if (Esection == ESection.Left)
                if (LeftSection.Displaying == IsDisplaying.Disks)
                    return true;
                else
                if (RightSection.Displaying == IsDisplaying.Disks)
                    return true;

            return false;
        }

        public void DeleteCurrentFile()
        {
            try
            {
                if (Esection == ESection.Left)
                {
                    if (LeftSection.Displaying == IsDisplaying.Search)
                        _searcher.Results.Remove(GetCurrentFile());
                }
                else if (Esection == ESection.Right)
                {
                    if (RightSection.Displaying == IsDisplaying.Search)
                        _searcher.Results.Remove(GetCurrentFile());
                }
            }
            catch (Exception) { }

            if (GetCurrentFile() is FileInfo)
                File.Delete(GetSelectedPath());
            else if (GetCurrentFile() is DirectoryInfo)
                Directory.Delete(GetSelectedPath(), true);
            else
                return;
        }

        public void MoveCurrentFile(bool replace = false)
        {
            if (Esection == ESection.Left)
            {
                if (replace)
                {
                    if (File.Exists(RPath + "\\" + GetSelectedPath(false)))
                        File.Delete(RPath + "\\" + GetSelectedPath(false));
                    else if (Directory.Exists(RPath + "\\" + GetSelectedPath(false)))
                        Directory.Delete(RPath + "\\" + GetSelectedPath(false), true);
                }

                if (GetCurrentFile() is FileInfo)
                    (GetCurrentFile() as FileInfo).MoveTo(RPath + "\\" + GetSelectedPath(false));
                else if (GetCurrentFile() is DirectoryInfo)
                    (GetCurrentFile() as DirectoryInfo).MoveTo(RPath + "\\" + GetSelectedPath(false));
                else
                    return;

                if (LeftSection.Displaying == IsDisplaying.Search)
                {
                    _searcher.Results.Remove(GetCurrentFile());
                }
            }
            else
            {
                if (replace)
                {
                    if (File.Exists(LPath + "\\" + GetSelectedPath(false)))
                        File.Delete(LPath + "\\" + GetSelectedPath(false));
                    else if (Directory.Exists(LPath + "\\" + GetSelectedPath(false)))
                        Directory.Delete(LPath + "\\" + GetSelectedPath(false), true);
                }

                if (GetCurrentFile() is FileInfo)
                    (GetCurrentFile() as FileInfo).MoveTo(LPath + "\\" + GetSelectedPath(false));
                else if (GetCurrentFile() is DirectoryInfo)
                    (GetCurrentFile() as DirectoryInfo).MoveTo(LPath + "\\" + GetSelectedPath(false));
                else
                    return;

                if (RightSection.Displaying == IsDisplaying.Search)
                {
                    _searcher.Results.Remove(GetCurrentFile());
                }
            }
        }

        public void CopyCurrentFile(bool replace = false)
        {
            if (Esection == ESection.Left)
            {
                if (replace)
                {
                    if (File.Exists(RPath + "\\" + GetSelectedPath(false)))
                        File.Delete(RPath + "\\" + GetSelectedPath(false));
                    else if (Directory.Exists(RPath + "\\" + GetSelectedPath(false)))
                        Directory.Delete(RPath + "\\" + GetSelectedPath(false), true);
                }

                if (GetCurrentFile() is FileInfo)
                    (GetCurrentFile() as FileInfo).CopyTo(RPath + "\\" + GetSelectedPath(false));
                else if (GetCurrentFile() is DirectoryInfo)
                    Helper.CopyFolder(new DirectoryInfo(GetSelectedPath()), RPath);
                else
                    return;

                Directory.SetCurrentDirectory(LPath);
            }
            else
            {
                if (replace)
                {
                    if (File.Exists(LPath + "\\" + GetSelectedPath(false)))
                        File.Delete(LPath + "\\" + GetSelectedPath(false));
                    else if (Directory.Exists(LPath + "\\" + GetSelectedPath(false)))
                        Directory.Delete(LPath + "\\" + GetSelectedPath(false), true);
                }

                if (GetCurrentFile() is FileInfo)
                    (GetCurrentFile() as FileInfo).CopyTo(LPath + "\\" + GetSelectedPath(false));
                else if (GetCurrentFile() is DirectoryInfo)
                    Helper.CopyFolder(new DirectoryInfo(GetSelectedPath()), LPath);
                else
                    return;

                Directory.SetCurrentDirectory(RPath);
            }
        }

        public void DisplayFilesFromTwoSections()
        {
            LeftSection.DisplayFiles();
            RightSection.DisplayFiles();
        }

        public string GetFileInfo()
        {
            string inf = "";

            if (GetSelectedPath() == "..")
                throw new Exception("This is not file");

            if (GetCurrentFile() is FileInfo)
            {
                double length = (GetCurrentFile() as FileInfo).Length / Math.Pow(1024, 2);
                string creation = (GetCurrentFile() as FileInfo).CreationTimeUtc.ToShortDateString();
                string creationTime = (GetCurrentFile() as FileInfo).CreationTimeUtc.ToShortTimeString();
                inf += $"Size: {length.ToString("F3")} MB. Creation time: {creation} {creationTime}";
            }
            else if (GetCurrentFile() is DirectoryInfo)
            {
                Counter c = new Counter();
                c.CountSize(new DirectoryInfo(GetSelectedPath()));
                c.Count(new DirectoryInfo(GetSelectedPath()));

                double length = c.FolderSize;
                int fcount = c.FilesCount;
                int dcount = c.DirectoriesCount;
                inf += $"Size: {length.ToString("F3")} MB. Files: {fcount}. Folders: {dcount}";
            }
            else if (GetCurrentFile() is DriveInfo)
            {
                long freeSpace = (GetCurrentFile() as DriveInfo).AvailableFreeSpace / (long)Math.Pow(1024, 3);
                long totalSpace = (GetCurrentFile() as DriveInfo).TotalSize / (long)Math.Pow(1024, 3);

                inf += $"Free space: {freeSpace.ToString("F3")} Gb / {totalSpace.ToString("F3")} Gb";
            }

            return inf;
        }

        public string GetFileAttributes()
        {
            if (GetSelectedPath() == "..")
                throw new Exception("This is not file");

            if (GetCurrentFile() is FileInfo)
            {
                var currentFile = GetCurrentFile() as FileInfo;

                return currentFile.Attributes.ToString();
            }
            else
            {
                var currentFile = GetCurrentFile() as DirectoryInfo;

                return currentFile.Attributes.ToString();
            }
        }

        public void RenameFile(string newName)
        {
            if (GetCurrentFile() is FileInfo)
            {
                (GetCurrentFile() as FileInfo).MoveTo(newName);
            }
            else if (GetCurrentFile() is DirectoryInfo)
            {
                if (GetCurrentFile() is DirectoryInfo)
                {
                    (GetCurrentFile() as DirectoryInfo).MoveTo(newName);
                }
            }
        }

        public void DisplayDisks()
        {
            _drives.SetDrives();
            SetDisks(_drives.Disks);
            DisplayFiles();
        }

        public void CreateFolder(string name)
        {
            if (name.Length < 1)
                return;

            Directory.CreateDirectory(name);
        }

        public void CreateFile(string name)
        {
            if (name.Length < 1)
                return;

            FileStream fs = new FileStream(name, FileMode.CreateNew, FileAccess.Write, FileShare.Inheritable);
            fs.Close();
        }

        public string GetCurrentPath()
        {
            if (Esection == ESection.Left)
                return LPath;
            else
                return RPath;
        }

        public void SearchFile(string name)
        {
            if (LeftSection.Displaying == IsDisplaying.Search || RightSection.Displaying == IsDisplaying.Search)
            {
                throw new InvalidOperationException();
            }

            _searcher.Clear();
            if (IsDispayingDisks())
            {
                foreach (var i in _drives.Disks)
                {
                    _searcher.Search(new DirectoryInfo(i.Name), name);
                }
            }
            else
            {
                _searcher.Search(new DirectoryInfo(GetCurrentPath()), name);
            }

            if (_searcher.Results.Count == 0)
                throw new NoResultException("no results");

            SetSearchResults(_searcher.GetResults());
        }
    }
}
