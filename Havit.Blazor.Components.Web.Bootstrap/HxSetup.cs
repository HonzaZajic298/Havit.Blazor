﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havit.Blazor.Components.Web.Bootstrap;
public static class HxSetup
{
	/// <summary>
	/// Renders <c>&lt;script&lt;</c> tag referencing corresponding Bootstrap JavaScript bundle with Popper.<br/>
	/// To be used in <c>_Layout.cshtml</c> as <c>@Html.Raw(HxSetup.RenderBootstrapJavaScriptReference())</c>.
	/// </summary>
	/// <remarks>
	/// We do not want to use TagHelper nor HTML Helper here as we do not want to introduce dependency on server-side ASP.NET Core (MVC/Razor) to our library (separate NuGet package would have to be created).
	/// </remarks>
	public static string RenderBootstrapJavaScriptReference()
	{
		return "<script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/js/bootstrap.bundle.min.js\" integrity=\"sha384-w76AqPfDkMBDXo30jS1Sgez6pr3x5MlQ1ZAGC+nuZB+EYdgRZgiwxhTBTkF7CXvN\" crossorigin=\"anonymous\"></script>";
	}

	/// <summary>
	/// Renders <c>&lt;link&lt;</c> tag referencing corresponding Bootstrap CSS.<br/>
	/// To be used in <c>_Layout.cshtml</c> as <c>@Html.Raw(HxSetup.RenderBootstrapCssReference())</c>.
	/// </summary>
	/// <remarks>
	/// We do not want to use TagHelper nor HTML Helper here as we do not want to introduce dependency on server-side ASP.NET Core (MVC/Razor) to our library (separate NuGet package would have to be created).
	/// </remarks>
	public static string RenderBootstrapCssReference(BootstrapFlavor bootstrapFlavor = BootstrapFlavor.HavitDefault)
	{
		return bootstrapFlavor switch
		{
			BootstrapFlavor.HavitDefault => "<link href=\"_content/Havit.Blazor.Components.Web.Bootstrap/bootstrap.css?v=" + VersionIdentifierHavitBlazorBootstrap + "\" rel=\"stylesheet\" />",
			BootstrapFlavor.PlainBoostrap => "<link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-GLhlTQ8iRABdZLl6O3oVMWSktQOp6b7In1Zl3/Jr59b6EGGoI1aFkw7cmDA6j6gD\" crossorigin=\"anonymous\">",
			_ => throw new ArgumentOutOfRangeException($"Unknown {nameof(BootstrapFlavor)} value {bootstrapFlavor}.")
		};
	}

	internal static string VersionIdentifierHavitBlazorBootstrap { get; } = Havit.Blazor.Components.Web.JSRuntimeExtensions.GetAssemblyVersionIdentifierForUri(typeof(HxSetup).Assembly);
}