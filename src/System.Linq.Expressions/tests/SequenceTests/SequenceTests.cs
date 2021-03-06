// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace System.Linq.Expressions.Tests
{
    public class Sequence_Tests
    {
        [Fact]
        public static void QueryableOfQueryable()
        {
            IQueryable<int> queryable1 = Queryable.AsQueryable<int>(new int[] { 1, 2, 3 });
            IQueryable<int>[] queryableArray1 = new IQueryable<int>[] { queryable1, queryable1 };
            IQueryable<IQueryable<int>> queryable2 = Queryable.AsQueryable<IQueryable<int>>(queryableArray1);
            ParameterExpression expression1 = Expression.Parameter(typeof(IQueryable<int>), "i");
            ParameterExpression[] expressionArray1 = new ParameterExpression[] { expression1 };
            IQueryable<IQueryable<int>> queryable3 = Queryable.Select<IQueryable<int>, IQueryable<int>>((IQueryable<IQueryable<int>>)queryable2, Expression.Lambda<Func<IQueryable<int>, IQueryable<int>>>(expression1, expressionArray1));
            int i = Queryable.Count<IQueryable<int>>((IQueryable<IQueryable<int>>)queryable3);
            Assert.Equal(2, i);
        }

        public struct CustomerRec
        {
            public string name;
            public int?[] total;
            public CustomerRec(string name, int?[] total) { this.name = name; this.total = total; }
        }

        public static IEnumerable<int?> indexSelector(CustomerRec cr, int index)
        {
            if (index == 4) return new int?[] { 23, 45, 61, 89 };
            else return new int?[] { };
        }

        [Fact]
        public static void SelectMany()
        {
            CustomerRec[] source = {new CustomerRec{name="Prakash", total=new int?[]{1, 2, 3, 4}},
                                    new CustomerRec{name="Bob", total=new int?[]{5, 6}},
                                    new CustomerRec{name="Chris", total=new int?[]{}},
                                    new CustomerRec{name=null, total=new int?[]{8, 9}},
                                    new CustomerRec{name="Prakash", total=new int?[]{-10, 100}}
                                };

            string[] expected = { "23", "45", "61", "89" };
            Func<CustomerRec, int, IEnumerable<int?>> collectionSelector = indexSelector;
            Func<CustomerRec, int?, string> resultSelector = (e, f) => f.ToString();

            var actual = source.SelectMany(collectionSelector, resultSelector);

            int i = 0;
            foreach (string s in actual)
            {
                Assert.Equal(s, expected[i]);
                i++;
            }
        }

        [Fact]
        public static void BasicWhere()
        {
            int[] ints = new int[] { 4, 1, 5, 3, 2 };
            foreach (int i in ints.Where(x => x < 4))
            {
                Assert.True(i < 4);
            }
        }

        [Fact]
        public static void BasicGroupBy()
        {
            int[] ints = new int[] { 4, 1, 5, 3, 2 };

            var vs = ints.Select(i => new { a = i, b = i / 2 }).GroupBy(g => g.b);

            foreach (var i in vs)
            {
                foreach (var j in i)
                {
                    Assert.Equal(j.a / 2, i.Key);
                }
            }
        }

        [Fact]
        public static void GroupBywithResultSelector()
        {
            char[] elements = { 'q', 'q', 'q', 'q', 'q' };

            var result = elements.GroupBy((e) => (e), (e, f) => new { Key = e, Element = f });

            foreach (var e in result)
            {
                Assert.Equal(5, e.Element.Count());
                foreach (var ele in e.Element)
                    Assert.Equal(ele, 'q');
            }
        }

        [Fact]
        public static void BasicAggregate()
        {
            int[] ints = new int[] { 4, 1, 5, 3, 2 };

            // Multiplies all the numbers in ints[].
            Assert.Equal(120, ints.Aggregate((current, i) => current * i));
        }

        private delegate bool Predicate<T0>(T0 p0);
        private delegate bool Predicate<T0, T1>(T0 p0, T1 p1);

        [Fact]
        public static void DistinctWithNulls()
        {
            int?[] ints = new int?[] { 1, 2, null, 4, 2, 1 };
            var distinct = ints.Distinct().ToList();
            Assert.Equal(4, distinct.Count);
            Assert.Equal(1, distinct[0]);
            Assert.Equal(2, distinct[1]);
            Assert.Null(distinct[2]);
            Assert.Equal(4, distinct[3]);
        }

        [Fact]
        public static void UnionWithNulls()
        {
            int?[] a = new int?[] { 1, 2, null, 4, 2, null, 1 };
            int?[] b = new int?[] { 2, 4, null, 5, null };
            var c = a.Union(b).ToList();
            Assert.Equal(5, c.Count);
            Assert.Equal(1, c[0]);
            Assert.Equal(2, c[1]);
            Assert.Null(c[2]);
            Assert.Equal(4, c[3]);
            Assert.Equal(5, c[4]);
        }

        [Fact]
        public static void IntersectWithNulls()
        {
            int?[] a = new int?[] { 1, 2, null, 4, 2, null, 1 };
            int?[] b = new int?[] { 2, 4, null, 5, null };
            var c = a.Intersect(b).ToList();
            Assert.Equal(3, c.Count);
            Assert.Equal(2, c[0]);
            Assert.Null(c[1]);
            Assert.Equal(4, c[2]);
        }

        [Fact]
        public static void ExceptWithNulls()
        {
            int?[] a = new int?[] { 1, 2, null, 4, 2, null, 1 };
            int?[] b = new int?[] { 2, 5 };
            var c = a.Except(b).ToList();
            Assert.Equal(3, c.Count);
            int?[] b2 = new int?[] { 2, 5, null };
            var c2 = a.Except(b2).ToList();
            Assert.Equal(2, c2.Count);
        }

        [Fact]
        public static void Sequence_NonList()
        {
            Assert.Equal(6, Enumerable.Range(0, 10).ElementAtOrDefault(6));
            Assert.Equal(default(int), Enumerable.Range(0, 10).ElementAtOrDefault(10));
            Assert.Equal(default(int), Enumerable.Range(0, 10).ElementAtOrDefault(-1));

            Assert.Equal(9, Enumerable.Range(0, 10).Last());
            Assert.Equal(9, Enumerable.Range(0, 10).LastOrDefault());
            Assert.Equal(default(int), Enumerable.Range(0, 0).LastOrDefault());
        }

        [Fact]
        public static void Contains_SourceNull()
        {
            IEnumerable<string> source = null;
            Assert.Throws<ArgumentNullException>("source", () => source.Contains(null));
        }

        public static IEnumerable<string> Contains_Source1()
        {
            var strings = new[] { "one", "two", null, "four" };

            foreach (var item in strings)
            {
                yield return item;
            }
        }

        public static IEnumerable<string> Contains_Source2()
        {
            var strings = new[] { "one", "two", "three", "four" };

            foreach (var item in strings)
            {
                yield return item;
            }
        }

        [Fact]
        public static void Contains_SourceNotCollection()
        {
            Assert.True(Contains_Source1().Contains(null));
            Assert.False(Contains_Source2().Contains(null));

            Assert.False(Contains_Source1().Contains("three"));
            Assert.True(Contains_Source2().Contains("three"));
        }

        public static IEnumerable<System.Int16> MaxMin_NumberSourceInt16()
        {
            for (System.Int16 i = 0; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Int32> MaxMin_NumberSourceInt32()
        {
            for (System.Int32 i = 0; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Int64> MaxMin_NumberSourceInt64()
        {
            for (System.Int64 i = 0; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.UInt16> MaxMin_NumberSourceUInt16()
        {
            for (System.UInt16 i = 0; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.UInt32> MaxMin_NumberSourceUInt32()
        {
            for (System.UInt32 i = 0; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.UInt64> MaxMin_NumberSourceUInt64()
        {
            for (System.UInt64 i = 0; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Single> MaxMin_NumberSourceSingle()
        {
            for (System.Single i = 0; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Double> MaxMin_NumberSourceDouble()
        {
            for (System.Double i = 0; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Decimal> MaxMin_NumberSourceDecimal()
        {
            for (System.Decimal i = 0; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Int16?> MaxMin_NullableNumberSourceInt16()
        {
            for (System.Int16 i = 0; i < 5; i++)
            {
                yield return i;
            }

            yield return null;

            for (System.Int16 i = 5; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Int32?> MaxMin_NullableNumberSourceInt32()
        {
            for (System.Int32 i = 0; i < 5; i++)
            {
                yield return i;
            }

            yield return null;

            for (System.Int32 i = 5; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Int64?> MaxMin_NullableNumberSourceInt64()
        {
            for (System.Int64 i = 0; i < 5; i++)
            {
                yield return i;
            }

            yield return null;

            for (System.Int64 i = 5; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.UInt16?> MaxMin_NullableNumberSourceUInt16()
        {
            for (System.UInt16 i = 0; i < 5; i++)
            {
                yield return i;
            }

            yield return null;

            for (System.UInt16 i = 5; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.UInt32?> MaxMin_NullableNumberSourceUInt32()
        {
            for (System.UInt32 i = 0; i < 5; i++)
            {
                yield return i;
            }

            yield return null;

            for (System.UInt32 i = 5; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.UInt64?> MaxMin_NullableNumberSourceUInt64()
        {
            for (System.UInt64 i = 0; i < 5; i++)
            {
                yield return i;
            }

            yield return null;

            for (System.UInt64 i = 5; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Single?> MaxMin_NullableNumberSourceSingle()
        {
            for (System.Single i = 0; i < 5; i++)
            {
                yield return i;
            }

            yield return null;

            for (System.Single i = 5; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Double?> MaxMin_NullableNumberSourceDouble()
        {
            for (System.Double i = 0; i < 5; i++)
            {
                yield return i;
            }

            yield return null;

            for (System.Double i = 5; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Decimal?> MaxMin_NullableNumberSourceDecimal()
        {
            for (System.Decimal i = 0; i < 5; i++)
            {
                yield return i;
            }

            yield return null;

            for (System.Decimal i = 5; i < 10; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<System.Int16?> MaxMin_AllNull()
        {
            for (System.Int16 i = 0; i < 10; i++)
            {
                yield return null;
            }
        }

        [Fact]
        public static void Range()
        {
            Assert.Equal(0, Enumerable.Range(0, 10).Min());
            Assert.Equal(9, Enumerable.Range(0, 10).Max());
            Assert.Equal(10, Enumerable.Range(0, 10).Count());
            Assert.Equal(0, Enumerable.Range(0, 0).Count());
        }

        [Fact]
        public static void Empty()
        {
            Assert.Equal(0, Enumerable.Empty<int>().Count());
        }

        [Fact]
        public static void RangeNegativeCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>("count", () => Enumerable.Range(0, -1));
        }

        [Fact]
        public static void MaxEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => Enumerable.Empty<int>().Max());
        }

        [Fact]
        public static void MinEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => Enumerable.Empty<int>().Min());
        }

        [Fact]
        public static void MaxNullSource()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).Max());
        }

        [Fact]
        public static void MinNullSource()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).Min());
        }

        [Fact]
        public static void MaxMin()
        {
            Assert.Null(MaxMin_AllNull().Cast<System.Int16?>().Max());
            Assert.Null(MaxMin_AllNull().Cast<System.Int32?>().Max());
            Assert.Null(MaxMin_AllNull().Cast<System.Int64?>().Max());
            Assert.Null(MaxMin_AllNull().Cast<System.UInt16?>().Max());
            Assert.Null(MaxMin_AllNull().Cast<System.UInt32?>().Max());
            Assert.Null(MaxMin_AllNull().Cast<System.UInt64?>().Max());
            Assert.Null(MaxMin_AllNull().Cast<System.Single?>().Max());
            Assert.Null(MaxMin_AllNull().Cast<System.Double?>().Max());
            Assert.Null(MaxMin_AllNull().Cast<System.Decimal?>().Max());

            Assert.Null(MaxMin_AllNull().Cast<System.Int16?>().Min());
            Assert.Null(MaxMin_AllNull().Cast<System.Int32?>().Min());
            Assert.Null(MaxMin_AllNull().Cast<System.Int64?>().Min());
            Assert.Null(MaxMin_AllNull().Cast<System.UInt16?>().Min());
            Assert.Null(MaxMin_AllNull().Cast<System.UInt32?>().Min());
            Assert.Null(MaxMin_AllNull().Cast<System.UInt64?>().Min());
            Assert.Null(MaxMin_AllNull().Cast<System.Single?>().Min());
            Assert.Null(MaxMin_AllNull().Cast<System.Double?>().Min());
            Assert.Null(MaxMin_AllNull().Cast<System.Decimal?>().Min());


            Assert.Equal(9, MaxMin_NumberSourceInt16().Max());
            Assert.Equal(9, MaxMin_NumberSourceInt32().Max());
            Assert.Equal(9, MaxMin_NumberSourceInt64().Max());
            Assert.Equal(9, MaxMin_NumberSourceUInt16().Max());
            Assert.Equal((uint)9, MaxMin_NumberSourceUInt32().Max());
            Assert.Equal((ulong)9, MaxMin_NumberSourceUInt64().Max());
            Assert.Equal(9, MaxMin_NumberSourceSingle().Max());
            Assert.Equal(9, MaxMin_NumberSourceDouble().Max());
            Assert.Equal(9, MaxMin_NumberSourceDecimal().Max());

            Assert.Equal(0, MaxMin_NumberSourceInt16().Min());
            Assert.Equal(0, MaxMin_NumberSourceInt32().Min());
            Assert.Equal(0, MaxMin_NumberSourceInt64().Min());
            Assert.Equal(0, MaxMin_NumberSourceUInt16().Min());
            Assert.Equal((uint)0, MaxMin_NumberSourceUInt32().Min());
            Assert.Equal((ulong)0, MaxMin_NumberSourceUInt64().Min());
            Assert.Equal(0, MaxMin_NumberSourceSingle().Min());
            Assert.Equal(0, MaxMin_NumberSourceDouble().Min());
            Assert.Equal(0, MaxMin_NumberSourceDecimal().Min());


            Assert.Equal((short?)9, MaxMin_NullableNumberSourceInt16().Max());
            Assert.Equal(9, MaxMin_NullableNumberSourceInt32().Max());
            Assert.Equal(9, MaxMin_NullableNumberSourceInt64().Max());
            Assert.Equal((ushort?)9, MaxMin_NullableNumberSourceUInt16().Max());
            Assert.Equal((uint?)9, MaxMin_NullableNumberSourceUInt32().Max());
            Assert.Equal((ulong?)9, MaxMin_NullableNumberSourceUInt64().Max());
            Assert.Equal(9, MaxMin_NullableNumberSourceSingle().Max());
            Assert.Equal(9, MaxMin_NullableNumberSourceDouble().Max());
            Assert.Equal(9, MaxMin_NullableNumberSourceDecimal().Max());

            Assert.Equal((short?)0, MaxMin_NullableNumberSourceInt16().Min());
            Assert.Equal(0, MaxMin_NullableNumberSourceInt32().Min());
            Assert.Equal(0, MaxMin_NullableNumberSourceInt64().Min());
            Assert.Equal((ushort?)0, MaxMin_NullableNumberSourceUInt16().Min());
            Assert.Equal((uint?)0, MaxMin_NullableNumberSourceUInt32().Min());
            Assert.Equal((ulong?)0, MaxMin_NullableNumberSourceUInt64().Min());
            Assert.Equal(0, MaxMin_NullableNumberSourceSingle().Min());
            Assert.Equal(0, MaxMin_NullableNumberSourceDouble().Min());
            Assert.Equal(0, MaxMin_NullableNumberSourceDecimal().Min());


            Assert.Null(MaxMin_AllNull().Cast<System.Int16?>().Max(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Int32?>().Max(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Int64?>().Max(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.UInt16?>().Max(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.UInt32?>().Max(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.UInt64?>().Max(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Single?>().Max(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Double?>().Max(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Decimal?>().Max(item => item));

            Assert.Null(MaxMin_AllNull().Cast<System.Int16?>().Min(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Int32?>().Min(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Int64?>().Min(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.UInt16?>().Min(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.UInt32?>().Min(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.UInt64?>().Min(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Single?>().Min(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Double?>().Min(item => item));
            Assert.Null(MaxMin_AllNull().Cast<System.Decimal?>().Min(item => item));


            Assert.Equal(9, MaxMin_NumberSourceInt16().Max(item => item));
            Assert.Equal(9, MaxMin_NumberSourceInt32().Max(item => item));
            Assert.Equal(9, MaxMin_NumberSourceInt64().Max(item => item));
            Assert.Equal(9, MaxMin_NumberSourceUInt16().Max(item => item));
            Assert.Equal((uint)9, MaxMin_NumberSourceUInt32().Max(item => item));
            Assert.Equal((ulong)9, MaxMin_NumberSourceUInt64().Max(item => item));
            Assert.Equal(9, MaxMin_NumberSourceSingle().Max(item => item));
            Assert.Equal(9, MaxMin_NumberSourceDouble().Max(item => item));
            Assert.Equal(9, MaxMin_NumberSourceDecimal().Max(item => item));

            Assert.Equal(0, MaxMin_NumberSourceInt16().Min(item => item));
            Assert.Equal(0, MaxMin_NumberSourceInt32().Min(item => item));
            Assert.Equal(0, MaxMin_NumberSourceInt64().Min(item => item));
            Assert.Equal(0, MaxMin_NumberSourceUInt16().Min(item => item));
            Assert.Equal((uint)0, MaxMin_NumberSourceUInt32().Min(item => item));
            Assert.Equal((ulong)0, MaxMin_NumberSourceUInt64().Min(item => item));
            Assert.Equal(0, MaxMin_NumberSourceSingle().Min(item => item));
            Assert.Equal(0, MaxMin_NumberSourceDouble().Min(item => item));
            Assert.Equal(0, MaxMin_NumberSourceDecimal().Min(item => item));


            Assert.Equal((short?)9, MaxMin_NullableNumberSourceInt16().Max(item => item));
            Assert.Equal(9, MaxMin_NullableNumberSourceInt32().Max(item => item));
            Assert.Equal(9, MaxMin_NullableNumberSourceInt64().Max(item => item));
            Assert.Equal((ushort?)9, MaxMin_NullableNumberSourceUInt16().Max(item => item));
            Assert.Equal((ushort?)9, MaxMin_NullableNumberSourceUInt32().Max(item => item));
            Assert.Equal((ulong?)9, MaxMin_NullableNumberSourceUInt64().Max(item => item));
            Assert.Equal(9, MaxMin_NullableNumberSourceSingle().Max(item => item));
            Assert.Equal(9, MaxMin_NullableNumberSourceDouble().Max(item => item));
            Assert.Equal(9, MaxMin_NullableNumberSourceDecimal().Max(item => item));

            Assert.Equal((short?)0, MaxMin_NullableNumberSourceInt16().Min(item => item));
            Assert.Equal(0, MaxMin_NullableNumberSourceInt32().Min(item => item));
            Assert.Equal(0, MaxMin_NullableNumberSourceInt64().Min(item => item));
            Assert.Equal((ushort?)0, MaxMin_NullableNumberSourceUInt16().Min(item => item));
            Assert.Equal((uint?)0, MaxMin_NullableNumberSourceUInt32().Min(item => item));
            Assert.Equal((ulong?)0, MaxMin_NullableNumberSourceUInt64().Min(item => item));
            Assert.Equal(0, MaxMin_NullableNumberSourceSingle().Min(item => item));
            Assert.Equal(0, MaxMin_NullableNumberSourceDouble().Min(item => item));
            Assert.Equal(0, MaxMin_NullableNumberSourceDecimal().Min(item => item));
        }

        [Fact]
        public static void Lookup()
        {
            Assert.Equal(9, Enumerable.Range(0, 10).ToDictionary(t => t.ToString())["9"]);

            ILookup<string, int> lookup = Enumerable.Range(0, 10).ToLookup(t => t.ToString(), t => t, EqualityComparer<string>.Default);

            Assert.Equal(1, ((ICollection<int>)lookup["9"]).Count);

            Assert.Equal(1, ((ICollection<int>)lookup["9"]).Count);

            Assert.Equal(9, ((IList<int>)lookup["9"])[0]);

            Assert.False(((ICollection<int>)lookup["9"]).Contains(3));

            Assert.True(((ICollection<int>)lookup["9"]).IsReadOnly);
        }

        [Fact]
        public static void GroupingKeyIsPublic()
        {
            // Grouping.Key needs to be public (not explicitly implemented) for the sake of WPF.

            object[] objs = { "Foo", 1.0M, "Bar", new { X = "X" }, 2.00M };
            object group = objs.GroupBy(x => x.GetType()).First();

            Type grouptype = group.GetType();
            PropertyInfo key = grouptype.GetProperty("Key", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(key);
        }
    }

    public class Expression_Tests
    {
        [Fact]
        public static void NewMakeBinary()
        {
            int? i = 10;
            double? j = 23.89;
            var left = Expression.Constant(i, typeof(Nullable<int>));
            var right = Expression.Constant(j, typeof(Nullable<double>));
            Expression<Func<int, double?>> conversion = (int k) => (double?)i;
            var v = Expression.MakeBinary(ExpressionType.Coalesce, left, right, false, null, conversion);
            Assert.NotNull(v);
        }

        [Fact]
        public static void ListInit()
        {
            NewExpression newExpr = Expression.New(typeof(List<int>));
            ConstantExpression init1 = Expression.Constant(4, typeof(int));
            Expression[] inits = new Expression[] { null, init1 };

            Assert.Throws<ArgumentNullException>("argument", () => Expression.ListInit(newExpr, inits));

            ElementInit[] einits = new ElementInit[] { };
            Assert.Throws<ArgumentException>(() => Expression.ListInit(newExpr, einits));
        }

        public void Add(ref int i)
        {
        }

        [Fact]
        public static void ElementInit()
        {
            MethodInfo mi1 = typeof(Expression_Tests).GetMethod("Add");
            ConstantExpression ce1 = Expression.Constant(4, typeof(int));

            Assert.Throws<ArgumentException>(() => Expression.ElementInit(mi1, new Expression[] { ce1 }));
        }

        public class Atom
        {
            public static implicit operator bool (Atom atom) { return true; }
            public Atom this[Atom b0] { get { return null; } }
            public Atom this[Atom b0, Atom b1] { get { return null; } }
        }

        [Fact]
        public static void EqualityBetweenReferenceAndInterfacesSucceeds()
        {
            // able to build reference comparison between interfaces
            Expression.Equal(Expression.Constant(null, typeof(IEnumerable)), Expression.Constant(null, typeof(IComparer)));
            Expression.NotEqual(Expression.Constant(null, typeof(IEnumerable)), Expression.Constant(null, typeof(IComparer)));

            // able to build reference comparison between reference type and interfaces
            Expression.Equal(Expression.Constant(null, typeof(BaseClass)), Expression.Constant(null, typeof(IEnumerable)));
            Expression.Equal(Expression.Constant(null, typeof(IEnumerable)), Expression.Constant(null, typeof(BaseClass)));
            Expression.NotEqual(Expression.Constant(null, typeof(BaseClass)), Expression.Constant(null, typeof(IEnumerable)));
            Expression.NotEqual(Expression.Constant(null, typeof(IEnumerable)), Expression.Constant(null, typeof(BaseClass)));
        }

        [Fact]
        public static void EqualityBetweenStructAndIterfaceFails()
        {
            Expression expStruct = Expression.Constant(5);
            Expression expIface = Expression.Constant(null, typeof(IComparable));
            Assert.Throws<InvalidOperationException>(() => Expression.Equal(expStruct, expIface));
        }

        [Fact]
        public static void EqualityBetweenInheritedTypesSucceeds()
        {
            Expression.Equal(Expression.Constant(null, typeof(BaseClass)), Expression.Constant(null, typeof(DerivedClass)));
            Expression.Equal(Expression.Constant(null, typeof(DerivedClass)), Expression.Constant(null, typeof(BaseClass)));
            Expression.NotEqual(Expression.Constant(null, typeof(BaseClass)), Expression.Constant(null, typeof(DerivedClass)));
            Expression.NotEqual(Expression.Constant(null, typeof(DerivedClass)), Expression.Constant(null, typeof(BaseClass)));
        }

        [Fact]
        public static void Regress_ThisPropertyCanBeOverloaded()
        {
            Expression<Predicate<Atom>> d = atom => atom && atom[atom];
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void Lambda(bool useInterpreter)
        {
            var paramI = Expression.Parameter(typeof(int), "i");
            var paramJ = Expression.Parameter(typeof(double), "j");
            var paramK = Expression.Parameter(typeof(decimal), "k");
            var paramL = Expression.Parameter(typeof(short), "l");

            Expression lambda = (Expression<Func<int, double, decimal, int>>)((int i, double j, decimal k) => i);

            Assert.Equal(typeof(Func<int, double, decimal, Func<int, double, decimal, int>>),
                Expression.Lambda(lambda, new[] { paramI, paramJ, paramK }).Type);

            lambda = (Expression<Func<int, double, decimal, short, int>>)((int i, double j, decimal k, short l) => i);

            Assert.Equal(typeof(Func<int, double, decimal, short, Func<int, double, decimal, short, int>>),
                Expression.Lambda(lambda, new[] { paramI, paramJ, paramK, paramL }).Compile(useInterpreter).GetType());

            Assert.False(String.IsNullOrEmpty(lambda.ToString()));
        }

        [Fact]
        public static void Arrays()
        {
            Expression<Func<int, int[]>> exp1 = i => new int[i];
            NewArrayExpression aex1 = exp1.Body as NewArrayExpression;
            Assert.NotNull(aex1);
            Assert.Equal(aex1.NodeType, ExpressionType.NewArrayBounds);

            Expression<Func<int[], int>> exp2 = (i) => i.Length;
            UnaryExpression uex2 = exp2.Body as UnaryExpression;
            Assert.NotNull(uex2);
            Assert.Equal(uex2.NodeType, ExpressionType.ArrayLength);
        }

        private void Method3<T, U, V>()
        {
        }

        private void Method4<T, U, V, W>()
        {
        }

        private void Method()
        {
        }

        [Fact]
        public static void CheckedExpressions()
        {
            Expression<Func<int, int, int>> exp = (a, b) => a + b;
            BinaryExpression bex = exp.Body as BinaryExpression;
            Assert.NotNull(bex);
            Assert.Equal(bex.NodeType, ExpressionType.Add);

            exp = (a, b) => checked(a + b);
            bex = exp.Body as BinaryExpression;
            Assert.NotNull(bex);
            Assert.Equal(bex.NodeType, ExpressionType.AddChecked);

            exp = (a, b) => a * b;
            bex = exp.Body as BinaryExpression;
            Assert.NotNull(bex);
            Assert.Equal(bex.NodeType, ExpressionType.Multiply);

            exp = (a, b) => checked(a * b);
            bex = exp.Body as BinaryExpression;
            Assert.NotNull(bex);
            Assert.Equal(bex.NodeType, ExpressionType.MultiplyChecked);

            Expression<Func<double, int>> exp2 = (a) => (int)a;
            UnaryExpression uex = exp2.Body as UnaryExpression;
            Assert.NotNull(uex);
            Assert.Equal(uex.NodeType, ExpressionType.Convert);

            exp2 = (a) => checked((int)a);
            uex = exp2.Body as UnaryExpression;
            Assert.NotNull(uex);
            Assert.Equal(uex.NodeType, ExpressionType.ConvertChecked);
        }

        protected virtual int Foo(int x)
        {
            return x;
        }

        [Fact]
        public static void VirtualCallExpressions()
        {
            Expression_Tests obj = new Expression_Tests();
            Expression<Func<int, int>> exp = x => obj.Foo(x);
            MethodCallExpression mc = exp.Body as MethodCallExpression;
            Assert.NotNull(mc);
            Assert.Equal(ExpressionType.Call, mc.NodeType);
        }


        [Fact]
        public static void ConstantNullWithValueTypeIsInvalid()
        {
            Assert.Throws<ArgumentException>(() => Expression.Constant(null, typeof(int)));
        }

        [Fact]
        public static void ConstantNullWithNullableValueType()
        {
            Expression.Constant(null, typeof(int?));
        }

        [Fact]
        public static void ConstantNullWithReferenceType()
        {
            Expression.Constant(null, typeof(Expression_Tests));
        }

        [Fact]
        public static void ConstantIntWithInterface()
        {
            Expression.Constant(10, typeof(IComparable));
        }

        [Fact]
        public static void ConstantIntWithNullableInt()
        {
            Expression.Constant(10, typeof(int?));
        }

        [Fact]
        public static void TestUserDefinedOperators()
        {
            TestUserDefinedMathOperators<U, U>();
            TestUserDefinedComparisonOperators<U, U>();
            TestUserDefinedBitwiseOperators<U, U>();
            TestUserDefinedMathOperators<U?, U?>();
            TestUserDefinedComparisonOperators<U?, U?>();
            TestUserDefinedBitwiseOperators<U?, U?>();

            TestUserDefinedComparisonOperators<B, B>();
            TestUserDefinedLogicalOperators<B, B>();
            TestUserDefinedComparisonOperators<B?, B?>();
            TestUserDefinedLogicalOperators<B?, B?>();

            TestUserDefinedMathOperators<M, N>();
            TestUserDefinedMathOperators<M?, N?>();
        }

        internal static bool IsNullableType(Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        internal static Type GetNonNullableType(Type type)
        {
            if (IsNullableType(type))
                type = type.GetGenericArguments()[0];
            return type;
        }

        public static void TestUserDefinedMathOperators<X, Y>()
        {
            ParameterExpression x = Expression.Parameter(typeof(X), "x");
            ParameterExpression y = Expression.Parameter(typeof(Y), "y");
            Type nnX = GetNonNullableType(typeof(X));
            Type nnY = GetNonNullableType(typeof(Y));
            AssertIsOp(Expression.Add(x, y), "op_Addition");
            AssertIsOp(Expression.Add(x, y, null), "op_Addition");
            AssertIsOp(Expression.Add(x, y, nnX.GetMethod("op_Subtraction")), "op_Subtraction");
            AssertIsOp(Expression.AddChecked(x, y), "op_Addition");
            AssertIsOp(Expression.AddChecked(x, y, null), "op_Addition");
            AssertIsOp(Expression.AddChecked(x, y, nnX.GetMethod("op_Subtraction")), "op_Subtraction");
            AssertIsOp(Expression.Subtract(x, y), "op_Subtraction");
            AssertIsOp(Expression.Subtract(x, y, null), "op_Subtraction");
            AssertIsOp(Expression.Subtract(x, y, nnX.GetMethod("op_Addition")), "op_Addition");
            AssertIsOp(Expression.SubtractChecked(x, y), "op_Subtraction");
            AssertIsOp(Expression.SubtractChecked(x, y, null), "op_Subtraction");
            AssertIsOp(Expression.SubtractChecked(x, y, nnX.GetMethod("op_Addition")), "op_Addition");
            AssertIsOp(Expression.Multiply(x, y), "op_Multiply");
            AssertIsOp(Expression.Multiply(x, y, null), "op_Multiply");
            AssertIsOp(Expression.Multiply(x, y, nnY.GetMethod("op_Division")), "op_Division");
            AssertIsOp(Expression.MultiplyChecked(x, y), "op_Multiply");
            AssertIsOp(Expression.MultiplyChecked(x, y, null), "op_Multiply");
            AssertIsOp(Expression.MultiplyChecked(x, y, nnY.GetMethod("op_Division")), "op_Division");
            AssertIsOp(Expression.Divide(x, y), "op_Division");
            AssertIsOp(Expression.Divide(x, y, null), "op_Division");
            AssertIsOp(Expression.Divide(x, y, nnY.GetMethod("op_Multiply")), "op_Multiply");
            AssertIsOp(Expression.Negate(x), "op_UnaryNegation");
            AssertIsOp(Expression.Negate(x, null), "op_UnaryNegation");
            AssertIsOp(Expression.Negate(x, nnX.GetMethod("op_OnesComplement")), "op_OnesComplement");
            AssertIsOp(Expression.NegateChecked(x), "op_UnaryNegation");
            AssertIsOp(Expression.NegateChecked(x, null), "op_UnaryNegation");
            AssertIsOp(Expression.NegateChecked(x, nnX.GetMethod("op_OnesComplement")), "op_OnesComplement");
        }

        public static void TestUserDefinedComparisonOperators<X, Y>()
        {
            ParameterExpression x = Expression.Parameter(typeof(X), "x");
            ParameterExpression y = Expression.Parameter(typeof(Y), "y");
            Type nnX = GetNonNullableType(typeof(X));
            AssertIsOp(Expression.LessThan(x, y), "op_LessThan");
            AssertIsOp(Expression.LessThan(x, y, false, null), "op_LessThan");
            AssertIsOp(Expression.LessThan(x, y, true, null), "op_LessThan");
            AssertIsOp(Expression.LessThan(x, y, false, nnX.GetMethod("op_GreaterThan")), "op_GreaterThan");
            AssertIsOp(Expression.LessThan(x, y, true, nnX.GetMethod("op_GreaterThan")), "op_GreaterThan");
            AssertIsOp(Expression.LessThanOrEqual(x, y), "op_LessThanOrEqual");
            AssertIsOp(Expression.LessThanOrEqual(x, y, false, null), "op_LessThanOrEqual");
            AssertIsOp(Expression.LessThanOrEqual(x, y, true, null), "op_LessThanOrEqual");
            AssertIsOp(Expression.LessThanOrEqual(x, y, false, nnX.GetMethod("op_GreaterThan")), "op_GreaterThan");
            AssertIsOp(Expression.LessThanOrEqual(x, y, true, nnX.GetMethod("op_GreaterThan")), "op_GreaterThan");
            AssertIsOp(Expression.GreaterThan(x, y), "op_GreaterThan");
            AssertIsOp(Expression.GreaterThan(x, y, false, null), "op_GreaterThan");
            AssertIsOp(Expression.GreaterThan(x, y, true, null), "op_GreaterThan");
            AssertIsOp(Expression.GreaterThan(x, y, false, nnX.GetMethod("op_LessThan")), "op_LessThan");
            AssertIsOp(Expression.GreaterThan(x, y, true, nnX.GetMethod("op_LessThan")), "op_LessThan");
            AssertIsOp(Expression.GreaterThanOrEqual(x, y), "op_GreaterThanOrEqual");
            AssertIsOp(Expression.GreaterThanOrEqual(x, y, false, null), "op_GreaterThanOrEqual");
            AssertIsOp(Expression.GreaterThanOrEqual(x, y, true, null), "op_GreaterThanOrEqual");
            AssertIsOp(Expression.GreaterThanOrEqual(x, y, false, nnX.GetMethod("op_LessThan")), "op_LessThan");
            AssertIsOp(Expression.GreaterThanOrEqual(x, y, true, nnX.GetMethod("op_LessThan")), "op_LessThan");
            AssertIsOp(Expression.Equal(x, y), "op_Equality");
            AssertIsOp(Expression.Equal(x, y, false, null), "op_Equality");
            AssertIsOp(Expression.Equal(x, y, true, null), "op_Equality");
            AssertIsOp(Expression.Equal(x, y, false, nnX.GetMethod("op_Inequality")), "op_Inequality");
            AssertIsOp(Expression.Equal(x, y, true, nnX.GetMethod("op_Inequality")), "op_Inequality");
            AssertIsOp(Expression.NotEqual(x, y), "op_Inequality");
            AssertIsOp(Expression.NotEqual(x, y, false, null), "op_Inequality");
            AssertIsOp(Expression.NotEqual(x, y, true, null), "op_Inequality");
            AssertIsOp(Expression.NotEqual(x, y, false, nnX.GetMethod("op_Equality")), "op_Equality");
            AssertIsOp(Expression.NotEqual(x, y, true, nnX.GetMethod("op_Equality")), "op_Equality");
        }

        public static void TestUserDefinedBitwiseOperators<X, Y>()
        {
            ParameterExpression x = Expression.Parameter(typeof(X), "x");
            ParameterExpression y = Expression.Parameter(typeof(Y), "y");
            Type nnX = GetNonNullableType(typeof(X));
            AssertIsOp(Expression.And(x, y), "op_BitwiseAnd");
            AssertIsOp(Expression.And(x, y, null), "op_BitwiseAnd");
            AssertIsOp(Expression.And(x, y, nnX.GetMethod("op_BitwiseOr")), "op_BitwiseOr");
            AssertIsOp(Expression.Or(x, y), "op_BitwiseOr");
            AssertIsOp(Expression.Or(x, y, null), "op_BitwiseOr");
            AssertIsOp(Expression.Or(x, y, nnX.GetMethod("op_BitwiseAnd")), "op_BitwiseAnd");
            AssertIsOp(Expression.ExclusiveOr(x, y), "op_ExclusiveOr");
            AssertIsOp(Expression.ExclusiveOr(x, y, null), "op_ExclusiveOr");
            AssertIsOp(Expression.ExclusiveOr(x, y, nnX.GetMethod("op_BitwiseAnd")), "op_BitwiseAnd");
            AssertIsOp(Expression.Not(x), "op_OnesComplement");
            AssertIsOp(Expression.Not(x, null), "op_OnesComplement");
            AssertIsOp(Expression.Not(x, nnX.GetMethod("op_UnaryNegation")), "op_UnaryNegation");
        }

        public static void TestUserDefinedLogicalOperators<X, Y>()
        {
            ParameterExpression x = Expression.Parameter(typeof(X), "x");
            ParameterExpression y = Expression.Parameter(typeof(Y), "y");
            Type nnX = GetNonNullableType(typeof(X));
            AssertIsOp(Expression.And(x, y), "op_BitwiseAnd");
            AssertIsOp(Expression.And(x, y, null), "op_BitwiseAnd");
            AssertIsOp(Expression.And(x, y, nnX.GetMethod("op_BitwiseOr")), "op_BitwiseOr");
            AssertIsOp(Expression.Or(x, y), "op_BitwiseOr");
            AssertIsOp(Expression.Or(x, y, null), "op_BitwiseOr");
            AssertIsOp(Expression.Or(x, y, nnX.GetMethod("op_BitwiseAnd")), "op_BitwiseAnd");
            AssertIsOp(Expression.ExclusiveOr(x, y), "op_ExclusiveOr");
            AssertIsOp(Expression.ExclusiveOr(x, y, null), "op_ExclusiveOr");
            AssertIsOp(Expression.ExclusiveOr(x, y, nnX.GetMethod("op_BitwiseAnd")), "op_BitwiseAnd");
            AssertIsOp(Expression.Not(x), "op_LogicalNot");
            AssertIsOp(Expression.Not(x, null), "op_LogicalNot");
            AssertIsOp(Expression.Not(x, nnX.GetMethod("op_UnaryNegation")), "op_UnaryNegation");
        }

        public static void AssertIsOp(BinaryExpression b, string opName)
        {
            Assert.NotNull(b.Method);
            Assert.Equal(opName, b.Method.Name);
        }

        public static void AssertIsOp(UnaryExpression u, string opName)
        {
            Assert.NotNull(u.Method);
            Assert.Equal(opName, u.Method.Name);
        }


        [Fact]
        public static void TestUserDefinedCoercions()
        {
            TestUserDefinedCoercion<M, N>();
            TestUserDefinedCoercion<M, N?>();
            TestUserDefinedCoercion<M?, N>();
            TestUserDefinedCoercion<M?, N?>();
        }

        public static void TestUserDefinedCoercion<X, Y>()
        {
            ParameterExpression x = Expression.Parameter(typeof(X), "x");
            ParameterExpression y = Expression.Parameter(typeof(Y), "y");
            Type nnX = GetNonNullableType(typeof(X));
            Type nnY = GetNonNullableType(typeof(Y));

            AssertIsCoercion(Expression.Convert(x, typeof(Y)), "op_Implicit", typeof(Y));
            AssertIsCoercion(Expression.Convert(x, typeof(Y), null), "op_Implicit", typeof(Y));
            AssertIsCoercion(Expression.Convert(x, typeof(Y), nnX.GetMethod("Foo")), "Foo", typeof(Y));
            AssertIsCoercion(Expression.Convert(y, typeof(X)), "op_Explicit", typeof(X));
            AssertIsCoercion(Expression.Convert(y, typeof(X), null), "op_Explicit", typeof(X));
            AssertIsCoercion(Expression.Convert(y, typeof(X), nnY.GetMethod("Bar")), "Bar", typeof(X));
            AssertIsCoercion(Expression.ConvertChecked(x, typeof(Y)), "op_Implicit", typeof(Y));
            AssertIsCoercion(Expression.ConvertChecked(x, typeof(Y), null), "op_Implicit", typeof(Y));
            AssertIsCoercion(Expression.ConvertChecked(x, typeof(Y), nnX.GetMethod("Foo")), "Foo", typeof(Y));
            AssertIsCoercion(Expression.ConvertChecked(y, typeof(X)), "op_Explicit", typeof(X));
            AssertIsCoercion(Expression.ConvertChecked(y, typeof(X), null), "op_Explicit", typeof(X));
            AssertIsCoercion(Expression.ConvertChecked(y, typeof(X), nnY.GetMethod("Bar")), "Bar", typeof(X));
            AssertIsCoercion(Expression.Convert(x, typeof(Y)), "op_Implicit", typeof(Y));
            AssertIsCoercion(Expression.Convert(x, typeof(Y), null), "op_Implicit", typeof(Y));
            AssertIsCoercion(Expression.Convert(x, typeof(Y), nnX.GetMethod("Foo")), "Foo", typeof(Y));
            AssertIsCoercion(Expression.Convert(y, typeof(X)), "op_Explicit", typeof(X));
            AssertIsCoercion(Expression.Convert(y, typeof(X), null), "op_Explicit", typeof(X));
            AssertIsCoercion(Expression.Convert(y, typeof(X), nnY.GetMethod("Bar")), "Bar", typeof(X));
        }

        public static void AssertIsCoercion(UnaryExpression u, string opName, Type expected)
        {
            Debug.WriteLine("Convert: {0} -> {1}", u.Operand.Type, u.Type);
            Assert.NotNull(u.Method);
            Assert.Equal(opName, u.Method.Name);
            Assert.Equal(expected, u.Type);
        }

        [Fact]
        public static void TestFuncLambda()
        {
            ParameterExpression p = Expression.Parameter(typeof(NWindProxy.Customer), "c");
            Expression body = Expression.PropertyOrField(p, "contactname");
            LambdaExpression lambda = Expression.Lambda(body, p);
            Assert.Equal(typeof(Func<NWindProxy.Customer, string>), lambda.Type);
        }

        [Fact]
        public static void TestGetFuncType()
        {
            // 1 type arg Func
            Type type = Expression.GetFuncType(new Type[] { typeof(int) });
            Assert.True(type.GetTypeInfo().IsGenericType);
            Assert.Equal(1, type.GetGenericArguments().Length);
            Assert.Equal(typeof(int), type.GetGenericArguments()[0]);

            // 2 type arg Func
            type = Expression.GetFuncType(new Type[] { typeof(int), typeof(string) });
            Assert.True(type.GetTypeInfo().IsGenericType);
            Assert.Equal(typeof(Func<,>), type.GetGenericTypeDefinition());
            Assert.Equal(typeof(int), type.GetGenericArguments()[0]);
            Assert.Equal(typeof(string), type.GetGenericArguments()[1]);

            // 3 type arg Func
            type = Expression.GetFuncType(new Type[] { typeof(string), typeof(int), typeof(decimal) });
            Assert.True(type.GetTypeInfo().IsGenericType);
            Assert.Equal(typeof(Func<,,>), type.GetGenericTypeDefinition());
            Assert.Equal(typeof(string), type.GetGenericArguments()[0]);
            Assert.Equal(typeof(int), type.GetGenericArguments()[1]);
            Assert.Equal(typeof(decimal), type.GetGenericArguments()[2]);

            // 4 type arg Func
            type = Expression.GetFuncType(new Type[] { typeof(string), typeof(int), typeof(decimal), typeof(float) });
            Assert.True(type.GetTypeInfo().IsGenericType);
            Assert.Equal(typeof(Func<,,,>), type.GetGenericTypeDefinition());
            Assert.Equal(typeof(string), type.GetGenericArguments()[0]);
            Assert.Equal(typeof(int), type.GetGenericArguments()[1]);
            Assert.Equal(typeof(decimal), type.GetGenericArguments()[2]);
            Assert.Equal(typeof(float), type.GetGenericArguments()[3]);

            // 5 type arg Func
            type = Expression.GetFuncType(new Type[] { typeof(NWindProxy.Customer), typeof(string), typeof(int), typeof(decimal), typeof(float) });
            Assert.True(type.GetTypeInfo().IsGenericType);
            Assert.Equal(typeof(Func<,,,,>), type.GetGenericTypeDefinition());
            Assert.Equal(typeof(NWindProxy.Customer), type.GetGenericArguments()[0]);
            Assert.Equal(typeof(string), type.GetGenericArguments()[1]);
            Assert.Equal(typeof(int), type.GetGenericArguments()[2]);
            Assert.Equal(typeof(decimal), type.GetGenericArguments()[3]);
            Assert.Equal(typeof(float), type.GetGenericArguments()[4]);
        }

        [Fact]
        public static void TestGetFuncTypeWithNullFails()
        {
            Assert.Throws<ArgumentNullException>("typeArgs", () => Expression.GetFuncType(null));
        }

        [Fact]
        public static void TestGetFuncTypeWithTooManyArgsFails()
        {
            Assert.Throws<ArgumentException>(() => Expression.GetFuncType(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) }));
        }

        [Fact]
        public static void TestPropertiesAndFieldsByName()
        {
            Expression p = Expression.Parameter(typeof(NWindProxy.Customer), "c");
            Assert.Equal("ContactName", Expression.PropertyOrField(p, "contactName").Member.Name);
            Assert.Equal("ContactName", Expression.Field(p, "CONTACTNAME").Member.Name);

            Expression t = Expression.Parameter(typeof(Expression_Tests), "t");
            Assert.Equal("IsFunky", Expression.PropertyOrField(t, "IsFunky").Member.Name);
            Assert.Equal("IsFunky", Expression.PropertyOrField(t, "isFunky").Member.Name);
            Assert.Equal("IsFunky", Expression.PropertyOrField(t, "isfunky").Member.Name);
            Assert.True(typeof(PropertyInfo).IsAssignableFrom(Expression.PropertyOrField(t, "isfunky").Member.GetType()));
            Assert.Equal("IsFunky", Expression.Property(t, "IsFunky").Member.Name);
            Assert.Equal("IsFunky", Expression.Property(t, "isFunky").Member.Name);
            Assert.Equal("IsFunky", Expression.Property(t, "ISFUNKY").Member.Name);
            Assert.True(typeof(PropertyInfo).IsAssignableFrom(Expression.Property(t, "isFunky").Member.GetType()));
            Assert.True(typeof(FieldInfo).IsAssignableFrom(Expression.Field(t, "_isFunky").Member.GetType()));
        }

        private bool IsFunky { get { return _isfunky; } }
        private bool _isfunky = true;

        [Fact]
        // tests calling instance methods by name (generic and non-generic)
        public static void TestCallInstanceMethodsByName()
        {
            Expression_Tests obj = new Expression_Tests();
            MethodCallExpression mc1 = Expression.Call(Expression.Constant(obj), "SomeMethod", null, Expression.Constant(5));
            Assert.Equal(typeof(int), mc1.Method.GetParameters()[0].ParameterType);
            MethodCallExpression mc2 = Expression.Call(Expression.Constant(obj), "Somemethod", null, Expression.Constant("Five"));
            Assert.Equal(typeof(string), mc2.Method.GetParameters()[0].ParameterType);
            MethodCallExpression mc3 = Expression.Call(Expression.Constant(obj), "someMethod", new Type[] { typeof(int), typeof(string) }, Expression.Constant(5), Expression.Constant("Five"));
            Assert.Equal(typeof(int), mc3.Method.GetParameters()[0].ParameterType);
            Assert.Equal(typeof(string), mc3.Method.GetParameters()[1].ParameterType);
        }

        private void SomeMethod(int someArg)
        {
        }

        private void SomeMethod(string someArg)
        {
        }

        private void SomeMethod<A, B>(A a, B b)
        {
        }

        [Fact]
        // this tests calling static methods by name (generic and non-generic)
        public static void TestCallStaticMethodsByName()
        {
            NWindProxy.Customer[] custs = new[] { new NWindProxy.Customer { CustomerID = "BUBBA", ContactName = "Bubba Gump" } };
            NWindProxy.Order[] orders = new[] { new NWindProxy.Order { CustomerID = "BUBBA" } };


            Expression query = custs.AsQueryable().Expression;
            Expression query2 = orders.AsQueryable().Expression;

            Expression<Func<NWindProxy.Customer, bool>> pred = c => c.CustomerID == "BUBBA";
            Expression<Func<NWindProxy.Customer, string>> selName = c => c.ContactName;
            Expression<Func<NWindProxy.Customer, string>> cId = c => c.CustomerID;
            Expression<Func<NWindProxy.Order, string>> ocId = o => o.CustomerID;
            Expression<Func<NWindProxy.Customer, IEnumerable<NWindProxy.Order>>> selOrders = c => c.Orders;
            Expression<Func<NWindProxy.Customer, NWindProxy.Customer, NWindProxy.Customer>> agg = (c1, c2) => c1;
            Expression<Func<NWindProxy.Customer, NWindProxy.Order, string>> joinPair = (c, o) => c.ContactName + o.CustomerID;
            Expression<Func<string, IEnumerable<NWindProxy.Customer>, string>> strCusts = (s, cs) => s + cs.Count();
            Expression<Func<string, IEnumerable<string>, string>> strStrs = (s, ss) => s + ss.Count();
            Expression<Func<NWindProxy.Customer, IEnumerable<NWindProxy.Order>, string>> custOrds = (c, os) => c.ContactName + os.Count();
            Expression<Func<string, NWindProxy.Customer, string>> agg2 = (s, c) => c.ContactName + s;

            Expression<Func<string, int>> aggsel = s => s.Length;

            Expression comparer = Expression.Constant(StringComparer.OrdinalIgnoreCase);

            Type[] taCust = new Type[] { typeof(NWindProxy.Customer) };
            Type[] taCustOrder = new Type[] { typeof(NWindProxy.Customer), typeof(NWindProxy.Order) };
            Type[] taCustOrderString = new Type[] { typeof(NWindProxy.Customer), typeof(NWindProxy.Order), typeof(string) };
            Type[] taCustString = new Type[] { typeof(NWindProxy.Customer), typeof(string) };
            Type[] taCustStringString = new Type[] { typeof(NWindProxy.Customer), typeof(string), typeof(string) };
            Type[] taCustStringInt = new Type[] { typeof(NWindProxy.Customer), typeof(string), typeof(int) };
            Type[] taCustOrderStringString = new Type[] { typeof(NWindProxy.Customer), typeof(NWindProxy.Order), typeof(string), typeof(string) };

            // test by calling all known Queryable methods

            CheckMethod("Aggregate", Expression.Call(typeof(Queryable), "Aggregate", taCust, query, agg));
            CheckMethod("Aggregate", Expression.Call(typeof(Queryable), "Aggregate", taCustString, query, Expression.Constant("Bubba"), agg2));
            CheckMethod("Aggregate", Expression.Call(typeof(Queryable), "Aggregate", taCustStringInt, query, Expression.Constant("Bubba"), agg2, aggsel));
            CheckMethod("All", Expression.Call(typeof(Queryable), "All", taCust, query, pred));
            CheckMethod("Any", Expression.Call(typeof(Queryable), "Any", taCust, query));
            CheckMethod("Any", Expression.Call(typeof(Queryable), "Any", taCust, query, pred));
            CheckMethod("Cast", Expression.Call(typeof(Queryable), "Cast", new Type[] { typeof(object) }, query));
            CheckMethod("Concat", Expression.Call(typeof(Queryable), "Concat", taCust, query, query));
            CheckMethod("Count", Expression.Call(typeof(Queryable), "Count", taCust, query));
            CheckMethod("Count", Expression.Call(typeof(Queryable), "Count", taCust, query, pred));
            CheckMethod("Distinct", Expression.Call(typeof(Queryable), "Distinct", taCust, query));
            CheckMethod("ElementAt", Expression.Call(typeof(Queryable), "ElementAt", taCust, query, Expression.Constant(1)));
            CheckMethod("ElementAtOrDefault", Expression.Call(typeof(Queryable), "ElementAtOrDefault", taCust, query, Expression.Constant(1)));
            CheckMethod("SequenceEqual", Expression.Call(typeof(Queryable), "SequenceEqual", taCust, query, query));
            CheckMethod("Except", Expression.Call(typeof(Queryable), "Except", taCust, query, query));
            CheckMethod("First", Expression.Call(typeof(Queryable), "First", taCust, query));
            CheckMethod("First", Expression.Call(typeof(Queryable), "First", taCust, query, pred));
            CheckMethod("FirstOrDefault", Expression.Call(typeof(Queryable), "FirstOrDefault", taCust, query));
            CheckMethod("FirstOrDefault", Expression.Call(typeof(Queryable), "FirstOrDefault", taCust, query, pred));
            CheckMethod("GroupBy", Expression.Call(typeof(Queryable), "GroupBy", taCustString, query, selName));
            CheckMethod("GroupBy", Expression.Call(typeof(Queryable), "GroupBy", taCustStringString, query, selName, selName));
            CheckMethod("GroupBy", Expression.Call(typeof(Queryable), "GroupBy", taCustString, query, selName, comparer));
            CheckMethod("GroupBy", Expression.Call(typeof(Queryable), "GroupBy", taCustStringString, query, selName, selName, comparer));
            CheckMethod("Intersect", Expression.Call(typeof(Queryable), "Intersect", taCust, query, query));
            CheckMethod("LongCount", Expression.Call(typeof(Queryable), "LongCount", taCust, query));
            CheckMethod("LongCount", Expression.Call(typeof(Queryable), "LongCount", taCust, query, pred));
            CheckMethod("Max", Expression.Call(typeof(Queryable), "Max", taCust, query));
            CheckMethod("Max", Expression.Call(typeof(Queryable), "Max", taCustString, query, selName));
            CheckMethod("Min", Expression.Call(typeof(Queryable), "Min", taCust, query));
            CheckMethod("Min", Expression.Call(typeof(Queryable), "Min", taCustString, query, selName));
            CheckMethod("OfType", Expression.Call(typeof(Queryable), "OfType", new Type[] { typeof(object) }, query));
            var ordered = Expression.Call(typeof(Queryable), "OrderBy", taCustString, query, selName);
            CheckMethod("OrderBy", ordered);
            ordered = Expression.Call(typeof(Queryable), "OrderByDescending", taCustString, query, selName);
            CheckMethod("OrderByDescending", ordered);
            CheckMethod("Reverse", Expression.Call(typeof(Queryable), "Reverse", taCust, query));
            CheckMethod("Select", Expression.Call(typeof(Queryable), "Select", taCustString, query, selName));
            CheckMethod("SelectMany", Expression.Call(typeof(Queryable), "SelectMany", taCustOrder, query, selOrders));
            CheckMethod("SelectMany", Expression.Call(typeof(Queryable), "SelectMany", taCustOrderString, query, selOrders, joinPair));
            CheckMethod("Single", Expression.Call(typeof(Queryable), "Single", taCust, query));
            CheckMethod("Single", Expression.Call(typeof(Queryable), "Single", taCust, query, pred));
            CheckMethod("SingleOrDefault", Expression.Call(typeof(Queryable), "SingleOrDefault", taCust, query));
            CheckMethod("SingleOrDefault", Expression.Call(typeof(Queryable), "SingleOrDefault", taCust, query, pred));
            CheckMethod("Skip", Expression.Call(typeof(Queryable), "Skip", taCust, query, Expression.Constant(1)));
            CheckMethod("SkipWhile", Expression.Call(typeof(Queryable), "SkipWhile", taCust, query, pred));
            CheckMethod("Take", Expression.Call(typeof(Queryable), "Take", taCust, query, Expression.Constant(1)));
            CheckMethod("TakeWhile", Expression.Call(typeof(Queryable), "TakeWhile", taCust, query, pred));
            CheckMethod("ThenBy", Expression.Call(typeof(Queryable), "ThenBy", taCustString, ordered, selName));
            CheckMethod("ThenByDescending", Expression.Call(typeof(Queryable), "ThenByDescending", taCustString, ordered, selName));
            CheckMethod("Union", Expression.Call(typeof(Queryable), "Union", taCust, query, query));
            CheckMethod("Where", Expression.Call(typeof(Queryable), "Where", taCust, query, pred));
            CheckMethod("Join", Expression.Call(typeof(Queryable), "Join", taCustOrderStringString, query, query2, cId, ocId, joinPair));
            CheckMethod("Join", Expression.Call(typeof(Queryable), "Join", taCustOrderStringString, query, query2, cId, ocId, joinPair, comparer));
            CheckMethod("GroupJoin", Expression.Call(typeof(Queryable), "GroupJoin", taCustOrderStringString, query, query2, cId, ocId, custOrds));
            CheckMethod("GroupJoin", Expression.Call(typeof(Queryable), "GroupJoin", taCustOrderStringString, query, query2, cId, ocId, custOrds, comparer));
            CheckMethod("Zip", Expression.Call(typeof(Queryable), "Zip", taCustOrderString, query, query2, joinPair));

            ConstructAggregates<int>(0);
            ConstructAggregates<int?>(0);
            ConstructAggregates<long>(0);
            ConstructAggregates<long?>(0);
            ConstructAggregates<double>(0);
            ConstructAggregates<double?>(0);
            ConstructAggregates<decimal>(0);
            ConstructAggregates<decimal?>(0);
        }

        private static void ConstructAggregates<T>(T value)
        {
            var values = Expression.Constant(new T[] { }.AsQueryable());
            var custs = Expression.Constant(new NWindProxy.Customer[] { }.AsQueryable());
            Expression<Func<NWindProxy.Customer, T>> cvalue = c => value;
            Type[] taCust = new Type[] { typeof(NWindProxy.Customer) };
            CheckMethod("Sum", Expression.Call(typeof(Queryable), "Sum", null, values));
            CheckMethod("Sum", Expression.Call(typeof(Queryable), "Sum", taCust, custs, cvalue));
            CheckMethod("Average", Expression.Call(typeof(Queryable), "Average", null, values));
            CheckMethod("Average", Expression.Call(typeof(Queryable), "Average", taCust, custs, cvalue));
        }

        private static void CheckMethod(string name, Expression expr)
        {
            Assert.Equal(ExpressionType.Call, expr.NodeType);
            MethodCallExpression mc = expr as MethodCallExpression;
            Assert.Equal(name, mc.Method.Name);

            Assert.Equal(typeof(Queryable), mc.Method.DeclaringType);
        }
    }

    public class Queryable_Tests
    {
        [Fact]
        public static void SequenceQueryToString()
        {
            var q = (new int[] { 1, 2, 3, 4, 5, 6 }).AsQueryable();
            var s = q.ToString();
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void DynamicSequenceQuery(bool useInterpreter)
        {
            ParameterExpression i = Expression.Parameter(typeof(int[]), "i");
            var e = Expression.Lambda<Func<int[], IEnumerable<int>>>(
                  Expression.Convert(
                    Expression.Call(
                       null,
                       typeof(Queryable).GetMethod("AsQueryable", new[] { typeof(IEnumerable) }),
                       new Expression[] {
                        Expression.Convert(i, typeof(IEnumerable<int>))
                        }
                       ),
                    typeof(IEnumerable<int>)
                    ),
                    new ParameterExpression[] { i }
                  );

            string result = "";
            foreach (var x in e.Compile(useInterpreter)(new[] { 1, 2, 3 }))
                result += ", " + x.ToString();

            Assert.Equal(", 1, 2, 3", result);
        }

        [Fact(Skip = "870811")]
        public static void QueryableMethods()
        {
            var s = new int[] { 1, 2, 3, 4, 5, 6 };
            var q = s.AsQueryable();
            var s2 = new int[] { 3, 5 };
            var q2 = s2.AsQueryable();
            var s3 = new int[] { -1, -2, 3, 4 };
            var q3 = s3.AsQueryable();
            var s4 = new int[] { -1, -2 };
            var q4 = s4.AsQueryable();
            var snInt = new int?[] { 1, 2, null, 4, 5, 6 };
            var qnInt = snInt.AsQueryable();
            var snDouble = new double?[] { 1, 2, null, 4, 5, 6 };
            var qnDouble = snDouble.AsQueryable();
            var snLong = new long?[] { 1, 2, null, 4, 5, 6 };
            var qnLong = snLong.AsQueryable();
            var snDecimal = new decimal?[] { 1, 2, null, 4, 5, 6 };
            var qnDecimal = snDecimal.AsQueryable();
            var sInt = new int[] { 1, 2, 3, 4, 5, 6 };
            var qInt = sInt.AsQueryable();
            var sDouble = new double[] { 1, 2, 3, 4, 5, 6 };
            var qDouble = sDouble.AsQueryable();
            var sLong = new long[] { 1, 2, 3, 4, 5, 6 };
            var qLong = sLong.AsQueryable();
            var sDecimal = new decimal[] { 1, 2, 3, 4, 5, 6 };
            var qDecimal = sDecimal.AsQueryable();
            var se = new int[] { };
            var qe = se.AsQueryable();
            var ss = new int[] { 9 };
            var qs = ss.AsQueryable();
            string[] names = new string[] { "zero", "one", "two", "three", "four", "five", "six" };
            var comp = StringComparer.OrdinalIgnoreCase;

            Assert.Equal(21, q.Aggregate((a, b) => a + b));
            Assert.Equal(21, q.Aggregate(0, (a, b) => a + b));
            Assert.Equal("21", q.Aggregate(0, (a, b) => a + b, (r) => r.ToString()));
            Assert.True(q.All(x => x < 7));
            Assert.False(q.All(x => x < 4));
            Assert.True(q.Any());
            Assert.False(qe.Any());
            Assert.True(q.Any(x => x > 5));
            Assert.False(q.Any(x => x < 0));
            Assert.Equal(6, q.Cast<object>().Count());
            Assert.Equal(12, q.Concat(q).Count());
            Assert.Equal(12, s.Concat(q).Count());
            Assert.Equal(12, q.Concat(s).Count());
            Assert.Equal(3, q.Count(x => x < 4));
            Assert.True(q.Contains(3));
            Assert.False(q.Contains(9));
            Assert.False(qe.Contains(5));
            Assert.Equal(6, q.Distinct().Count());
            Assert.Equal(1, qe.DefaultIfEmpty().Count());
            Assert.Equal(4, q.ElementAt(3));
            Assert.Equal(4, q.ToList().ElementAt(3));
            Assert.Equal(4, s.ElementAt(3));
            Assert.Equal(4, s.ToList().ElementAt(3));
            Assert.Equal(0, q.ElementAtOrDefault(10));
            Assert.Equal(0, q.ToList().ElementAtOrDefault(10));
            Assert.Equal(4, s.ElementAtOrDefault(3));
            Assert.Equal(4, s.ToList().ElementAtOrDefault(3));
            Assert.Equal(0, s.ElementAtOrDefault(10));
            Assert.Equal(0, s.ToList().ElementAtOrDefault(10));
            Assert.True(q.SequenceEqual(((IEnumerable<int>)q).AsEnumerable()));
            Assert.True(q.SequenceEqual(q));
            Assert.True(s.SequenceEqual(q));
            Assert.True(q.SequenceEqual(s));
            Assert.False(q.SequenceEqual(q2));
            Assert.False(q.SequenceEqual(q2.Reverse()));
            Assert.False(q2.SequenceEqual(q));
            Assert.False(q2.SequenceEqual(q.Reverse()));
            Assert.False(q3.SequenceEqual(q4));
            Assert.False(q4.SequenceEqual(q3));
            Assert.Equal(4, q.Except(q2).Count());
            Assert.Equal(4, s.Except(q2).Count());
            Assert.Equal(4, q.Except(s2).Count());
            Assert.Equal(1, q.First());
            Assert.Equal(4, q.First(x => x > 3));
            Assert.Equal(4, ((IEnumerable<int>)q).First(x => x > 3));
            Assert.Equal(0, qe.FirstOrDefault());
            Assert.Equal(1, q.FirstOrDefault());
            Assert.Equal(1, q.ToList().FirstOrDefault());
            Assert.Equal(1, Enumerable.Range(1, 6).FirstOrDefault());
            Assert.Equal(1, Enumerable.Range(1, 6).ToList().FirstOrDefault());
            Assert.Equal(6, q.FirstOrDefault(x => x > 5));
            Assert.Equal(0, q.FirstOrDefault(x => x > 10));
            Assert.Equal(6, q.ToList().FirstOrDefault(x => x > 5));
            Assert.Equal(0, q.ToList().FirstOrDefault(x => x > 10));
            Assert.Equal(6, Enumerable.Range(1, 6).FirstOrDefault(x => x > 5));
            Assert.Equal(0, Enumerable.Range(1, 6).FirstOrDefault(x => x > 10));
            Assert.Equal(4, q.GroupBy(x => x / 2).Count());
            Assert.Equal(3, q.GroupBy(x => names[x].Length, x => x).Count());
            Assert.Equal(6, q.GroupBy(x => names[x]).Count());
            Assert.Equal(6, q.GroupBy(x => names[x], x => x, comp).Count());
            Assert.Equal(2, q.Intersect(q2).Count());
            Assert.Equal(6, q.Last());
            Assert.Equal(6, q.Last(x => x > 3));
            Assert.Equal(0, qe.LastOrDefault());
            Assert.Equal(6, s.LastOrDefault());
            Assert.Equal(6, q.LastOrDefault());
            Assert.Equal(6, q.ToList().LastOrDefault());
            Assert.Equal(6, Enumerable.Range(1, 6).LastOrDefault());
            Assert.Equal(6, q.LastOrDefault(x => x > 5));
            Assert.Equal(0, q.LastOrDefault(x => x > 10));
            Assert.Equal(6, q.ToList().LastOrDefault(x => x > 5));
            Assert.Equal(0, q.ToList().LastOrDefault(x => x > 10));
            Assert.Equal(6, Enumerable.Range(1, 6).ToList().LastOrDefault(x => x > 5));
            Assert.Equal(0, Enumerable.Range(1, 6).ToList().LastOrDefault(x => x > 10));
            Assert.Equal(6L, q.LongCount());
            Assert.Equal(3L, q.LongCount(x => x <= 3));
            Assert.Equal(6, q.Max());
            Assert.Equal(36, q.Max(x => x * x));
            Assert.Equal(6, q.Reverse().Max());
            Assert.Equal(36, q.Reverse().Max(x => x * x));
            Assert.Equal(1, q.Min());
            Assert.Equal(1, q.Min(x => x * x));
            Assert.Equal(1, q.Reverse().Min());
            Assert.Equal(1, q.Reverse().Min(x => x * x));

            Assert.Equal(6, sInt.Max());
            Assert.Equal(36, sInt.Max(x => x * x));
            Assert.Equal(6, sInt.Reverse().Max());
            Assert.Equal(36, sInt.Reverse().Max(x => x * x));
            Assert.Equal(1, sInt.Min());
            Assert.Equal(1, sInt.Min(x => x * x));
            Assert.Equal(1, sInt.Reverse().Min());
            Assert.Equal(1, sInt.Reverse().Min(x => x * x));
            Assert.Equal(6, sDouble.Max());
            Assert.Equal(36, sDouble.Max(x => x * x));
            Assert.Equal(6, sDouble.Reverse().Max());
            Assert.Equal(36, sDouble.Reverse().Max(x => x * x));
            Assert.Equal(1, sDouble.Min());
            Assert.Equal(1, sDouble.Min(x => x * x));
            Assert.Equal(1, sDouble.Reverse().Min());
            Assert.Equal(1, sDouble.Reverse().Min(x => x * x));
            Assert.Equal(6, sLong.Max());
            Assert.Equal(36, sLong.Max(x => x * x));
            Assert.Equal(6, sLong.Reverse().Max());
            Assert.Equal(36, sLong.Reverse().Max(x => x * x));
            Assert.Equal(1, sLong.Min());
            Assert.Equal(1, sLong.Min(x => x * x));
            Assert.Equal(1, sLong.Reverse().Min());
            Assert.Equal(1, sLong.Reverse().Min(x => x * x));
            Assert.Equal(6, sDecimal.Max());
            Assert.Equal(36, sDecimal.Max(x => x * x));
            Assert.Equal(6, sDecimal.Reverse().Max());
            Assert.Equal(36, sDecimal.Reverse().Max(x => x * x));
            Assert.Equal(1, sDecimal.Min());
            Assert.Equal(1, sDecimal.Min(x => x * x));
            Assert.Equal(1, sDecimal.Reverse().Min());
            Assert.Equal(1, sDecimal.Reverse().Min(x => x * x));

            Assert.Equal(6, snInt.Max());
            Assert.Equal(36, snInt.Max(x => x * x));
            Assert.Equal(6, snInt.Reverse().Max());
            Assert.Equal(36, snInt.Reverse().Max(x => x * x));
            Assert.Equal(1, snInt.Min());
            Assert.Equal(1, snInt.Min(x => x * x));
            Assert.Equal(1, snInt.Reverse().Min());
            Assert.Equal(1, snInt.Reverse().Min(x => x * x));
            Assert.Equal(6, snDouble.Max());
            Assert.Equal(36, snDouble.Max(x => x * x));
            Assert.Equal(6, snDouble.Reverse().Max());
            Assert.Equal(36, snDouble.Reverse().Max(x => x * x));
            Assert.Equal(1, snDouble.Min());
            Assert.Equal(1, snDouble.Min(x => x * x));
            Assert.Equal(1, snDouble.Reverse().Min());
            Assert.Equal(1, snDouble.Reverse().Min(x => x * x));
            Assert.Equal(6, snLong.Max());
            Assert.Equal(36, snLong.Max(x => x * x));
            Assert.Equal(6, snLong.Reverse().Max());
            Assert.Equal(36, snLong.Reverse().Max(x => x * x));
            Assert.Equal(1, snLong.Min());
            Assert.Equal(1, snLong.Min(x => x * x));
            Assert.Equal(1, snLong.Reverse().Min());
            Assert.Equal(1, snLong.Reverse().Min(x => x * x));
            Assert.Equal(6, snDecimal.Max());
            Assert.Equal(36, snDecimal.Max(x => x * x));
            Assert.Equal(6, snDecimal.Reverse().Max());
            Assert.Equal(36, snDecimal.Reverse().Max(x => x * x));
            Assert.Equal(1, snDecimal.Min());
            Assert.Equal(1, snDecimal.Min(x => x * x));
            Assert.Equal(1, snDecimal.Reverse().Min());
            Assert.Equal(1, snDecimal.Reverse().Min(x => x * x));


            Assert.Equal(6, qInt.Max());
            Assert.Equal(36, qInt.Max(x => x * x));
            Assert.Equal(6, qInt.Reverse().Max());
            Assert.Equal(36, qInt.Reverse().Max(x => x * x));
            Assert.Equal(1, qInt.Min());
            Assert.Equal(1, qInt.Min(x => x * x));
            Assert.Equal(1, qInt.Reverse().Min());
            Assert.Equal(1, qInt.Reverse().Min(x => x * x));
            Assert.Equal(6, qDouble.Max());
            Assert.Equal(36, qDouble.Max(x => x * x));
            Assert.Equal(6, qDouble.Reverse().Max());
            Assert.Equal(36, qDouble.Reverse().Max(x => x * x));
            Assert.Equal(1, qDouble.Min());
            Assert.Equal(1, qDouble.Min(x => x * x));
            Assert.Equal(1, qDouble.Reverse().Min());
            Assert.Equal(1, qDouble.Reverse().Min(x => x * x));
            Assert.Equal(6, qLong.Max());
            Assert.Equal(36, qLong.Max(x => x * x));
            Assert.Equal(6, qLong.Reverse().Max());
            Assert.Equal(36, qLong.Reverse().Max(x => x * x));
            Assert.Equal(1, qLong.Min());
            Assert.Equal(1, qLong.Min(x => x * x));
            Assert.Equal(1, qLong.Reverse().Min());
            Assert.Equal(1, qLong.Reverse().Min(x => x * x));
            Assert.Equal(6, qDecimal.Max());
            Assert.Equal(36, qDecimal.Max(x => x * x));
            Assert.Equal(6, qDecimal.Reverse().Max());
            Assert.Equal(36, qDecimal.Reverse().Max(x => x * x));
            Assert.Equal(1, qDecimal.Min());
            Assert.Equal(1, qDecimal.Min(x => x * x));
            Assert.Equal(1, qDecimal.Reverse().Min());
            Assert.Equal(1, qDecimal.Reverse().Min(x => x * x));
            Assert.Equal(6, qnInt.Max());
            Assert.Equal(36, qnInt.Max(x => x * x));
            Assert.Equal(6, qnInt.Reverse().Max());
            Assert.Equal(36, qnInt.Reverse().Max(x => x * x));
            Assert.Equal(1, qnInt.Min());
            Assert.Equal(1, qnInt.Min(x => x * x));
            Assert.Equal(1, qnInt.Reverse().Min());
            Assert.Equal(1, qnInt.Reverse().Min(x => x * x));
            Assert.Equal(6, qnDouble.Max());
            Assert.Equal(36, qnDouble.Max(x => x * x));
            Assert.Equal(6, qnDouble.Reverse().Max());
            Assert.Equal(36, qnDouble.Reverse().Max(x => x * x));
            Assert.Equal(1, qnDouble.Min());
            Assert.Equal(1, qnDouble.Min(x => x * x));
            Assert.Equal(1, qnDouble.Reverse().Min());
            Assert.Equal(1, qnDouble.Reverse().Min(x => x * x));
            Assert.Equal(6, qnLong.Max());
            Assert.Equal(36, qnLong.Max(x => x * x));
            Assert.Equal(6, qnLong.Reverse().Max());
            Assert.Equal(36, qnLong.Reverse().Max(x => x * x));
            Assert.Equal(1, qnLong.Min());
            Assert.Equal(1, qnLong.Min(x => x * x));
            Assert.Equal(1, qnLong.Reverse().Min());
            Assert.Equal(1, qnLong.Reverse().Min(x => x * x));
            Assert.Equal(6, qnDecimal.Max());
            Assert.Equal(36, qnDecimal.Max(x => x * x));
            Assert.Equal(6, qnDecimal.Reverse().Max());
            Assert.Equal(36, qnDecimal.Reverse().Max(x => x * x));
            Assert.Equal(1, qnDecimal.Min());
            Assert.Equal(1, qnDecimal.Min(x => x * x));
            Assert.Equal(1, qnDecimal.Reverse().Min());
            Assert.Equal(1, qnDecimal.Reverse().Min(x => x * x));
            Assert.Equal(6, q.OfType<int>().Count());
            Assert.Equal(0, q.OfType<string>().Count());
            Assert.Equal(5, q.OrderBy(x => names[x]).First());
            Assert.Equal(2, q.OrderByDescending(x => names[x]).First());
            Assert.Equal(2, q.OrderBy(x => names[x / 2]).ThenBy(x => x).First());
            Assert.Equal(1, q.OrderByDescending(x => names[x / 2]).ThenByDescending(x => x).First());
            Assert.Equal(5, q.OrderBy(x => names[x], comp).First());
            Assert.Equal(2, q.OrderByDescending(x => names[x], comp).First());
            Assert.Equal(2, q.OrderBy(x => names[x / 2], comp).ThenBy(x => x).First());
            Assert.Equal(1, q.OrderByDescending(x => names[x / 2], comp).ThenByDescending(x => x).First());
            Assert.Equal(6, q.Select(x => x * x).Count());
            Assert.Equal(12, q.SelectMany(x => q2).Count());
            Assert.Equal(12, q.SelectMany(x => q2, (x, y) => new { x, y }).Count());
            Assert.Equal(6, q.Select((x, i) => x * i).Count());
            Assert.Equal(12, q.SelectMany((x, i) => q2).Count());
            Assert.Equal(9, qs.Single());
            Assert.Equal(6, q.Single(x => x > 5));
            Assert.Equal(9, qs.SingleOrDefault());
            Assert.Equal(0, qe.SingleOrDefault());
            Assert.Equal(6, q.SingleOrDefault(x => x > 5));
            Assert.Equal(0, q.SingleOrDefault(x => x > 10));
            Assert.Equal(4, q.Skip(2).Count());
            Assert.Equal(3, q.SkipWhile(x => x < 4).Count());
            Assert.Equal(3, q.SkipWhile((x, i) => x < 4).Count());
            Assert.Equal(3, q.Take(3).Count());
            Assert.Equal(3, q.TakeWhile(x => x < 4).Count());
            Assert.Equal(3, q.TakeWhile((x, i) => x < 4).Count());
            Assert.Equal(6, q.Union(q).Count());
            Assert.Equal(6, s.Union(q).Count());
            Assert.Equal(6, q.Union(s).Count());
            Assert.Equal(7, q.Union(qs).Count());
            Assert.Equal(3, q.Where(x => x > 3).Count());
            Assert.Equal(6, q.Where((x, i) => x > i).Count());

            Assert.Equal(2, q.Join(q2, v => v, v => v, (x, y) => new { x, y }).Count());
            Assert.Equal(2, s.Join(q2, v => v, v => v, (x, y) => new { x, y }).Count());
            Assert.Equal(2, q.Join(s2, v => v, v => v, (x, y) => new { x, y }).Count());
            Assert.Equal(6, q.GroupJoin(q2, v => v, v => v, (x, ys) => new { x, ys }).Count());
            Assert.Equal(6, s.GroupJoin(q2, v => v, v => v, (x, ys) => new { x, ys }).Count());
            Assert.Equal(6, q.GroupJoin(s2, v => v, v => v, (x, ys) => new { x, ys }).Count());

            // Aggregates  (count and longcount included above)
            Assert.Equal(1, q.Min());
            Assert.Equal(1, q.Min(x => x));
            Assert.Equal(6, q.Max());
            Assert.Equal(6, q.Max(x => x));
            Assert.Equal(21, q.Sum());
            Assert.Equal(21, q.Select(x => (int?)x).Sum());
            Assert.Equal(21, q.Select(x => (long)x).Sum());
            Assert.Equal(21, q.Select(x => (long?)x).Sum());
            Assert.Equal(21, q.Select(x => (double)x).Sum());
            Assert.Equal(21, q.Select(x => (double?)x).Sum());
            Assert.Equal(21, q.Select(x => (decimal)x).Sum());
            Assert.Equal(21, q.Select(x => (decimal?)x).Sum());
            Assert.Equal(21, q.Sum(x => x));
            Assert.Equal(21, q.Sum(x => (int?)x));
            Assert.Equal(21, q.Sum(x => (long)x));
            Assert.Equal(21, q.Sum(x => (long?)x));
            Assert.Equal(21, q.Sum(x => (double)x));
            Assert.Equal(21, q.Sum(x => (double?)x));
            Assert.Equal(21, q.Sum(x => (decimal)x));
            Assert.Equal(21, q.Sum(x => (decimal?)x));
            Assert.Equal(3.5, q.Average());
            Assert.Equal(3.5, q.Select(x => (int?)x).Average());
            Assert.Equal(3.5, q.Select(x => (long)x).Average());
            Assert.Equal(3.5, q.Select(x => (long?)x).Average());
            Assert.Equal(3.5, q.Select(x => (double)x).Average());
            Assert.Equal(3.5, q.Select(x => (double?)x).Average());
            Assert.Equal(3.5m, q.Select(x => (decimal)x).Average());
            Assert.Equal(3.5m, q.Select(x => (decimal?)x).Average());
            Assert.Equal(3.5, q.Average(x => x));
            Assert.Equal(3.5, q.Average(x => (int?)x));
            Assert.Equal(3.5, q.Average(x => (long)x));
            Assert.Equal(3.5, q.Average(x => (long?)x));
            Assert.Equal(3.5, q.Average(x => (double)x));
            Assert.Equal(3.5, q.Average(x => (double?)x));
            Assert.Equal(3.5m, q.Average(x => (decimal)x));
            Assert.Equal(3.5m, q.Average(x => (decimal?)x));
            Assert.Null((new int?[] { }).Average());
            Assert.Null((new long?[] { }).Average());
            Assert.Null((new double?[] { }).Average());
            Assert.Null((new decimal?[] { }).Average());

            Assert.Equal(13, s.Zip(s2, (a, b) => a * b).Sum());
            Assert.Equal(13, q.Zip(q2, (a, b) => a * b).Sum());
            Assert.Equal(34, s2.Zip(s2, (a, b) => a * b).Sum());
            Assert.Equal(34, q2.Zip(q2, (a, b) => a * b).Sum());
        }

        [Fact]
        public static void MatchSequencePattern()
        {
            List<MethodInfo> list = GetMissingExtensionMethods(
                typeof(System.Linq.Enumerable),
                typeof(System.Linq.Queryable),
                 new string[] {
                     "ToLookup",
                     "ToDictionary",
                     "ToArray",
                     "AsEnumerable",
                     "ToList",
                     "Fold",
                     "LeftJoin",
                     "Append",
                     "Prepend",
                 }
                ).ToList();

            if (list.Count > 0)
            {
                Console.WriteLine("Enumerable methods not defined by Queryable\n");
                foreach (MethodInfo m in list)
                {
                    Console.WriteLine(m);
                }
                Console.WriteLine();
            }

            List<MethodInfo> list2 = GetMissingExtensionMethods(
                typeof(System.Linq.Queryable),
                typeof(System.Linq.Enumerable),
                 new string[] {
                     "AsQueryable"
                 }
                ).ToList();

            if (list2.Count > 0)
            {
                Console.WriteLine("Queryable methods not defined by Enumerable\n");
                foreach (MethodInfo m in list2)
                {
                    Console.WriteLine(m);
                }
            }
            Assert.Equal(Enumerable.Empty<string>(), list.Select(mi => mi.Name));
            Assert.Equal(Enumerable.Empty<string>(), list2.Select(mi => mi.Name));
            Assert.True(list.Count == 0 && list2.Count == 0);
        }

        private static IEnumerable<MethodInfo> GetMissingExtensionMethods(Type a, Type b, string[] excludedMethods)
        {
            var dex = excludedMethods.ToDictionary(s => s);

            var aMethods =
                a.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.CustomAttributes.Any(c => c.AttributeType == typeof(System.Runtime.CompilerServices.ExtensionAttribute)))
                .ToLookup(m => m.Name);

            MethodComparer mc = new MethodComparer();
            var bMethods = b.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.CustomAttributes.Any(c => c.AttributeType == typeof(System.Runtime.CompilerServices.ExtensionAttribute)))
                .ToLookup(m => m, mc);

            // look for sequence operator methods not defined by DLINQ
            foreach (var group in aMethods)
            {
                if (dex.ContainsKey(group.Key))
                    continue;
                foreach (MethodInfo m in group)
                {
                    if (!bMethods.Contains(m))
                        yield return m;
                }
            }
        }

        private class MethodComparer : IEqualityComparer<MethodInfo>
        {
            public int GetHashCode(MethodInfo m)
            {
                return m.Name.GetHashCode();
            }

            public bool Equals(MethodInfo a, MethodInfo b)
            {
                if (a.Name != b.Name)
                    return false;
                ParameterInfo[] pas = a.GetParameters();
                ParameterInfo[] pbs = b.GetParameters();
                if (pas.Length != pbs.Length)
                    return false;
                Type[] aArgs = a.GetGenericArguments();
                Type[] bArgs = b.GetGenericArguments();
                for (int i = 0, n = pas.Length; i < n; i++)
                {
                    ParameterInfo pa = pas[i];
                    ParameterInfo pb = pbs[i];
                    Type ta = Strip(pa.ParameterType);
                    Type tb = Strip(pb.ParameterType);
                    if (ta.GetTypeInfo().IsGenericType && tb.GetTypeInfo().IsGenericType)
                    {
                        if (ta.GetGenericTypeDefinition() != tb.GetGenericTypeDefinition())
                            return false;
                    }
                    else if (ta.IsGenericParameter && tb.IsGenericParameter)
                    {
                        return this.GetArgIndex(aArgs, ta) == this.GetArgIndex(bArgs, tb);
                    }
                    else if (ta != tb)
                    {
                        return false;
                    }
                }
                return true;
            }

            private int GetArgIndex(Type[] args, Type arg)
            {
                for (int i = 0, n = args.Length; i < n; i++)
                {
                    if (args[i] == arg)
                        return i;
                }
                return -1;
            }

            private Type Strip(Type t)
            {
                if (t.GetTypeInfo().IsGenericType)
                {
                    Type g = t;
                    if (!g.GetTypeInfo().IsGenericTypeDefinition)
                        g = t.GetGenericTypeDefinition();
                    if (g == typeof(IQueryable<>) || g == typeof(IEnumerable<>))
                        return typeof(IEnumerable);
                    if (g == typeof(Expression<>))
                        return t.GetGenericArguments()[0];
                    if (g == typeof(IOrderedEnumerable<>) || g == typeof(IOrderedQueryable<>))
                        return typeof(IOrderedQueryable);
                }
                else
                {
                    if (t == typeof(IQueryable))
                        return typeof(IEnumerable);
                }
                return t;
            }
        }
    }

    namespace NWindProxy
    {
        public class Customer
        {
            public string CustomerID;
            public string ContactName;
            public string CompanyName;
            public List<Order> Orders = new List<Order>();
        }
        public class Order
        {
            public string CustomerID;
        }
    }

    public class Compiler_Tests
    {
        public class AndAlso
        {
            public bool value;
            public AndAlso(bool value) { this.value = value; }
            public static AndAlso operator &(AndAlso a1, AndAlso a2) { return new AndAlso(a1.value && a2.value); }
            public static bool operator true(AndAlso a) { return a.value; }
            public static bool operator false(AndAlso a) { return !a.value; }
            public static AndAlso operator !(AndAlso a) { return new AndAlso(false); }
            public override string ToString() { return value.ToString(); }
        }

        [Theory(Skip = "870811")]
        [ClassData(typeof(CompilationTypes))]
        public static void TestAndAlso(bool useInterpreter)
        {
            AndAlso a1 = new AndAlso(true);
            Func<AndAlso> f1 = () => a1 && !a1;
            AndAlso r1 = f1();

            Expression<Func<AndAlso>> e = () => a1 && !a1;
            AndAlso r2 = e.Compile(useInterpreter)();

            Assert.Equal(r2.value, r1.value);
        }

        public struct TC1
        {
            public string Name;
            public int data;

            public TC1(string name, int data)
            {
                Name = name;
                this.data = data;
            }
            public static TC1 operator &(TC1 t1, TC1 t2) { return new TC1("And", 01); }
            public static TC1 operator |(TC1 t1, TC1 t2) { return new TC1("Or", 02); }
            public static TC1 Meth1(TC1 t1, TC1 t2) { return new TC1(); }
            public static bool operator true(TC1 a) { return true; }
            public static bool operator false(TC1 a) { return false; }
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ObjectCallOnValueType(bool useInterpreter)
        {
            object st_local = new TC1();
            var mi = typeof(object).GetMethod("ToString");
            var lam = Expression.Lambda<Func<string>>(Expression.Call(Expression.Constant(st_local), mi, null), null);
            var f = lam.Compile(useInterpreter);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void AndAlsoLift(bool useInterpreter)
        {
            TC1? tc1 = new TC1("lhs", 324589);
            TC1? tc2 = new TC1("rhs", 324589);

            ConstantExpression left = Expression.Constant(tc1, typeof(TC1?));
            ConstantExpression right = Expression.Constant(tc2, typeof(TC1?));
            ParameterExpression p0 = Expression.Parameter(typeof(TC1?), "tc1");
            ParameterExpression p1 = Expression.Parameter(typeof(TC1?), "tc2");

            BinaryExpression result = (BinaryExpression)Expression.AndAlso(left, right);
            Expression<Func<TC1?>> e1 = Expression.Lambda<Func<TC1?>>(
                Expression.Invoke(
                    Expression.Lambda<Func<TC1?, TC1?, TC1?>>((result), new ParameterExpression[] { p0, p1 }),
                    new Expression[] { left, right }),
               Enumerable.Empty<ParameterExpression>());

            Func<TC1?> f1 = e1.Compile(useInterpreter);
            Assert.NotNull(f1());
            Assert.Equal(f1().Value.Name, "And");

            BinaryExpression resultOr = (BinaryExpression)Expression.OrElse(left, right);
            Expression<Func<TC1?>> e2 = Expression.Lambda<Func<TC1?>>(
                Expression.Invoke(
                    Expression.Lambda<Func<TC1?, TC1?, TC1?>>((resultOr), new ParameterExpression[] { p0, p1 }),
                    new Expression[] { left, right }),
               Enumerable.Empty<ParameterExpression>());

            Func<TC1?> f2 = e2.Compile(useInterpreter);
            Assert.NotNull(f2());
            Assert.Equal(f2().Value.Name, "lhs");

            var constant = Expression.Constant(1.0, typeof(double));
            Assert.Throws<ArgumentException>(() => Expression.Lambda<Func<double?>>(constant, null));
        }

        public static int GetBound()
        {
            return 1;
        }

        public int Bound
        {
            get
            {
                return 3;
            }
        }

        public struct Complex
        {
            public int x;
            public int y;

            public static Complex operator +(Complex c)
            {
                Complex temp = new Complex();
                temp.x = c.x + 1;
                temp.y = c.y + 1;
                return temp;
            }
            public Complex(int x, int y) { this.x = x; this.y = y; }
        }

        public class CustomerWriteBack
        {
            private Customer _cust;
            public string m_x = "ha ha ha";
            public string Func0(ref string x)
            {
                x = "Changed";
                return x;
            }
            public string X
            {
                get { return m_x; }
                set { m_x = value; }
            }
            public static int Funct1(ref int i)
            {
                return i = 5;
            }
            public static int Prop { get { return 7; } set { Assert.Equal(5, value); } }

            public Customer Cust
            {
                get
                {
                    if (_cust == null) _cust = new Customer(98007, "Sree");
                    return _cust;
                }
                set
                {
                    _cust = value;
                }
            }

            public Customer ComputeCust(ref Customer cust)
            {
                cust.zip = 90008;
                cust.name = "SreeCho";
                return cust;
            }
        }

        public class Customer
        {
            public int zip;
            public string name;
            public Customer(int zip, string name) { this.zip = zip; this.name = name; }
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void Writeback(bool useInterpreter)
        {
            CustomerWriteBack a = new CustomerWriteBack();
            var t = typeof(CustomerWriteBack);
            var mi = t.GetMethod("Func0");
            var pi = t.GetMethod("get_X");
            var piCust = t.GetMethod("get_Cust");
            var miCust = t.GetMethod("ComputeCust");

            Expression<Func<int>> e1 =
                    Expression.Lambda<Func<int>>(
                        Expression.Call(typeof(CustomerWriteBack).GetMethod("Funct1"), new[] { Expression.Property(null, typeof(CustomerWriteBack).GetProperty("Prop")) }),
                        null);
            var f1 = e1.Compile(useInterpreter);
            int result = f1();
            Assert.Equal(5, result);

            Expression<Func<string>> e = Expression.Lambda<Func<string>>(
                 Expression.Call(
                     Expression.Constant(a, typeof(CustomerWriteBack)),
                     mi,
                     new Expression[] { Expression.Property(Expression.Constant(a, typeof(CustomerWriteBack)), pi) }
                     ),
                 null);
            var f = e.Compile(useInterpreter);
            var r = f();
            Assert.Equal(a.m_x, "Changed");

            Expression<Func<Customer>> e2 = Expression.Lambda<Func<Customer>>(
                 Expression.Call(
                     Expression.Constant(a, typeof(CustomerWriteBack)),
                     miCust,
                     new Expression[] { Expression.Property(Expression.Constant(a, typeof(CustomerWriteBack)), piCust) }
                     ),
                 null);
            var f2 = e2.Compile(useInterpreter);
            var r2 = f2();
            Assert.True(a.Cust.zip == 90008 && a.Cust.name == "SreeCho");
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void UnaryPlus(bool useInterpreter)
        {
            ConstantExpression ce = Expression.Constant((UInt16)10);

            UnaryExpression result = Expression.UnaryPlus(ce);

            Assert.Throws<InvalidOperationException>(() =>
            {
                //unary Plus Operator
                byte val = 10;
                Expression<Func<byte>> e =
                    Expression.Lambda<Func<byte>>(
                        Expression.UnaryPlus(Expression.Constant(val, typeof(byte))),
                        Enumerable.Empty<ParameterExpression>());
            });

            //Userdefined objects
            Complex comp = new Complex(10, 20);
            Expression<Func<Complex>> e1 =
                Expression.Lambda<Func<Complex>>(
                    Expression.UnaryPlus(Expression.Constant(comp, typeof(Complex))),
                    Enumerable.Empty<ParameterExpression>());
            Func<Complex> f1 = e1.Compile(useInterpreter);
            Complex comp1 = f1();
            Assert.True((comp1.x == comp.x + 1 && comp1.y == comp.y + 1));

            Expression<Func<Complex, Complex>> testExpr = (x) => +x;
            Assert.Equal(testExpr.ToString(), "x => +x");
            var v = testExpr.Compile(useInterpreter);
        }

        private struct S
        {
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void CompileRelationOveratorswithIsLiftToNullTrue(bool useInterpreter)
        {
            int? x = 10;
            int? y = 2;
            ParameterExpression p1 = Expression.Parameter(typeof(int?), "x");
            ParameterExpression p2 = Expression.Parameter(typeof(int?), "y");

            Expression<Func<int?, int?, bool?>> e = Expression.Lambda<Func<int?, int?, bool?>>(
                Expression.GreaterThan(p1, p2, true, null), new ParameterExpression[] { p1, p2 });
            var f = e.Compile(useInterpreter);
            var r = f(x, y);
            Assert.True(r.Value);

            Expression<Func<int?, int?, bool?>> e1 = Expression.Lambda<Func<int?, int?, bool?>>(
                Expression.LessThan(p1, p2, true, null), new ParameterExpression[] { p1, p2 });
            f = e1.Compile(useInterpreter);
            r = f(x, y);
            Assert.False(r.Value);

            Expression<Func<int?, int?, bool?>> e2 = Expression.Lambda<Func<int?, int?, bool?>>(
                Expression.GreaterThanOrEqual(p1, p2, true, null), new ParameterExpression[] { p1, p2 });
            f = e2.Compile(useInterpreter);
            r = f(x, y);
            Assert.True(r.Value);

            Expression<Func<int?, int?, bool?>> e3 = Expression.Lambda<Func<int?, int?, bool?>>(
                Expression.Equal(p1, p2, true, null), new ParameterExpression[] { p1, p2 });
            f = e3.Compile(useInterpreter);
            r = f(x, y);
            Assert.False(r.Value);

            Expression<Func<int?, int?, bool?>> e4 = Expression.Lambda<Func<int?, int?, bool?>>(
                Expression.LessThanOrEqual(p1, p2, true, null), new ParameterExpression[] { p1, p2 });
            f = e4.Compile(useInterpreter);
            r = f(x, y);
            Assert.False(r.Value);

            Expression<Func<int?, int?, bool?>> e5 = Expression.Lambda<Func<int?, int?, bool?>>(
                Expression.NotEqual(p1, p2, true, null), new ParameterExpression[] { p1, p2 });
            f = e5.Compile(useInterpreter);
            r = f(x, y);
            Assert.True(r.Value);

            int? n = 10;
            Expression<Func<bool?>> e6 = Expression.Lambda<Func<bool?>>(
                Expression.NotEqual(
                    Expression.Constant(n, typeof(int?)),
                    Expression.Convert(Expression.Constant(null, typeof(Object)), typeof(int?)),
                    true,
                    null),
                null);
            var f6 = e6.Compile(useInterpreter);
            Assert.Null(f6());
        }

        private class TestClass : IEquatable<TestClass>
        {
            private int _val;
            public TestClass(string S, int Val)
            {
                this.S = S;
                _val = Val;
            }

            public string S;
            public int Val { get { return _val; } set { _val = value; } }

            public override bool Equals(object o)
            {
                return (o is TestClass) && Equals((TestClass)o);
            }
            public bool Equals(TestClass other)
            {
                return other.S == S;
            }
            public override int GetHashCode()
            {
                return S.GetHashCode();
            }
        }


        private class AnonHelperClass1
        {
            public Expression<Func<decimal>> mem1;
            public AnonHelperClass1(Expression<Func<decimal>> mem1) { this.mem1 = mem1; }
        }

        [Theory(Skip = "870811")]
        [ClassData(typeof(CompilationTypes))]
        public static void NewExpressionwithMemberAssignInit(bool useInterpreter)
        {
            var s = "Bad Mojo";
            int val = 10;

            ConstructorInfo constructor = typeof(TestClass).GetConstructor(new Type[] { typeof(string), typeof(int) });
            MemberInfo[] members = new MemberInfo[] { typeof(TestClass).GetField("S"), typeof(TestClass).GetProperty("Val") };
            Expression[] expressions = new Expression[] { Expression.Constant(s, typeof(string)), Expression.Constant(val, typeof(int)) };

            Expression<Func<TestClass>> e = Expression.Lambda<Func<TestClass>>(
               Expression.New(constructor, expressions, members),
               Enumerable.Empty<ParameterExpression>());
            Func<TestClass> f = e.Compile(useInterpreter);
            Assert.True(object.Equals(f(), new TestClass(s, val)));

            List<MemberInfo> members1 = new List<MemberInfo>();
            members1.Add(typeof(TestClass).GetField("S"));
            members1.Add(typeof(TestClass).GetProperty("Val"));

            Expression<Func<TestClass>> e1 = Expression.Lambda<Func<TestClass>>(
               Expression.New(constructor, expressions, members1),
               Enumerable.Empty<ParameterExpression>());
            Func<TestClass> f1 = e1.Compile(useInterpreter);
            Assert.True(object.Equals(f1(), new TestClass(s, val)));
            MemberInfo mem1 = typeof(AnonHelperClass1).GetField("mem1");
            LambdaExpression ce1 = Expression.Lambda(Expression.Constant(45m, typeof(decimal)));
            ConstructorInfo constructor1 = typeof(AnonHelperClass1).GetConstructor(new Type[] { typeof(Expression<Func<decimal>>) });

            Expression[] arguments = new Expression[] { ce1 };
            MemberInfo[] members2 = new MemberInfo[] { mem1 };

            NewExpression result = Expression.New(constructor1, arguments, members2);
            Assert.NotNull(result);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void TypeAsNullableToObject(bool useInterpreter)
        {
            Expression<Func<object>> e = Expression.Lambda<Func<object>>(Expression.TypeAs(Expression.Constant(0, typeof(int?)), typeof(object)));
            Func<object> f = e.Compile(useInterpreter); // System.ArgumentException: Unhandled unary: TypeAs
            Assert.Equal(0, f());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void TypesIsConstantValueType(bool useInterpreter)
        {
            Expression<Func<bool>> e = Expression.Lambda<Func<bool>>(Expression.TypeIs(Expression.Constant(5), typeof(object)));
            Func<bool> f = e.Compile(useInterpreter);
            Assert.True(f());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConstantEmitsValidIL(bool useInterpreter)
        {
            Expression<Func<byte>> e = Expression.Lambda<Func<byte>>(Expression.Constant((byte)0), Enumerable.Empty<ParameterExpression>());
            Func<byte> f = e.Compile(useInterpreter);
            Assert.Equal((byte)0, f());
        }

        public struct MyStruct : IComparable
        {
            int IComparable.CompareTo(object other)
            {
                return 0;
            }
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void Casts(bool useInterpreter)
        {
            // System.ValueType to value type
            Assert.Equal(10, TestCast<System.ValueType, int>(10, useInterpreter));

            // System.ValueType to enum type
            Assert.Equal(ExpressionType.Add, TestCast<System.ValueType, ExpressionType>(ExpressionType.Add, useInterpreter));

            // System.Enum to enum type
            Assert.Equal(ExpressionType.Add, TestCast<System.Enum, ExpressionType>(ExpressionType.Add, useInterpreter));

            // System.ValueType to nullable value type
            Assert.Equal(10, TestCast<System.ValueType, int?>(10, useInterpreter));

            // System.ValueType to nullable enum type
            Assert.Equal(ExpressionType.Add, TestCast<System.ValueType, ExpressionType?>(ExpressionType.Add, useInterpreter));

            // System.Enum to nullable enum type
            Assert.Equal(ExpressionType.Add, TestCast<System.Enum, ExpressionType?>(ExpressionType.Add, useInterpreter));

            // Enum to System.Enum
            Assert.Equal(ExpressionType.Add, TestCast<ExpressionType, System.Enum>(ExpressionType.Add, useInterpreter));

            // Enum to System.ValueType
            Assert.Equal(ExpressionType.Add, TestCast<ExpressionType, System.ValueType>(ExpressionType.Add, useInterpreter));

            // nullable enum to System.Enum
            Assert.Equal(ExpressionType.Add, TestCast<ExpressionType?, System.Enum>(ExpressionType.Add, useInterpreter));

            // nullable enum to System.ValueType
            Assert.Equal(ExpressionType.Add, TestCast<ExpressionType?, System.ValueType>(ExpressionType.Add, useInterpreter));

            // nullable to object (box)
            Assert.Equal(10, TestCast<int?, object>(10, useInterpreter));

            // object to nullable (unbox)
            Assert.Equal(10, TestCast<object, int?>(10, useInterpreter));

            // nullable to interface (box + cast)
            TestCast<int?, IComparable>(10, useInterpreter);

            // interface to nullable (unbox)
            TestCast<IComparable, int?>(10, useInterpreter);

            // interface to interface
            TestCast<IComparable, IEquatable<int>>(10, useInterpreter);
            TestCast<IEquatable<int>, IComparable>(10, useInterpreter);

            // value type to object (box)
            Assert.Equal(10, TestCast<int, object>(10, useInterpreter));

            // object to value type (unbox)
            Assert.Equal(10, TestCast<object, int>(10, useInterpreter));

            // tests with user defined struct
            TestCast<ValueType, MyStruct>(new MyStruct(), useInterpreter);
            TestCast<MyStruct, ValueType>(new MyStruct(), useInterpreter);
            TestCast<object, MyStruct>(new MyStruct(), useInterpreter);
            TestCast<MyStruct, object>(new MyStruct(), useInterpreter);
            TestCast<IComparable, MyStruct>(new MyStruct(), useInterpreter);
            TestCast<MyStruct, IComparable>(new MyStruct(), useInterpreter);
            TestCast<ValueType, MyStruct?>(new MyStruct(), useInterpreter);
            TestCast<MyStruct?, ValueType>(new MyStruct(), useInterpreter);
            TestCast<object, MyStruct?>(new MyStruct(), useInterpreter);
            TestCast<MyStruct?, object>(new MyStruct(), useInterpreter);
            TestCast<IComparable, MyStruct?>(new MyStruct(), useInterpreter);
            TestCast<MyStruct?, IComparable>(new MyStruct(), useInterpreter);
        }

        private static S TestCast<T, S>(T value, bool useInterpreter)
        {
            Func<S> d = Expression.Lambda<Func<S>>(Expression.Convert(Expression.Constant(value, typeof(T)), typeof(S))).Compile(useInterpreter);
            return d();
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void Conversions(bool useInterpreter)
        {
            Assert.Equal((byte)10, TestConvert<byte, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<sbyte, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<short, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<ushort, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<int, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<uint, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<long, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<ulong, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<float, byte>(10.0f, useInterpreter));
            Assert.Equal((byte)10, TestConvert<float, byte>(10.0f, useInterpreter));
            Assert.Equal((byte)10, TestConvert<double, byte>(10.0, useInterpreter));
            Assert.Equal((byte)10, TestConvert<double, byte>(10.0, useInterpreter));
            Assert.Equal((byte)10, TestConvert<decimal, byte>(10m, useInterpreter));
            Assert.Equal((byte)10, TestConvert<decimal, byte>(10m, useInterpreter));

            Assert.Equal((short)10, TestConvert<byte, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<sbyte, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<short, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<ushort, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<int, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<uint, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<long, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<ulong, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<float, short>(10.0f, useInterpreter));
            Assert.Equal((short)10, TestConvert<double, short>(10.0, useInterpreter));
            Assert.Equal((short)10, TestConvert<decimal, short>(10m, useInterpreter));

            Assert.Equal((int)10, TestConvert<byte, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<sbyte, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<short, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<ushort, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<int, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<uint, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<long, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<ulong, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<float, int>(10.0f, useInterpreter));
            Assert.Equal((int)10, TestConvert<double, int>(10.0, useInterpreter));
            Assert.Equal((int)10, TestConvert<decimal, int>(10m, useInterpreter));

            Assert.Equal((long)10, TestConvert<byte, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<sbyte, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<short, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<ushort, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<int, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<uint, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<long, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<ulong, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<float, long>(10.0f, useInterpreter));
            Assert.Equal((long)10, TestConvert<double, long>(10.0, useInterpreter));
            Assert.Equal((long)10, TestConvert<decimal, long>(10m, useInterpreter));

            Assert.Equal((double)10, TestConvert<byte, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<sbyte, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<short, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<ushort, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<int, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<uint, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<long, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<ulong, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<float, double>(10.0f, useInterpreter));
            Assert.Equal((double)10, TestConvert<double, double>(10.0, useInterpreter));
            Assert.Equal((double)10, TestConvert<decimal, double>(10m, useInterpreter));

            Assert.Equal((decimal)10, TestConvert<byte, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<sbyte, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<short, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<ushort, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<int, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<uint, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<long, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<ulong, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<float, decimal>(10.0f, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<double, decimal>(10.0, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<decimal, decimal>(10m, useInterpreter));

            // nullable to non-nullable
            Assert.Equal((byte)10, TestConvert<byte?, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<sbyte?, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<short?, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<ushort?, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<int?, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<uint?, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<long?, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<ulong?, byte>(10, useInterpreter));
            Assert.Equal((byte)10, TestConvert<float?, byte>(10.0f, useInterpreter));
            Assert.Equal((byte)10, TestConvert<float?, byte>(10.0f, useInterpreter));
            Assert.Equal((byte)10, TestConvert<double?, byte>(10.0, useInterpreter));
            Assert.Equal((byte)10, TestConvert<double?, byte>(10.0, useInterpreter));
            Assert.Equal((byte)10, TestConvert<decimal?, byte>(10m, useInterpreter));
            Assert.Equal((byte)10, TestConvert<decimal?, byte>(10m, useInterpreter));

            Assert.Equal((short)10, TestConvert<byte?, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<sbyte?, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<short?, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<ushort?, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<int?, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<uint?, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<long?, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<ulong?, short>(10, useInterpreter));
            Assert.Equal((short)10, TestConvert<float?, short>(10.0f, useInterpreter));
            Assert.Equal((short)10, TestConvert<double?, short>(10.0, useInterpreter));
            Assert.Equal((short)10, TestConvert<decimal?, short>(10m, useInterpreter));

            Assert.Equal((int)10, TestConvert<byte?, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<sbyte?, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<short?, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<ushort?, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<int?, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<uint?, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<long?, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<ulong?, int>(10, useInterpreter));
            Assert.Equal((int)10, TestConvert<float?, int>(10.0f, useInterpreter));
            Assert.Equal((int)10, TestConvert<double?, int>(10.0, useInterpreter));
            Assert.Equal((int)10, TestConvert<decimal?, int>(10m, useInterpreter));

            Assert.Equal((long)10, TestConvert<byte?, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<sbyte?, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<short?, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<ushort?, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<int?, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<uint?, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<long?, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<ulong?, long>(10, useInterpreter));
            Assert.Equal((long)10, TestConvert<float?, long>(10.0f, useInterpreter));
            Assert.Equal((long)10, TestConvert<double?, long>(10.0, useInterpreter));
            Assert.Equal((long)10, TestConvert<decimal?, long>(10m, useInterpreter));

            Assert.Equal((double)10, TestConvert<byte?, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<sbyte?, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<short?, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<ushort?, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<int?, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<uint?, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<long?, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<ulong?, double>(10, useInterpreter));
            Assert.Equal((double)10, TestConvert<float?, double>(10.0f, useInterpreter));
            Assert.Equal((double)10, TestConvert<double?, double>(10.0, useInterpreter));
            Assert.Equal((double)10, TestConvert<decimal?, double>(10m, useInterpreter));

            Assert.Equal((decimal)10, TestConvert<byte?, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<sbyte?, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<short?, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<ushort?, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<int?, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<uint?, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<long?, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<ulong?, decimal>(10, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<float?, decimal>(10.0f, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<double?, decimal>(10.0, useInterpreter));
            Assert.Equal((decimal)10, TestConvert<decimal?, decimal>(10m, useInterpreter));

            // non-nullable to nullable
            Assert.Equal((byte?)10, TestConvert<byte, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<sbyte, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<short, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<ushort, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<int, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<uint, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<long, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<ulong, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<float, byte?>(10.0f, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<float, byte?>(10.0f, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<double, byte?>(10.0, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<double, byte?>(10.0, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<decimal, byte?>(10m, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<decimal, byte?>(10m, useInterpreter));

            Assert.Equal((short?)10, TestConvert<byte, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<sbyte, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<short, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<ushort, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<int, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<uint, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<long, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<ulong, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<float, short?>(10.0f, useInterpreter));
            Assert.Equal((short?)10, TestConvert<double, short?>(10.0, useInterpreter));
            Assert.Equal((short?)10, TestConvert<decimal, short?>(10m, useInterpreter));

            Assert.Equal((int?)10, TestConvert<byte, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<sbyte, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<short, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<ushort, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<int, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<uint, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<long, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<ulong, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<float, int?>(10.0f, useInterpreter));
            Assert.Equal((int?)10, TestConvert<double, int?>(10.0, useInterpreter));
            Assert.Equal((int?)10, TestConvert<decimal, int?>(10m, useInterpreter));

            Assert.Equal((long?)10, TestConvert<byte, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<sbyte, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<short, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<ushort, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<int, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<uint, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<long, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<ulong, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<float, long?>(10.0f, useInterpreter));
            Assert.Equal((long?)10, TestConvert<double, long?>(10.0, useInterpreter));
            Assert.Equal((long?)10, TestConvert<decimal, long?>(10m, useInterpreter));

            Assert.Equal((double?)10, TestConvert<byte, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<sbyte, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<short, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<ushort, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<int, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<uint, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<long, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<ulong, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<float, double?>(10.0f, useInterpreter));
            Assert.Equal((double?)10, TestConvert<double, double?>(10.0, useInterpreter));
            Assert.Equal((double?)10, TestConvert<decimal, double?>(10m, useInterpreter));

            Assert.Equal((decimal?)10, TestConvert<byte, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<sbyte, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<short, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<ushort, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<int, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<uint, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<long, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<ulong, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<float, decimal?>(10.0f, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<double, decimal?>(10.0, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<decimal, decimal?>(10m, useInterpreter));

            // nullable to nullable
            Assert.Equal((byte?)10, TestConvert<byte?, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<sbyte?, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<short?, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<ushort?, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<int?, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<uint?, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<long?, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<ulong?, byte?>(10, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<float?, byte?>(10.0f, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<float?, byte?>(10.0f, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<double?, byte?>(10.0, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<double?, byte?>(10.0, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<decimal?, byte?>(10m, useInterpreter));
            Assert.Equal((byte?)10, TestConvert<decimal?, byte?>(10m, useInterpreter));

            Assert.Equal((short?)10, TestConvert<byte?, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<sbyte?, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<short?, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<ushort?, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<int?, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<uint?, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<long?, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<ulong?, short?>(10, useInterpreter));
            Assert.Equal((short?)10, TestConvert<float?, short?>(10.0f, useInterpreter));
            Assert.Equal((short?)10, TestConvert<double?, short?>(10.0, useInterpreter));
            Assert.Equal((short?)10, TestConvert<decimal?, short?>(10m, useInterpreter));

            Assert.Equal((int?)10, TestConvert<byte?, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<sbyte?, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<short?, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<ushort?, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<int?, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<uint?, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<long?, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<ulong?, int?>(10, useInterpreter));
            Assert.Equal((int?)10, TestConvert<float?, int?>(10.0f, useInterpreter));
            Assert.Equal((int?)10, TestConvert<double?, int?>(10.0, useInterpreter));
            Assert.Equal((int?)10, TestConvert<decimal?, int?>(10m, useInterpreter));

            Assert.Equal((long?)10, TestConvert<byte?, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<sbyte?, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<short?, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<ushort?, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<int?, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<uint?, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<long?, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<ulong?, long?>(10, useInterpreter));
            Assert.Equal((long?)10, TestConvert<float?, long?>(10.0f, useInterpreter));
            Assert.Equal((long?)10, TestConvert<double?, long?>(10.0, useInterpreter));
            Assert.Equal((long?)10, TestConvert<decimal?, long?>(10m, useInterpreter));

            Assert.Equal((double?)10, TestConvert<byte?, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<sbyte?, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<short?, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<ushort?, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<int?, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<uint?, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<long?, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<ulong?, double?>(10, useInterpreter));
            Assert.Equal((double?)10, TestConvert<float?, double?>(10.0f, useInterpreter));
            Assert.Equal((double?)10, TestConvert<double?, double?>(10.0, useInterpreter));
            Assert.Equal((double?)10, TestConvert<decimal?, double?>(10m, useInterpreter));

            Assert.Equal((decimal?)10, TestConvert<byte?, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<sbyte?, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<short?, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<ushort?, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<int?, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<uint?, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<long?, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<ulong?, decimal?>(10, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<float?, decimal?>(10.0f, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<double?, decimal?>(10.0, useInterpreter));
            Assert.Equal((decimal?)10, TestConvert<decimal?, decimal?>(10m, useInterpreter));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConvertMinMax(bool useInterpreter)
        {
            unchecked
            {
                Assert.Equal((float)uint.MaxValue, TestConvert<uint, float>(uint.MaxValue, useInterpreter));
                Assert.Equal((double)uint.MaxValue, TestConvert<uint, double>(uint.MaxValue, useInterpreter));
                Assert.Equal((float?)uint.MaxValue, TestConvert<uint, float?>(uint.MaxValue, useInterpreter));
                Assert.Equal((double?)uint.MaxValue, TestConvert<uint, double?>(uint.MaxValue, useInterpreter));

                Assert.Equal((float)ulong.MaxValue, TestConvert<ulong, float>(ulong.MaxValue, useInterpreter));
                Assert.Equal((double)ulong.MaxValue, TestConvert<ulong, double>(ulong.MaxValue, useInterpreter));
                Assert.Equal((float?)ulong.MaxValue, TestConvert<ulong, float?>(ulong.MaxValue, useInterpreter));
                Assert.Equal((double?)ulong.MaxValue, TestConvert<ulong, double?>(ulong.MaxValue, useInterpreter));

                /*
                 * needs more thought about what should happen.. these have undefined runtime behavior.
                 * results dependon whether values are in registers or locals, debug or retail etc.
                 * 
                float fmin = float.MinValue;
                float fmax = float.MaxValue;
                double dmin = double.MinValue;
                double dmax = double.MaxValue;

                Assert.AreEqual((uint)fmin, TestConvert<float, uint>(fmin, useInterpreter));
                Assert.AreEqual((ulong)fmax, TestConvert<float, ulong>(fmax, useInterpreter));
                Assert.AreEqual((uint?)fmin, TestConvert<float, uint?>(fmin, useInterpreter));
                Assert.AreEqual((ulong?)fmax, TestConvert<float, ulong?>(fmax, useInterpreter));

                Assert.AreEqual((uint)dmin, TestConvert<double, uint>(dmin, useInterpreter));
                Assert.AreEqual((ulong)dmax, TestConvert<double, ulong>(dmax, useInterpreter));
                Assert.AreEqual((uint?)dmin, TestConvert<double, uint?>(dmin, useInterpreter));
                Assert.AreEqual((ulong?)dmax, TestConvert<double, ulong?>(dmax, useInterpreter));
                 */

                Assert.Equal((float)(uint?)uint.MaxValue, TestConvert<uint?, float>(uint.MaxValue, useInterpreter));
                Assert.Equal((double)(uint?)uint.MaxValue, TestConvert<uint?, double>(uint.MaxValue, useInterpreter));
                Assert.Equal((float?)(uint?)uint.MaxValue, TestConvert<uint?, float?>(uint.MaxValue, useInterpreter));
                Assert.Equal((double?)(uint?)uint.MaxValue, TestConvert<uint?, double?>(uint.MaxValue, useInterpreter));

                Assert.Equal((float)(ulong?)ulong.MaxValue, TestConvert<ulong?, float>(ulong.MaxValue, useInterpreter));
                Assert.Equal((double)(ulong?)ulong.MaxValue, TestConvert<ulong?, double>(ulong.MaxValue, useInterpreter));
                Assert.Equal((float?)(ulong?)ulong.MaxValue, TestConvert<ulong?, float?>(ulong.MaxValue, useInterpreter));
                Assert.Equal((double?)(ulong?)ulong.MaxValue, TestConvert<ulong?, double?>(ulong.MaxValue, useInterpreter));
            }
        }

        private static S TestConvert<T, S>(T value, bool useInterpreter)
        {
            Func<S> d = Expression.Lambda<Func<S>>(Expression.Convert(Expression.Constant(value, typeof(T)), typeof(S))).Compile(useInterpreter);
            return d();
        }

        private static S TestConvertChecked<T, S>(T value, bool useInterpreter)
        {
            Func<S> d = Expression.Lambda<Func<S>>(Expression.ConvertChecked(Expression.Constant(value, typeof(T)), typeof(S))).Compile(useInterpreter);
            return d();
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConvertNullToInt(bool useInterpreter)
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                Expression<Func<ValueType, int>> e = v => (int)v;
                Func<ValueType, int> f = e.Compile(useInterpreter);
                f(null);
            });
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ShiftWithMismatchedNulls(bool useInterpreter)
        {
            Expression<Func<byte?, int, int?>> e = (byte? b, int i) => (byte?)(b << i);
            var f = e.Compile(useInterpreter);
            Assert.Equal(20, f(5, 2));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void CoalesceChars(bool useInterpreter)
        {
            ParameterExpression x = Expression.Parameter(typeof(char?), "x");
            ParameterExpression y = Expression.Parameter(typeof(char?), "y");
            Expression<Func<char?, char?, char?>> e =
                Expression.Lambda<Func<char?, char?, char?>>(
                    Expression.Coalesce(x, y),
                    new ParameterExpression[] { x, y });
            Func<char?, char?, char?> f = e.Compile(useInterpreter);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConvertToChar(bool useInterpreter)
        {
            Func<char> f = Expression.Lambda<Func<Char>>(Expression.Convert(Expression.Constant((byte)65), typeof(char))).Compile(useInterpreter);
            Assert.Equal('A', f());

            Func<char> f2 = Expression.Lambda<Func<Char>>(Expression.Convert(Expression.Constant(65), typeof(char))).Compile(useInterpreter);
            Assert.Equal('A', f2());

            Func<char> f3 = Expression.Lambda<Func<Char>>(Expression.Convert(Expression.Constant(-1), typeof(char))).Compile(useInterpreter);
            char c3 = f3();
            Func<int> f4 = Expression.Lambda<Func<int>>(Expression.Convert(Expression.Constant(c3), typeof(int))).Compile(useInterpreter);
            Assert.Equal(UInt16.MaxValue, f4());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void MixedTypeNullableOps(bool useInterpreter)
        {
            Expression<Func<decimal, int?, decimal?>> e = (d, i) => d + i;
            var f = e.Compile(useInterpreter);
            var result = f(1.0m, 4);
            Debug.WriteLine(result);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NullGuidConstant(bool useInterpreter)
        {
            Expression<Func<Guid?, bool>> f2 = g2 => g2 != null;
            var d2 = f2.Compile(useInterpreter);
            Assert.True(d2(Guid.NewGuid()));
            Assert.False(d2(null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void AddNullConstants(bool useInterpreter)
        {
            Expression<Func<int?>> f = Expression.Lambda<Func<int?>>(
                Expression.Add(
                    Expression.Constant(null, typeof(int?)),
                    Expression.Constant(1, typeof(int?))
                    ));

            var result = f.Compile(useInterpreter)();
            Assert.False(result.HasValue);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void CallWithRefParam(bool useInterpreter)
        {
            Expression<Func<int, int>> f = x => x + MethodWithRefParam(ref x) + x;
            Func<int, int> d = f.Compile(useInterpreter);
            Assert.Equal(113, d(10));
        }

        public static int MethodWithRefParam(ref int x)
        {
            x = 3;
            return 100;
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void CallWithOutParam(bool useInterpreter)
        {
            Expression<Func<int, int>> f = x => x + MethodWithOutParam(out x) + x;
            Func<int, int> d = f.Compile(useInterpreter);
            Assert.Equal(113, d(10));
        }

        public static int MethodWithOutParam(out int x)
        {
            x = 3;
            return 100;
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NewArrayInvoke(bool useInterpreter)
        {
            Expression<Func<int, string[]>> linq1 = (a => new string[a]);
            InvocationExpression linq1a = Expression.Invoke(linq1, new Expression[] { Expression.Constant(3) });
            Expression<Func<string[]>> linq1b = Expression.Lambda<Func<string[]>>(linq1a, new ParameterExpression[] { });
            Func<string[]> f = linq1b.Compile(useInterpreter);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void LiftedAddDateTimeTimeSpan(bool useInterpreter)
        {
            Expression<Func<DateTime?, TimeSpan, DateTime?>> f = (x, y) => x + y;
            Assert.Equal(ExpressionType.Add, f.Body.NodeType);
            Debug.WriteLine(f);
            Func<DateTime?, TimeSpan, DateTime?> d = f.Compile(useInterpreter);
            DateTime? dt = DateTime.Now;
            TimeSpan ts = new TimeSpan(3, 2, 1);
            DateTime? dt2 = dt + ts;
            Assert.Equal(dt2, d(dt, ts));
            Assert.Null(d(null, ts));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void LiftedAddDateTimeTimeSpan2(bool useInterpreter)
        {
            Expression<Func<DateTime?, TimeSpan?, DateTime?>> f = (x, y) => x + y;
            Assert.Equal(ExpressionType.Add, f.Body.NodeType);
            Debug.WriteLine(f);
            Func<DateTime?, TimeSpan?, DateTime?> d = f.Compile(useInterpreter);
            DateTime? dt = DateTime.Now;
            TimeSpan? ts = new TimeSpan(3, 2, 1);
            DateTime? dt2 = dt + ts;
            Assert.Equal(dt2, d(dt, ts));
            Assert.Null(d(null, ts));
            Assert.Null(d(dt, null));
            Assert.Null(d(null, null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void LiftedSubDateTime(bool useInterpreter)
        {
            Expression<Func<DateTime?, DateTime?, TimeSpan?>> f = (x, y) => x - y;
            Assert.Equal(ExpressionType.Subtract, f.Body.NodeType);
            Debug.WriteLine(f);
            Func<DateTime?, DateTime?, TimeSpan?> d = f.Compile(useInterpreter);
            DateTime? dt1 = DateTime.Now;
            DateTime? dt2 = new DateTime(2006, 5, 1);
            TimeSpan? ts = dt1 - dt2;
            Assert.Equal(ts, d(dt1, dt2));
            Assert.Null(d(null, dt2));
            Assert.Null(d(dt1, null));
            Assert.Null(d(null, null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void LiftedEqualDateTime(bool useInterpreter)
        {
            Expression<Func<DateTime?, DateTime?, bool>> f = (x, y) => x == y;
            Assert.Equal(ExpressionType.Equal, f.Body.NodeType);
            Debug.WriteLine(f);
            Func<DateTime?, DateTime?, bool> d = f.Compile(useInterpreter);
            DateTime? dt1 = DateTime.Now;
            DateTime? dt2 = new DateTime(2006, 5, 1);
            Assert.True(d(dt1, dt1));
            Assert.False(d(dt1, dt2));
            Assert.False(d(null, dt2));
            Assert.False(d(dt1, null));
            Assert.True(d(null, null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void LiftedNotEqualDateTime(bool useInterpreter)
        {
            Expression<Func<DateTime?, DateTime?, bool>> f = (x, y) => x != y;
            Assert.Equal(ExpressionType.NotEqual, f.Body.NodeType);
            Debug.WriteLine(f);
            Func<DateTime?, DateTime?, bool> d = f.Compile(useInterpreter);
            DateTime? dt1 = DateTime.Now;
            DateTime? dt2 = new DateTime(2006, 5, 1);
            Assert.False(d(dt1, dt1));
            Assert.True(d(dt1, dt2));
            Assert.True(d(null, dt2));
            Assert.True(d(dt1, null));
            Assert.False(d(null, null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void LiftedLessThanDateTime(bool useInterpreter)
        {
            Expression<Func<DateTime?, DateTime?, bool>> f = (x, y) => x < y;
            Assert.Equal(ExpressionType.LessThan, f.Body.NodeType);
            Debug.WriteLine(f);
            Func<DateTime?, DateTime?, bool> d = f.Compile(useInterpreter);
            DateTime? dt1 = DateTime.Now;
            DateTime? dt2 = new DateTime(2006, 5, 1);
            Assert.False(d(dt1, dt1));
            Assert.True(d(dt2, dt1));
            Assert.False(d(null, dt2));
            Assert.False(d(dt1, null));
            Assert.False(d(null, null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void LessThanDateTime(bool useInterpreter)
        {
            Expression<Func<DateTime, DateTime, bool>> f = (x, y) => x < y;
            Assert.Equal(ExpressionType.LessThan, f.Body.NodeType);
            Debug.WriteLine(f);
            Func<DateTime, DateTime, bool> d = f.Compile(useInterpreter);
            DateTime dt1 = DateTime.Now;
            DateTime dt2 = new DateTime(2006, 5, 1);
            Assert.False(d(dt1, dt1));
            Assert.True(d(dt2, dt1));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void InvokeLambda(bool useInterpreter)
        {
            Expression<Func<int, int>> f = x => x + 1;
            InvocationExpression ie = Expression.Invoke(f, Expression.Constant(5));
            Expression<Func<int>> lambda = Expression.Lambda<Func<int>>(ie);
            Func<int> d = lambda.Compile(useInterpreter);
            Assert.Equal(6, d());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void CallCompiledLambda(bool useInterpreter)
        {
            Expression<Func<int, int>> f = x => x + 1;
            var compiled = f.Compile(useInterpreter);
            Expression<Func<int>> lambda = () => compiled(5);
            Func<int> d = lambda.Compile(useInterpreter);
            Assert.Equal(6, d());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void CallCompiledLambdaWithTypeMissing(bool useInterpreter)
        {
            Expression<Func<object, bool>> f = x => x == Type.Missing;
            var compiled = f.Compile(useInterpreter);
            Expression<Func<object, bool>> lambda = x => compiled(x);
            Func<object, bool> d = lambda.Compile(useInterpreter);
            Assert.Equal(true, d(Type.Missing));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void InvokeQuotedLambda(bool useInterpreter)
        {
            Expression<Func<int, int>> f = x => x + 1;
            InvocationExpression ie = Expression.Invoke(Expression.Quote(f), Expression.Constant(5));
            Expression<Func<int>> lambda = Expression.Lambda<Func<int>>(ie);
            Func<int> d = lambda.Compile(useInterpreter);
            Assert.Equal(6, d());
        }

        [Theory(Skip = "870811")]
        [ClassData(typeof(CompilationTypes))]
        public static void InvokeComputedLambda(bool useInterpreter)
        {
            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            ParameterExpression y = Expression.Parameter(typeof(int), "y");
            Expression call = Expression.Call(null, typeof(Compiler_Tests).GetMethod("ComputeLambda", BindingFlags.Static | BindingFlags.Public), new Expression[] { y });
            InvocationExpression ie = Expression.Invoke(call, x);
            Expression<Func<int, int, int>> lambda = Expression.Lambda<Func<int, int, int>>(ie, x, y);

            Func<int, int, int> d = lambda.Compile(useInterpreter);
            Assert.Equal(14, d(5, 9));
            Assert.Equal(40, d(5, 8));
        }

        public static Expression<Func<int, int>> ComputeLambda(int y)
        {
            if ((y & 1) != 0)
                return x => x + y;
            else
                return x => x * y;
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void InvokeComputedDelegate(bool useInterpreter)
        {
            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            ParameterExpression y = Expression.Parameter(typeof(int), "y");
            Expression call = Expression.Call(null, typeof(Compiler_Tests).GetMethod("ComputeDelegate", BindingFlags.Static | BindingFlags.Public), new Expression[] { y });
            InvocationExpression ie = Expression.Invoke(call, x);
            Expression<Func<int, int, int>> lambda = Expression.Lambda<Func<int, int, int>>(ie, x, y);

            Func<int, int, int> d = lambda.Compile(useInterpreter);
            Assert.Equal(14, d(5, 9));
            Assert.Equal(40, d(5, 8));
        }

        public static Func<int, int> ComputeDelegate(int y)
        {
            if ((y & 1) != 0)
                return x => x + y;
            else
                return x => x * y;
        }

        [Fact]
        public static void InvokeNonTypedLambdaFails()
        {
            Expression call = Expression.Call(null, typeof(Compiler_Tests).GetMethod("ComputeDynamicLambda", BindingFlags.Static | BindingFlags.Public), new Expression[] { });
            Assert.Throws<ArgumentException>(() => Expression.Invoke(call, null));
        }

        public static LambdaExpression ComputeDynamicLambda()
        {
            return null;
        }

        [Fact]
        public static void InvokeNonTypedDelegateFails()
        {
            Expression call = Expression.Call(null, typeof(Compiler_Tests).GetMethod("ComputeDynamicDelegate", BindingFlags.Static | BindingFlags.Public), new Expression[] { });
            Assert.Throws<ArgumentException>(() => Expression.Invoke(call, null));
        }

        public static Delegate ComputeDynamicDelegate()
        {
            return null;
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NestedQuotedLambdas(bool useInterpreter)
        {
            Expression<Func<int, Expression<Func<int, int>>>> f = a => b => a + b;
            Func<int, Expression<Func<int, int>>> d = f.Compile(useInterpreter);
            Expression<Func<int, int>> f2 = d(3);
            Func<int, int> d2 = f2.Compile(useInterpreter);
            int v = d2(4);
            Assert.Equal(7, v);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void StaticMethodCall(bool useInterpreter)
        {
            Expression<Func<int, int, int>> f = (a, b) => Math.Max(a, b);
            var d = f.Compile(useInterpreter);
            Assert.Equal(4, d(3, 4));
        }

        [Theory(Skip = "870811")]
        [ClassData(typeof(CompilationTypes))]
        public static void CallOnCapturedInstance(bool useInterpreter)
        {
            Foo foo = new Foo();
            Expression<Func<int, int>> f = (a) => foo.Zip(a);
            var d = f.Compile(useInterpreter);
            Assert.Equal(225, d(15));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void VirtualCall(bool useInterpreter)
        {
            Foo bar = new Bar();
            Expression<Func<Foo, string>> f = foo => foo.Virt();
            var d = f.Compile(useInterpreter);
            Assert.Equal("Bar", d(bar));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NestedLambda(bool useInterpreter)
        {
            Expression<Func<int, int>> f = (a) => M1(a, (b) => b * b);
            var d = f.Compile(useInterpreter);
            Assert.Equal(100, d(10));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NestedLambdaWithOuterArg(bool useInterpreter)
        {
            Expression<Func<int, int>> f = (a) => M1(a + a, (b) => b * a);
            var d = f.Compile(useInterpreter);
            Assert.Equal(200, d(10));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NestedExpressionLambda(bool useInterpreter)
        {
            Expression<Func<int, int>> f = (a) => M2(a, (b) => b * b);
            var d = f.Compile(useInterpreter);
            Assert.Equal(10, d(10));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NestedExpressionLambdaWithOuterArg(bool useInterpreter)
        {
            Expression<Func<int, int>> f = (a) => M2(a, (b) => b * a);
            var d = f.Compile(useInterpreter);
            Assert.Equal(99, d(99));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ArrayInitializedWithLiterals(bool useInterpreter)
        {
            Expression<Func<int[]>> f = () => new int[] { 1, 2, 3, 4, 5 };
            var d = f.Compile(useInterpreter);
            int[] v = d();
            Assert.Equal(5, v.Length);
        }

        [Theory(Skip = "870811")]
        [ClassData(typeof(CompilationTypes))]
        public static void ArrayInitializedWithCapturedInstance(bool useInterpreter)
        {
            Foo foo = new Foo();
            Expression<Func<Foo[]>> f = () => new Foo[] { foo };
            var d = f.Compile(useInterpreter);
            Foo[] v = d();
            Assert.Equal(1, v.Length);
            Assert.Equal(foo, v[0]);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NullableAddition(bool useInterpreter)
        {
            Expression<Func<double?, double?>> f = (v) => v + v;
            var d = f.Compile(useInterpreter);
            Assert.Equal(20.0, d(10.0));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NullableComparedToLiteral(bool useInterpreter)
        {
            Expression<Func<int?, bool>> f = (v) => v > 10;
            var d = f.Compile(useInterpreter);
            Assert.True(d(12));
            Assert.False(d(5));
            Assert.True(d(int.MaxValue));
            Assert.False(d(int.MinValue));
            Assert.False(d(null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NullableModuloLiteral(bool useInterpreter)
        {
            Expression<Func<double?, double?>> f = (v) => v % 10;
            var d = f.Compile(useInterpreter);
            Assert.Equal(5.0, d(15.0));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ArrayIndexer(bool useInterpreter)
        {
            Expression<Func<int[], int, int>> f = (v, i) => v[i];
            var d = f.Compile(useInterpreter);
            int[] ints = new[] { 1, 2, 3 };
            Assert.Equal(3, d(ints, 2));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConvertToNullableDouble(bool useInterpreter)
        {
            Expression<Func<int?, double?>> f = (v) => (double?)v;
            var d = f.Compile(useInterpreter);
            Assert.Equal(10.0, d(10));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void UnboxToInt(bool useInterpreter)
        {
            Expression<Func<object, int>> f = (a) => (int)a;
            var d = f.Compile(useInterpreter);
            Assert.Equal(5, d(5));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void TypeIs(bool useInterpreter)
        {
            Expression<Func<Foo, bool>> f = x => x is Foo;
            var d = f.Compile(useInterpreter);
            Assert.True(d(new Foo()));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void TypeAs(bool useInterpreter)
        {
            Expression<Func<Foo, Bar>> f = x => x as Bar;
            var d = f.Compile(useInterpreter);
            Assert.Null(d(new Foo()));
            Assert.NotNull(d(new Bar()));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void Coalesce(bool useInterpreter)
        {
            Expression<Func<int?, int>> f = x => x ?? 5;
            var d = f.Compile(useInterpreter);
            Assert.Equal(5, d(null));
            Assert.Equal(2, d(2));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void CoalesceRefTypes(bool useInterpreter)
        {
            Expression<Func<string, string>> f = x => x ?? "nil";
            var d = f.Compile(useInterpreter);
            Assert.Equal("nil", d(null));
            Assert.Equal("Not Nil", d("Not Nil"));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void Conditional(bool useInterpreter)
        {
            Expression<Func<int, int, int>> f = (x, y) => x > 5 ? x : y;
            var d = f.Compile(useInterpreter);
            Assert.Equal(7, d(7, 4));
            Assert.Equal(6, d(3, 6));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void MultiDimensionalArrayAccess(bool useInterpreter)
        {
            Expression<Func<int, int, int[,], int>> f = (x, y, a) => a[x, y];
            var d = f.Compile(useInterpreter);
            int[,] array = new int[2, 2] { { 0, 1 }, { 2, 3 } };
            Assert.Equal(3, d(1, 1, array));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NewClassWithMemberIntializer(bool useInterpreter)
        {
            Expression<Func<int, ClassX>> f = v => new ClassX { A = v };
            var d = f.Compile(useInterpreter);
            Assert.Equal(5, d(5).A);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NewStructWithArgs(bool useInterpreter)
        {
            Expression<Func<int, StructZ>> f = v => new StructZ(v);
            var d = f.Compile(useInterpreter);
            Assert.Equal(5, d(5).A);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NewStructWithArgsAndMemberInitializer(bool useInterpreter)
        {
            Expression<Func<int, StructZ>> f = v => new StructZ(v) { A = v + 1 };
            var d = f.Compile(useInterpreter);
            Assert.Equal(6, d(5).A);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NewClassWithMemberIntializers(bool useInterpreter)
        {
            Expression<Func<int, ClassX>> f = v => new ClassX { A = v, B = v };
            var d = f.Compile(useInterpreter);
            Assert.Equal(5, d(5).A);
            Assert.Equal(7, d(7).B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NewStructWithMemberIntializer(bool useInterpreter)
        {
            Expression<Func<int, StructX>> f = v => new StructX { A = v };
            var d = f.Compile(useInterpreter);
            Assert.Equal(5, d(5).A);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NewStructWithMemberIntializers(bool useInterpreter)
        {
            Expression<Func<int, StructX>> f = v => new StructX { A = v, B = v };
            var d = f.Compile(useInterpreter);
            Assert.Equal(5, d(5).A);
            Assert.Equal(7, d(7).B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ListInitializer(bool useInterpreter)
        {
            Expression<Func<int, List<ClassY>>> f = x => new List<ClassY> { new ClassY { B = x } };
            var d = f.Compile(useInterpreter);
            List<ClassY> list = d(5);
            Assert.Equal(1, list.Count);
            Assert.Equal(5, list[0].B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ListInitializerLong(bool useInterpreter)
        {
            Expression<Func<int, List<ClassY>>> f = x => new List<ClassY> { new ClassY { B = x }, new ClassY { B = x + 1 }, new ClassY { B = x + 2 } };
            var d = f.Compile(useInterpreter);
            List<ClassY> list = d(5);
            Assert.Equal(3, list.Count);
            Assert.Equal(5, list[0].B);
            Assert.Equal(6, list[1].B);
            Assert.Equal(7, list[2].B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ListInitializerInferred(bool useInterpreter)
        {
            Expression<Func<int, List<ClassY>>> f = x => new List<ClassY> { new ClassY { B = x }, new ClassY { B = x + 1 }, new ClassY { B = x + 2 } };
            var d = f.Compile(useInterpreter);
            List<ClassY> list = d(5);
            Assert.Equal(3, list.Count);
            Assert.Equal(5, list[0].B);
            Assert.Equal(6, list[1].B);
            Assert.Equal(7, list[2].B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public void NewClassWithMemberListIntializer(bool useInterpreter)
        {
            Expression<Func<int, ClassX>> f =
                v => new ClassX { A = v, B = v + 1, Ys = { new ClassY { B = v + 2 } } };
            var d = f.Compile(useInterpreter);
            ClassX x = d(5);
            Assert.Equal(5, x.A);
            Assert.Equal(6, x.B);
            Assert.Equal(1, x.Ys.Count);
            Assert.Equal(7, x.Ys[0].B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public void NewClassWithMemberListOfStructIntializer(bool useInterpreter)
        {
            Expression<Func<int, ClassX>> f =
                v => new ClassX { A = v, B = v + 1, SYs = { new StructY { B = v + 2 } } };
            var d = f.Compile(useInterpreter);
            ClassX x = d(5);
            Assert.Equal(5, x.A);
            Assert.Equal(6, x.B);
            Assert.Equal(1, x.SYs.Count);
            Assert.Equal(7, x.SYs[0].B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NewClassWithMemberMemberIntializer(bool useInterpreter)
        {
            Expression<Func<int, ClassX>> f =
                v => new ClassX { A = v, B = v + 1, Y = { B = v + 2 } };
            var d = f.Compile(useInterpreter);
            ClassX x = d(5);
            Assert.Equal(5, x.A);
            Assert.Equal(6, x.B);
            Assert.Equal(7, x.Y.B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public void NewStructWithMemberListIntializer(bool useInterpreter)
        {
            Expression<Func<int, StructX>> f =
                v => new StructX { A = v, B = v + 1, Ys = { new ClassY { B = v + 2 } } };
            var d = f.Compile(useInterpreter);
            StructX x = d(5);
            Assert.Equal(5, x.A);
            Assert.Equal(6, x.B);
            Assert.Equal(1, x.Ys.Count);
            Assert.Equal(7, x.Ys[0].B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public void NewStructWithStructMemberMemberIntializer(bool useInterpreter)
        {
            Expression<Func<int, StructX>> f =
                v => new StructX { A = v, B = v + 1, SY = new StructY { B = v + 2 } };
            var d = f.Compile(useInterpreter);
            StructX x = d(5);
            Assert.Equal(5, x.A);
            Assert.Equal(6, x.B);
            Assert.Equal(7, x.SY.B);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void StructStructMemberInitializationThroughPropertyThrowsException(bool useInterpreter)
        {
            Expression<Func<int, StructX>> f = GetExpressionTreeForMemberInitializationThroughProperty<StructX>();
            Assert.Throws<InvalidOperationException>(() => f.Compile(useInterpreter));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ClassStructMemberInitializationThroughPropertyThrowsException(bool useInterpreter)
        {
            Expression<Func<int, ClassX>> f = GetExpressionTreeForMemberInitializationThroughProperty<ClassX>();
            Assert.Throws<InvalidOperationException>(() => f.Compile(useInterpreter));
        }

        private static Expression<Func<int, T>> GetExpressionTreeForMemberInitializationThroughProperty<T>()
        {
            // Generate the expression:
            //   v => new T { A = v, B = v + 1, SYP = { B = v + 2 } };
            var parameterV = Expression.Parameter(typeof(int), "v");
            return
                Expression.Lambda<Func<int, T>>(
                    // new T { A = v, B= v + 1, SYP = { B = v + 2 } }
                    Expression.MemberInit(
                        // new T 
                        Expression.New(typeof(T).GetConstructor(new Type[0])),

                        // { A = v, B= v + 1, SYP = { B = v + 2 } };
                        new MemberBinding[] {
                            // A = v
                            Expression.Bind(typeof(T).GetField("A"), parameterV),

                            // B = v + 1
                            Expression.Bind(typeof(T).GetField("B"), Expression.Add(parameterV, Expression.Constant(1, typeof(int)))),

                            // SYP = { B = v + 2 } 
                            Expression.MemberBind(
                                typeof(T).GetMethod("get_SYP"),
                                new MemberBinding[] {
                                    Expression.Bind(
                                        typeof(StructY).GetField("B"),
                                        Expression.Add(
                                            parameterV,
                                            Expression.Constant(2, typeof(int))
                                        )
                                    )
                                }
                            )
                        }
                    ),

                    // v =>
                    new ParameterExpression[] { parameterV }
                );
        }


        [Fact]
        public static void ShortCircuitAnd()
        {
            int[] values = new[] { 1, 2, 3, 4, 5 };

            var q = from v in values.AsQueryable()
                    where v == 100 && BadJuju(v) == 10
                    select v;

            var list = q.ToList();
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public static void ShortCircuitOr()
        {
            int[] values = new[] { 1, 2, 3, 4, 5 };

            var q = from v in values.AsQueryable()
                    where v != 100 || BadJuju(v) == 10
                    select v;

            var list = q.ToList();
            Assert.Equal(values.Length, list.Count);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void UnaryOperators(bool useInterpreter)
        {
            // Not
            Assert.False(TestUnary<bool, bool>(ExpressionType.Not, true, useInterpreter));
            Assert.True(TestUnary<bool, bool>(ExpressionType.Not, false, useInterpreter));
            Assert.False((bool)TestUnary<bool?, bool?>(ExpressionType.Not, true, useInterpreter));
            Assert.True((bool)TestUnary<bool?, bool?>(ExpressionType.Not, false, useInterpreter));
            Assert.Null(TestUnary<bool?, bool?>(ExpressionType.Not, null, useInterpreter));
            Assert.Equal(~1, TestUnary<int, int>(ExpressionType.Not, 1, useInterpreter));
            Assert.Equal(~1, TestUnary<int?, int?>(ExpressionType.Not, 1, useInterpreter));
            Assert.Null(TestUnary<int?, int?>(ExpressionType.Not, null, useInterpreter));

            // Negate
            Assert.Equal(-1, TestUnary<int, int>(ExpressionType.Negate, 1, useInterpreter));
            Assert.Equal(-1, TestUnary<int?, int?>(ExpressionType.Negate, 1, useInterpreter));
            Assert.Null(TestUnary<int?, int?>(ExpressionType.Negate, null, useInterpreter));
            Assert.Equal(-1, TestUnary<int, int>(ExpressionType.NegateChecked, 1, useInterpreter));
            Assert.Equal(-1, TestUnary<int?, int?>(ExpressionType.NegateChecked, 1, useInterpreter));
            Assert.Null(TestUnary<int?, int?>(ExpressionType.NegateChecked, null, useInterpreter));

            Assert.Equal(-1, TestUnary<decimal, decimal>(ExpressionType.Negate, 1, useInterpreter));
            Assert.Equal(-1, TestUnary<decimal?, decimal?>(ExpressionType.Negate, 1, useInterpreter));
            Assert.Null(TestUnary<decimal?, decimal?>(ExpressionType.Negate, null, useInterpreter));
            Assert.Equal(-1, TestUnary<decimal, decimal>(ExpressionType.NegateChecked, 1, useInterpreter));
            Assert.Equal(-1, TestUnary<decimal?, decimal?>(ExpressionType.NegateChecked, 1, useInterpreter));
            Assert.Null(TestUnary<decimal?, decimal?>(ExpressionType.NegateChecked, null, useInterpreter));
        }

        private static R TestUnary<T, R>(Expression<Func<T, R>> f, T v, bool useInterpreter)
        {
            Func<T, R> d = f.Compile(useInterpreter);
            R rv = d(v);
            return rv;
        }

        private static R TestUnary<T, R>(ExpressionType op, T v, bool useInterpreter)
        {
            ParameterExpression p = Expression.Parameter(typeof(T), "v");
            Expression<Func<T, R>> f = Expression.Lambda<Func<T, R>>(Expression.MakeUnary(op, p, null), p);
            return TestUnary(f, v, useInterpreter);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ShiftULong(bool useInterpreter)
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Expression<Func<ulong>> e =
                  Expression.Lambda<Func<ulong>>(
                    Expression.RightShift(
                        Expression.Constant((ulong)5, typeof(ulong)),
                        Expression.Constant((ulong)1, typeof(ulong))),
                    Enumerable.Empty<ParameterExpression>());
                Func<ulong> f = e.Compile(useInterpreter);
                f();
            });
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void MultiplyMinInt(bool useInterpreter)
        {
            Assert.Throws<OverflowException>(() =>
            {
                Func<long> f = Expression.Lambda<Func<long>>(
                  Expression.MultiplyChecked(
                    Expression.Constant((long)-1, typeof(long)),
                    Expression.Constant(long.MinValue, typeof(long))),
                    Enumerable.Empty<ParameterExpression>()
                    ).Compile(useInterpreter);
                f();
            });
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void MultiplyMinInt2(bool useInterpreter)
        {
            Assert.Throws<OverflowException>(() =>
            {
                Func<long> f = Expression.Lambda<Func<long>>(
                  Expression.MultiplyChecked(
                    Expression.Constant(long.MinValue, typeof(long)),
                    Expression.Constant((long)-1, typeof(long))),
                  Enumerable.Empty<ParameterExpression>()).Compile(useInterpreter);
                f();
            });
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConvertSignedToUnsigned(bool useInterpreter)
        {
            Func<ulong> f = Expression.Lambda<Func<ulong>>(Expression.Convert(Expression.Constant((sbyte)-1), typeof(ulong))).Compile(useInterpreter);
            Assert.Equal(UInt64.MaxValue, f());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConvertUnsignedToSigned(bool useInterpreter)
        {
            Func<sbyte> f = Expression.Lambda<Func<sbyte>>(Expression.Convert(Expression.Constant(UInt64.MaxValue), typeof(sbyte))).Compile(useInterpreter);
            Assert.Equal((sbyte)-1, f());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConvertCheckedSignedToUnsigned(bool useInterpreter)
        {
            Func<ulong> f = Expression.Lambda<Func<ulong>>(Expression.ConvertChecked(Expression.Constant((sbyte)-1), typeof(ulong))).Compile(useInterpreter);
            Assert.Throws<OverflowException>(() => f());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConvertCheckedUnsignedToSigned(bool useInterpreter)
        {
            Func<sbyte> f = Expression.Lambda<Func<sbyte>>(Expression.ConvertChecked(Expression.Constant(UInt64.MaxValue), typeof(sbyte))).Compile(useInterpreter);
            Assert.Throws<OverflowException>(() => f());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void IntSwitch1(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(int));
            var p1 = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Assign(p1, Expression.Constant("default")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("hello")), Expression.Constant(1)),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("two")), Expression.Constant(2)),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lala")), Expression.Constant(1)));

            var block = Expression.Block(new ParameterExpression[] { p1 }, s, p1);

            Func<int, string> f = Expression.Lambda<Func<int, string>>(block, p).Compile(useInterpreter);

            Assert.Equal("hello", f(1));
            Assert.Equal("two", f(2));
            Assert.Equal("default", f(3));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NullableIntSwitch1(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(int?));
            var p1 = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Assign(p1, Expression.Constant("default")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("hello")), Expression.Constant((int?)1, typeof(int?))),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("two")), Expression.Constant((int?)2, typeof(int?))),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lala")), Expression.Constant((int?)1, typeof(int?))));

            var block = Expression.Block(new ParameterExpression[] { p1 }, s, p1);

            Func<int?, string> f = Expression.Lambda<Func<int?, string>>(block, p).Compile(useInterpreter);

            Assert.Equal("hello", f(1));
            Assert.Equal("two", f(2));
            Assert.Equal("default", f(null));
            Assert.Equal("default", f(3));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NullableIntSwitch2(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(int?));
            var p1 = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Assign(p1, Expression.Constant("default")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("hello")), Expression.Constant((int?)1, typeof(int?))),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("two")), Expression.Constant((int?)2, typeof(int?))),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("null")), Expression.Constant((int?)null, typeof(int?))),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lala")), Expression.Constant((int?)1, typeof(int?))));

            var block = Expression.Block(new ParameterExpression[] { p1 }, s, p1);

            Func<int?, string> f = Expression.Lambda<Func<int?, string>>(block, p).Compile(useInterpreter);

            Assert.Equal("hello", f(1));
            Assert.Equal("two", f(2));
            Assert.Equal("null", f(null));
            Assert.Equal("default", f(3));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void IntSwitch2(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(byte));
            var p1 = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Assign(p1, Expression.Constant("default")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("hello")), Expression.Constant((byte)1)),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("two")), Expression.Constant((byte)2)),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lala")), Expression.Constant((byte)1)));

            var block = Expression.Block(new ParameterExpression[] { p1 }, s, p1);

            Func<byte, string> f = Expression.Lambda<Func<byte, string>>(block, p).Compile(useInterpreter);

            Assert.Equal("hello", f(1));
            Assert.Equal("two", f(2));
            Assert.Equal("default", f(3));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void IntSwitch3(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(uint));
            var p1 = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Assign(p1, Expression.Constant("default")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("hello")), Expression.Constant((uint)1)),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("two")), Expression.Constant((uint)2)),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lala")), Expression.Constant((uint)1)),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("wow")), Expression.Constant(uint.MaxValue)));

            var block = Expression.Block(new ParameterExpression[] { p1 }, s, p1);

            Func<uint, string> f = Expression.Lambda<Func<uint, string>>(block, p).Compile(useInterpreter);

            Assert.Equal("hello", f(1));
            Assert.Equal("wow", f(uint.MaxValue));
            Assert.Equal("two", f(2));
            Assert.Equal("default", f(3));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void StringSwitch(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Constant("default"),
                Expression.SwitchCase(Expression.Constant("hello"), Expression.Constant("hi")),
                Expression.SwitchCase(Expression.Constant("lala"), Expression.Constant("bye")));

            Func<string, string> f = Expression.Lambda<Func<string, string>>(s, p).Compile(useInterpreter);

            Assert.Equal("hello", f("hi"));
            Assert.Equal("lala", f("bye"));
            Assert.Equal("default", f("hi2"));
            Assert.Equal("default", f(null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void StringSwitch1(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(string));
            var p1 = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Assign(p1, Expression.Constant("default")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("hello")), Expression.Constant("hi")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("null")), Expression.Constant(null, typeof(string))),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lala")), Expression.Constant("bye")));

            var block = Expression.Block(new ParameterExpression[] { p1 }, s, p1);

            Func<string, string> f = Expression.Lambda<Func<string, string>>(block, p).Compile(useInterpreter);

            Assert.Equal("hello", f("hi"));
            Assert.Equal("lala", f("bye"));
            Assert.Equal("default", f("hi2"));
            Assert.Equal("null", f(null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void StringSwitchNotConstant(bool useInterpreter)
        {
            Expression<Func<string>> expr1 = () => new string('a', 5);
            Expression<Func<string>> expr2 = () => new string('q', 5);

            var p = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Constant("default"),
                Expression.SwitchCase(Expression.Invoke(expr1), Expression.Invoke(expr2)),
                Expression.SwitchCase(Expression.Constant("lala"), Expression.Constant("bye")));

            Func<string, string> f = Expression.Lambda<Func<string, string>>(s, p).Compile(useInterpreter);

            Assert.Equal("aaaaa", f("qqqqq"));
            Assert.Equal("lala", f("bye"));
            Assert.Equal("default", f("hi2"));
            Assert.Equal("default", f(null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ObjectSwitch1(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(object));
            var p1 = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Assign(p1, Expression.Constant("default")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("hello")), Expression.Constant("hi")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("null")), Expression.Constant(null, typeof(string))),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lala")), Expression.Constant("bye")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lalala")), Expression.Constant("hi")));

            var block = Expression.Block(new ParameterExpression[] { p1 }, s, p1);

            Func<object, string> f = Expression.Lambda<Func<object, string>>(block, p).Compile(useInterpreter);

            Assert.Equal("hello", f("hi"));
            Assert.Equal("lala", f("bye"));
            Assert.Equal("default", f("hi2"));
            Assert.Equal("default", f("HI"));
            Assert.Equal("null", f(null));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void DefaultOnlySwitch(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(int));
            var s = Expression.Switch(p, Expression.Constant(42));

            var fInt32Int32 = Expression.Lambda<Func<int, int>>(s, p).Compile(useInterpreter);

            Assert.Equal(42, fInt32Int32(0));
            Assert.Equal(42, fInt32Int32(1));
            Assert.Equal(42, fInt32Int32(-1));

            s = Expression.Switch(typeof(object), p, Expression.Constant("A test string"), null);

            var fInt32Object = Expression.Lambda<Func<int, object>>(s, p).Compile(useInterpreter);

            Assert.Equal("A test string", fInt32Object(0));
            Assert.Equal("A test string", fInt32Object(1));
            Assert.Equal("A test string", fInt32Object(-1));

            p = Expression.Parameter(typeof(string));
            s = Expression.Switch(p, Expression.Constant("foo"));

            var fStringString = Expression.Lambda<Func<string, string>>(s, p).Compile(useInterpreter);

            Assert.Equal("foo", fStringString("bar"));
            Assert.Equal("foo", fStringString(null));
            Assert.Equal("foo", fStringString("foo"));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NoDefaultOrCasesSwitch(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(int));
            var s = Expression.Switch(p, (Expression)null);

            var f = Expression.Lambda<Action<int>>(s, p).Compile(useInterpreter);

            f(0);

            Assert.Equal(s.Type, typeof(void));
        }

        [Fact]
        public static void TypedNoDefaultOrCasesSwitch()
        {
            var p = Expression.Parameter(typeof(int));
            // A SwitchExpression with neither a defaultBody nor any cases can not be any type except void.
            Assert.Throws<ArgumentException>(() => Expression.Switch(typeof(int), p, (Expression)null, null));
        }

        private delegate int RefSettingDelegate(ref bool changed);

        private delegate void JustRefSettingDelegate(ref bool changed);

        public static int QuestionMeaning(ref bool changed)
        {
            changed = true;
            return 42;
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void DefaultOnlySwitchWithSideEffect(bool useInterpreter)
        {
            bool changed = false;
            var pOut = Expression.Parameter(typeof(bool).MakeByRefType(), "changed");
            var switchValue = Expression.Call(typeof(Compiler_Tests).GetMethod("QuestionMeaning"), pOut);
            var s = Expression.Switch(switchValue, Expression.Constant(42));

            var fInt32Int32 = Expression.Lambda<RefSettingDelegate>(s, pOut).Compile(useInterpreter);

            Assert.False(changed);
            Assert.Equal(42, fInt32Int32(ref changed));
            Assert.True(changed);
            changed = false;
            Assert.Equal(42, fInt32Int32(ref changed));
            Assert.True(changed);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NoDefaultOrCasesSwitchWithSideEffect(bool useInterpreter)
        {
            bool changed = false;
            var pOut = Expression.Parameter(typeof(bool).MakeByRefType(), "changed");
            var switchValue = Expression.Call(typeof(Compiler_Tests).GetMethod("QuestionMeaning"), pOut);
            var s = Expression.Switch(switchValue, (Expression)null);

            var f = Expression.Lambda<JustRefSettingDelegate>(s, pOut).Compile(useInterpreter);

            Assert.False(changed);
            f(ref changed);
            Assert.True(changed);
        }

        static class System_Linq_Expressions_Expression_TDelegate__1
        {
            public static T Default<T>() { return default(T); }
            public static void UseSystem_Linq_Expressions_Expression_TDelegate__1(bool call) // call this passing false
            {
                if (call)
                {
                    Default<System.Linq.Expressions.Expression<System.Object>>().Compile();
                    Default<System.Linq.Expressions.Expression<System.Object>>().Update(
                Default<System.Linq.Expressions.Expression>(),
                Default<System.Collections.Generic.IEnumerable<System.Linq.Expressions.ParameterExpression>>());
                }
            }
        }

        [Fact]
        public static void ExprT_Update()
        {
            System_Linq_Expressions_Expression_TDelegate__1.UseSystem_Linq_Expressions_Expression_TDelegate__1(false);
        }

        public class TestComparers
        {
            public static bool CaseInsensitiveStringCompare(string s1, string s2)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(s1, s2);
            }
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void SwitchWithComparison(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(string));
            var p1 = Expression.Parameter(typeof(string));
            var s = Expression.Switch(p,
                Expression.Assign(p1, Expression.Constant("default")),
                typeof(TestComparers).GetMethod("CaseInsensitiveStringCompare"),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("hello")), Expression.Constant("hi")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("null")), Expression.Constant(null, typeof(string))),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lala")), Expression.Constant("bye")),
                Expression.SwitchCase(Expression.Assign(p1, Expression.Constant("lalala")), Expression.Constant("hi")));

            var block = Expression.Block(new ParameterExpression[] { p1 }, s, p1);

            Func<string, string> f = Expression.Lambda<Func<string, string>>(block, p).Compile(useInterpreter);

            Assert.Equal("hello", f("hi"));
            Assert.Equal("lala", f("bYe"));
            Assert.Equal("default", f("hi2"));
            Assert.Equal("hello", f("HI"));
            Assert.Equal("null", f(null));
        }

        public enum MyEnum
        {
            Value
        }

        public class EnumOutLambdaClass
        {
            public static void Bar(out MyEnum o)
            {
                o = MyEnum.Value;
            }

            public static void BarRef(ref MyEnum o)
            {
                o = MyEnum.Value;
            }
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void UninitializedEnumOut(bool useInterpreter)
        {
            var x = Expression.Variable(typeof(MyEnum), "x");

            var expression = Expression.Lambda<Action>(
                            Expression.Block(
                            new[] { x },
                            Expression.Call(null, typeof(EnumOutLambdaClass).GetMethod("Bar"), x)));

            expression.Compile(useInterpreter)();
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void DefaultEnumRef(bool useInterpreter)
        {
            var x = Expression.Variable(typeof(MyEnum), "x");

            var expression = Expression.Lambda<Action>(
                            Expression.Block(
                            new[] { x },
                            Expression.Assign(x, Expression.Default(typeof(MyEnum))),
                            Expression.Call(null, typeof(EnumOutLambdaClass).GetMethod("BarRef"), x)));

            expression.Compile(useInterpreter)();
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void BinaryOperators(bool useInterpreter)
        {
            // AndAlso
            Assert.True(TestBinary<bool, bool>(ExpressionType.AndAlso, true, true, useInterpreter));
            Assert.False(TestBinary<bool, bool>(ExpressionType.AndAlso, false, true, useInterpreter));
            Assert.False(TestBinary<bool, bool>(ExpressionType.AndAlso, true, false, useInterpreter));
            Assert.False(TestBinary<bool, bool>(ExpressionType.AndAlso, false, false, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.AndAlso, true, true, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.AndAlso, true, false, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.AndAlso, false, true, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.AndAlso, false, false, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.AndAlso, true, null, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.AndAlso, null, true, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.AndAlso, false, null, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.AndAlso, null, false, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.AndAlso, null, null, useInterpreter));

            // OrElse
            Assert.True(TestBinary<bool, bool>(ExpressionType.OrElse, true, true, useInterpreter));
            Assert.True(TestBinary<bool, bool>(ExpressionType.OrElse, false, true, useInterpreter));
            Assert.True(TestBinary<bool, bool>(ExpressionType.OrElse, true, false, useInterpreter));
            Assert.False(TestBinary<bool, bool>(ExpressionType.OrElse, false, false, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.OrElse, true, true, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.OrElse, true, false, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.OrElse, false, true, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.OrElse, false, false, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.OrElse, true, null, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.OrElse, null, true, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.OrElse, false, null, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.OrElse, null, false, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.OrElse, null, null, useInterpreter));

            // And
            Assert.True(TestBinary<bool, bool>(ExpressionType.And, true, true, useInterpreter));
            Assert.False(TestBinary<bool, bool>(ExpressionType.And, false, true, useInterpreter));
            Assert.False(TestBinary<bool, bool>(ExpressionType.And, true, false, useInterpreter));
            Assert.False(TestBinary<bool, bool>(ExpressionType.And, false, false, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.And, true, true, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.And, true, false, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.And, false, true, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.And, false, false, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.And, true, null, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.And, null, true, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.And, false, null, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.And, null, false, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.And, null, null, useInterpreter));
            Assert.Equal(2, TestBinary<int, int>(ExpressionType.And, 2, 3, useInterpreter));
            Assert.Equal(2, TestBinary<int?, int?>(ExpressionType.And, 2, 3, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.And, null, 3, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.And, 2, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.And, null, null, useInterpreter));

            // Or
            Assert.True(TestBinary<bool, bool>(ExpressionType.Or, true, true, useInterpreter));
            Assert.True(TestBinary<bool, bool>(ExpressionType.Or, false, true, useInterpreter));
            Assert.True(TestBinary<bool, bool>(ExpressionType.Or, true, false, useInterpreter));
            Assert.False(TestBinary<bool, bool>(ExpressionType.Or, false, false, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.Or, true, true, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.Or, true, false, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.Or, false, true, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.Or, false, false, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.Or, true, null, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.Or, null, true, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.Or, false, null, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.Or, null, false, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.Or, null, null, useInterpreter));
            Assert.Equal(3, TestBinary<int, int>(ExpressionType.Or, 2, 1, useInterpreter));
            Assert.Equal(3, TestBinary<int?, int?>(ExpressionType.Or, 2, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Or, null, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Or, 2, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Or, null, 1, useInterpreter));

            // ExclusiveOr
            Assert.False(TestBinary<bool, bool>(ExpressionType.ExclusiveOr, true, true, useInterpreter));
            Assert.True(TestBinary<bool, bool>(ExpressionType.ExclusiveOr, true, false, useInterpreter));
            Assert.True(TestBinary<bool, bool>(ExpressionType.ExclusiveOr, false, true, useInterpreter));
            Assert.False(TestBinary<bool, bool>(ExpressionType.ExclusiveOr, false, false, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.ExclusiveOr, true, true, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.ExclusiveOr, true, false, useInterpreter));
            Assert.True((bool)TestBinary<bool?, bool?>(ExpressionType.ExclusiveOr, false, true, useInterpreter));
            Assert.False((bool)TestBinary<bool?, bool?>(ExpressionType.ExclusiveOr, false, false, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.ExclusiveOr, true, null, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.ExclusiveOr, null, true, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.ExclusiveOr, false, null, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.ExclusiveOr, null, false, useInterpreter));
            Assert.Null(TestBinary<bool?, bool?>(ExpressionType.ExclusiveOr, null, null, useInterpreter));
            Assert.Equal(4, TestBinary<int, int>(ExpressionType.ExclusiveOr, 5, 1, useInterpreter));
            Assert.Equal(4, TestBinary<int?, int?>(ExpressionType.ExclusiveOr, 5, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.ExclusiveOr, null, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.ExclusiveOr, 5, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.ExclusiveOr, null, null, useInterpreter));

            // Equal
            Assert.False(TestBinary<int, bool>(ExpressionType.Equal, 1, 2, useInterpreter));
            Assert.True(TestBinary<int, bool>(ExpressionType.Equal, 1, 1, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.Equal, 1, 2, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.Equal, 1, 1, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.Equal, null, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.Equal, 1, null, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.Equal, null, null, useInterpreter));

            Assert.False(TestBinary<decimal, bool>(ExpressionType.Equal, 1, 2, useInterpreter));
            Assert.True(TestBinary<decimal, bool>(ExpressionType.Equal, 1, 1, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.Equal, 1, 2, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.Equal, 1, 1, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.Equal, null, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.Equal, 1, null, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.Equal, null, null, useInterpreter));

            // NotEqual
            Assert.True(TestBinary<int, bool>(ExpressionType.NotEqual, 1, 2, useInterpreter));
            Assert.False(TestBinary<int, bool>(ExpressionType.NotEqual, 1, 1, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.NotEqual, 1, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.NotEqual, 1, 1, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.NotEqual, null, 2, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.NotEqual, 1, null, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.NotEqual, null, null, useInterpreter));

            Assert.True(TestBinary<decimal, bool>(ExpressionType.NotEqual, 1, 2, useInterpreter));
            Assert.False(TestBinary<decimal, bool>(ExpressionType.NotEqual, 1, 1, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.NotEqual, 1, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.NotEqual, 1, 1, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.NotEqual, null, 2, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.NotEqual, 1, null, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.NotEqual, null, null, useInterpreter));

            // LessThan
            Assert.True(TestBinary<int, bool>(ExpressionType.LessThan, 1, 2, useInterpreter));
            Assert.False(TestBinary<int, bool>(ExpressionType.LessThan, 2, 1, useInterpreter));
            Assert.False(TestBinary<int, bool>(ExpressionType.LessThan, 2, 2, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.LessThan, 1, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.LessThan, 2, 1, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.LessThan, 2, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.LessThan, null, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.LessThan, 2, null, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.LessThan, null, null, useInterpreter));

            Assert.True(TestBinary<decimal, bool>(ExpressionType.LessThan, 1, 2, useInterpreter));
            Assert.False(TestBinary<decimal, bool>(ExpressionType.LessThan, 2, 1, useInterpreter));
            Assert.False(TestBinary<decimal, bool>(ExpressionType.LessThan, 2, 2, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.LessThan, 1, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.LessThan, 2, 1, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.LessThan, 2, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.LessThan, null, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.LessThan, 2, null, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.LessThan, null, null, useInterpreter));

            // LessThanOrEqual
            Assert.True(TestBinary<int, bool>(ExpressionType.LessThanOrEqual, 1, 2, useInterpreter));
            Assert.False(TestBinary<int, bool>(ExpressionType.LessThanOrEqual, 2, 1, useInterpreter));
            Assert.True(TestBinary<int, bool>(ExpressionType.LessThanOrEqual, 2, 2, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.LessThanOrEqual, 1, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.LessThanOrEqual, 2, 1, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.LessThanOrEqual, 2, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.LessThanOrEqual, null, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.LessThanOrEqual, 2, null, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.LessThanOrEqual, null, null, useInterpreter));

            Assert.True(TestBinary<decimal, bool>(ExpressionType.LessThanOrEqual, 1, 2, useInterpreter));
            Assert.False(TestBinary<decimal, bool>(ExpressionType.LessThanOrEqual, 2, 1, useInterpreter));
            Assert.True(TestBinary<decimal, bool>(ExpressionType.LessThanOrEqual, 2, 2, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.LessThanOrEqual, 1, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.LessThanOrEqual, 2, 1, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.LessThanOrEqual, 2, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.LessThanOrEqual, null, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.LessThanOrEqual, 2, null, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.LessThanOrEqual, null, null, useInterpreter));

            // GreaterThan
            Assert.False(TestBinary<int, bool>(ExpressionType.GreaterThan, 1, 2, useInterpreter));
            Assert.True(TestBinary<int, bool>(ExpressionType.GreaterThan, 2, 1, useInterpreter));
            Assert.False(TestBinary<int, bool>(ExpressionType.GreaterThan, 2, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.GreaterThan, 1, 2, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.GreaterThan, 2, 1, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.GreaterThan, 2, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.GreaterThan, null, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.GreaterThan, 2, null, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.GreaterThan, null, null, useInterpreter));

            Assert.False(TestBinary<decimal, bool>(ExpressionType.GreaterThan, 1, 2, useInterpreter));
            Assert.True(TestBinary<decimal, bool>(ExpressionType.GreaterThan, 2, 1, useInterpreter));
            Assert.False(TestBinary<decimal, bool>(ExpressionType.GreaterThan, 2, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.GreaterThan, 1, 2, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.GreaterThan, 2, 1, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.GreaterThan, 2, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.GreaterThan, null, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.GreaterThan, 2, null, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.GreaterThan, null, null, useInterpreter));

            // GreaterThanOrEqual
            Assert.False(TestBinary<int, bool>(ExpressionType.GreaterThanOrEqual, 1, 2, useInterpreter));
            Assert.True(TestBinary<int, bool>(ExpressionType.GreaterThanOrEqual, 2, 1, useInterpreter));
            Assert.True(TestBinary<int, bool>(ExpressionType.GreaterThanOrEqual, 2, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.GreaterThanOrEqual, 1, 2, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.GreaterThanOrEqual, 2, 1, useInterpreter));
            Assert.True(TestBinary<int?, bool>(ExpressionType.GreaterThanOrEqual, 2, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.GreaterThanOrEqual, null, 2, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.GreaterThanOrEqual, 2, null, useInterpreter));
            Assert.False(TestBinary<int?, bool>(ExpressionType.GreaterThanOrEqual, null, null, useInterpreter));

            Assert.False(TestBinary<decimal, bool>(ExpressionType.GreaterThanOrEqual, 1, 2, useInterpreter));
            Assert.True(TestBinary<decimal, bool>(ExpressionType.GreaterThanOrEqual, 2, 1, useInterpreter));
            Assert.True(TestBinary<decimal, bool>(ExpressionType.GreaterThanOrEqual, 2, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.GreaterThanOrEqual, 1, 2, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.GreaterThanOrEqual, 2, 1, useInterpreter));
            Assert.True(TestBinary<decimal?, bool>(ExpressionType.GreaterThanOrEqual, 2, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.GreaterThanOrEqual, null, 2, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.GreaterThanOrEqual, 2, null, useInterpreter));
            Assert.False(TestBinary<decimal?, bool>(ExpressionType.GreaterThanOrEqual, null, null, useInterpreter));

            // Add
            Assert.Equal(3, TestBinary<int, int>(ExpressionType.Add, 1, 2, useInterpreter));
            Assert.Equal(3, TestBinary<int?, int?>(ExpressionType.Add, 1, 2, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Add, null, 2, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Add, 1, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Add, null, null, useInterpreter));

            Assert.Equal(3, TestBinary<decimal, decimal>(ExpressionType.Add, 1, 2, useInterpreter));
            Assert.Equal(3, TestBinary<decimal?, decimal?>(ExpressionType.Add, 1, 2, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Add, null, 2, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Add, 1, null, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Add, null, null, useInterpreter));

            // AddChecked
            Assert.Equal(3, TestBinary<int, int>(ExpressionType.AddChecked, 1, 2, useInterpreter));
            Assert.Equal(3, TestBinary<int?, int?>(ExpressionType.AddChecked, 1, 2, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.AddChecked, null, 2, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.AddChecked, 1, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.AddChecked, null, null, useInterpreter));

            // Subtract
            Assert.Equal(1, TestBinary<int, int>(ExpressionType.Subtract, 2, 1, useInterpreter));
            Assert.Equal(1, TestBinary<int?, int?>(ExpressionType.Subtract, 2, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Subtract, null, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Subtract, 2, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Subtract, null, null, useInterpreter));

            Assert.Equal(1, TestBinary<decimal, decimal>(ExpressionType.Subtract, 2, 1, useInterpreter));
            Assert.Equal(1, TestBinary<decimal?, decimal?>(ExpressionType.Subtract, 2, 1, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Subtract, null, 1, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Subtract, 2, null, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Subtract, null, null, useInterpreter));

            // SubtractChecked
            Assert.Equal(1, TestBinary<int, int>(ExpressionType.SubtractChecked, 2, 1, useInterpreter));
            Assert.Equal(1, TestBinary<int?, int?>(ExpressionType.SubtractChecked, 2, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.SubtractChecked, null, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.SubtractChecked, 2, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.SubtractChecked, null, null, useInterpreter));

            // Multiply
            Assert.Equal(2, TestBinary<int, int>(ExpressionType.Multiply, 2, 1, useInterpreter));
            Assert.Equal(2, TestBinary<int?, int?>(ExpressionType.Multiply, 2, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Multiply, null, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Multiply, 2, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Multiply, null, null, useInterpreter));

            Assert.Equal(2, TestBinary<decimal, decimal>(ExpressionType.Multiply, 2, 1, useInterpreter));
            Assert.Equal(2, TestBinary<decimal?, decimal?>(ExpressionType.Multiply, 2, 1, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Multiply, null, 1, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Multiply, 2, null, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Multiply, null, null, useInterpreter));

            // MultiplyChecked
            Assert.Equal(2, TestBinary<int, int>(ExpressionType.MultiplyChecked, 2, 1, useInterpreter));
            Assert.Equal(2, TestBinary<int?, int?>(ExpressionType.MultiplyChecked, 2, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.MultiplyChecked, null, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.MultiplyChecked, 2, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.MultiplyChecked, null, null, useInterpreter));

            // Divide
            Assert.Equal(2, TestBinary<int, int>(ExpressionType.Divide, 5, 2, useInterpreter));
            Assert.Equal(2, TestBinary<int?, int?>(ExpressionType.Divide, 5, 2, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Divide, null, 2, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Divide, 5, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Divide, null, null, useInterpreter));

            Assert.Equal(2.5m, TestBinary<decimal, decimal>(ExpressionType.Divide, 5, 2, useInterpreter));
            Assert.Equal(2.5m, TestBinary<decimal?, decimal?>(ExpressionType.Divide, 5, 2, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Divide, null, 2, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Divide, 5, null, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Divide, null, null, useInterpreter));

            // Modulo 
            Assert.Equal(3, TestBinary<int, int>(ExpressionType.Modulo, 7, 4, useInterpreter));
            Assert.Equal(3, TestBinary<int?, int?>(ExpressionType.Modulo, 7, 4, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Modulo, null, 4, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Modulo, 7, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.Modulo, null, null, useInterpreter));

            Assert.Equal(3, TestBinary<decimal, decimal>(ExpressionType.Modulo, 7, 4, useInterpreter));
            Assert.Equal(3, TestBinary<decimal?, decimal?>(ExpressionType.Modulo, 7, 4, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Modulo, null, 4, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Modulo, 7, null, useInterpreter));
            Assert.Null(TestBinary<decimal?, decimal?>(ExpressionType.Modulo, null, null, useInterpreter));

            // Power
            Assert.Equal(16, TestBinary<double, double>(ExpressionType.Power, 2, 4, useInterpreter));
            Assert.Equal(16, TestBinary<double?, double?>(ExpressionType.Power, 2, 4, useInterpreter));
            Assert.Null(TestBinary<double?, double?>(ExpressionType.Power, null, 4, useInterpreter));
            Assert.Null(TestBinary<double?, double?>(ExpressionType.Power, 2, null, useInterpreter));
            Assert.Null(TestBinary<double?, double?>(ExpressionType.Power, null, null, useInterpreter));

            // LeftShift
            Assert.Equal(10, TestBinary<int, int>(ExpressionType.LeftShift, 5, 1, useInterpreter));
            Assert.Equal(10, TestBinary<int?, int?>(ExpressionType.LeftShift, 5, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.LeftShift, null, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.LeftShift, 5, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.LeftShift, null, null, useInterpreter));

            // RightShift
            Assert.Equal(2, TestBinary<int, int>(ExpressionType.RightShift, 4, 1, useInterpreter));
            Assert.Equal(2, TestBinary<int?, int?>(ExpressionType.RightShift, 4, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.RightShift, null, 1, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.RightShift, 4, null, useInterpreter));
            Assert.Null(TestBinary<int?, int?>(ExpressionType.RightShift, null, null, useInterpreter));
        }

        private static R TestBinary<T, R>(Expression<Func<T, T, R>> f, T v1, T v2, bool useInterpreter)
        {
            Func<T, T, R> d = f.Compile(useInterpreter);
            R rv = d(v1, v2);
            return rv;
        }

        private static R TestBinary<T, R>(ExpressionType op, T v1, T v2, bool useInterpreter)
        {
            ParameterExpression p1 = Expression.Parameter(typeof(T), "v1");
            ParameterExpression p2 = Expression.Parameter(typeof(T), "v2");
            Expression<Func<T, T, R>> f = Expression.Lambda<Func<T, T, R>>(Expression.MakeBinary(op, p1, p2), p1, p2);
            return TestBinary(f, v1, v2, useInterpreter);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void TestConvertToNullable(bool useInterpreter)
        {
            Expression<Func<int, int?>> f = x => (int?)x;
            Assert.Equal(f.Body.NodeType, ExpressionType.Convert);
            Func<int, int?> d = f.Compile(useInterpreter);
            Assert.Equal(2, d(2));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void TestNullableMethods(bool useInterpreter)
        {
            TestNullableCall(new ArraySegment<int>(), (v) => v.HasValue, (v) => v.HasValue, useInterpreter);
            TestNullableCall(5.1, (v) => v.GetHashCode(), (v) => v.GetHashCode(), useInterpreter);
            TestNullableCall(5L, (v) => v.ToString(), (v) => v.ToString(), useInterpreter);
            TestNullableCall(5, (v) => v.GetValueOrDefault(7), (v) => v.GetValueOrDefault(7), useInterpreter);
            TestNullableCall(42, (v) => v.Equals(42), (v) => v.Equals(42), useInterpreter);
            TestNullableCall(42, (v) => v.Equals(0), (v) => v.Equals(0), useInterpreter);
            TestNullableCall(5, (v) => v.GetValueOrDefault(), (v) => v.GetValueOrDefault(), useInterpreter);

            Expression<Func<int?, int>> f = x => x.Value;
            Func<int?, int> d = f.Compile(useInterpreter);
            Assert.Equal(2, d(2));
            Assert.Throws<InvalidOperationException>(() => d(null));
        }

        private static void TestNullableCall<T, U>(T arg, Func<T?, U> f, Expression<Func<T?, U>> e, bool useInterpreter)
            where T : struct
        {
            Func<T?, U> d = e.Compile(useInterpreter);
            Assert.Equal(f(arg), d(arg));
            Assert.Equal(f(null), d(null));
        }

        public static int BadJuju(int v)
        {
            throw new Exception("Bad Juju");
        }

        public static int M1(int v, Func<int, int> f)
        {
            return f(v);
        }

        public static int M2(int v, Expression<Func<int, int>> f)
        {
            return v;
        }

        public class Foo
        {
            public Foo()
            {
            }
            public int Zip(int y)
            {
                return y * y;
            }
            public virtual string Virt()
            {
                return "Foo";
            }
        }

        public class Bar : Foo
        {
            public Bar()
            {
            }
            public override string Virt()
            {
                return "Bar";
            }
        }

        public class ClassX
        {
            public int A;
            public int B;
            public int C;
            public ClassY Y = new ClassY();
            public ClassY YP
            {
                get { return this.Y; }
            }
            public List<ClassY> Ys = new List<ClassY>();
            public List<StructY> SYs = new List<StructY>();
            public StructY SY;
            public StructY SYP
            {
                get { return this.SY; }
            }
        }

        public class ClassY
        {
            public int B;
            public int PB
            {
                get { return this.B; }
                set { this.B = value; }
            }
        }

        public class StructX
        {
            public int A;
            public int B;
            public int C;
            public ClassY Y = new ClassY();
            public ClassY YP
            {
                get { return this.Y; }
            }
            public List<ClassY> Ys = new List<ClassY>();
            public StructY SY;
            public StructY SYP
            {
                get { return this.SY; }
            }
        }

        public struct StructY
        {
            public int B;
            public int PB
            {
                get { return this.B; }
                set { this.B = value; }
            }
        }

        public struct StructZ
        {
            public int A;
            public StructZ(int a)
            {
                this.A = a;
            }
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void PropertyAccess(bool useInterpreter)
        {
            NWindProxy.Customer cust = new NWindProxy.Customer { CustomerID = "BUBBA", ContactName = "Bubba Gump" };
            ParameterExpression c = Expression.Parameter(typeof(NWindProxy.Customer), "c");
            ParameterExpression c2 = Expression.Parameter(typeof(NWindProxy.Customer), "c2");

            Assert.Equal(cust, Expression.Lambda(c, c).Compile(useInterpreter).DynamicInvoke(cust));
            Assert.Equal(cust.ContactName, Expression.Lambda(Expression.PropertyOrField(c, "ContactName"), c).Compile(useInterpreter).DynamicInvoke(cust));
            Assert.Equal(cust.Orders, Expression.Lambda(Expression.PropertyOrField(c, "Orders"), c).Compile(useInterpreter).DynamicInvoke(cust));
            Assert.Equal(cust.CustomerID, Expression.Lambda(Expression.PropertyOrField(c, "CustomerId"), c).Compile(useInterpreter).DynamicInvoke(cust));
            Assert.True((bool)Expression.Lambda(Expression.Equal(Expression.PropertyOrField(c, "CustomerId"), Expression.PropertyOrField(c, "CUSTOMERID")), c).Compile(useInterpreter).DynamicInvoke(cust));
            Assert.True((bool)
                Expression.Lambda(
                    Expression.And(
                        Expression.Equal(Expression.PropertyOrField(c, "CustomerId"), Expression.PropertyOrField(c2, "CustomerId")),
                        Expression.Equal(Expression.PropertyOrField(c, "ContactName"), Expression.PropertyOrField(c2, "ContactName"))
                        ),
                    c, c2)
                .Compile(useInterpreter).DynamicInvoke(cust, cust));
        }

        private static void ArimeticOperatorTests(Type type, object value, bool testUnSigned, bool useInterpreter)
        {
            ParameterExpression p = Expression.Parameter(type, "x");
            if (testUnSigned)
                Expression.Lambda(Expression.Negate(p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.Add(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.Subtract(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.Multiply(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.Divide(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
        }

        private static void RelationalOperatorTests(Type type, object value, bool testModulo, bool useInterpreter)
        {
            ParameterExpression p = Expression.Parameter(type, "x");
            if (testModulo)
                Expression.Lambda(Expression.Modulo(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.Equal(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.NotEqual(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.LessThan(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.LessThanOrEqual(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.GreaterThan(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.GreaterThanOrEqual(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
        }

        private static void NumericOperatorTests(Type type, object value, bool testModulo, bool testUnSigned, bool useInterpreter)
        {
            ArimeticOperatorTests(type, value, testUnSigned, useInterpreter);
            RelationalOperatorTests(type, value, testModulo, useInterpreter);
        }

        private static void NumericOperatorTests(Type type, object value, bool useInterpreter)
        {
            NumericOperatorTests(type, value, true, true, useInterpreter);
        }

        private static void IntegerOperatorTests(Type type, object value, bool testModulo, bool testUnsigned, bool useInterpreter)
        {
            NumericOperatorTests(type, value, testModulo, testUnsigned, useInterpreter);
            LogicalOperatorTests(type, value, useInterpreter);
        }

        private static void LogicalOperatorTests(Type type, object value, bool useInterpreter)
        {
            ParameterExpression p = Expression.Parameter(type, "x");
            Expression.Lambda(Expression.Not(p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.Or(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.And(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
            Expression.Lambda(Expression.ExclusiveOr(p, p), p).Compile(useInterpreter).DynamicInvoke(new object[] { value });
        }

        private static void IntegerOperatorTests(Type type, object value, bool useInterpreter)
        {
            IntegerOperatorTests(type, value, true, true, useInterpreter);
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void NumericOperators(bool useInterpreter)
        {
            RelationalOperatorTests(typeof(sbyte), (sbyte)1, false, useInterpreter);
            LogicalOperatorTests(typeof(sbyte), (sbyte)1, useInterpreter);
            RelationalOperatorTests(typeof(short), (short)1, false, useInterpreter);
            LogicalOperatorTests(typeof(sbyte), (sbyte)1, useInterpreter);
            IntegerOperatorTests(typeof(int), 1, useInterpreter);
            IntegerOperatorTests(typeof(long), (long)1, useInterpreter);
            RelationalOperatorTests(typeof(byte), (byte)1, false, useInterpreter);
            LogicalOperatorTests(typeof(byte), (byte)1, useInterpreter);
            RelationalOperatorTests(typeof(ushort), (ushort)1, false, useInterpreter);
            LogicalOperatorTests(typeof(ushort), (ushort)1, useInterpreter);
            IntegerOperatorTests(typeof(uint), (uint)1, true, false, useInterpreter);
            IntegerOperatorTests(typeof(ulong), (ulong)1, true, false, useInterpreter);
            NumericOperatorTests(typeof(float), (float)1, useInterpreter);
            NumericOperatorTests(typeof(double), (double)1, useInterpreter);
            NumericOperatorTests(typeof(decimal), (decimal)1, useInterpreter);

            RelationalOperatorTests(typeof(sbyte?), (sbyte?)1, false, useInterpreter);
            LogicalOperatorTests(typeof(sbyte?), (sbyte?)1, useInterpreter);
            RelationalOperatorTests(typeof(short?), (short?)1, false, useInterpreter);
            LogicalOperatorTests(typeof(short?), (short?)1, useInterpreter);
            IntegerOperatorTests(typeof(int?), (int?)1, useInterpreter);
            IntegerOperatorTests(typeof(long?), (long?)1, useInterpreter);
            RelationalOperatorTests(typeof(byte?), (byte?)1, false, useInterpreter);
            LogicalOperatorTests(typeof(byte?), (byte?)1, useInterpreter);
            RelationalOperatorTests(typeof(ushort?), (ushort?)1, false, useInterpreter);
            LogicalOperatorTests(typeof(ushort?), (ushort?)1, useInterpreter);
            IntegerOperatorTests(typeof(uint?), (uint?)1, true, false, useInterpreter);
            IntegerOperatorTests(typeof(ulong?), (ulong?)1, true, false, useInterpreter);

            NumericOperatorTests(typeof(float?), (float?)1, useInterpreter);
            NumericOperatorTests(typeof(double?), (double?)1, useInterpreter);
            NumericOperatorTests(typeof(decimal?), (decimal?)1, useInterpreter);
        }

        private static void TrueBooleanOperatorTests(Type type, object arg1, object arg2, bool useInterpreter)
        {
            ParameterExpression x = Expression.Parameter(type, "x");
            ParameterExpression y = Expression.Parameter(type, "y");
            Expression.Lambda(Expression.AndAlso(x, y), x, y).Compile(useInterpreter).DynamicInvoke(new object[] { arg1, arg2 });
            Expression.Lambda(Expression.OrElse(x, y), x, y).Compile(useInterpreter).DynamicInvoke(new object[] { arg1, arg2 });
            GeneralBooleanOperatorTests(type, arg1, arg2, useInterpreter);
        }

        private static void GeneralBooleanOperatorTests(Type type, object arg1, object arg2, bool useInterpreter)
        {
            ParameterExpression x = Expression.Parameter(type, "x");
            ParameterExpression y = Expression.Parameter(type, "y");
            Expression.Lambda(Expression.And(x, y), x, y).Compile(useInterpreter).DynamicInvoke(new object[] { arg1, arg2 });
            Expression.Lambda(Expression.Or(x, y), x, y).Compile(useInterpreter).DynamicInvoke(new object[] { arg1, arg2 });
            Expression.Lambda(Expression.Not(x), x).Compile(useInterpreter).DynamicInvoke(new object[] { arg1 });
            Expression.Lambda(Expression.Equal(x, y), x, y).Compile(useInterpreter).DynamicInvoke(new object[] { arg1, arg2 });
            Expression.Lambda(Expression.NotEqual(x, y), x, y).Compile(useInterpreter).DynamicInvoke(new object[] { arg1, arg2 });
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void BooleanOperators(bool useInterpreter)
        {
            TrueBooleanOperatorTests(typeof(bool), true, false, useInterpreter);
            TrueBooleanOperatorTests(typeof(bool?), true, false, useInterpreter);
        }
    }

    public struct U
    {
        public static U operator +(U x, U y) { throw new NotImplementedException(); }
        public static U operator -(U x, U y) { throw new NotImplementedException(); }
        public static U operator *(U x, U y) { throw new NotImplementedException(); }
        public static U operator /(U x, U y) { throw new NotImplementedException(); }
        public static U operator <(U x, U y) { throw new NotImplementedException(); }
        public static U operator <=(U x, U y) { throw new NotImplementedException(); }
        public static U operator >(U x, U y) { throw new NotImplementedException(); }
        public static U operator >=(U x, U y) { throw new NotImplementedException(); }
        public static U operator ==(U x, U y) { throw new NotImplementedException(); }
        public static U operator !=(U x, U y) { throw new NotImplementedException(); }
        public static U operator &(U x, U y) { throw new NotImplementedException(); }
        public static U operator |(U x, U y) { throw new NotImplementedException(); }
        public static U operator ^(U x, U y) { throw new NotImplementedException(); }
        public static U operator -(U x) { throw new NotImplementedException(); }
        public static U operator ~(U x) { throw new NotImplementedException(); }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public struct B
    {
        public static B operator <(B x, B y) { throw new NotImplementedException(); }
        public static B operator <=(B x, B y) { throw new NotImplementedException(); }
        public static B operator >(B x, B y) { throw new NotImplementedException(); }
        public static B operator >=(B x, B y) { throw new NotImplementedException(); }
        public static B operator ==(B x, B y) { throw new NotImplementedException(); }
        public static B operator !=(B x, B y) { throw new NotImplementedException(); }
        public static B operator &(B x, B y) { throw new NotImplementedException(); }
        public static B operator |(B x, B y) { throw new NotImplementedException(); }
        public static B operator ^(B x, B y) { throw new NotImplementedException(); }
        public static B operator !(B x) { throw new NotImplementedException(); }
        public static B operator -(B x) { throw new NotImplementedException(); }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public struct M
    {
        public static M operator +(M m, N n) { throw new NotImplementedException(); }
        public static M operator -(M m, N n) { throw new NotImplementedException(); }
        public static M operator -(M m) { throw new NotImplementedException(); }
        public static M operator ~(M m) { throw new NotImplementedException(); }
        public static explicit operator M(N n) { throw new NotImplementedException(); }
        public static N Foo(M m) { throw new NotImplementedException(); }
    }

    public struct N
    {
        public static M operator *(M m, N n) { throw new NotImplementedException(); }
        public static M operator /(M m, N n) { throw new NotImplementedException(); }
        public static N operator -(N n) { throw new NotImplementedException(); }
        public static implicit operator N(M m) { throw new NotImplementedException(); }
        public static M Bar(N n) { throw new NotImplementedException(); }
    }

    public class DerivedClass : BaseClass
    {
    }

    internal class AssertionException : Exception
    {
        public AssertionException(string msg)
            : base(msg)
        {
        }
    }
}
