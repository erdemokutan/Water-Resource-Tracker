using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterWatch.Models;
using WaterWatch.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace WaterWatch.Repositories
{
    public class WaterConsumptionRepository : IWaterConsumptionRepository
    {
        private readonly IDataContext _context;

        public WaterConsumptionRepository(IDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WaterConsumption>> GetAll()
        {
            SaveData();
            return await _context.Consumptions.ToListAsync();
        }

        public async Task<IEnumerable<WaterConsumption>> GetTopTenConsumers()
        {
            var q = _context.Consumptions
                .OrderByDescending(avgKL => avgKL.averageMonthlyKL)
                .Take(10)
                .ToListAsync();
            return await q;
        }
        
        public void SaveData()
        {
            var res_dataset= _context.Consumptions.ToList();

            if(res_dataset.Count()==0)
            {
                Console.WriteLine("No Data!");

                var geoJSON=File.ReadAllText("C:\\Users\\ERDEM OKUTAN\\Downloads\\water_consumptionfirst.geojson");
                dynamic jsonObj =JsonConvert.DeserializeObject(geoJSON);

                foreach(var feature in jsonObj["features"])
                {

                    //Extract values from the file object using the fields 
                    string str_neighbourhood=feature["properties"]["neighbourhood"];
                    string str_suburb_group=feature["properties"]["suburb_group"];
                    string str_avgMonthlyKL=feature["properties"]["averageMonthlyKL"];
                    string str_geometry=feature["geometry"]["coordinates"].ToString(Newtonsoft.Json.Formatting.None);

                    //Apply Transformations

                    //Remove .0's from values

                    string conv_avgMthlyKL= str_avgMonthlyKL.Replace(".0","");
                    //Convert string to int 
                    int avgMthlyKL=Convert.ToInt32(conv_avgMthlyKL);

                    //Load the data into our table

                    WaterConsumption wc =new()
                    {
                        neighbourhood=str_neighbourhood,
                        suburb_group=str_suburb_group,
                        averageMonthlyKL=avgMthlyKL,
                        coordinates=str_geometry
                        
                    };

                    _context.Consumptions.Add(wc);
                    _context.SaveChanges();

                }
            }

            else
            {
                Console.WriteLine("Data     Loaded");
            }
        }
    }
}