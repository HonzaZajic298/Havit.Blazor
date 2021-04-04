﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Havit.Blazor.Components.Web.Infrastructure;
using Havit.Diagnostics.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

// TODO Floating labels: FloatingLabelEffective - set default value to false

namespace Havit.Blazor.Components.Web.Bootstrap
{
	/// <summary>
	/// A base class for form input components. This base class automatically integrates
	/// with an Microsoft.AspNetCore.Components.Forms.EditContext, which must be supplied
	/// as a cascading parameter.
	/// Extends <see cref="InputBase{TValue}"/> class.
	/// 
	/// Adds support for rendering bootstrap based input with validator.
	/// See also https://v5.getbootstrap.com/docs/5.0/forms/overview/.
	/// </summary>
	public abstract class HxInputBase<TValue> : InputBase<TValue>, ICascadeEnabledComponent
	{
		/// <summary>
		/// CSS class used for invalid input.
		/// </summary>
		public const string InvalidCssClass = "is-invalid";

		/// <inheritdoc />
		[CascadingParameter] public FormState FormState { get; set; }

		/// <summary>
		/// Label to render before input (or after input for Checkbox).		
		/// </summary>
		[Parameter] public string Label { get; set; }

		/// <summary>
		/// Label to render before input (or after input for Checkbox).
		/// </summary>
		[Parameter] public RenderFragment LabelTemplate { get; set; }

		/// <summary>
		/// Hint to render after input as form-text.
		/// </summary>
		[Parameter] public string Hint { get; set; }

		/// <summary>
		/// Hint to render after input as form-text.
		/// </summary>
		[Parameter] public RenderFragment HintTemplate { get; set; }

		/// <summary>
		/// Custom CSS class to render with wrapping div.
		/// </summary>
		[Parameter] public new string CssClass { get; set; }

		/// <summary>
		/// Custom CSS class to render with the label.
		/// </summary>
		[Parameter] public string LabelCssClass { get; set; }

		/// <summary>
		/// https://getbootstrap.com/docs/5.0/forms/floating-labels/
		/// </summary>
		[Parameter] public bool? FloatingLabel { get; set; } = true;

		/// <summary>
		/// Custom CSS class to render with the input element.
		/// </summary>
		[Parameter] public string InputCssClass { get; set; }

		/// <summary>
		/// When <c>false</c>, validation message is not rendered. Default is <c>true</c>.
		/// </summary>
		[Parameter] public bool ShowValidationMessage { get; set; } = true;

		/// <summary>
		/// When true, HxChipGenerator is used to generate chip item(s). Default is <c>true</c>.
		/// </summary>
		[Parameter] public bool GenerateChip { get; set; } = true;

		/// <summary>
		/// Chip template.
		/// </summary>
		[Parameter] public RenderFragment ChipTemplate { get; set; }

		/// <inheritdoc />
		[Parameter] public bool? Enabled { get; set; }

		/// <summary>
		/// Returns effective value for <see cref="Enabled"/> property.
		/// </summary>
		protected virtual bool EnabledEffective => CascadeEnabledComponent.EnabledEffective(this);

		protected virtual bool FloatingLabelEffective => FloatingLabel ?? true; // TODO Floating labels: TRUE jen pro účely ladění!

		/// <summary>
		/// CSS class to be rendered with the wrapping div.
		/// </summary>
		private protected virtual string CoreCssClass => CssClassHelper.Combine("hx-form-group position-relative", FloatingLabelEffective ? "form-floating" : null);

		/// <summary>
		/// CSS class to be rendered with the input element.
		/// </summary>
		private protected virtual string CoreInputCssClass => "form-control";

		/// <summary>
		/// CSS class to be rendered with the label.
		/// </summary>
		private protected virtual string CoreLabelCssClass => "form-label";

		/// <summary>
		/// CSS class to be rendered with the hint.
		/// </summary>
		private protected virtual string CoreHintCssClass => "form-text";

		/// <summary>
		/// ID if the input element. Autogenerated when used with label.
		/// </summary>
		protected string InputId { get; private set; }

		/// <summary>
		/// Input ElementReference.
		/// Can be null. 
		/// </summary>
		protected ElementReference InputElement { get; set; }

		/// <summary>
		/// Elements rendering order. Overriden in the <see cref="HxInputCheckbox"/> component.
		/// </summary>
		protected virtual InputRenderOrder RenderOrder => FloatingLabelEffective ? InputRenderOrder.InputLabel : InputRenderOrder.LabelInput;

		private EditContext autoCreatedEditContext;

		public override Task SetParametersAsync(ParameterView parameters)
		{
			parameters.SetParameterProperties(this); // set properties to the component
			EnsureCascadingEditContext(); // create edit context when there was none
			return base.SetParametersAsync(ParameterView.Empty); // process base method (validations & EditContext property logic)
		}

		/// <summary>
		/// When there is no EditContext cascading parameter, lets create a new one and assing it to CascadedEditContext private property in a base InputBase class.
		/// </summary>
		/// <remarks>
		/// Even there is a protected EditContext property we cannot assign a value. When doing so InvalidOperationException exception is thrown.
		/// </remarks>
		private void EnsureCascadingEditContext()
		{
			var cascadedEditContextProperty = typeof(InputBase<TValue>).GetProperty("CascadedEditContext", BindingFlags.NonPublic | BindingFlags.Instance);

			if (cascadedEditContextProperty.GetValue(this) == null)
			{
				autoCreatedEditContext ??= new EditContext(new object());
				cascadedEditContextProperty.SetValue(this, autoCreatedEditContext);
			}
		}

		/// <inheritdoc />
		protected override sealed void BuildRenderTree(RenderTreeBuilder builder)
		{
			// no base call

			string cssClass = CssClassHelper.Combine(CoreCssClass, CssClass);

			// pokud nemáme css class, label, ani hint, budeme renderovat jako čistý input
			bool renderDiv = !String.IsNullOrEmpty(cssClass)
				|| !String.IsNullOrEmpty(Label)
				|| (LabelTemplate != null)
				|| !String.IsNullOrEmpty(Hint)
				|| (HintTemplate != null);

			// in checkbox label is renderead after input but we need InputId.
			if (!String.IsNullOrEmpty(Label) || (LabelTemplate != null))
			{
				EnsureInputId();
			}

			if (renderDiv)
			{
				builder.OpenElement(1, "div");
				if (!String.IsNullOrEmpty(cssClass))
				{
					builder.AddAttribute(2, "class", cssClass);
				}
			}

			switch (RenderOrder)
			{
				case InputRenderOrder.LabelInput:

					// majority component

					builder.OpenRegion(3);
					BuildRenderLabel(builder);
					builder.CloseRegion();

					builder.OpenRegion(4);
					BuildRenderInputAndValidationMessage(builder); // abychom mohli do inputu přidat div
					builder.CloseRegion();

					break;

				case InputRenderOrder.InputLabel:

					// checkbox 

					builder.OpenRegion(6);
					BuildRenderInputDecorated(builder);
					builder.CloseRegion();

					builder.OpenRegion(7);
					BuildRenderLabel(builder);
					builder.CloseRegion();

					builder.OpenRegion(8);
					BuildRenderValidationMessage(builder);
					builder.CloseRegion();

					break;

				default: throw new InvalidOperationException($"Unknown RenderOrder: {RenderOrder}");
			}

			builder.OpenRegion(9);
			BuildRenderHint(builder);
			builder.CloseRegion();

			if (renderDiv)
			{
				builder.CloseElement();
			}

			if (GenerateChip)
			{
				builder.OpenRegion(10);
				RenderChipGenerator(builder);
				builder.CloseRegion();
			}
		}

		/// <summary>
		/// Renders input and validation.
		/// Enables to render input-group wrapper in descendants.
		/// </summary>
		protected virtual void BuildRenderInputAndValidationMessage(RenderTreeBuilder builder)
		{
			// breaks the rule - ancesor is designed for descenant

			builder.OpenRegion(1);
			BuildRenderInputDecorated(builder);
			builder.CloseRegion();

			builder.OpenRegion(2);
			BuildRenderValidationMessage(builder);
			builder.CloseRegion();
		}

		/// <summary>
		/// Renders label when properties set.
		/// </summary>
		protected virtual void BuildRenderLabel(RenderTreeBuilder builder)
		{
			//  <label for="formGroupExampleInput">Example label</label>
			if (!String.IsNullOrEmpty(Label) || (LabelTemplate != null))
			{
				builder.OpenElement(1, "label");
				builder.AddAttribute(2, "for", InputId);
				builder.AddAttribute(3, "class", CssClassHelper.Combine(CoreLabelCssClass, LabelCssClass));
				builder.AddEventStopPropagationAttribute(4, "onclick", true); // TODO: Chceme onclick:stopPropagation na labelech všech inputů, nebo jen checkboxy? Má to být  nastavitelné?
				if (LabelTemplate == null)
				{
					builder.AddContent(5, Label);
				}
				builder.AddContent(6, LabelTemplate);
				builder.CloseElement();
			}
		}

		/// <summary>
		/// Render input. Enables to use some wrapping html, used for input-group in descenant.
		/// </summary>
		protected virtual void BuildRenderInputDecorated(RenderTreeBuilder builder)
		{
			// breaks the rule - ancesor is designed for descenant
			BuildRenderInput(builder);
		}

		/// <summary>
		/// Renders input.
		/// </summary>
		protected abstract void BuildRenderInput(RenderTreeBuilder builder);

		/// <summary>
		/// Add common attributes to the input.
		/// </summary>
		private protected virtual void BuildRenderInput_AddCommonAttributes(RenderTreeBuilder builder, string typeValue)
		{
			builder.AddMultipleAttributes(1, AdditionalAttributes);
			builder.AddAttribute(2, "id", InputId);
			builder.AddAttribute(3, "type", typeValue);
			builder.AddAttribute(4, "class", GetInputCssClassToRender());
			builder.AddAttribute(5, "disabled", !EnabledEffective);
			if (FloatingLabelEffective)
			{
				builder.AddAttribute(6, "placeholder", "placeholder"); // there must be a nonempty value (which is not visible)
			}
		}

		/// <summary>
		/// Renders hint when property HintTemplate set.
		/// </summary>
		protected virtual void BuildRenderHint(RenderTreeBuilder builder)
		{
			if (!String.IsNullOrEmpty(Hint) || (HintTemplate != null))
			{
				builder.OpenElement(1, "div");
				builder.AddAttribute(2, "class", CoreHintCssClass);
				builder.AddContent(3, Hint);
				builder.AddContent(4, HintTemplate);
				builder.CloseElement();
			}
		}

		/// <summary>
		/// Renders validation message (component <see cref="HxValidationMessage{TValue}" />) when not disabled (<seealso cref="ShowValidationMessage" />).
		/// </summary>
		protected virtual void BuildRenderValidationMessage(RenderTreeBuilder builder)
		{
			if (ShowValidationMessage)
			{
				//<div class="invalid-feedback">
				//Please provide a valid city.
				//</div>
				builder.OpenComponent<HxValidationMessage<TValue>>(1);
				if (autoCreatedEditContext != null)
				{
					builder.AddAttribute(2, nameof(HxValidationMessage<TValue>.EditContext), autoCreatedEditContext);
				}
				builder.AddAttribute(3, nameof(HxValidationMessage<TValue>.For), ValueExpression);
				builder.CloseComponent();
			}
		}

		/// <summary>
		/// Renders chip generator.
		/// </summary>
		protected virtual void RenderChipGenerator(RenderTreeBuilder builder)
		{
			if (!EqualityComparer<TValue>.Default.Equals(CurrentValue, default(TValue))) // TODO: virtual method
			{
				builder.OpenComponent<HxChipGenerator>(0);
				builder.AddAttribute(1, nameof(HxChipGenerator.ChildContent), (RenderFragment)RenderChipTemplate);
				builder.AddAttribute(2, nameof(HxChipGenerator.ChipRemoveAction), GetChipRemoveAction());

				builder.CloseComponent();
			}
		}

		/// <summary>
		/// Returns chip template.
		/// </summary>
		protected void RenderChipTemplate(RenderTreeBuilder builder)
		{
			if (ChipTemplate != null)
			{
				builder.AddContent(0, ChipTemplate);
			}
			else
			{
				builder.AddContent(1, Label);
				builder.AddContent(2, ": ");
				builder.OpenRegion(3);
				RenderChipValue(builder);
				builder.CloseRegion();
			}
		}

		protected virtual void RenderChipValue(RenderTreeBuilder builder)
		{
			builder.AddContent(0, CurrentValueAsString);
		}

		/// <summary>
		/// Returns action to remove chip from model.
		/// </summary>
		protected virtual Action<object> GetChipRemoveAction()
		{
			string fieldName = this.FieldIdentifier.FieldName; // carefully! don't use this in lambda below to allow it for GC
			Action<object> removeAction = (model) => model.GetType().GetProperty(fieldName).SetValue(model, default(TValue));

			return removeAction;
		}

		/// <summary>
		/// Gives focus to an input element.
		/// </summary>
		public virtual async ValueTask FocusAsync()
		{
			if (EqualityComparer<ElementReference>.Default.Equals(InputElement, default))
			{
				throw new InvalidOperationException($"Cannot focus {this.GetType()}. The method must be called after first render.");
			}
			await InputElement.FocusAsync();
		}
		/// <summary>
		/// Sets InputId to a random value when empty.
		/// </summary>
		protected void EnsureInputId()
		{
			if (String.IsNullOrEmpty(InputId))
			{
				InputId = "el" + Guid.NewGuid().ToString("N");
			}
		}

		/// <summary>
		/// Returns true when the Value is considered do be valid. Otherwise false.
		/// </summary>
		private protected bool IsValueValid() => EditContext.GetValidationMessages(FieldIdentifier).Any();

		/// <summary>
		/// Gets css class for input.
		/// </summary>
		protected virtual string GetInputCssClassToRender()
		{
			string validationCssClass = IsValueValid() ? InvalidCssClass : null;
			return CssClassHelper.Combine(CoreInputCssClass, InputCssClass, validationCssClass, (this is IInputWithSize inputithSize) ? inputithSize.GetInputSizeCssClass() : null);
		}

		/// <summary>
		/// Returns attribute from the bounded property if exists. Otherwise returns null.
		/// </summary>
		protected TAttribute GetValueAttribute<TAttribute>()
			where TAttribute : Attribute
		{
			return FieldIdentifier.Model.GetType().GetMember(FieldIdentifier.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Single().GetCustomAttribute<TAttribute>();
		}
	}
}