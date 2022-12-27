using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using NetStarter.Resources;
using System.Web.Routing;

namespace NetStarter.CustomHelper
{
    public static class CustomHelper
    {
        public static MvcHtmlString CustomDropDownList(string id, List<SelectListItem> selectListItems)
        {
            string selectedText = selectListItems.Where(a => a.Selected == true).Select(a => a.Text).FirstOrDefault();

            var buildDiv = new TagBuilder("div");
            buildDiv.MergeAttribute("class", "select-wrapper");

            var buildSelect = new TagBuilder("div");
            buildSelect.MergeAttribute("class", "select");

            var buildSelectTrigger = new TagBuilder("div");
            buildSelectTrigger.MergeAttribute("class", "select__trigger");

            var buildSelectSpan = new TagBuilder("span");
            buildSelectSpan.SetInnerText(string.IsNullOrEmpty(selectedText) ? Resource.PleaseSelect : selectedText);

            var buildArrow = new TagBuilder("div");
            buildArrow.MergeAttribute("class", "arrow");

            var buildArrowIcon = new TagBuilder("i");
            buildArrowIcon.MergeAttribute("class", "fa-solid fa-caret-down");

            buildArrow.InnerHtml += buildArrowIcon;
            buildSelectTrigger.InnerHtml += buildSelectSpan;
            buildSelectTrigger.InnerHtml += buildArrow;

            var buildCustomOptions = new TagBuilder("div");
            buildCustomOptions.MergeAttribute("class", "custom-options");

            var buildInput = new TagBuilder("input");
            buildInput.GenerateId(id);
            buildInput.MergeAttribute("class", "custom-option");
            buildInput.MergeAttribute("name", id);
            buildInput.MergeAttribute("for", id);
            buildInput.MergeAttribute("hidden", "hidden");
            buildInput.MergeAttribute("value", "");
            buildInput.ToString(TagRenderMode.SelfClosing);

            var buildPlaceholder = new TagBuilder("span");
            buildPlaceholder.SetInnerText(Resource.PleaseSelect);
            if (!string.IsNullOrEmpty(selectedText))
            {
                buildPlaceholder.MergeAttribute("class", "custom-option");
            }
            else
            {
                buildPlaceholder.MergeAttribute("class", "custom-option selected");
            }
            buildPlaceholder.MergeAttribute("value", "null");
            buildPlaceholder.MergeAttribute("data-value", "null");
            buildCustomOptions.InnerHtml += buildPlaceholder;

            foreach (var item in selectListItems)
            {
                var buildOption = new TagBuilder("span");
                buildOption.MergeAttribute("value", item.Value);
                buildOption.MergeAttribute("data-value", item.Value);
                buildOption.SetInnerText(item.Text);
                buildOption.ToString(TagRenderMode.Normal);
                if (item.Selected == true)
                {
                    buildInput.MergeAttribute("value", item.Value);
                    buildOption.MergeAttribute("class", "custom-option selected");
                }
                else
                {
                    buildOption.MergeAttribute("class", "custom-option");
                }
                buildCustomOptions.InnerHtml += buildOption;
            }

            buildSelect.InnerHtml += buildInput;
            buildSelect.InnerHtml += buildSelectTrigger;
            buildSelect.InnerHtml += buildCustomOptions;

            buildDiv.InnerHtml += buildSelect;

            return new MvcHtmlString(buildDiv.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString CustomMultiSelect(string id, List<SelectListItem> selectListItems)
        {
            var buildDiv = new TagBuilder("div");

            var buildSelect = new TagBuilder("select");
            buildSelect.GenerateId(id);
            buildSelect.MergeAttribute("name", id);
            buildSelect.MergeAttribute("for", id);
            buildSelect.MergeAttribute("class", "multichoice");
            buildSelect.MergeAttribute("multiple", "multiple");
            buildSelect.MergeAttribute("placeholder", Resource.Selectmultiple);

            foreach (var item in selectListItems)
            {
                var buildOption = new TagBuilder("option");
                buildOption.MergeAttribute("value", item.Value);
                buildOption.SetInnerText(item.Text);
                buildOption.ToString(TagRenderMode.Normal);
                if (item.Selected == true)
                {
                    buildOption.MergeAttribute("selected", "selected");
                }

                buildSelect.InnerHtml += buildOption;
            }

            buildDiv.InnerHtml += buildSelect;

            return new MvcHtmlString(buildDiv.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString CustomMultiSelectFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var buildDiv = new TagBuilder("div");
            return new MvcHtmlString(buildDiv.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString CustomSearchAndSelect(string id, string datalistId, List<SelectListItem> selectListItems)
        {
            var buildDiv = new TagBuilder("div");

            var buildInput = new TagBuilder("input");
            buildInput.GenerateId(id);
            buildInput.MergeAttribute("name", id);
            buildInput.MergeAttribute("for", id);
            buildInput.MergeAttribute("class", "form-control");
            buildInput.MergeAttribute("list", datalistId);
            buildInput.MergeAttribute("placeholder", Resource.TypetoSearch);
            buildInput.ToString(TagRenderMode.SelfClosing);

            var buildSelect = new TagBuilder("datalist");
            buildSelect.GenerateId(datalistId);

            foreach (var item in selectListItems)
            {
                var buildOption = new TagBuilder("option");
                buildOption.MergeAttribute("value", item.Value);
                buildOption.SetInnerText(item.Text);
                buildOption.ToString(TagRenderMode.Normal);
                if (item.Selected == true)
                {
                    buildInput.MergeAttribute("value", item.Value);
                    buildOption.MergeAttribute("selected", "selected");
                }

                buildSelect.InnerHtml += buildOption;
            }

            buildDiv.InnerHtml += buildInput;
            buildDiv.InnerHtml += buildSelect;

            return new MvcHtmlString(buildDiv.ToString(TagRenderMode.Normal));
        }

    }
}