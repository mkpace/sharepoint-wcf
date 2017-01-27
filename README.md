# sharepoint-wcf

Sample code for custom application API providing access to SharePoint Server API. 

The entity diagram for the codebase:

#Persistence Layer
Clean separation of concerns (SoC) within the persistence layer.
API provides services to manage the SharePoint server lists and libraries from the entry-point Domain Manager.
![kingpin-persistence](https://cloud.githubusercontent.com/assets/3538129/22384391/f018e9c2-e482-11e6-8c0e-f68d6710f68d.png)

#Domain Entities
Represent the application domain and classes used in the system. These object all inherit from IKPItem a core interface providing
the implementation for serializing/deserializing the objects into native SharePoint "items".
![kingpin-entities](https://cloud.githubusercontent.com/assets/3538129/22384389/eff96fc0-e482-11e6-9f89-7b3e5b25bdf3.png)

#Entity Lookups
Represent the "lookup" lists used by the Domain Entities - the "relational" tables in this model.
![kingpin-lookups](https://cloud.githubusercontent.com/assets/3538129/22384390/f0129ce8-e482-11e6-8d0c-d4c4e5ebe39a.png)

