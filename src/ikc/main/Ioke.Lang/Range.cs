namespace Ioke.Lang {
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    using Ioke.Lang.Util;

    public class Range : IokeData {
        IokeObject from;
        IokeObject to;
        bool inclusive;
        bool inverted = false;

        public Range(IokeObject from, IokeObject to, bool inclusive, bool inverted) {
            this.from = from;
            this.to = to;
            this.inclusive = inclusive;
            this.inverted = inverted;
        }

        public static IokeObject GetFrom(object range) {
            return ((Range)IokeObject.dataOf(range)).From;
        }

        public static IokeObject GetTo(object range) {
            return ((Range)IokeObject.dataOf(range)).To;
        }

        public static bool IsInclusive(object range) {
            return ((Range)IokeObject.dataOf(range)).IsInclusive();
        }

        public IokeObject From {
            get { return from; }
        }

        public IokeObject To {
            get { return to; }
        }

        public bool IsInclusive() {
            return inclusive;
        }

        public class RangeIterator {
            private IokeObject start;
            private IokeObject end;
            private readonly bool inclusive;
            private IokeObject context;
            //            private IokeObject message;
            private IokeObject messageToSend;
            private readonly Runtime runtime;
            private bool oneIteration = false;
            private bool doLast = true;

            public RangeIterator(IokeObject start, IokeObject end, bool inclusive, bool inverted, IokeObject context, IokeObject message) {
                this.runtime = context.runtime;
                this.start = start;
                this.end = end;
                this.inclusive = inclusive;
                this.context = context;
                //                this.message = message;

                messageToSend = runtime.succMessage;
                if(inverted) {
                    messageToSend = runtime.predMessage;
                }
            }

            public bool hasNext() {
                bool sameEndpoints = IokeObject.IsObjectTrue(Interpreter.Send(runtime.eqMessage, context, start, end));
                bool shouldGoOver = (doLast && inclusive);
                bool sameStartPoint = sameEndpoints && inclusive && !oneIteration;
                return !sameEndpoints || shouldGoOver || sameStartPoint;
            }

            public object next() {
                IokeObject obj = start;
                if(!IokeObject.IsObjectTrue(Interpreter.Send(runtime.eqMessage, context, start, end))) {
                    oneIteration = true;
                    start = (IokeObject)Interpreter.Send(messageToSend, context, start);
                    doLast = true;
                    return obj;
                } else {
                    if(inclusive && doLast) {
                        doLast = false;
                        return obj;
                    }
                }
                return null;
            }
        }

        public override void Init(IokeObject obj) {
            Runtime runtime = obj.runtime;
            obj.Kind = "Range";
            obj.Mimics(IokeObject.As(IokeObject.FindCell(runtime.Mixins, "Sequenced"), null), runtime.nul, runtime.nul);

            obj.RegisterMethod(runtime.NewNativeMethod("returns true if the left hand side range is equal to the right hand side range.",
                                                       new TypeCheckingNativeMethod("==", TypeCheckingArgumentsDefinition.builder()
                                                                                    .ReceiverMustMimic(runtime.Range)
                                                                                    .WithRequiredPositional("other")
                                                                                    .Arguments,
                                                                                    (method, on, args, keywords, context, message) => {
                                                                                        Range d = (Range)IokeObject.dataOf(on);
                                                                                        object other = args[0];

                                                                                        return ((other is IokeObject) &&
                                                                                                (IokeObject.dataOf(other) is Range)
                                                                                                && d.inclusive == ((Range)IokeObject.dataOf(other)).inclusive
                                                                                                && d.from.Equals(((Range)IokeObject.dataOf(other)).from)
                                                                                                && d.to.Equals(((Range)IokeObject.dataOf(other)).to)) ? context.runtime.True : context.runtime.False;
                                                                                    })));

            obj.RegisterMethod(runtime.NewNativeMethod("will return a new inclusive Range based on the two arguments",
                                                       new NativeMethod("inclusive", DefaultArgumentsDefinition.builder()
                                                                        .WithRequiredPositional("from")
                                                                        .WithRequiredPositional("to")
                                                                        .Arguments,
                                                                        (method, context, message, on, outer) => {
                                                                            var args = new SaneArrayList();
                                                                            outer.ArgumentsDefinition.GetEvaluatedArguments(context, message, on, args, new SaneDictionary<string, object>());

                                                                            object from = args[0];
                                                                            object to = args[1];

                                                                            bool comparing = IokeObject.IsMimic(from, IokeObject.As(context.runtime.Mixins.body.Get("Comparing"), context), context);
                                                                            bool inverted = false;

                                                                            if(comparing) {
                                                                                object result = Interpreter.Send(context.runtime.spaceShipMessage, context, from, to);
                                                                                if(result != context.runtime.nil && Number.ExtractInt(result, message, context) == 1) {
                                                                                    inverted = true;
                                                                                }
                                                                            }

                                                                            return runtime.NewRange(IokeObject.As(from, context), IokeObject.As(to, context), true, inverted);
                                                                        })));

            obj.RegisterMethod(runtime.NewNativeMethod("will return a new exclusive Range based on the two arguments",
                                                       new NativeMethod("exclusive", DefaultArgumentsDefinition.builder()
                                                                        .WithRequiredPositional("from")
                                                                        .WithRequiredPositional("to")
                                                                        .Arguments,
                                                                        (method, context, message, on, outer) => {
                                                                            var args = new SaneArrayList();
                                                                            outer.ArgumentsDefinition.GetEvaluatedArguments(context, message, on, args, new SaneDictionary<string, object>());

                                                                            object from = args[0];
                                                                            object to = args[1];

                                                                            bool comparing = IokeObject.IsMimic(from, IokeObject.As(context.runtime.Mixins.body.Get("Comparing"), context), context);
                                                                            bool inverted = false;

                                                                            if(comparing) {
                                                                                object result = Interpreter.Send(context.runtime.spaceShipMessage, context, from, to);
                                                                                if(result != context.runtime.nil && Number.ExtractInt(result, message, context) == 1) {
                                                                                    inverted = true;
                                                                                }
                                                                            }

                                                                            return runtime.NewRange(IokeObject.As(from, context), IokeObject.As(to, context), false, inverted);
                                                                        })));

            obj.RegisterMethod(runtime.NewNativeMethod("returns true if the receiver is an exclusive range, false otherwise",
                                                       new NativeMethod.WithNoArguments("exclusive?",
                                                                                        (method, context, message, on, outer) => {
                                                                                            outer.ArgumentsDefinition.GetEvaluatedArguments(context, message, on, new SaneArrayList(), new SaneDictionary<string, object>());
                                                                                            return ((Range)IokeObject.dataOf(on)).inclusive ? context.runtime.False : context.runtime.True;
                                                                                        })));

            obj.RegisterMethod(runtime.NewNativeMethod("returns true if the receiver is an inclusive range, false otherwise",
                                                       new NativeMethod.WithNoArguments("inclusive?",
                                                                                        (method, context, message, on, outer) => {
                                                                                            outer.ArgumentsDefinition.GetEvaluatedArguments(context, message, on, new SaneArrayList(), new SaneDictionary<string, object>());
                                                                                            return ((Range)IokeObject.dataOf(on)).inclusive ? context.runtime.True : context.runtime.False;
                                                                                        })));

            obj.RegisterMethod(runtime.NewNativeMethod("returns the 'from' part of the range",
                                                       new TypeCheckingNativeMethod.WithNoArguments("from", obj,
                                                                                                    (method, on, args, keywords, context, message) => {
                                                                                                        return ((Range)IokeObject.dataOf(on)).from;
                                                                                                    })));

            obj.RegisterMethod(runtime.NewNativeMethod("returns the 'to' part of the range",
                                                       new NativeMethod.WithNoArguments("to",
                                                                                        (method, context, message, on, outer) => {
                                                                                            outer.ArgumentsDefinition.GetEvaluatedArguments(context, message, on, new SaneArrayList(), new SaneDictionary<string, object>());
                                                                                            return ((Range)IokeObject.dataOf(on)).to;
                                                                                        })));

            obj.RegisterMethod(runtime.NewNativeMethod("returns a new sequence to iterate over this range",
                                                       new TypeCheckingNativeMethod.WithNoArguments("seq", obj,
                                                                                                    (method, on, args, keywords, context, message) => {
                                                                                                        IokeObject ob = method.runtime.Iterator2Sequence.AllocateCopy(null, null);
                                                                                                        ob.MimicsWithoutCheck(method.runtime.Iterator2Sequence);
                                                                                                        Range r = ((Range)IokeObject.dataOf(on));
                                                                                                        ob.Data = new Sequence.Iterator2Sequence(new RangeIterator(r.from, r.to, r.inclusive, r.inverted, context, message));
                                                                                                        return ob;
                                                                                                    })));

            obj.RegisterMethod(runtime.NewNativeMethod("takes either one or two or three arguments. if one argument is given, it should be a message chain that will be sent to each object in the range. the result will be thrown away. if two arguments are given, the first is an unevaluated name that will be set to each of the values in the range in succession, and then the second argument will be evaluated in a scope with that argument in it. if three arguments is given, the first one is an unevaluated name that will be set to the index of each element, and the other two arguments are the name of the argument for the value, and the actual code. the code will evaluate in a lexical context, and if the argument name is available outside the context, it will be shadowed. the method will return the range.",
                                                       new NativeMethod("each", DefaultArgumentsDefinition.builder()
                                                                        .WithOptionalPositionalUnevaluated("indexOrArgOrCode")
                                                                        .WithOptionalPositionalUnevaluated("argOrCode")
                                                                        .WithOptionalPositionalUnevaluated("code")
                                                                        .Arguments,
                                                                        (method, context, message, on, outer) => {
                                                                            outer.ArgumentsDefinition.CheckArgumentCount(context, message, on);
                                                                            IokeObject from = IokeObject.As(((Range)IokeObject.dataOf(on)).from, context);
                                                                            IokeObject to = IokeObject.As(((Range)IokeObject.dataOf(on)).to, context);
                                                                            bool inclusive = ((Range)IokeObject.dataOf(on)).inclusive;

                                                                            IokeObject messageToSend = context.runtime.succMessage;
                                                                            if(((Range)IokeObject.dataOf(on)).inverted) {
                                                                                messageToSend = context.runtime.predMessage;
                                                                            }

                                                                            switch(message.Arguments.Count) {
                                                                            case 0: {
                                                                                return Interpreter.Send(runtime.seqMessage, context, on);
                                                                            }
                                                                            case 1: {
                                                                                IokeObject code = IokeObject.As(message.Arguments[0], context);

                                                                                object current = from;

                                                                                while(!IokeObject.IsObjectTrue(Interpreter.Send(context.runtime.eqMessage, context, current, to))) {
                                                                                    context.runtime.interpreter.Evaluate(code, context, context.RealContext, current);
                                                                                    current = Interpreter.Send(messageToSend, context, current);
                                                                                }
                                                                                if(inclusive) {
                                                                                    context.runtime.interpreter.Evaluate(code, context, context.RealContext, current);
                                                                                }

                                                                                break;
                                                                            }
                                                                            case 2: {
                                                                                IokeObject c = context.runtime.NewLexicalContext(context, "Lexical activation context for Range#each", context);
                                                                                string name = IokeObject.As(message.Arguments[0], context).Name;
                                                                                IokeObject code = IokeObject.As(message.Arguments[1], context);

                                                                                object current = from;

                                                                                while(!IokeObject.IsObjectTrue(Interpreter.Send(context.runtime.eqMessage, context, current, to))) {
                                                                                    c.SetCell(name, current);
                                                                                    context.runtime.interpreter.Evaluate(code, c, c.RealContext, c);
                                                                                    current = Interpreter.Send(messageToSend, context, current);
                                                                                }
                                                                                if(inclusive) {
                                                                                    c.SetCell(name, current);
                                                                                    context.runtime.interpreter.Evaluate(code, c, c.RealContext, c);
                                                                                }

                                                                                break;
                                                                            }
                                                                            case 3: {
                                                                                IokeObject c = context.runtime.NewLexicalContext(context, "Lexical activation context for Range#each", context);
                                                                                string iname = IokeObject.As(message.Arguments[0], context).Name;
                                                                                string name = IokeObject.As(message.Arguments[1], context).Name;
                                                                                IokeObject code = IokeObject.As(message.Arguments[2], context);

                                                                                int index = 0;

                                                                                object current = from;

                                                                                while(!IokeObject.IsObjectTrue(Interpreter.Send(context.runtime.eqMessage, context, current, to))) {
                                                                                    c.SetCell(name, current);
                                                                                    c.SetCell(iname, runtime.NewNumber(index++));
                                                                                    context.runtime.interpreter.Evaluate(code, c, c.RealContext, c);
                                                                                    current = Interpreter.Send(messageToSend, context, current);
                                                                                }
                                                                                if(inclusive) {
                                                                                    c.SetCell(name, current);
                                                                                    c.SetCell(iname, runtime.NewNumber(index++));
                                                                                    context.runtime.interpreter.Evaluate(code, c, c.RealContext, c);
                                                                                }

                                                                                break;
                                                                            }
                                                                            }
                                                                            return on;
                                                                        })));

            obj.RegisterMethod(runtime.NewNativeMethod("returns true if the argument is within the confines of this range. how this comparison is done depends on if the object mimics Comparing. If it does, < and > will be used. If not, all the available entries in this range will be enumerated using 'succ'/'pred' until either the end or the element we're looking for is found. in that case, comparison is done with '=='",
                                                       new NativeMethod("===", DefaultArgumentsDefinition.builder()
                                                                        .WithRequiredPositional("other")
                                                                        .Arguments,
                                                                        (method, context, message, on, outer) => {
                                                                            var args = new SaneArrayList();
                                                                            outer.ArgumentsDefinition.GetEvaluatedArguments(context, message, on, args, new SaneDictionary<string, object>());
                                                                            object other = args[0];

                                                                            IokeObject from = IokeObject.As(((Range)IokeObject.dataOf(on)).from, context);
                                                                            IokeObject to = IokeObject.As(((Range)IokeObject.dataOf(on)).to, context);
                                                                            bool comparing = IokeObject.IsMimic(from, IokeObject.As(context.runtime.Mixins.body.Get("Comparing"), context));
                                                                            bool inclusive = ((Range)IokeObject.dataOf(on)).inclusive;

                                                                            if(comparing) {
                                                                                IokeObject firstMessage = context.runtime.lteMessage;
                                                                                IokeObject secondMessageInclusive = context.runtime.gteMessage;
                                                                                IokeObject secondMessageExclusive = context.runtime.gtMessage;

                                                                                if(((Range)IokeObject.dataOf(on)).inverted) {
                                                                                    firstMessage = context.runtime.gteMessage;
                                                                                    secondMessageInclusive = context.runtime.lteMessage;
                                                                                    secondMessageExclusive = context.runtime.ltMessage;
                                                                                }

                                                                                if(IokeObject.IsObjectTrue(Interpreter.Send(firstMessage, context, from, other)) &&
                                                                                   ((inclusive &&
                                                                                     IokeObject.IsObjectTrue(Interpreter.Send(secondMessageInclusive, context, to, other))) ||
                                                                                    IokeObject.IsObjectTrue(Interpreter.Send(secondMessageExclusive, context, to, other)))) {
                                                                                    return context.runtime.True;
                                                                                } else {
                                                                                    return context.runtime.False;
                                                                                }
                                                                            } else {
                                                                                IokeObject messageToSend = context.runtime.succMessage;
                                                                                if(((Range)IokeObject.dataOf(on)).inverted) {
                                                                                    messageToSend = context.runtime.predMessage;
                                                                                }

                                                                                object current = from;

                                                                                while(!IokeObject.IsObjectTrue(Interpreter.Send(context.runtime.eqMessage, context, current, to))) {
                                                                                    if(IokeObject.IsObjectTrue(Interpreter.Send(context.runtime.eqMessage, context, current, other))) {
                                                                                        return context.runtime.True;
                                                                                    }
                                                                                    current = Interpreter.Send(messageToSend, context, current);
                                                                                }

                                                                                if(inclusive && IokeObject.IsObjectTrue(Interpreter.Send(context.runtime.eqMessage, context, to, other))) {
                                                                                    return context.runtime.True;
                                                                                }
                                                                                return context.runtime.False;
                                                                            }
                                                                        })));

            obj.RegisterMethod(runtime.NewNativeMethod("Returns a text inspection of the object",
                                                       new NativeMethod.WithNoArguments("inspect",
                                                                                        (method, context, message, on, outer) => {
                                                                                            outer.ArgumentsDefinition.GetEvaluatedArguments(context, message, on, new SaneArrayList(), new SaneDictionary<string, object>());
                                                                                            return method.runtime.NewText(Range.GetInspect(on));
                                                                                        })));

            obj.RegisterMethod(runtime.NewNativeMethod("Returns a brief text inspection of the object",
                                                       new NativeMethod.WithNoArguments("notice",
                                                                                        (method, context, message, on, outer) => {
                                                                                            outer.ArgumentsDefinition.GetEvaluatedArguments(context, message, on, new SaneArrayList(), new SaneDictionary<string, object>());
                                                                                            return method.runtime.NewText(Range.GetNotice(on));
                                                                                        })));
        }

        public override IokeData CloneData(IokeObject obj, IokeObject m, IokeObject context) {
            return new Range(from, to, inclusive, inverted);
        }

        public static string GetInspect(object on) {
            return ((Range)(IokeObject.dataOf(on))).Inspect(on);
        }

        public static string GetNotice(object on) {
            return ((Range)(IokeObject.dataOf(on))).Notice(on);
        }

        public string Inspect(object obj) {
            StringBuilder sb = new StringBuilder();

            sb.Append(IokeObject.Inspect(from));
            if(inclusive) {
                sb.Append("..");
            } else {
                sb.Append("...");
            }
            sb.Append(IokeObject.Inspect(to));

            return sb.ToString();
        }

        public string Notice(object obj) {
            StringBuilder sb = new StringBuilder();

            sb.Append(IokeObject.Notice(from));
            if(inclusive) {
                sb.Append("..");
            } else {
                sb.Append("...");
            }
            sb.Append(IokeObject.Notice(to));

            return sb.ToString();
        }
    }
}
