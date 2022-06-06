using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;

namespace EquipTracking
{
        class SaveAndLoadService
        {
            private readonly string AgrMachPath;
            private readonly string EventPath;

            public SaveAndLoadService(string Apath, string Epath)
            {
                AgrMachPath = Apath;
                EventPath = Epath;
            }
            public BindingList<AgrMachinery> LoadAllAgrMachinery()
            {
                var fileExists = File.Exists(AgrMachPath);
                if (!fileExists)
                {
                    File.CreateText(AgrMachPath).Dispose();
                    return new BindingList<AgrMachinery>();
                }
                using (var reader = File.OpenText(AgrMachPath))
                {
                    var fileText = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<BindingList<AgrMachinery>>(fileText);
                }

            }

            public void SaveAllAgrMachinery(object AgrMachineryList)
            {
                using (StreamWriter writer = File.CreateText(AgrMachPath))
                {
                    string output = JsonConvert.SerializeObject(AgrMachineryList);
                    writer.Write(output);
                }
            }

            public LinkedList<AgrMachinery> LoadEventsList()
            {
                var fileExists = File.Exists(EventPath);
                if (!fileExists)
                {
                    File.CreateText(EventPath).Dispose();
                    return new LinkedList<AgrMachinery>();
                }
                using (var reader = File.OpenText(EventPath))
                {
                    var fileText = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<LinkedList<AgrMachinery>>(fileText);
                }

            }
            public void SaveEventsList(object EventsList)
            {
                using (StreamWriter writer = File.CreateText(EventPath))
                {
                    string output = JsonConvert.SerializeObject(EventsList);
                    writer.Write(output);
                }
            }
        
    }

}
