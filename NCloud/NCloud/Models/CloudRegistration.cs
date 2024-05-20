using NCloud.ConstantData;
using NCloud.Security;
using System.Text;

namespace NCloud.Models
{
    /// <summary>
    /// Abstract class for physical items in file system
    /// </summary>
    public abstract class CloudRegistration
    {
        public string? IconPath { get; set; }
        public string? SharedName { get; set; }
        public string? HashedPath { get; set; }
        public string? ItemPath { get; set; }
        public bool IsConnectedToApp { get; set; }
        public bool IsConnectedToWeb { get; set; }
        
        public CloudRegistration(bool isSharedInApp, bool isPublic, string? id = null)
        {
            IconPath = null!;
            IsConnectedToApp = isSharedInApp;
            IsConnectedToWeb = isPublic;
            HashedPath = HashManager.EncryptString(id);
        }


        /// <summary>
        /// Abstract method to return that object is file
        /// </summary>
        /// <returns>True if file, otherwise false</returns>
        public abstract bool IsFile();


        /// <summary>
        /// Abstract method to return that object is folder
        /// </summary>
        /// <returns>True if folder, otherwise false</returns>
        public abstract bool IsFolder();

        /// <summary>
        /// Abstract method to return object
        /// </summary>
        /// <returns>The name of the object</returns>
        public abstract string ReturnName();
        
        /// <summary>
        /// static method to create instance of child class using specified rules
        /// </summary>
        /// <param name="clipBoardData">Data from cloud clipboard</param>
        /// <returns>The created object (CloudFile, CloudFolder)</returns>
        public static CloudRegistration? RegistrationPathFactory(string clipBoardData)
        {
            if (clipBoardData.Length < 2)
                return null;

            if (clipBoardData.StartsWith(Constants.SelectedFileStarterSymbol))
            {
                return new CloudFile(clipBoardData[2..]);
            }
            else if (clipBoardData.StartsWith(Constants.SelectedFolderStarterSymbol))
            {
                return new CloudFolder(clipBoardData[2..]);
            }

            return null;
        }
    }
}
