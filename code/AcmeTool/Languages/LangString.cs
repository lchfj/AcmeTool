using System.Globalization;

namespace AcmeTool.Languages;

public static class LangString
{
    public static readonly ICertWorkMessage Cur = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh" ? new CertWorkMessageZhCN() : new CertWorkMessageEn();
}
