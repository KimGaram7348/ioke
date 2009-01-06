/*
 * See LICENSE file in distribution for copyright and licensing information.
 */
package ioke.lang;

import java.util.List;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Set;

import org.jregex.Matcher;
import org.jregex.Pattern;
import org.jregex.MatchIterator;
import org.jregex.MatchResult;

import ioke.lang.exceptions.ControlFlow;

/**
 * @author <a href="mailto:ola.bini@gmail.com">Ola Bini</a>
 */
public class RegexpMatch extends IokeData {
    private IokeObject regexp;
    private MatchResult mr;
    private IokeObject target;

    public RegexpMatch(IokeObject regexp, MatchResult mr, IokeObject target) {
        this.regexp = regexp;
        this.mr = mr;
        this.target = target;
    }
    
    public static Object getTarget(Object on) throws ControlFlow {
        return ((RegexpMatch)IokeObject.data(on)).target;
    }

    public static Object getRegexp(Object on) throws ControlFlow {
        return ((RegexpMatch)IokeObject.data(on)).regexp;
    }

    @Override
    public void init(IokeObject obj) throws ControlFlow {
        final Runtime runtime = obj.runtime;
        obj.setKind("Regexp Match");

        obj.registerMethod(runtime.newJavaMethod("Returns the target that this match was created against", new JavaMethod.WithNoArguments("target") {
                @Override
                public Object activate(IokeObject method, IokeObject context, IokeObject message, Object on) throws ControlFlow {
                    getArguments().getEvaluatedArguments(context, message, on, new ArrayList<Object>(), new HashMap<String, Object>());
                    return getTarget(on);
                }
            }));

        obj.registerMethod(runtime.newJavaMethod("returns a list of all the named groups in the regular expression used to create this match", new JavaMethod.WithNoArguments("names") {
                @Override
                public Object activate(IokeObject method, IokeObject context, IokeObject message, Object on) throws ControlFlow {
                    getArguments().getEvaluatedArguments(context, message, on, new ArrayList<Object>(), new HashMap<String, Object>());

                    Set names = Regexp.getRegexp(getRegexp(on)).getGroupNames();
                    List<Object> theNames = new ArrayList<Object>();
                    for(Object name : names) {
                        theNames.add(context.runtime.getSymbol(((String)name)));
                    }
                    return context.runtime.newList(theNames);
                }
            }));
    }
}