using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;

namespace EPP.Models
{
    public partial class PictureVariantDialogData : ObservableObject
    {
        [ObservableProperty]
        private List<string> _variants;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private string _selected;

        public Action<string> OnConfirm { get; private set; }

        public PictureVariantDialogData(List<string> variants, string selected, Action<string> onConfirm)
        {
            Variants = variants;
            Selected = selected;
            OnConfirm = onConfirm;
        }

        [RelayCommand(CanExecute = nameof(CanConfirm))]
        public void Confirm()
        {
            OnConfirm(Selected);
            DialogHost.GetDialogSession("MainDialogHost")?.Close();
        }

        public bool CanConfirm()
        {
            return Selected != null && Variants.Contains(Selected);
        }
    }
}
