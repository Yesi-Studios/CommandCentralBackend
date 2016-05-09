using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace CommandCentral.ClientAccess.Service
{
    /// <summary>
    /// Prototypes the endpoints for the service.
    /// </summary>
    [ServiceContract]
    public interface ICommandCentralService
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
