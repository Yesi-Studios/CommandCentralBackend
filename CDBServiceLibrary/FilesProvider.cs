using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UnifiedServiceFramework
{
    public static class FilesProvider
    {

        private static readonly string _filesDirectory = @"E:\UNIFIED_FILES_DIRECTORY";

        private static bool _isInitialized = false;

        private static ConcurrentDictionary<string, string> _fileNamesCache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Gets the list of all file names contained in the file names cache.
        /// </summary>
        public static List<string> FileNamesCache
        {
            get
            {
                return _fileNamesCache.Keys.ToList();
            }
        }

        /// <summary>
        /// Initializes the Files Provider, creating the directory if it doesn't exist and loading all file names into the file names cache.
        /// </summary>
        /// <returns></returns>
        public static void Initialize()
        {
            //Make sure the provider isn't already initialized.
            if (_isInitialized)
                throw new Exception("The files provider is already initialized!");

            //Make sure the directory exists.  If it doesn't, create it.
            if (!Directory.Exists(_filesDirectory))
                Directory.CreateDirectory(_filesDirectory);

            //Now let's go get all the file names from this directory and make sure they are all guids.
            List<string> fileNames = Directory.GetFiles(_filesDirectory).Select(x => Path.GetFileName(x)).ToList();

            //Make sure they're all GUIDs.
            if (!fileNames.All(x =>
                {
                    Guid temp;
                    return Guid.TryParse(x, out temp);
                }))
                throw new Exception("Files in the directory may have no extension and must be guids.");

            //Now set the cache to this.
            fileNames.ForEach(x =>
                {
                    if (!_fileNamesCache.TryAdd(x, x))
                        throw new Exception(string.Format("There was an exception while trying to add the file name, '{0}', to the files provider cache.", x));
                });
        }

        /// <summary>
        /// Releases the Files Provider cache.
        /// </summary>
        public static void Release()
        {
            _fileNamesCache.Clear();
        }

        /// <summary>
        /// Saves a given byte array to the file system and assigns it a unique GUID, then returns that guid.
        /// </summary>
        /// <param name="fileBytes"></param>
        /// <returns></returns>
        public static async Task<string> SaveFileAsync(byte[] fileBytes)
        {
            try
            {
                string id = Guid.NewGuid().ToString();

                await Task.Run(() => File.Create(Path.Combine(_filesDirectory, id), fileBytes.Length, FileOptions.RandomAccess));

                //Now add the ID to the cache.
                if (!_fileNamesCache.TryAdd(id, id))
                    throw new Exception(string.Format("There was an exception while trying to add the file name, '{0}', to the files provider cache.", id));

                return id;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads a file given a specific ID and throws exceptions in the event the file could not be Loaded.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<byte[]> LoadFileAsync(string id)
        {
            try
            {
                if (!_fileNamesCache.ContainsKey(id))
                    throw new UnifiedServiceFramework.Framework.ServiceException(string.Format("The file with the ID, '{0}', could not be loaded.", id), Framework.ErrorTypes.Validation);

                //We're still going to check if the file exists.
                if (!await DoesFileExist(Path.Combine(_filesDirectory, id)))
                    throw new UnifiedServiceFramework.Framework.ServiceException(string.Format("The file with the ID, '{0}', was contained in the cache, but was not in the file system.", id), Framework.ErrorTypes.Validation);

                //Ok, since it exists, now we can go load it.
                return await Task.Run<byte[]>(() => File.ReadAllBytes(Path.Combine(_filesDirectory, id)));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Removes the given file from the file system and creates a new one and returns that file's new ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<string> UpdateFileAsync(string id, byte[] file)
        {
            try
            {
                //If the cache contains the ID, we need to delete it and the file.
                if (_fileNamesCache.ContainsKey(id))
                {
                    await DeleteFileAsync(Path.Combine(_filesDirectory, id));
                }

                //The file wasn't in the cache, but let's see if it's in the file system, if so, that's bad.
                if (!File.Exists(Path.Combine(_filesDirectory, id)))
                    throw new UnifiedServiceFramework.Framework.ServiceException(string.Format("The file with the ID, '{0}', was not in the cache, but was in the file system.", id), Framework.ErrorTypes.Validation);

                //Ok, now that the file is deleted, let's add it with a new ID.
                return await SaveFileAsync(file);

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Removes a file from both the cache and the file system.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task DeleteFileAsync(string id)
        {
            try
            {
                if (!_fileNamesCache.ContainsKey(id))
                    throw new UnifiedServiceFramework.Framework.ServiceException(string.Format("The file with the ID, '{0}', could not be deleted.", id), Framework.ErrorTypes.Validation);

                //We're still going to check if the file exists.
                if (!await DoesFileExist(Path.Combine(_filesDirectory, id)))
                    throw new UnifiedServiceFramework.Framework.ServiceException(string.Format("A file with the ID, '{0}', existed in the cache but not in the file system.", id), Framework.ErrorTypes.Validation);

                //Now that we know it exists, let's remove it from teh cache, and then the file system.
                string temp;
                if (!_fileNamesCache.TryRemove(id, out temp))
                    throw new Exception(string.Format("There was an exception while trying to remove the file name, '{0}', from the files provider cache.", id));

                //Now from teh file system.
                await Task.Run(() => File.Delete(Path.Combine(_filesDirectory, id)));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// An asynchrounous version of File.Exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<bool> DoesFileExist(string path)
        {
            try
            {
                return await Task.Run<bool>(() => File.Exists(path));
            }
            catch
            {
                throw;
            }
        }

    }
}
