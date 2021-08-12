
using System;
using System.Collections.Generic;
using System.Reflection;
using ClientCodeGen.Models;
using Hagi.Shared.Api;

namespace ClientCodeGen.Templates
{
    public class AllRequestsTemplate : BaseTemplate<object>
    {
        protected List<RequestModel> AllRequests { get; private set; } = null!;

        protected override void OnStart()
        {
            this.AllRequests = this.GetAllRequests();
            base.OnStart();
        }

        private List<RequestModel> GetAllRequests()
        {
            if (this.Model is List<RequestModel> r)
            {
                return r;
            }

            List<RequestModel> requests = new List<RequestModel>();

            foreach (Type type in typeof(HostRequest).Assembly.GetTypes())
            {
                RequestAttribute? requestAttribute = type.GetCustomAttribute<RequestAttribute>();

                if (requestAttribute != null)
                {
                    requestAttribute.RequestType = type;
                    RequestModel requestModel = new RequestModel(requestAttribute, type);

                    requests.Add(requestModel);

                    foreach (PropertyInfo propertyInfo in type.GetProperties())
                    {
                        OptionAttribute? optionAttribute = propertyInfo.GetCustomAttribute<OptionAttribute>();
                        if (optionAttribute is { Hide: false })
                        {
                            optionAttribute.PropertyInfo = propertyInfo;
                            requestModel.Options.Add(optionAttribute);
                        }
                    }
                }
            }

            return requests;
        }

    }
}