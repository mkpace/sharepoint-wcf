using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Reporting;
using Amazon.Kingpin.WCF2.Diagnostics;

namespace Amazon.Kingpin.WCF2.Http
{
    /// <summary>
    /// Utilities to support Http Requests and Responses
    /// </summary>
    public class HttpUtilities
    {
        /// <summary>
        /// Generates an exception response object that serializes out the exception and inner exception message
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ex"></param>
        /// <param name="httpAction"></param>
        /// <returns></returns>
        public static Message GenerateExceptionResponse(WebOperationContext ctx, Exception ex, string httpAction)
        {
            return GenerateExceptionResponse(ctx, ex, httpAction, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Overloaded - generates an exception response object that serializes out the exception and inner exception message
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ex"></param>
        /// <param name="httpAction"></param>
        /// <param name="httpStatusCode"></param>
        /// <returns></returns>
        public static Message GenerateExceptionResponse(WebOperationContext ctx, Exception ex, string httpAction, HttpStatusCode httpStatusCode)
        {
            ErrorResponse response = new ErrorResponse(ex, httpAction);
            ctx.OutgoingResponse.StatusCode = httpStatusCode;
            return ctx.CreateJsonResponse<ErrorResponse>(response);
        }

        /// <summary>
        /// Generates a successful response with the serialized <paramref name="Response"/>Response object
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="ctx">The current WebOperationContext</param>
        /// <param name="item">Object to serialize</param>
        /// <param name="diagnostics">Currently is time in milliseconds</param>
        /// <returns>Serilaized Message object</returns>
        internal static Message GenerateResponse<T>(WebOperationContext ctx, T item, string diagnostics)
        {
            Response<T> response = new Response<T>(item, diagnostics);
            ctx.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            return GenerateResponse<T>(ctx, new List<T>() { item }, diagnostics);
        }

        /// <summary>
        /// Overloaded: generates a successful response with the serialized <paramref name="Response"/>Response object
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="ctx">The current WebOperationContext</param>
        /// <param name="items">Collection of objects</param>
        /// <param name="diagnostics">Currently is time in milliseconds</param>
        /// <returns>Serilaized Message object</returns>        
        internal static Message GenerateResponse<T>(WebOperationContext ctx, List<T> items, string diagnostics)
        {
            Response<T> response;

            if(items.Count == 1)
                response = new Response<T>(items[0], diagnostics);
            else
                response = new Response<T>(items, diagnostics);

            ctx.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            return ctx.CreateJsonResponse<Response<T>>(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="items"></param>
        /// <param name="timer"></param>
        /// <returns></returns>
        internal static Message GenerateResponse<T>(WebOperationContext ctx, List<T> items, KPTimer timer)
        {
            Response<T> response;
            response = new Response<T>(items, timer);
            ctx.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            return ctx.CreateJsonResponse<Response<T>>(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="item"></param>
        /// <param name="timer"></param>
        /// <returns></returns>
        internal static Message GenerateResponse<T>(WebOperationContext ctx, T item, KPTimer timer)
        {
            Response<T> response;
            response = new Response<T>(item, timer);
            ctx.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            return ctx.CreateJsonResponse<Response<T>>(response);
        }
    }
}
