using DiversityPhone.Model;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace DiversityPhone.Interface
{

    public interface IKeyMappingService
    {
        int? ResolveToServerKey(DBObjectType ownerType, int ownerID);
        int? ResolveToLocalKey(DBObjectType ownerType, int ownerID);
        void AddMapping(DBObjectType ownerType, int ownerID, int serverID);
    }

    public static class MappingExtensions
    {
        public static int? ResolveKey(this IKeyMappingService mapping, IMappedEntity owner)
        {
            Contract.Requires(mapping != null);
            Contract.Requires(owner != null);

            return mapping.ResolveToServerKey(owner.EntityType, owner.EntityID.Value);
        }

        public static int EnsureKey(this IKeyMappingService mapping, DBObjectType ownerType, int ownerID)
        {
            Contract.Requires(mapping != null);

            var key = mapping.ResolveToServerKey(ownerType, ownerID);
            if (!key.HasValue)
                throw new KeyNotFoundException(string.Format("no Mapping for type {0}, id {1} found", ownerType, ownerID));
            else
                return key.Value;
        }

        public static void AddMapping(this IKeyMappingService mapping, IMappedEntity owner, int serverID)
        {
            Contract.Requires(mapping != null);
            Contract.Requires(owner != null);

            mapping.AddMapping(owner.EntityType, owner.EntityID.Value, serverID);
        }


    }
}
