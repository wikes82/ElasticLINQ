﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Mapping;
using ElasticLinq.Request;
using ElasticLinq.Request.Visitors;
using ElasticLinq.Response;
using ElasticLinq.Utility;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ElasticLinq
{
    /// <summary>
    /// Query provider implementation for ElasticSearch.
    /// </summary>
    public sealed class ElasticQueryProvider : IQueryProvider
    {
        private readonly ElasticConnection connection;
        private readonly IElasticMapping mapping;

        public TextWriter Log { get; set; }

        public ElasticQueryProvider(ElasticConnection connection, IElasticMapping mapping)
        {
            Argument.EnsureNotNull("connection", connection);
            Argument.EnsureNotNull("mapping", mapping);

            this.connection = connection;
            this.mapping = mapping;
        }

        internal ElasticConnection Connection
        {
            get { return connection; }
        }

        internal IElasticMapping Mapping
        {
            get { return mapping; }
        }

        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            Argument.EnsureNotNull("expresssion", expression);

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException("expression");

            return new ElasticQuery<T>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Argument.EnsureNotNull("expresssion", expression);

            var elementType = TypeHelper.GetSequenceElementType(expression.Type);
            var queryType = typeof(ElasticQuery<>).MakeGenericType(elementType);
            try
            {
                return (IQueryable)Activator.CreateInstance(queryType, new object[] { this, expression });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            Argument.EnsureNotNull("expresssion", expression);

            return (TResult)ExecuteInternal(expression);
        }

        public object Execute(Expression expression)
        {
            Argument.EnsureNotNull("expresssion", expression);

            return ExecuteInternal(expression);
        }

        private object ExecuteInternal(Expression expression)
        {
            var translation = ElasticQueryTranslator.Translate(mapping, expression);
            var elementType = TypeHelper.GetSequenceElementType(expression.Type);

            var log = Log ?? StreamWriter.Null;
            log.WriteLine("Type is " + elementType);

            var searchTask = new ElasticRequestProcessor(connection, log).Search(translation.SearchRequest);
            try
            {
                var response = searchTask.GetAwaiter().GetResult();
                if (response == null)
                    throw new InvalidOperationException("No HTTP response received.");

                var list = ElasticResponseMaterializer.Materialize(response.hits.hits, elementType, translation.Projector);
                return translation.FinalTransform(list);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}