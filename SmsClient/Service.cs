using System;
using System.Collections.Generic;

namespace SmsClient
{
    public class Service
    {
        public string Name;
        public int MaxLength;
        public Type ServiceType;

        public override string ToString()
        {
            return this.Name;
        }

        public static List<Service> GetList()
        {
            List<Service> services = new List<Service>();

            services.Add(new Service { Name = "WAY2SMS", MaxLength = 140, ServiceType = typeof(SmsClient.SmsClientWay2sms) });
            //services.Add(new Service { Name = "160BY2", MaxLength = 140, ServiceType = typeof(SmsClient.SmsClient160By2) });

            return services;
        }
    }
}
