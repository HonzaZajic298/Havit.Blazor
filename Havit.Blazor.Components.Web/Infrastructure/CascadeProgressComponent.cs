﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace Havit.Blazor.Components.Web.Infrastructure
{
	/// <summary>
	/// <see cref="ICascadeProgressComponent"/> helper method.
	/// </summary>
	public static class CascadeProgressComponent
	{
		/// <summary>
		/// Effective value of InProgress. When InProgress is not set, receives value from ProgressState or defaults to false.
		/// </summary>
		public static bool InProgressEffective(ICascadeProgressComponent component)
		{			
			return component.InProgress ?? component.ProgressState?.InProgress ?? false;
		}
	}
}
