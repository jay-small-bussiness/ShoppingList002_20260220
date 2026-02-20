using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList002.Models.Sync
{
    public enum SyncPlan
    {
        Free,
        Solo,
        Family
    }

    public class SyncContext
    {
        public SyncPlan Plan { get; set; }   // Free / Solo / Family

        public bool IsFamilyMode => Plan == SyncPlan.Family;

        public int? FamilyId { get; set; }   // Family のときだけ
        public int? UserId { get; set; }     // server 側ユーザーID
    }

}
