using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nms.Core
{
    public static class HackedUri
    {
        private const GenericUriParserOptions c_Options =
            GenericUriParserOptions.Default |
            GenericUriParserOptions.DontUnescapePathDotsAndSlashes |
            GenericUriParserOptions.Idn |
            GenericUriParserOptions.IriParsing;

        private static readonly GenericUriParser s_SyntaxHttp = new GenericUriParser(c_Options);
        private static readonly GenericUriParser s_SyntaxHttps = new GenericUriParser(c_Options);

        static HackedUri()
        {
            // Initialize the scheme 
            FieldInfo fieldInfoSchemeName = typeof (UriParser).GetField("m_Scheme",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfoSchemeName == null)
            {
                throw new MissingFieldException("'m_Scheme' field not found");
            }
            fieldInfoSchemeName.SetValue(s_SyntaxHttp, "http");
            fieldInfoSchemeName.SetValue(s_SyntaxHttps, "https");

            FieldInfo fieldInfoPort = typeof (UriParser).GetField("m_Port",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfoPort == null)
            {
                throw new MissingFieldException("'m_Port' field not found");
            }
            fieldInfoPort.SetValue(s_SyntaxHttp, 80);
            fieldInfoPort.SetValue(s_SyntaxHttps, 443);
        }

        public static Uri Create(string url)
        {
            Uri result = new Uri(url);

            if (url.IndexOf("%2F", StringComparison.OrdinalIgnoreCase) != -1)
            {
                UriParser parser = null;
                switch (result.Scheme.ToLowerInvariant())
                {
                    case "http":
                        parser = s_SyntaxHttp;
                        break;
                    case "https":
                        parser = s_SyntaxHttps;
                        break;
                }

                if (parser != null)
                {
                    // Associate the parser 
                    FieldInfo fieldInfo = typeof (Uri).GetField("m_Syntax",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (fieldInfo == null)
                    {
                        throw new MissingFieldException("'m_Syntax' field not found");
                    }
                    fieldInfo.SetValue(result, parser);
                }
            }
            return result;
        }
    }
}