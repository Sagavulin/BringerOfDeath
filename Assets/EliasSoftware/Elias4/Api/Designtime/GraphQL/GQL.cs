using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EliasSoftware.Elias4.Designtime
{
    // ----------------------------------------------------------------------------
    // Query("name", SelectionSet(...))                     = query name { ... }
    // Mutation("name", SelectionSet(...))                  = mutation name { ... }
    // ----------------------------------------------------------------------------
    // SelectionSet(Field("1"), Field("2"), ...)            = { 1, 2, ... }
    // Arguments(Arg("arg1", "1"), Arg("arg2", "2"), ...)   = (arg1:1, arg2:2, ...)
    // Field("name")                                        = name
    // Field("alias", Field("name"))                        = alias:name
    // Field("name", Arguments(Arg("arg","1")))             = name(arg: 1)
    // Field(Field("name"), SelectionSet(...))              = name { ... }
    // ----------------------------------------------------------------------------
    // Fragments (https://spec.graphql.org/October2021/#sec-Language.Fragments)
    // Value (https://spec.graphql.org/October2021/#sec-Input-Values)
    // Var (https://spec.graphql.org/October2021/#sec-Language.Variables)
    // ----------------------------------------------------------------------------

    public interface ITerm {
        string Bake();
    }
    public interface IArg: ITerm {}
    public interface IArguments: ITerm {}
    public interface ISelectionSet: ITerm {}
    public interface IField: ITerm {}
    public interface IQuery: ITerm {}
    public interface IMutation: ITerm {}

    public static class GQL {

        public struct LanguageObject:
	        IArg, IArguments, ISelectionSet,
            IField, IQuery, IMutation
        {
            private global::System.Func<string> bake;
			public string Bake()
                => bake.Invoke();
            public LanguageObject(global::System.Func<string> bakeFn)
                => bake = bakeFn;
		}

        // Term creators
        public static ITerm Term(string content)
            => new LanguageObject(() => content);
        public static IArg Arg(string argName, string argValue)
            => new LanguageObject(() => argName + ": " + argValue);
        public static IArguments Args(params IArg[] args) {
            return new LanguageObject(() => {
                var content = "( ";
				foreach(var arg in args)
                    content += arg.Bake() + ", ";
                content = content.Remove(content.Length - 2);
                content += " )";
                return content;
            });
        }
        public static IField Field(string name)
            => new LanguageObject( () => name );
        public static IField Field(string alias, IField field)
            => new LanguageObject( () => alias + ": " + field.Bake() );
        public static IField Field(string name, IArguments args)
            => new LanguageObject( () => name + args.Bake() );
        public static IField Field(string name, IArguments args, ISelectionSet set)
            => new LanguageObject( () => name + args.Bake() + " " + set.Bake() );
        public static IField Field(string name, ISelectionSet set)
            => new LanguageObject( () => name + " " + set.Bake() );
        public static ISelectionSet Selection(params IField[] fields) {
            return new LanguageObject(() => {
                var content = "{ ";
				foreach(var field in fields)
                    content += field.Bake() + ", ";
                content = content.Remove(content.Length - 2);
                content += " }";
                return content;
            });
        }
        public static IQuery Query(string queryName, ISelectionSet set)
            => new LanguageObject( () => "query " + queryName + " " + set.Bake() );
        public static IQuery Query(ISelectionSet set)
            => new LanguageObject( () => "query " + set.Bake() );
        public static IMutation Mutation(ISelectionSet set)
            => new LanguageObject( () => "mutation " + set.Bake() );
    }

    public delegate ISelectionSet SelectAn(params string[] argValues);
    public delegate ISelectionSet SelectA1(string argValue);
    public delegate ISelectionSet SelectA2(string argValue1, string argValue2);
    public delegate ISelectionSet SelectA3(string argValue1, string argValue2, string argValue3);

    public delegate ISelectionSet SelectA1WithContinuation(
        string argValue,
        ISelectionSet innerContinuation);

    public delegate ISelectionSet SelectA2WithContinuation(
        string argValue1,
        string argValue2,
        ISelectionSet innerContinuation);

    public delegate ISelectionSet SelectA3WithContinuation(
        string argValue1,
        string argValue2,
        string argValue3,
        ISelectionSet innerContinuation);

    public static class GQLTool {
        public static string[] Path(params string[] path) => path;
        public static ISelectionSet SelectPathWithContinuation(string[] pathWithRootRHS, IField innerField) {
            var innerSelect = GQL.Selection(innerField);
            for(int i = pathWithRootRHS.Length-1; i >= 0; i--) {
                innerSelect = GQL.Selection(
                                    GQL.Field(pathWithRootRHS[i],
                                        innerSelect));
            }
            return innerSelect;
        }
        public static ISelectionSet SelectPath(params string[] pathWithRootRHS) {
            int len = pathWithRootRHS.Length;
            var innerSelect = GQL.Selection(
                    GQL.Field(pathWithRootRHS[len-1])
                );
            for(int i = len-2; i >= 0; i--) {
                innerSelect = GQL.Selection(
                                    GQL.Field(pathWithRootRHS[i],
                                        innerSelect));
            }
            return innerSelect;
        }
        public static SelectAn BindSelect(string resultAlias,
            string fieldName,
            params string[] argNames)
        {
            return (argValues) => {
                int numNames = argNames.Length;
                int numValues = argValues.Length;

                Debug.AssertFormat(numNames == numValues,
                    "Mismatching number of values and arguments for {0} (names={1}, values={2})",
                    fieldName, numNames, numValues);

                int nParameters = Mathf.Min(numNames, numValues);
                var combined = new List<IArg>();
                for(int i = 0; i < nParameters; i++)
                    combined.Add(GQL.Arg(argNames[i], argValues[i]));

                return GQL.Selection(
                    GQL.Field(resultAlias,
                        GQL.Field(fieldName,
                            GQL.Args(combined.ToArray()))));
            };
        }

        public static SelectA1 BindSelect(string resultAlias,
            string fieldName,
            string argName)
        {
            return (argValue) => {
                return GQL.Selection(
                    GQL.Field(resultAlias,
                        GQL.Field(fieldName,
                            GQL.Args(GQL.Arg(argName, argValue)))));
            };
        }

        public static SelectA2 BindSelect(string resultAlias,
            string fieldName,
            string argName1,
            string argName2)
        {
            return (argValue1, argValue2) => {
                return GQL.Selection(
                    GQL.Field(resultAlias,
                        GQL.Field(fieldName,
                            GQL.Args(GQL.Arg(argName1, argValue1),
                                     GQL.Arg(argName2, argValue2)))));
            };
        }

        public static SelectA3 BindSelect(string resultAlias,
            string fieldName,
            string argName1,
            string argName2,
            string argName3)
        {
            return (argValue1, argValue2, argValue3) => {
                return GQL.Selection(
                    GQL.Field(resultAlias,
                        GQL.Field(fieldName,
                            GQL.Args(GQL.Arg(argName1, argValue1),
                                     GQL.Arg(argName2, argValue2),
                                     GQL.Arg(argName3, argValue3)))));
            };
        }

        public static SelectA1WithContinuation BindSelectWithContinuation(string resultAlias,
            string fieldName,
            string argName)
        {
            return (argValue, continuation) => {
                return GQL.Selection(
                    GQL.Field(resultAlias,
                        GQL.Field(fieldName,
                            GQL.Args(GQL.Arg(argName, argValue)),
                                continuation)));
            };
        }
        public static SelectA2WithContinuation BindSelectWithContinuation(string resultAlias,
            string fieldName,
            string argName1,
            string argName2)
        {
            return (argValue1, argValue2, continuation) => {
                return GQL.Selection(
                    GQL.Field(resultAlias,
                        GQL.Field(fieldName,
                            GQL.Args(GQL.Arg(argName1, argValue1),
                                     GQL.Arg(argName2, argValue2)),
                                continuation)));
            };
        }

        public static SelectA3WithContinuation BindSelectWithContinuation(string resultAlias,
            string fieldName,
            string argName1,
            string argName2,
            string argName3)
        {
            return (argValue1, argValue2, argValue3, continuation) => {
                return GQL.Selection(
                    GQL.Field(resultAlias,
                        GQL.Field(fieldName,
                            GQL.Args(GQL.Arg(argName1, argValue1),
                                     GQL.Arg(argName2, argValue2),
                                     GQL.Arg(argName3, argValue3)),
                                continuation)));
            };
        }

    }
}
