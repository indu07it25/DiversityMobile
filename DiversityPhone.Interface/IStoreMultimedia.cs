using System.Diagnostics.Contracts;
using System.IO;

namespace DiversityPhone.Interface
{
    [ContractClass(typeof(IStoreMultimediaContract))]
    public interface IStoreMultimedia
    {
        /// <summary>
        /// Stores the data for one Multimedia File 
        /// </summary>
        /// <param name="fileNameHint">Desired File Name</param>
        /// <param name="data"></param>
        /// <returns>A string URI identifying the multimedia item for later retrieval</returns>
        string StoreMultimedia(string fileNameHint, Stream data);

        Stream GetMultimedia(string URI);

        void DeleteMultimedia(string URI);

        void ClearAllMultimedia();
    }

    [ContractClassFor(typeof(IStoreMultimedia))]
    internal abstract class IStoreMultimediaContract : IStoreMultimedia
    {

        public string StoreMultimedia(string fileNameHint, Stream data)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(fileNameHint), "Invalid Filename");
            return default(string);
        }

        public Stream GetMultimedia(string URI)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteMultimedia(string URI)
        {
            throw new System.NotImplementedException();
        }

        public void ClearAllMultimedia()
        {
            throw new System.NotImplementedException();
        }
    }
}
