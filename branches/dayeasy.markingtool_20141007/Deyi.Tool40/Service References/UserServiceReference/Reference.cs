﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18444
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Deyi.Tool.UserServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ResultPacket", Namespace="http://schemas.datacontract.org/2004/07/DayEZ.Framework")]
    [System.SerializableAttribute()]
    public partial class ResultPacket : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DescriptionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsErrorField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Description {
            get {
                return this.DescriptionField;
            }
            set {
                if ((object.ReferenceEquals(this.DescriptionField, value) != true)) {
                    this.DescriptionField = value;
                    this.RaisePropertyChanged("Description");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsError {
            get {
                return this.IsErrorField;
            }
            set {
                if ((this.IsErrorField.Equals(value) != true)) {
                    this.IsErrorField = value;
                    this.RaisePropertyChanged("IsError");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="UserLoginInfo", Namespace="http://schemas.datacontract.org/2004/07/DayEZ.API.User.Entity.User")]
    [System.SerializableAttribute()]
    public partial class UserLoginInfo : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EmailField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PasswordField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string VerifyCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string VerifyCodeInSessionField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email {
            get {
                return this.EmailField;
            }
            set {
                if ((object.ReferenceEquals(this.EmailField, value) != true)) {
                    this.EmailField = value;
                    this.RaisePropertyChanged("Email");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Password {
            get {
                return this.PasswordField;
            }
            set {
                if ((object.ReferenceEquals(this.PasswordField, value) != true)) {
                    this.PasswordField = value;
                    this.RaisePropertyChanged("Password");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string VerifyCode {
            get {
                return this.VerifyCodeField;
            }
            set {
                if ((object.ReferenceEquals(this.VerifyCodeField, value) != true)) {
                    this.VerifyCodeField = value;
                    this.RaisePropertyChanged("VerifyCode");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string VerifyCodeInSession {
            get {
                return this.VerifyCodeInSessionField;
            }
            set {
                if ((object.ReferenceEquals(this.VerifyCodeInSessionField, value) != true)) {
                    this.VerifyCodeInSessionField = value;
                    this.RaisePropertyChanged("VerifyCodeInSession");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="UserLoginResult", Namespace="http://schemas.datacontract.org/2004/07/DayEZ.API.User.Entity.User")]
    [System.SerializableAttribute()]
    public partial class UserLoginResult : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Deyi.Tool.UserServiceReference.UserInAgencyBasicInfo[] AgencyRolesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AvatarFileField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EmailField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private long IDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IDNoField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime LastLoginAtField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LastLoginIPField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NicknameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Deyi.Tool.UserServiceReference.UserStatus StatusField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string TrueNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Deyi.Tool.UserServiceReference.UserInAgencyBasicInfo[] AgencyRoles {
            get {
                return this.AgencyRolesField;
            }
            set {
                if ((object.ReferenceEquals(this.AgencyRolesField, value) != true)) {
                    this.AgencyRolesField = value;
                    this.RaisePropertyChanged("AgencyRoles");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AvatarFile {
            get {
                return this.AvatarFileField;
            }
            set {
                if ((object.ReferenceEquals(this.AvatarFileField, value) != true)) {
                    this.AvatarFileField = value;
                    this.RaisePropertyChanged("AvatarFile");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email {
            get {
                return this.EmailField;
            }
            set {
                if ((object.ReferenceEquals(this.EmailField, value) != true)) {
                    this.EmailField = value;
                    this.RaisePropertyChanged("Email");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public long ID {
            get {
                return this.IDField;
            }
            set {
                if ((this.IDField.Equals(value) != true)) {
                    this.IDField = value;
                    this.RaisePropertyChanged("ID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string IDNo {
            get {
                return this.IDNoField;
            }
            set {
                if ((object.ReferenceEquals(this.IDNoField, value) != true)) {
                    this.IDNoField = value;
                    this.RaisePropertyChanged("IDNo");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime LastLoginAt {
            get {
                return this.LastLoginAtField;
            }
            set {
                if ((this.LastLoginAtField.Equals(value) != true)) {
                    this.LastLoginAtField = value;
                    this.RaisePropertyChanged("LastLoginAt");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LastLoginIP {
            get {
                return this.LastLoginIPField;
            }
            set {
                if ((object.ReferenceEquals(this.LastLoginIPField, value) != true)) {
                    this.LastLoginIPField = value;
                    this.RaisePropertyChanged("LastLoginIP");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Nickname {
            get {
                return this.NicknameField;
            }
            set {
                if ((object.ReferenceEquals(this.NicknameField, value) != true)) {
                    this.NicknameField = value;
                    this.RaisePropertyChanged("Nickname");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Deyi.Tool.UserServiceReference.UserStatus Status {
            get {
                return this.StatusField;
            }
            set {
                if ((this.StatusField.Equals(value) != true)) {
                    this.StatusField = value;
                    this.RaisePropertyChanged("Status");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string TrueName {
            get {
                return this.TrueNameField;
            }
            set {
                if ((object.ReferenceEquals(this.TrueNameField, value) != true)) {
                    this.TrueNameField = value;
                    this.RaisePropertyChanged("TrueName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="UserInAgencyBasicInfo", Namespace="http://schemas.datacontract.org/2004/07/DayEZ.API.User.Entity.User")]
    [System.SerializableAttribute()]
    public partial class UserInAgencyBasicInfo : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private long AgencyIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AgencyNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsSubjectLoadFormulaField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int RoleIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string RoleNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte StageIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string StageNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte SubjectIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string SubjectNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public long AgencyID {
            get {
                return this.AgencyIDField;
            }
            set {
                if ((this.AgencyIDField.Equals(value) != true)) {
                    this.AgencyIDField = value;
                    this.RaisePropertyChanged("AgencyID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AgencyName {
            get {
                return this.AgencyNameField;
            }
            set {
                if ((object.ReferenceEquals(this.AgencyNameField, value) != true)) {
                    this.AgencyNameField = value;
                    this.RaisePropertyChanged("AgencyName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsSubjectLoadFormula {
            get {
                return this.IsSubjectLoadFormulaField;
            }
            set {
                if ((this.IsSubjectLoadFormulaField.Equals(value) != true)) {
                    this.IsSubjectLoadFormulaField = value;
                    this.RaisePropertyChanged("IsSubjectLoadFormula");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int RoleID {
            get {
                return this.RoleIDField;
            }
            set {
                if ((this.RoleIDField.Equals(value) != true)) {
                    this.RoleIDField = value;
                    this.RaisePropertyChanged("RoleID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string RoleName {
            get {
                return this.RoleNameField;
            }
            set {
                if ((object.ReferenceEquals(this.RoleNameField, value) != true)) {
                    this.RoleNameField = value;
                    this.RaisePropertyChanged("RoleName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte StageID {
            get {
                return this.StageIDField;
            }
            set {
                if ((this.StageIDField.Equals(value) != true)) {
                    this.StageIDField = value;
                    this.RaisePropertyChanged("StageID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string StageName {
            get {
                return this.StageNameField;
            }
            set {
                if ((object.ReferenceEquals(this.StageNameField, value) != true)) {
                    this.StageNameField = value;
                    this.RaisePropertyChanged("StageName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte SubjectID {
            get {
                return this.SubjectIDField;
            }
            set {
                if ((this.SubjectIDField.Equals(value) != true)) {
                    this.SubjectIDField = value;
                    this.RaisePropertyChanged("SubjectID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string SubjectName {
            get {
                return this.SubjectNameField;
            }
            set {
                if ((object.ReferenceEquals(this.SubjectNameField, value) != true)) {
                    this.SubjectNameField = value;
                    this.RaisePropertyChanged("SubjectName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="UserStatus", Namespace="http://schemas.datacontract.org/2004/07/DayEZ.API.User.Entity")]
    public enum UserStatus : byte {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Normal = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Locked = 1,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="UserRegisteInfo", Namespace="http://schemas.datacontract.org/2004/07/DayEZ.API.User.Entity.User")]
    [System.SerializableAttribute()]
    public partial class UserRegisteInfo : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EmailField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private long InviteCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsAgreeProtocolField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NicknameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PasswordField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string RePasswordField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string VerifyCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string VerifyCodeInSessionField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email {
            get {
                return this.EmailField;
            }
            set {
                if ((object.ReferenceEquals(this.EmailField, value) != true)) {
                    this.EmailField = value;
                    this.RaisePropertyChanged("Email");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public long InviteCode {
            get {
                return this.InviteCodeField;
            }
            set {
                if ((this.InviteCodeField.Equals(value) != true)) {
                    this.InviteCodeField = value;
                    this.RaisePropertyChanged("InviteCode");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsAgreeProtocol {
            get {
                return this.IsAgreeProtocolField;
            }
            set {
                if ((this.IsAgreeProtocolField.Equals(value) != true)) {
                    this.IsAgreeProtocolField = value;
                    this.RaisePropertyChanged("IsAgreeProtocol");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Nickname {
            get {
                return this.NicknameField;
            }
            set {
                if ((object.ReferenceEquals(this.NicknameField, value) != true)) {
                    this.NicknameField = value;
                    this.RaisePropertyChanged("Nickname");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Password {
            get {
                return this.PasswordField;
            }
            set {
                if ((object.ReferenceEquals(this.PasswordField, value) != true)) {
                    this.PasswordField = value;
                    this.RaisePropertyChanged("Password");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string RePassword {
            get {
                return this.RePasswordField;
            }
            set {
                if ((object.ReferenceEquals(this.RePasswordField, value) != true)) {
                    this.RePasswordField = value;
                    this.RaisePropertyChanged("RePassword");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string VerifyCode {
            get {
                return this.VerifyCodeField;
            }
            set {
                if ((object.ReferenceEquals(this.VerifyCodeField, value) != true)) {
                    this.VerifyCodeField = value;
                    this.RaisePropertyChanged("VerifyCode");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string VerifyCodeInSession {
            get {
                return this.VerifyCodeInSessionField;
            }
            set {
                if ((object.ReferenceEquals(this.VerifyCodeInSessionField, value) != true)) {
                    this.VerifyCodeInSessionField = value;
                    this.RaisePropertyChanged("VerifyCodeInSession");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.dayez.net/", ConfigurationName="UserServiceReference.User")]
    public interface User {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/IsNicknameUnique", ReplyAction="http://www.dayez.net/User/IsNicknameUniqueResponse")]
        Deyi.Tool.UserServiceReference.ResultPacket IsNicknameUnique(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/IsEmailUnique", ReplyAction="http://www.dayez.net/User/IsEmailUniqueResponse")]
        Deyi.Tool.UserServiceReference.ResultPacket IsEmailUnique(string email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/UserLogin", ReplyAction="http://www.dayez.net/User/UserLoginResponse")]
        Deyi.Tool.UserServiceReference.ResultPacket UserLogin(out Deyi.Tool.UserServiceReference.UserLoginResult userBasicInfo, Deyi.Tool.UserServiceReference.UserLoginInfo model);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/UserRegiste", ReplyAction="http://www.dayez.net/User/UserRegisteResponse")]
        Deyi.Tool.UserServiceReference.ResultPacket UserRegiste(Deyi.Tool.UserServiceReference.UserRegisteInfo model);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetTotalStudentCount", ReplyAction="http://www.dayez.net/User/GetTotalStudentCountResponse")]
        int GetTotalStudentCount();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetAvatar", ReplyAction="http://www.dayez.net/User/GetAvatarResponse")]
        string GetAvatar(long userID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetMyAgencyUserIDs", ReplyAction="http://www.dayez.net/User/GetMyAgencyUserIDsResponse")]
        long[] GetMyAgencyUserIDs(long userID, long agencyID, byte subjectID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetOtherAgencyUserIDs", ReplyAction="http://www.dayez.net/User/GetOtherAgencyUserIDsResponse")]
        long[] GetOtherAgencyUserIDs(long userID, long agencyID, byte subjectID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetNicknames", ReplyAction="http://www.dayez.net/User/GetNicknamesResponse")]
        System.Collections.Generic.Dictionary<long, string> GetNicknames(long[] userIDs);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetClassIDNames", ReplyAction="http://www.dayez.net/User/GetClassIDNamesResponse")]
        System.Collections.Generic.Dictionary<long, string> GetClassIDNames(long agencyID, long userID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetStudentIDIDNos", ReplyAction="http://www.dayez.net/User/GetStudentIDIDNosResponse")]
        System.Collections.Generic.Dictionary<long, string> GetStudentIDIDNos(long classID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetUserIDTrueNames", ReplyAction="http://www.dayez.net/User/GetUserIDTrueNamesResponse")]
        System.Collections.Generic.Dictionary<long, string> GetUserIDTrueNames(long[] arrUserID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetStudentCodeTrueNameByUserIDs", ReplyAction="http://www.dayez.net/User/GetStudentCodeTrueNameByUserIDsResponse")]
        System.Collections.Generic.Dictionary<string, string> GetStudentCodeTrueNameByUserIDs(long[] arrUserID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.dayez.net/User/GetStudentCodeTrueName", ReplyAction="http://www.dayez.net/User/GetStudentCodeTrueNameResponse")]
        System.Collections.Generic.Dictionary<string, string> GetStudentCodeTrueName(string[] arrIDNo);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface UserChannel : Deyi.Tool.UserServiceReference.User, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class UserClient : System.ServiceModel.ClientBase<Deyi.Tool.UserServiceReference.User>, Deyi.Tool.UserServiceReference.User {
        
        public UserClient() {
        }
        
        public UserClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public UserClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public UserClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public UserClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Deyi.Tool.UserServiceReference.ResultPacket IsNicknameUnique(string nickname) {
            return base.Channel.IsNicknameUnique(nickname);
        }
        
        public Deyi.Tool.UserServiceReference.ResultPacket IsEmailUnique(string email) {
            return base.Channel.IsEmailUnique(email);
        }
        
        public Deyi.Tool.UserServiceReference.ResultPacket UserLogin(out Deyi.Tool.UserServiceReference.UserLoginResult userBasicInfo, Deyi.Tool.UserServiceReference.UserLoginInfo model) {
            return base.Channel.UserLogin(out userBasicInfo, model);
        }
        
        public Deyi.Tool.UserServiceReference.ResultPacket UserRegiste(Deyi.Tool.UserServiceReference.UserRegisteInfo model) {
            return base.Channel.UserRegiste(model);
        }
        
        public int GetTotalStudentCount() {
            return base.Channel.GetTotalStudentCount();
        }
        
        public string GetAvatar(long userID) {
            return base.Channel.GetAvatar(userID);
        }
        
        public long[] GetMyAgencyUserIDs(long userID, long agencyID, byte subjectID) {
            return base.Channel.GetMyAgencyUserIDs(userID, agencyID, subjectID);
        }
        
        public long[] GetOtherAgencyUserIDs(long userID, long agencyID, byte subjectID) {
            return base.Channel.GetOtherAgencyUserIDs(userID, agencyID, subjectID);
        }
        
        public System.Collections.Generic.Dictionary<long, string> GetNicknames(long[] userIDs) {
            return base.Channel.GetNicknames(userIDs);
        }
        
        public System.Collections.Generic.Dictionary<long, string> GetClassIDNames(long agencyID, long userID) {
            return base.Channel.GetClassIDNames(agencyID, userID);
        }
        
        public System.Collections.Generic.Dictionary<long, string> GetStudentIDIDNos(long classID) {
            return base.Channel.GetStudentIDIDNos(classID);
        }
        
        public System.Collections.Generic.Dictionary<long, string> GetUserIDTrueNames(long[] arrUserID) {
            return base.Channel.GetUserIDTrueNames(arrUserID);
        }
        
        public System.Collections.Generic.Dictionary<string, string> GetStudentCodeTrueNameByUserIDs(long[] arrUserID) {
            return base.Channel.GetStudentCodeTrueNameByUserIDs(arrUserID);
        }
        
        public System.Collections.Generic.Dictionary<string, string> GetStudentCodeTrueName(string[] arrIDNo) {
            return base.Channel.GetStudentCodeTrueName(arrIDNo);
        }
    }
}