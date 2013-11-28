﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Response.Model;
using ElasticLinq.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace ElasticLinq.Mapping
{
    /// <summary>
    /// Trivial mapping implementation to just lower case everything and pluralize type names.
    /// </summary>
    public class TrivialElasticMapping : IElasticMapping
    {
        public string GetFieldName(MemberInfo memberInfo)
        {
            Argument.EnsureNotNull("memberInfo", memberInfo);

            return memberInfo.Name.ToCamelCase();
        }

        public string GetTypeName(Type type)
        {
            Argument.EnsureNotNull("type", type);

            return type.Name.ToPlural().ToCamelCase();
        }

        public JToken GetObjectSource(Type docType, Hit hit)
        {
            Argument.EnsureNotNull("hit", hit);

            return hit._source;
        }
    }
}