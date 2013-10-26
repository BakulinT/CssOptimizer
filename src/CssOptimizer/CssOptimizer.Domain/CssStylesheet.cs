﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using CssOptimizer.Domain.Utils;

namespace CssOptimizer.Domain
{
	public class CssStylesheet
	{

		private readonly List<CssSelector> _selectors = new List<CssSelector>();
		private readonly List<string> _imports = new List<string>();

		public Uri Url { get; set; }
		public IEnumerable<CssSelector> Selectors { get { return _selectors; } }
		public IEnumerable<string> Imports { get { return _imports; } }

		public CssStylesheet(Uri url, string css)
		{
			Ensure.NotNullOrEmpty(css, "css");

			Url = url;

			ProcessImports(css);
			Process(CleanUp(css));
		}

		private void ProcessImports(string css)
		{
			var regex = new Regex(@"@import\s+(url\(['\""]|['\""])([a-z][a-z0-9+\-.:/]*)(['\""]\)|['\""])");

			var matchResults = regex.Match(css);
			while (matchResults.Success)
			{
				var group = matchResults.Groups[2];
				if (group.Success)
				{
					_imports.Add(group.Value);
				}
				matchResults = matchResults.NextMatch();
			} 
		}

		private void Process(string css)
		{
			var selectors = new List<string>();

			var rules = css.Split('}');
			foreach (var rule in rules)
			{
				if (rule.IndexOf('{') > -1)
				{
					selectors.AddRange(ExtractSelectors(rule));
				}
			}

			FillSelectors(selectors);
		}

		private void FillSelectors(IEnumerable<string> selectors)
		{
			selectors.Distinct().ToList().ForEach(s =>
			{
				try
				{
					var cssSelector = new CssSelector(s);
					_selectors.Add(cssSelector);
				}
				catch (UnsupportedSelectorException ex)
				{
					
				}
			});
		}

		private IEnumerable<string> ExtractSelectors(string rule)
		{
			var selectors = new List<string>();

			var selectorsPart = rule.Split('{')[0];

			selectors.AddRange(selectorsPart.Split(new[] { ',' , ';'}).Select(s => s.Trim()).ToList());

			return selectors;
		}

		

		/// <summary>
		/// Удаляет комментарии
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private string CleanUp(string input)
		{
			var copy = input;
			copy = new Regex("(/\\*(.|[\r\n])*?\\*/)|(//.*)")
						.Replace(copy, "")
						.Replace("\r", "")
						.Replace("\n", "");
			return copy.Trim().ToLower();
		}
	}
}
