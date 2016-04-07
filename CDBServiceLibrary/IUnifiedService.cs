using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;

namespace UnifiedServiceFramework
{
    /// <summary>
    /// Prototypes the endpoints for the service.
    /// </summary>
    [ServiceContract]
    public interface IUnifiedService
    {

        /// <summary>
        /// The endpoint through which all calls reach the service.  
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/{endpoint}", Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<string> InvokeGenericEndpointAsync(Stream data, string endpoint);

        /// <summary>
        /// Returns documentation for a given endpoint.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "/man/{endpoint}", BodyStyle = WebMessageBodyStyle.Bare)]
        Task<Stream> GetDocumentationForEndpoint(string endpoint);

        /// <summary>
        /// Returns documentation for all endpoints.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "/man", BodyStyle = WebMessageBodyStyle.Bare)]
        Task<Stream> GetAllDocumentation();
       
    }
}
