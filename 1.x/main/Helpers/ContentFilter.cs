﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Awful
{
    public class ContentFilter
    {
        private static readonly ContentFilter instance = new ContentFilter();
        private readonly Random _seed;
        private readonly IList<string> _blackList;
        private readonly Regex _regex;
        private readonly IList<string> _filterChar = new List<string>() { "&", "^", "%", "$" };
        private readonly AwfulSettings _settings;
        private ContentFilter()
        {
            this._blackList = CreateBlacklist(1000);
            this._regex = CreateRegex(this._blackList);
            this._settings = new AwfulSettings();
            this._seed = new Random();
        }

        public static string Censor(string content)
        {
            if (!instance._settings.AreParentalControlsEnabled) return content;

            content = instance._regex.Replace(content, "****");
            return content;
        }

        private IList<string> CreateBlacklist(int capacity)
        {
            // set word filters here
            var list = new List<string>(capacity)
            {
                "fuck", "shit", "piss", "dick", "nigger", "coon", "dyke", "faggot", "cock", "balls",
                "pussy", "vagina", "clitoris", "ass", "bitch", "penis", "vag", "cum", "semen", "masturbate",
                "orgasm", "rape", "anal", "sex", "tits", "tit", "titties", "juggs", "cunt", "pubic", "mons",
                "hentai", "pedo", "lolicon"
            };

            return list;
        }

        private Regex CreateRegex(IList<string> list)
        {
            var builder = new StringBuilder();
            for(int i = 0; i < list.Count - 1; i++)
            {
                builder.Append(list[i]);
                builder.Append("|");
            }

            builder.Append(list[list.Count - 1]);
            
            var options = RegexOptions.IgnoreCase;
            var regex = new Regex(builder.ToString(), options);
            return regex;
        }

        private string AddFilter(string word)
        {
            int count = word.Length;
            int filterCount = this._filterChar.Count;
            var builder = new StringBuilder();
            for(int i = 0; i < count; i++)
            {
                int index = this._seed.Next(filterCount);
                builder.Append(this._filterChar[index]);
            }

            return builder.ToString();
        }
    }
}
