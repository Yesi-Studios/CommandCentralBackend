using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace CCServ.ServiceManagement.Service
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
        string InvokeGenericEndpointAsync(Stream data, string endpoint);

        /// <summary>
        /// The prototype for the endpoint method that replied to all preflight options requests.
        /// </summary>
        [OperationContract]
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*")]
        void GetOptions();

        /// <summary>
        /// Gets stats about the service.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "/statistics/{mode}?password={password}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        string GetStatistics(string mode, string password);

    }
}
