
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DFFDelivery
{
	public class Order
	{
		public int ID;
		public string Address;
		public DateTime Time;
		public string SenderName;
		public string SenderPhone;
		public string RecipientName;
		public string RecipientPhone;
		public string ExtraPhone;
		public int Sum;
		public int Prepaid;
		public int Take;
		public string Note;

		public Order(int ID,
		             string Address,
		             DateTime Time,
		             string SenderName,
		             string SenderPhone,
		             string RecipientName,
		             string RecipientPhone,
		             string ExtraPhone,
		             int Sum,
		             int Prepaid,
		             string Note)
		{
			this.ID = ID; //not visible
			this.Address = Address;
			this.Time = Time;
			this.SenderName = SenderName;
			this.SenderPhone = SenderPhone;
			this.RecipientName = RecipientName;
			this.RecipientPhone = RecipientPhone;
			this.ExtraPhone = ExtraPhone;
			this.Sum = Sum;
			this.Prepaid = Prepaid;
			this.Note = Note;
			this.Take = Sum - Prepaid;
		}
		
		public Order( )
		{
		}
	}
}

