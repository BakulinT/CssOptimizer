﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CssOptimizer.Domain
{
	public static class HtmlDocumentExtensions
	{
		private static IEnumerable<HtmlNode> SelectNodeCollection(this HtmlNode node, string xPath)
		{
			return node.SelectNodes(xPath) ?? new HtmlNodeCollection(null);
		}

		public static IEnumerable<string> GetExternalCssUrls(this HtmlDocument html)
		{
			return html.DocumentNode
				.SelectNodeCollection("//link[@rel='stylesheet' and (@href)]")
				.Select(link => link.Attributes["href"].Value.Trim())
				.Distinct()
				.ToList();
		}

		public static IEnumerable<Uri> GetInternalLinks(this HtmlDocument html, Uri baseUrl)
		{
			var domain = baseUrl.GetLeftPart(UriPartial.Authority);


			var xPath = String.Format(@"//a[
							starts-with(@href, '{0}') 
							or starts-with(@href, '/') 
							or starts-with(@href, './') 
							or starts-with(@href, '../')]", domain);

			var hrefValues = html.DocumentNode
				.SelectNodeCollection(xPath)
				.AsParallel()
				.Select(link => link.Attributes["href"].Value).ToList();

			return hrefValues.Distinct().Select(z => new Uri(new Uri(domain), z)).ToList();
		}

		public static string GetInlineStyles(this HtmlDocument html)
		{
			return html.DocumentNode
				.SelectNodeCollection("//style")
				.Aggregate(String.Empty, (inline, node) => inline + Regex.Replace(node.InnerText, @"<!--[\s\S]*?-->", "").Trim());
		}

		public static bool IsSelectorInUse(this HtmlDocument html, CssSelector selector)
		{
			bool isSelectorInUse =true;
			try
			{
				isSelectorInUse = html.DocumentNode.SelectNodes(selector.ToXPath()) != null;
			}
			catch (Exception)
			{
				
			}
			
			return isSelectorInUse;
		}
	}
}
