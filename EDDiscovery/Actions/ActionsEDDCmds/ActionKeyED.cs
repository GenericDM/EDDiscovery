﻿/*
 * Copyright © 2017 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseUtils;
using AudioExtensions;
using Conditions;
using ActionLanguage;

namespace EDDiscovery.Actions
{
    public class ActionKeyED : ActionKey        // extends Key
    {
        class AKP : BaseUtils.EnhancedSendKeys.AdditionalKeyParser      // AKP parser to pass to SendKeys
        {
            public EliteDangerousCore.BindingsFile bindingsfile;

            public Tuple<string, int, string> Parse(string s)
            {
                if ( s.Length > 0 && s.StartsWith("{"))     // frontier bindings start with decoration
                {
                    int endindex = s.IndexOf("}");
                    if ( endindex>=0 )                      // valid {}
                    {
                        string binding = s.Substring(1, endindex - 1);

                        List<Tuple<EliteDangerousCore.BindingsFile.Device, EliteDangerousCore.BindingsFile.Assignment>> matches 
                                    = bindingsfile.FindAssignedFunc(binding, EliteDangerousCore.BindingsFile.KeyboardDeviceName);   // just give me keyboard bindings, thats all i can do

                        if ( matches != null )
                        {
                            string keyseq = "";
                            foreach( var k in matches[0].Item2.keys )       // all keys.. list out for pressing (may need more work)
                            {
                                Keys vkey = DirectInputDevices.KeyConversion.FrontierNameToKeys(k.Key);
                                if ( vkey == Keys.None )
                                {
                                    return new Tuple<string, int, string>(null, 0, "Conversion of Frontier key " + k.Key + " failed ");
                                }

                                keyseq += vkey.VKeyToString() + " ";
                            }

                            return new Tuple<string, int, string>(keyseq, endindex + 1, null);
                        }
                        else
                            return new Tuple<string, int, string>(null, 0, "For binding " + binding + " the current bindings file has no key assignments");

                    }
                }

                return new Tuple<string, int, string>(null, 0, null);
            }
        }

        static public string Menu(Form parent, System.Drawing.Icon ic, string userdata, EliteDangerousCore.BindingsFile bf)
        {
            List<string> decorated = (from x in bf.KeyNames select "{"+x+"}").ToList();
            return Menu(parent, ic, userdata, decorated, new AKP() { bindingsfile = bf });
        
        }

        public override bool ConfigurationMenu(Form parent, ActionCoreController cp, List<string> eventvars)    // override again to expand any functionality
        {
            ActionController ac = cp as ActionController;

            string ud = Menu(parent, cp.Icon, userdata , ac.FrontierBindings );      // base has no additional keys
            if (ud != null)
            {
                userdata = ud;
                return true;
            }
            else
                return false;
        }

        public override bool ExecuteAction(ActionProgramRun ap)
        {
            ActionController ac = ap.actioncontroller as ActionController;
            return ExecuteAction(ap, new AKP() { bindingsfile = ac.FrontierBindings }); //base, TBD pass in tx funct
        }
    }
}
