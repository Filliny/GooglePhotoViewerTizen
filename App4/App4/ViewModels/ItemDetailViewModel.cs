using System;

using App4.Models;

namespace App4.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public ItemInst ItemInst { get; set; }
        public ItemDetailViewModel(ItemInst itemInst = null)
        {
            Title = itemInst?.Text;
            ItemInst = itemInst;
        }
    }
}
