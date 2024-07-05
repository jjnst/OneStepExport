using System;
using System.Globalization;
using UnityEngine;

namespace Kittehface.Framework20
{
	public static class LanguageExtensions
	{
		public static SystemLanguage ToSystemLanguage(this Language value)
		{
			switch (value)
			{
			case Language.EnglishAmerican:
			case Language.EnglishBritish:
				return SystemLanguage.English;
			case Language.FrenchCanadian:
				return SystemLanguage.French;
			case Language.SpanishLatinAmerican:
				return SystemLanguage.Spanish;
			case Language.BrazilianPortuguese:
				return SystemLanguage.Portuguese;
			case Language.Unknown:
				return SystemLanguage.Unknown;
			default:
				if (Enum.IsDefined(typeof(SystemLanguage), (int)value))
				{
					return (SystemLanguage)value;
				}
				return SystemLanguage.Unknown;
			}
		}

		public static Language FromLangaugeCode(string code)
		{
			string text = code.Trim();
			switch (text.ToLowerInvariant())
			{
			case "af":
				return Language.Afrikaans;
			case "ar":
				return Language.Arabic;
			case "eu":
				return Language.Basque;
			case "be":
				return Language.Belarusian;
			case "bg":
				return Language.Bulgarian;
			case "ca":
				return Language.Catalan;
			case "zh":
				return Language.Chinese;
			case "zh-hans":
				return Language.ChineseSimplified;
			case "zh-hant":
				return Language.ChineseTraditional;
			case "cs":
				return Language.Czech;
			case "da":
				return Language.Danish;
			case "nl":
				return Language.Dutch;
			case "en":
				return Language.English;
			case "et":
				return Language.Estonian;
			case "fo":
				return Language.Faroese;
			case "fi":
				return Language.Finnish;
			case "fr":
				return Language.French;
			case "de":
				return Language.German;
			case "el":
				return Language.Greek;
			case "iw":
				return Language.Hebrew;
			case "hu":
				return Language.Hungarian;
			case "is":
				return Language.Icelandic;
			case "in":
				return Language.Indonesian;
			case "it":
				return Language.Italian;
			case "ja":
				return Language.Japanese;
			case "ko":
				return Language.Korean;
			case "lv":
				return Language.Latvian;
			case "lt":
				return Language.Lithuanian;
			case "nb":
				return Language.Norwegian;
			case "pl":
				return Language.Polish;
			case "pt":
				return Language.Portuguese;
			case "ro":
				return Language.Romanian;
			case "ru":
				return Language.Russian;
			case "sh":
				return Language.SerboCroatian;
			case "sr":
				return Language.SerboCroatian;
			case "hr":
				return Language.SerboCroatian;
			case "sk":
				return Language.Slovak;
			case "sl":
				return Language.Slovenian;
			case "es":
				return Language.Spanish;
			case "sv":
				return Language.Swedish;
			case "th":
				return Language.Thai;
			case "tr":
				return Language.Turkish;
			case "uk":
				return Language.Ukrainian;
			case "vi":
				return Language.Vietnamese;
			case "en-us":
				return Language.EnglishAmerican;
			case "en-gb":
				return Language.EnglishBritish;
			case "fr-ca":
				return Language.FrenchCanadian;
			case "es-419":
				return Language.SpanishLatinAmerican;
			case "pt-br":
				return Language.BrazilianPortuguese;
			default:
				return Language.Unknown;
			}
		}

		public static Language FromCultureCode(string code)
		{
			string text = ((code != null) ? code.Trim() : "");
			switch (text.ToLowerInvariant())
			{
			case "af-za":
				return Language.Afrikaans;
			case "ar-ae":
				return Language.Arabic;
			case "eu-es":
				return Language.Basque;
			case "be-by":
				return Language.Belarusian;
			case "bg-bg":
				return Language.Bulgarian;
			case "ca-es":
				return Language.Catalan;
			case "zh-cn":
				return Language.Chinese;
			case "zh-hans":
				return Language.ChineseSimplified;
			case "zh-hant":
				return Language.ChineseTraditional;
			case "cs-cz":
				return Language.Czech;
			case "da-dk":
				return Language.Danish;
			case "nl-nl":
				return Language.Dutch;
			case "en":
				return Language.English;
			case "en-ca":
				return Language.EnglishAmerican;
			case "en-us":
				return Language.EnglishAmerican;
			case "en-gb":
				return Language.EnglishBritish;
			case "et-ee":
				return Language.Estonian;
			case "fo-fo":
				return Language.Faroese;
			case "fi-fi":
				return Language.Finnish;
			case "fr-fr":
				return Language.French;
			case "fr-ca":
				return Language.FrenchCanadian;
			case "de-de":
				return Language.German;
			case "el-gr":
				return Language.Greek;
			case "he-il":
				return Language.Hebrew;
			case "hu-hu":
				return Language.Hungarian;
			case "is-is":
				return Language.Icelandic;
			case "id-id":
				return Language.Indonesian;
			case "it-it":
				return Language.Italian;
			case "ja-jp":
				return Language.Japanese;
			case "ko-kr":
				return Language.Korean;
			case "lv-lv":
				return Language.Latvian;
			case "lt-lt":
				return Language.Lithuanian;
			case "nb-no":
				return Language.Norwegian;
			case "pl-pl":
				return Language.Polish;
			case "pt-br":
				return Language.BrazilianPortuguese;
			case "pt-pt":
				return Language.Portuguese;
			case "ro-ro":
				return Language.Romanian;
			case "ru-ru":
				return Language.Russian;
			case "hr-hr":
				return Language.SerboCroatian;
			case "sk-sk":
				return Language.Slovak;
			case "sl-si":
				return Language.Slovenian;
			case "es-es":
				return Language.Spanish;
			case "es-us":
				return Language.SpanishLatinAmerican;
			case "es-mx":
				return Language.SpanishLatinAmerican;
			case "es-419":
				return Language.SpanishLatinAmerican;
			case "sv-se":
				return Language.Swedish;
			case "th-th":
				return Language.Thai;
			case "tr-tr":
				return Language.Turkish;
			case "uk-ua":
				return Language.Ukrainian;
			case "vi-vn":
				return Language.Vietnamese;
			default:
				return Language.Unknown;
			}
		}

		public static CultureInfo CultureFromLanguage(Language language)
		{
			string text = "";
			switch (language)
			{
			case Language.Afrikaans:
				text = "af-ZA";
				break;
			case Language.Arabic:
				text = "ar-AE";
				break;
			case Language.Basque:
				text = "eu-ES";
				break;
			case Language.Belarusian:
				text = "be-BY";
				break;
			case Language.Bulgarian:
				text = "bg-BG";
				break;
			case Language.Catalan:
				text = "ca-ES";
				break;
			case Language.Chinese:
			case Language.ChineseSimplified:
			case Language.ChineseTraditional:
				text = "zh-CN";
				break;
			case Language.Czech:
				text = "cs-CZ";
				break;
			case Language.Danish:
				text = "da-DK";
				break;
			case Language.Dutch:
				text = "nl-NL";
				break;
			default:
				text = "en-US";
				break;
			case Language.Estonian:
				text = "et-EE";
				break;
			case Language.Faroese:
				text = "fo-FO";
				break;
			case Language.Finnish:
				text = "fi-FI";
				break;
			case Language.French:
				text = "fr-FR";
				break;
			case Language.German:
				text = "de-DE";
				break;
			case Language.Greek:
				text = "el-GR";
				break;
			case Language.Hebrew:
				text = "he-IL";
				break;
			case Language.Icelandic:
				text = "is-IS";
				break;
			case Language.Indonesian:
				text = "id-ID";
				break;
			case Language.Italian:
				text = "it-IT";
				break;
			case Language.Japanese:
				text = "ja-JP";
				break;
			case Language.Korean:
				text = "ko-KR";
				break;
			case Language.Latvian:
				text = "lv-LV";
				break;
			case Language.Lithuanian:
				text = "lt-LT";
				break;
			case Language.Norwegian:
				text = "nb-NO";
				break;
			case Language.Polish:
				text = "pl-PL";
				break;
			case Language.Portuguese:
				text = "pt-PT";
				break;
			case Language.Romanian:
				text = "ro-RO";
				break;
			case Language.Russian:
				text = "ru-RU";
				break;
			case Language.SerboCroatian:
				text = "hu-HU";
				break;
			case Language.Slovak:
				text = "sk-SK";
				break;
			case Language.Slovenian:
				text = "sl-SI";
				break;
			case Language.Spanish:
				text = "es-ES";
				break;
			case Language.Swedish:
				text = "sv-SE";
				break;
			case Language.Thai:
				text = "th-TH";
				break;
			case Language.Turkish:
				text = "tr-TR";
				break;
			case Language.Ukrainian:
				text = "uk-UA";
				break;
			case Language.Vietnamese:
				text = "vi-VN";
				break;
			case Language.Hungarian:
				text = "hu-HU";
				break;
			case Language.EnglishAmerican:
				text = "en-US";
				break;
			case Language.EnglishBritish:
				text = "en-GB";
				break;
			case Language.FrenchCanadian:
				text = "fr-CA";
				break;
			case Language.SpanishLatinAmerican:
				text = "es-419";
				break;
			case Language.BrazilianPortuguese:
				text = "pt-BR";
				break;
			}
			try
			{
				return new CultureInfo(text);
			}
			catch
			{
				return CultureInfo.InvariantCulture;
			}
		}
	}
}
