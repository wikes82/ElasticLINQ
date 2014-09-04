﻿// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Request.Criteria;
using ElasticLinq.Request.Facets;
using ElasticLinq.Request.Visitors;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace ElasticLinq.Test.Request.Visitors.ElasticQueryTranslation
{
    public class ElasticQueryTranslationSingularTests : ElasticQueryTranslationTestsBase
    {
        [Fact]
        public void FirstTranslatesToSizeOfOneWithExistsFilter()
        {
            var first = MakeQueryableExpression("First", Robots);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal(1, request.Size);
            Assert.Equal("exists [id]", request.Filter.ToString());
        }

        [Fact]
        public void FirstOrDefaultTranslatesToSizeOfOneWithExistsFilter()
        {
            var first = MakeQueryableExpression("FirstOrDefault", Robots);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal(1, request.Size);
            Assert.Equal("exists [id]", request.Filter.ToString());
        }

        [Fact]
        public void FirstWithPredicateTranslatesToSizeOfOneWithFilter()
        {
            const string expectedTermValue = "Josef";
            Expression<Func<Robot, bool>> lambda = r => r.Name == expectedTermValue;
            var first = MakeQueryableExpression("First", Robots, lambda);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal(1, request.Size);
            var termCriteria = Assert.IsType<TermCriteria>(request.Filter);
            Assert.Equal("prefix.name", termCriteria.Field);
            Assert.Equal(expectedTermValue, termCriteria.Value);
        }

        [Fact]
        public void FirstOrDefaultWithPredicateTranslatesToSizeOfOneWithFilter()
        {
            const string expectedTermValue = "Josef";
            Expression<Func<Robot, bool>> lambda = r => r.Name == expectedTermValue;
            var first = MakeQueryableExpression("FirstOrDefault", Robots, lambda);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal(1, request.Size);
            var termCriteria = Assert.IsType<TermCriteria>(request.Filter);
            Assert.Equal("prefix.name", termCriteria.Field);
            Assert.Equal(expectedTermValue, termCriteria.Value);
        }

        [Fact]
        public void SingleTranslatesToSizeOfTwoWithNoFilter()
        {
            var first = MakeQueryableExpression("Single", Robots);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal(2, request.Size);
            Assert.Equal("exists [id]", request.Filter.ToString());
        }

        [Fact]
        public void SingleOrDefaultTranslatesToSizeOfTwoWithNoFilter()
        {
            var first = MakeQueryableExpression("SingleOrDefault", Robots);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal(2, request.Size);
            Assert.Equal("exists [id]", request.Filter.ToString());
        }

        [Fact]
        public void SingleWithPredicateTranslatesToSizeOfTwoWithFilter()
        {
            const string expectedTermValue = "Josef";
            Expression<Func<Robot, bool>> lambda = r => r.Name == expectedTermValue;
            var first = MakeQueryableExpression("Single", Robots, lambda);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal(2, request.Size);
            var termCriteria = Assert.IsType<TermCriteria>(request.Filter);
            Assert.Equal("prefix.name", termCriteria.Field);
            Assert.Equal(expectedTermValue, termCriteria.Value);
        }

        [Fact]
        public void SingleOrDefaultWithPredicateTranslatesToSizeOfTwoWithFilter()
        {
            const string expectedTermValue = "Josef";
            Expression<Func<Robot, bool>> lambda = r => r.Name == expectedTermValue;
            var first = MakeQueryableExpression("SingleOrDefault", Robots, lambda);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal(2, request.Size);
            var termCriteria = Assert.IsType<TermCriteria>(request.Filter);
            Assert.Equal("prefix.name", termCriteria.Field);
            Assert.Equal(expectedTermValue, termCriteria.Value);
        }

        private static Expression MakeQueryableExpression<TSource>(string name, IQueryable<TSource> source, params Expression[] parameters)
        {
            var method = MakeQueryableMethod<TSource>(name, parameters.Length + 1);
            return Expression.Call(method, new[] { source.Expression }.Concat(parameters).ToArray());
        }

        private static MethodInfo MakeQueryableMethod<TSource>(string name, int parameterCount)
        {
            return typeof(Queryable).FindMembers
                (MemberTypes.Method,
                    BindingFlags.Static | BindingFlags.Public,
                    (info, criteria) => info.Name.Equals(criteria), name)
                .OfType<MethodInfo>()
                .Single(a => a.GetParameters().Length == parameterCount)
                .MakeGenericMethod(typeof(TSource));
        }


        [Fact]
        public void CountTranslatesToSearchTypeOfCount()
        {
            var first = MakeQueryableExpression("Count", Robots);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal("count", request.SearchType);
            Assert.IsType<ExistsCriteria>(request.Filter);
        }

        [Fact]
        public void CountWithPredicateTranslatesToSearchTypeOfCount()
        {
            const string expectedTermValue = "Josef";
            Expression<Func<Robot, bool>> lambda = r => r.Name == expectedTermValue;
            var first = MakeQueryableExpression("Count", Robots, lambda);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal("count", request.SearchType);
            var termCriteria = Assert.IsType<TermCriteria>(request.Filter);
            Assert.Equal("prefix.name", termCriteria.Field);
            Assert.Equal(expectedTermValue, termCriteria.Value);
        }

        [Fact]
        public void CountTranslatesToFacetWhenGroupBy()
        {
            var first = Robots.GroupBy(g => 1).Select(a => a.Count());

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first.Expression).SearchRequest;

            Assert.Equal("count", request.SearchType);
            Assert.Null(request.Filter);

            var facet = Assert.Single(request.Facets);
            Assert.IsType<FilterFacet>(facet);
        }

        [Fact]
        public void LongCountTranslatesToSearchTypeOfCount()
        {
            var first = MakeQueryableExpression("LongCount", Robots);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal("count", request.SearchType);
            Assert.IsType<ExistsCriteria>(request.Filter);
        }

        [Fact]
        public void LongCountWithPredicateTranslatesToSearchTypeOfCount()
        {
            const string expectedTermValue = "Josef";
            Expression<Func<Robot, bool>> lambda = r => r.Name == expectedTermValue;
            var first = MakeQueryableExpression("LongCount", Robots, lambda);

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first).SearchRequest;

            Assert.Equal("count", request.SearchType);
            var termCriteria = Assert.IsType<TermCriteria>(request.Filter);
            Assert.Equal("prefix.name", termCriteria.Field);
            Assert.Equal(expectedTermValue, termCriteria.Value);
        }

        [Fact]
        public void LongCountTranslatesToFacetWhenGroupBy()
        {
            var first = Robots.GroupBy(g => 1).Select(a => a.LongCount());

            var request = ElasticQueryTranslator.Translate(Mapping, "prefix", first.Expression).SearchRequest;

            Assert.Equal("count", request.SearchType);
            Assert.Null(request.Filter);

            var facet = Assert.Single(request.Facets);
            Assert.IsType<FilterFacet>(facet);
        }
    }
}