﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DFHRIS.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10.204.241.226")]
        public string smtpServer {
            get {
                return ((string)(this["smtpServer"]));
            }
            set {
                this["smtpServer"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("glsisys.life@fbt.com")]
        public string mailAccount {
            get {
                return ((string)(this["mailAccount"]));
            }
            set {
                this["mailAccount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Aa87522899")]
        public string mailPwd {
            get {
                return ((string)(this["mailPwd"]));
            }
            set {
                this["mailPwd"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("glsisys.life@fbt.com:FRTXmlMonitor;jimmy.chllu@fubon.com:盧嘉良;joan2.lin@fubon.com:" +
            "林幸蘭;ilien.yen@fubon.com:顏秀蓮;daiyu.huang@fubon.com:黃黛鈺;bianco.lee@fubon.com:李彥賢;m" +
            "ark.ch.chang@fubon.com:張家華;titan.ph.huang@fubon.com:黃柏翰;")]
        public string mailSend {
            get {
                return ((string)(this["mailSend"]));
            }
            set {
                this["mailSend"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public string DEP_LEVEL {
            get {
                return ((string)(this["DEP_LEVEL"]));
            }
            set {
                this["DEP_LEVEL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("XB0K0")]
        public string PAR_DEP {
            get {
                return ((string)(this["PAR_DEP"]));
            }
            set {
                this["PAR_DEP"] = value;
            }
        }
    }
}