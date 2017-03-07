//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Autofac;
//using Microsoft.Bot.Builder.Luis;

//namespace Microsoft.Bot.Builder.Azure
//{
//    public class DomainTerminologyModule : Module
//    {
//        protected override void Load(ContainerBuilder builder)
//        {
//            base.Load(builder);
//        }
//    }

//    public interface ITerminologyStorageSource
//    {
//        List<DomainTerminologyItem> ReadItems();
//        void Add(DomainTerminologyItem item);
//        void Delete(DomainTerminologyItem item);
//    }

//    public class JsonTerminologyCatalogStorageClient : ITerminologyStorageSource
//    {

//        public List<DomainTerminologyItem> ReadItems()
//        {
//            throw new NotImplementedException();
//        }

//        public void Add(DomainTerminologyItem item)
//        {
//            throw new NotImplementedException();
//        }

//        public void Delete(DomainTerminologyItem item)
//        {
            
//        }
//    }

//    public interface ITerminologyClient
//    {

//    }

//    public class TerminologyClient : ITerminologyClient
//    {
//        private readonly ITerminologyStorageSource _terminologyStorageSource;
//        private LuisService _luisService;

//        public TerminologyClient(ITerminologyStorageSource storage, LuisService luisService)   // add a search service to parameter
//        {
//            _luisService = luisService;
//            _terminologyStorageSource = storage;
//        }

//        void CreateIfNotExists(DomainTerminologyItem item)
//        {
//            _terminologyStorageSource.Add(item);
//            //ToDo:add the term to model
//        }

//        public DomainTerminologyItem FindByAlias(string alias)
//        {
//            throw new NotImplementedException();
//        }

//        public List<DomainTerminologyItem> FindAllByAlias(string alias)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class DomainTerminologyItem
//    {
//        public string Domain { get; set; }
//        public string FormalName { get; set; }
//        public string Alias { get; set; }
//        public string Context { get; set; }

//    }
//}
