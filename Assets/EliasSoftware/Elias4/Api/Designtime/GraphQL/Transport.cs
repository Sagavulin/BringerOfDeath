// #define ELIAS_LOG_GQL
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.IO;
using System;

namespace EliasSoftware.Elias4.Designtime {

    using Newtonsoft.Json;
    using Elias4.Native.Designtime;
    using Elias4.Native;

    [Serializable]
    public class ApiConsumer {

        [SerializeField]
        private string _key = string.Empty;

        [SerializeField]
        private string _name = string.Empty;

        public string ApiKey { get => _key; }
        public string ApiConsumerName { get => _name; }
        public ApiConsumer(string apiKey="", string consumerName="") {
            _key = apiKey;
            _name = consumerName;
        }
        public bool IsValid => _key != null && _key.Length > 0;
    }

    public class EliasRequest {
        ITerm request;
        public EliasRequest(ITerm term) {
            request = term;
        }
        public EliasJsonResponse Send(string apiKey = null) {
            var requestString = request.Bake();
            if(apiKey == null) {
#if ELIAS_LOG_GQL
                Debug.LogFormat("Calling > {0}",
                    requestString);
#endif
                var res = Transport.Invoke(requestString, null, null);
                Debug.Assert(res.IsDone);
                return res;
            } else {
#if ELIAS_LOG_GQL
                Debug.LogFormat("Calling > {0}\nKey: {1}",
                    requestString,
                    apiKey);
#endif
                var res = Transport.Invoke(requestString, null, apiKey);
                Debug.Assert(res.IsDone);
                return res;
            }
        }
    }

    public class EliasJsonResponse {
        private IRequestHandler requestHandler;
        private int localRequestId = -100;
        private string response = null;
        public EliasJsonResponse(int requestId,
            IRequestHandler handler)
        {
            requestHandler = handler;
            localRequestId = requestId;
            Debug.Assert(handler.GetRequestState(localRequestId) != RequestState.Undefined);
        }

        public bool IsDone => requestHandler.GetRequestState(localRequestId) != RequestState.Waiting;
        public bool RequestFailed => requestHandler.GetRequestState(localRequestId) == RequestState.Failed;

        public string GetResponse() {
            if(response == null)
                response = requestHandler.TransferData(localRequestId, null);
            if(response == null)
                return "";
            return response;
        }

        public struct JSONResult<T> {
            public T data;
            public List<string> errorMessages;
        }

        private IEnumerable<string> ReadErrorMessages(string jsonString) {

            StringReader strReader = new StringReader(jsonString);
            JsonTextReader reader = new JsonTextReader(strReader);

            bool dataFieldDetected = false;
            bool errorFieldDetected = false;

            // "errors": [ ... ]
            //   "message": "..."
            //   "locations": [{...}, ...]
            //     "line": "..."
            //     "column": "..."
            //   "path": ["", ...]

            // if property == "errors"
            //      expect StartArray
            //          while not EndArray expect StartObject
            //              while not EndObject expect message
            //                  yield message

            while (reader.Read())
            {
                if( reader.TokenType == JsonToken.PropertyName
                    && reader.Value.ToString() == "errors" )
                {
                    reader.Read();
                    if(reader.TokenType != JsonToken.StartArray)
                        break;

                    while( reader.Read() && reader.TokenType != JsonToken.EndArray )
                    {
                        if( reader.TokenType == JsonToken.StartObject )
                        {
                            while( reader.Read() && reader.TokenType != JsonToken.EndObject )
                            {
                                if( reader.TokenType == JsonToken.PropertyName
                                    && reader.Value.ToString() == "message" )
                                {
                                    errorFieldDetected = true;
                                    reader.Read();
                                    yield return reader.Value.ToString();
                                }
                                else if (reader.Value != null) {
                                    reader.Skip();
                                }
                            }
                        }
                    }
                }
                else if (reader.Value != null) {
                    if( reader.TokenType == JsonToken.PropertyName
                        && reader.Value.ToString() == "data" )
                        dataFieldDetected = true;
                    reader.Skip();
                }
            }

            if( dataFieldDetected == false && errorFieldDetected == false)
                yield return "Malformed JSON response, no data field found";
        }

        public JSONResult<T> TryDeserializeJSON<T>(T type_spec)
            where T : class
        {
            // Note: dynamic objects are defined at runtime.
            // I'm keeping this functionality for now because
            // it is useful for prototyping. 
            
            var result = new JSONResult<T>() {
                data = null,
                errorMessages = null
            };

            var respStr = GetResponse();

#if ELIAS_LOG_GQL
            Debug.Log( respStr );
#endif

            if( respStr == null
                || respStr.Length == 0)
                return result;

            // json has "data" and not "errors" => valid response
            // json has "errors"                => error response

            var errorMsgIter = ReadErrorMessages(respStr);
            foreach( var eMsg in errorMsgIter )
            {
                if( result.errorMessages == null )
                    result.errorMessages = new List<string>();
                result.errorMessages.Add( eMsg );
            }

            // errorMessages will not be empty if data
            // and errors are both missing
            if( result.errorMessages != null )
                return result;

            // no errors detected, we assume we can 
            // deserialize the data field

            try
            {
                var deserialized = JsonConvert.DeserializeAnonymousType(respStr,
                    new { data = type_spec });
                Debug.Assert(deserialized.data != null,
                    "deserialized JSON object was null");
                result.data = deserialized.data;
            }
            catch(Exception e)
            {    
                result.errorMessages = new List<string> {
                    string.Format("Failed to deserialize JSON\n{0}", respStr)
                };
                Debug.LogException(e);
            }

            return result;
        }
	}

    public struct RequestHandlerStats {
        public int nWaiting;
        public int nReceived;
        public int nFailed;
        public int nUndefined;
        public int nIdsTotal;

        public override string ToString() {
            return string.Format("Number of IDs:\t{0}\nWaiting:\t\t{1}\nReceived:\t{2}\nFailed:\t\t{3}\nUndefined:\t{4}",
                nIdsTotal, nWaiting, nReceived, nFailed, nUndefined);
        }
    }

    public enum RequestState {
        Undefined,
        Waiting,
        Failed,
        Successful
    }

    public interface IRequestHandler {
        RequestState GetRequestState(int requestId);
        string TransferData(int requestId,
            string returnedOnFailure=null);
    }

    public class RequestHandler:
        IRequestHandler
    {
        private static Dictionary<int, string> localIdToResponseMap = new Dictionary<int, string>();
        private static Dictionary<int, RequestState> localIdToState = new Dictionary<int, RequestState>();
        private static int localRequestIdCntr = 0;

        private void ResponseHandler(char_ptr response, IntPtr localRequestIDPtr) {
            int localId = ((int)localRequestIDPtr);
            bool responseMapHasRequestId = localIdToResponseMap.ContainsKey(localId);
            if(responseMapHasRequestId) {
                // marshalling already copied the native string 
                // into managed memory
                localIdToResponseMap[localId] = response.data;
            } else {
                Debug.LogErrorFormat("Received response for invalid local request id {0}",
                    localId);
            }
        }

        private int InitNewRequest() {
            int reqId =  localRequestIdCntr++;
            localIdToResponseMap[reqId] = null;
            localIdToState[reqId] = RequestState.Waiting;
            return reqId;
        }

        public RequestState GetRequestState(int requestId) {
            if(localIdToState.ContainsKey(requestId) == false)
                return RequestState.Undefined;
            return localIdToState[requestId];
        }

        public EliasJsonResponse Invoke(
            string message,
            string variables,
            string apiKey)
        {
            var reqId = InitNewRequest();
            var invokationResult = EliasDesigntimeLib
                    .elias_designtime_call(
                        message,
                        variables,
                        apiKey, 
                        ResponseHandler,
                        (IntPtr)reqId);

            if(invokationResult == 0) {
                localIdToState[reqId] = RequestState.Failed;
            } else {
                localIdToState[reqId] = RequestState.Successful;
            }

            return new EliasJsonResponse(reqId, this);
        }

		public string TransferData(int requestId,
            string returnedOnFailure = null)
		{
            RequestState state;
            if( localIdToState.TryGetValue(requestId, out state) == false ) {
                Debug.Assert(localIdToResponseMap.ContainsKey(requestId) == false);
                return returnedOnFailure;
            }

            Debug.Assert(localIdToResponseMap.ContainsKey(requestId),
                "missing response entry");

            string value = null;
            if( localIdToResponseMap.TryGetValue(requestId, out value) == false )
                return returnedOnFailure;

            localIdToResponseMap.Remove(requestId);
            localIdToState.Remove(requestId);

            return value;
		}


        public RequestHandlerStats GetStats() {

            HashSet<int> ids = new HashSet<int>();
            ids.UnionWith(localIdToState.Keys);
            ids.UnionWith(localIdToResponseMap.Keys);

            var stats = new RequestHandlerStats() {
                nWaiting = 0,
                nFailed = 0,
                nReceived = 0,
                nUndefined = 0,
                nIdsTotal = ids.Count
            };

            foreach(var id in ids) {
                var state = GetRequestState(id);
                stats.nWaiting += (state == RequestState.Waiting) ? 1 : 0;
                stats.nReceived += (state == RequestState.Successful) ? 1 : 0;
                stats.nFailed += (state == RequestState.Failed) ? 1 : 0;
                stats.nUndefined += (state == RequestState.Undefined) ? 1 : 0;
            }

            return stats;
        }
	}

    public static class Transport
    {
        private static RequestHandler requestHandler = new RequestHandler();
        public static RequestHandlerStats GetCurrentStats() => requestHandler.GetStats();
        public static EliasJsonResponse Invoke(
                string message, string variables = null, string apiKey = null
            ) => requestHandler.Invoke(message, variables, apiKey);

    }
}
