﻿using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;

namespace BlazorComponent.I18n;

public class I18n
{
    private const string CultureCookieKey = "Masa_I18nConfig_Culture";

    private readonly CookieStorage _cookieStorage;

    public I18n(CookieStorage cookieStorage, IHttpContextAccessor httpContextAccessor)
    {
        _cookieStorage = cookieStorage;

        string cultureName;
        if (httpContextAccessor.HttpContext is not null)
        {
            cultureName = httpContextAccessor.HttpContext.Request.Cookies[CultureCookieKey];

            if (cultureName is null)
            {
                var acceptLanguage = httpContextAccessor.HttpContext.Request.Headers["accept-language"].FirstOrDefault();
                if (acceptLanguage is not null)
                {
                    cultureName = acceptLanguage
                                  .Split(",")
                                  .Select(lang =>
                                  {
                                      var arr = lang.Split(';');
                                      if (arr.Length == 1) return (arr[0], 1);
                                      else return (arr[0], Convert.ToDecimal(arr[1].Split("=")[1]));
                                  })
                                  .OrderByDescending(lang => lang.Item2)
                                  .FirstOrDefault(lang => I18nCache.ContainsCulture(lang.Item1)).Item1;
                }
            }
        }
        else
        {
            cultureName = _cookieStorage.GetCookie(CultureCookieKey) ?? "";
        }

        var culture = GetValidCulture(cultureName);

        SetCultureAndLocale(culture);
    }

    [NotNull]
    public CultureInfo? Culture { get; private set; }

    [NotNull]
    public IReadOnlyDictionary<string, string>? Locale { get; private set; }

    public void SetCulture(CultureInfo culture)
    {
        _cookieStorage?.SetItemAsync(CultureCookieKey, culture);

        SetCultureAndLocale(culture);

        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    public void AddLocale(CultureInfo culture, IReadOnlyDictionary<string, string>? locale, bool isDefault = false)
    {
        I18nCache.AddLocale(culture, locale, isDefault);
    }

    public IEnumerable<CultureInfo> SupportedCultures => I18nCache.GetCultures();

    [return: NotNullIfNotNull("defaultValue")]
    public string? T(string? key, [DoesNotReturnIf(true)] bool whenNullReturnKey = true, string? defaultValue = null, params object[] args)
    {
        string? value;

        if (key is null)
        {
            value = null;
        }
        else
        {
            value = Locale.GetValueOrDefault(key);

            if (value is null && whenNullReturnKey)
            {
                if (key.StartsWith(".") || key.EndsWith("."))
                {
                    value = key;
                }
                else
                {
                    value = key.Split('.').Last();
                }
            }
        }

        if (value is null)
        {
            return defaultValue;
        }

        if (args.Length == 0)
        {
            return value;
        }

        try
        {
            return string.Format(value, args);
        }
        catch (FormatException)
        {
            return value;
        }
    }

    [return: NotNullIfNotNull("defaultValue")]
    public string? T(string? scope, string? key, [DoesNotReturnIf(true)] bool whenNullReturnKey = true,
        [CallerArgumentExpression("key")] string? defaultValue = null,
        params object[] args)
    {
        string? value;

        if (key is null)
        {
            value = null;
        }
        else
        {
            value = Locale.GetValueOrDefault($"{scope}.{key}");

            if (value is null && whenNullReturnKey)
            {
                if (key.StartsWith(".") || key.EndsWith("."))
                {
                    value = key;
                }
                else
                {
                    value = key.Split('.').Last();
                }
            }
        }

        if (value is null)
        {
            return defaultValue;
        }

        if (args.Length == 0)
        {
            return value;
        }

        try
        {
            return string.Format(value, args);
        }
        catch (FormatException)
        {
            return value;
        }
    }

    private void SetCultureAndLocale(CultureInfo culture)
    {
        if (!EmbeddedLocales.ContainsLocale(culture))
        {
            AddLocale(culture, EmbeddedLocales.GetSpecifiedLocale(culture));
        }

        Culture = culture;
        Locale = I18nCache.GetLocale(culture);
    }

    private static CultureInfo GetValidCulture(string cultureName)
    {
        CultureInfo culture;

        try
        {
            culture = CultureInfo.CreateSpecificCulture(cultureName);
        }
        catch (Exception e)
        {
            culture = CultureInfo.CurrentUICulture;
        }

        if (culture.Name == string.Empty)
        {
            culture = I18nCache.DefaultCulture;
        }

        // https://github.com/dotnet/runtime/issues/18998#issuecomment-254565364
        // `CultureInfo.CreateSpecificCulture` has the different behavior in different OS,
        // so need to standardize the culture.
        return culture.Name switch
        {
            "zh-Hans-CN" => new CultureInfo("zh-CN"),
            "zh-Hant-CN" => new CultureInfo("zh-TW"),
            _ => culture
        };
    }
}
