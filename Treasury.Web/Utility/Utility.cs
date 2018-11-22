using System;
using System.Collections;
using System.Collections.Generic;
using Treasury.Web.ViewModels;

namespace Treasury.WebUtility
{
    public static class SetFile
    {
        /// <summary>
        /// 設定位置 & txtLog檔名
        /// </summary>
        static SetFile()
        {
            ProgramName = "Treasury"; //專案名稱
            FileUploads = "FileUploads"; //上傳檔案放置位置
            FileDownloads = "FileDownloads"; //下載檔案放置位置
        }

        public static string FileDownloads { get; private set; }
        public static string FileUploads { get; private set; }
        public static string ProgramName { get; private set; }
    }

    public class SelectOption 
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class SelectOption_Comparer : IEqualityComparer<SelectOption>
    {
        public bool Equals(SelectOption x, SelectOption y)
        {
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return (x.Text == y.Text && x.Value == y.Value);
        }

        public int GetHashCode(SelectOption obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;
            return obj.Value.GetHashCode() * obj.Text.GetHashCode();
        }

    }

    public class TDAApprSearchDetailViewModel_Comparer : IEqualityComparer<TDAApprSearchDetailViewModel>
    {
        public bool Equals(TDAApprSearchDetailViewModel x, TDAApprSearchDetailViewModel y)
        {
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return (x.vAply_No == y.vAply_No);
        }

        public int GetHashCode(TDAApprSearchDetailViewModel obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;
            return obj.vAply_No.GetHashCode();
        }
    }

    public class RadioButton
    {
        public RadioButton()
        {
            Id = string.Empty;
            Name = string.Empty;
            Text = string.Empty;
            Value = string.Empty;
        }

        public string Name { get; set; }
        public bool Checked { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public string Id { get; set; }
    }

    public class CheckBoxListInfo
    {
        public string DisplayText { get; set; }
        public bool IsChecked { get; set; }
        public string Value { get; set; }
    }

    public class FormateTitle
    {
        public string OldTitle { get; set; }
        public string NewTitle { get; set; }
    }

}