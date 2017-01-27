using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.DataPersistence.Access
{
    public static class EntityCache<T>
    {
        private static bool isDisabled;
        public static Dictionary<string, Dictionary<string, DateTime>> EntityTimestamps { get; set; }
        public static Dictionary<string, Dictionary<string, List<T>>> Entities { get; set; }

        static EntityCache()
        {
            // initialize storage
            EntityTimestamps = new Dictionary<string, Dictionary<string, DateTime>>();
            Entities = new Dictionary<string, Dictionary<string, List<T>>>();
        }

        public static void EnableCache(bool enabled)
        {
            isDisabled = !enabled;
        }

        public static void FlushCache()
        {
            EntityTimestamps.Clear();
            Entities.Clear();
        }

        /// <summary>
        /// Check validity of entity cache
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public static bool IsValid(string teamUrl, string entityName, DateTime lastModifiedDt)
        {
            // entity timestamp of now will force refresh on cache
            DateTime cacheTimestamp = DateTime.Now;
            bool isValid = false;
            
            if (isDisabled) { return false; }

            try
            {
                // get entity timestamp
                if (EntityTimestamps.Count > 0)
                {
                    // do we have the cached team
                    if (EntityTimestamps.ContainsKey(teamUrl))
                    {
                        // is the entity for the team cached
                        if (EntityTimestamps[teamUrl].ContainsKey(entityName))
                        {
                            // get the last fetched time
                            cacheTimestamp = EntityTimestamps[teamUrl][entityName];
                            // check cache timestamp against last modified on list
                            if (DateTime.Compare(lastModifiedDt, cacheTimestamp) <= 0)
                            {
                                isValid = true;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Error: teamUrl: {0}; entity: {1}", teamUrl, entityName), ex.InnerException);
            }

            return isValid;
        }

        public static void UpdateCache(string teamUrl, string entityName, List<T> entities, DateTime lastModifiedDt)
        {
            // do we have cached teams?
            if (Entities.ContainsKey(teamUrl))
            {
                // do we have cached entities
                if(Entities[teamUrl].ContainsKey(entityName))
                {
                    // replace cached entities
                    Entities[teamUrl][entityName] = entities;
                    // replace timestamps
                    EntityTimestamps[teamUrl][entityName] = lastModifiedDt;
                }
                else
                {
                    // add the entity to the existing team
                    Entities[teamUrl].Add(entityName, entities);
                    // add the expiration to the team/entity
                    EntityTimestamps[teamUrl].Add(entityName, lastModifiedDt);
                }
            }
            else
            {
                // create a new entity dict
                Dictionary<string, List<T>> newEntities = new Dictionary<string, List<T>>();
                // add the entity name and the entities
                newEntities.Add(entityName, entities);
                // add the entity to the team
                Entities.Add(teamUrl, newEntities);

                // create a new timestamp
                Dictionary<string, DateTime> newExpiration = new Dictionary<string, DateTime>();
                // add the entity list timestamp
                newExpiration.Add(entityName, lastModifiedDt);
                // add the entity timestamp to the team
                EntityTimestamps.Add(teamUrl, newExpiration);

            }
        }
    }
}
