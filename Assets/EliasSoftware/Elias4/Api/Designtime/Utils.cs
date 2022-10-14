using System.Text;
using UnityEngine;

namespace EliasSoftware.Elias4.Designtime {

    using EliasSoftware.Elias4;

    public static class Utils {
        public static string QuotedString(string str) => "\"" + str + "\"";
        public static string BlockQuotedString(string str) => "\"\"\"" + str + "\"\"\"";

        public static T SendAndExpect<T>(   ITerm gqlRequest,
                                            T expectedFormat,
                                            string apiKey,
                                            T returnOnError = null,
                                            bool supressErrorMsg = false)
            where T: class
        {
            var request = new EliasRequest(gqlRequest);
            var response = request.Send(apiKey);
            var res = response.TryDeserializeJSON(expectedFormat);

            if(res.data != null)
                return res.data;

            if(res.errorMessages != null && supressErrorMsg == false)
            {
                var errStrBuilder = new StringBuilder();
                errStrBuilder.Append("Error(s) while processing request: \n");
                foreach(var message in res.errorMessages)
                    errStrBuilder.AppendLine(message);
                Debug.LogError(errStrBuilder.ToString());
            }

            return returnOnError;
        }
    }
}
